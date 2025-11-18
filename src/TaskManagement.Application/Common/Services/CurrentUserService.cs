using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Constants;
using static TaskManagement.Domain.Constants.CustomClaimTypes;

namespace TaskManagement.Application.Common.Services;

/// <summary>
///     Implementation of ICurrentUserService that retrieves user information from HttpContext
///     with support for override via IMemoryCache for testing purposes.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public Guid? GetUserId()
    {
        // Check for override first (for testing)
        if (_memoryCache.TryGetValue(CacheKeys.CurrentUserOverride, out CurrentUserOverride? overrideValue) && overrideValue != null)
        {
            return overrideValue.UserId;
        }

        // Fall back to HttpContext
        var userIdClaim = GetClaimValue(UserId);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }

    public string? GetUserEmail()
    {
        // Check for override first (for testing)
        if (_memoryCache.TryGetValue(CacheKeys.CurrentUserOverride, out CurrentUserOverride? overrideValue) && overrideValue != null)
        {
            return overrideValue.UserEmail;
        }

        // Fall back to HttpContext
        return GetUserPrincipal()?.Identity?.Name ?? GetClaimValue(Email);
    }

    public ClaimsPrincipal? GetUserPrincipal()
    {
        // Check for override first (for testing)
        if (_memoryCache.TryGetValue(CacheKeys.CurrentUserOverride, out CurrentUserOverride? overrideValue) && overrideValue != null)
        {
            // Create a mock ClaimsPrincipal from override
            var claims = new List<Claim>();
            if (overrideValue.UserId.HasValue)
            {
                claims.Add(new Claim(UserId, overrideValue.UserId.Value.ToString()));
            }

            if (!string.IsNullOrEmpty(overrideValue.UserEmail))
            {
                claims.Add(new Claim(ClaimTypes.Name, overrideValue.UserEmail));
                claims.Add(new Claim(Email, overrideValue.UserEmail));
            }

            if (!string.IsNullOrEmpty(overrideValue.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, overrideValue.Role));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        // Fall back to HttpContext
        return _httpContextAccessor.HttpContext?.User;
    }

    public string? GetClaimValue(string claimType)
    {
        var principal = GetUserPrincipal();
        return principal?.FindFirst(claimType)?.Value;
    }

    public bool IsAuthenticated()
    {
        var principal = GetUserPrincipal();
        return principal?.Identity?.IsAuthenticated ?? false;
    }
}

/// <summary>
///     Class for storing user override values in memory cache.
///     Made public so it can be used by TestingController.
/// </summary>
public class CurrentUserOverride
{
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Role { get; set; }
}

