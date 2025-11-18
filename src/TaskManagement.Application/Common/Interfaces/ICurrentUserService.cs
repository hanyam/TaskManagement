using System.Security.Claims;

namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Service for accessing the current authenticated user information.
///     Supports override mechanism for testing via IMemoryCache.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    ///     Gets the current user's ID (Guid).
    /// </summary>
    /// <returns>The user ID if available, otherwise null.</returns>
    Guid? GetUserId();

    /// <summary>
    ///     Gets the current user's email/username.
    /// </summary>
    /// <returns>The user email/username if available, otherwise null.</returns>
    string? GetUserEmail();

    /// <summary>
    ///     Gets the current user's principal (ClaimsPrincipal).
    /// </summary>
    /// <returns>The ClaimsPrincipal if available, otherwise null.</returns>
    ClaimsPrincipal? GetUserPrincipal();

    /// <summary>
    ///     Gets a specific claim value from the current user.
    /// </summary>
    /// <param name="claimType">The claim type to retrieve.</param>
    /// <returns>The claim value if found, otherwise null.</returns>
    string? GetClaimValue(string claimType);

    /// <summary>
    ///     Checks if the current user is authenticated.
    /// </summary>
    /// <returns>True if authenticated, otherwise false.</returns>
    bool IsAuthenticated();
}

