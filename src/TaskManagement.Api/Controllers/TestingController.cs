using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Application.Common.Services;
using TaskManagement.Domain.Common;

namespace TaskManagement.Api.Controllers;

/// <summary>
///     Controller for testing overrides (current user and date/time).
///     Only available in Development and Test environments.
/// </summary>
[ApiController]
[Route("testing")]
public class TestingController(IMemoryCache memoryCache) : ControllerBase
{
    private readonly IMemoryCache _memoryCache = memoryCache;

    /// <summary>
    ///     Sets the current user override for testing purposes.
    ///     Only available in Development and Test environments.
    /// </summary>
    /// <param name="request">The user override request.</param>
    /// <returns>Success or error response.</returns>
    [HttpPost("current-user")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult SetCurrentUserOverride([FromBody] SetCurrentUserOverrideRequest request)
    {
        if (!IsTestingEnvironment())
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));
        }

        if (request.UserId == null && string.IsNullOrEmpty(request.UserEmail))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Either UserId or UserEmail must be provided.",
                HttpContext.TraceIdentifier));
        }

        var overrideValue = new CurrentUserOverride
        {
            UserId = request.UserId,
            UserEmail = request.UserEmail,
            Role = request.Role
        };

        // Store in cache with no expiration (cleared manually or on app restart)
        _memoryCache.Set(CacheKeys.CurrentUserOverride, overrideValue);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "Current user override set successfully",
            userId = overrideValue.UserId,
            userEmail = overrideValue.UserEmail,
            role = overrideValue.Role
        }));
    }

    /// <summary>
    ///     Gets the current user override value.
    ///     Only available in Development and Test environments.
    /// </summary>
    /// <returns>The current user override or null if not set.</returns>
    [HttpGet("current-user")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetCurrentUserOverride()
    {
        if (!IsTestingEnvironment())
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));
        }

        if (_memoryCache.TryGetValue(CacheKeys.CurrentUserOverride, out CurrentUserOverride? overrideValue))
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                userId = overrideValue?.UserId,
                userEmail = overrideValue?.UserEmail,
                role = overrideValue?.Role
            }));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "No current user override is set"
        }));
    }

    /// <summary>
    ///     Removes the current user override.
    ///     Only available in Development and Test environments.
    /// </summary>
    /// <returns>Success response.</returns>
    [HttpDelete("current-user")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult RemoveCurrentUserOverride()
    {
        if (!IsTestingEnvironment())
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));
        }

        _memoryCache.Remove(CacheKeys.CurrentUserOverride);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "Current user override removed successfully"
        }));
    }

    /// <summary>
    ///     Sets the current date/time override for testing purposes.
    ///     Only available in Development and Test environments.
    /// </summary>
    /// <param name="request">The date override request (UTC).</param>
    /// <returns>Success or error response.</returns>
    [HttpPost("current-date")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult SetCurrentDateOverride([FromBody] SetCurrentDateOverrideRequest request)
    {
        if (!IsTestingEnvironment())
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));
        }

        // Ensure the date is UTC
        var utcDate = request.UtcDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.UtcDate, DateTimeKind.Utc)
            : request.UtcDate.ToUniversalTime();

        // Store in cache with no expiration (cleared manually or on app restart)
        _memoryCache.Set(CacheKeys.CurrentDateOverride, utcDate);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "Current date override set successfully",
            utcDate = utcDate,
            localDate = utcDate.ToLocalTime()
        }));
    }

    /// <summary>
    ///     Gets the current date/time override value.
    ///     Only available in Development and Test environments.
    /// </summary>
    /// <returns>The current date override or null if not set.</returns>
    [HttpGet("current-date")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetCurrentDateOverride()
    {
        if (!IsTestingEnvironment())
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));
        }

        if (_memoryCache.TryGetValue(CacheKeys.CurrentDateOverride, out DateTime? overrideValue))
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                utcDate = overrideValue,
                localDate = overrideValue?.ToLocalTime()
            }));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "No current date override is set",
            currentUtcDate = DateTime.UtcNow,
            currentLocalDate = DateTime.Now
        }));
    }

    /// <summary>
    ///     Removes the current date/time override.
    ///     Only available in Development and Test environments.
    /// </summary>
    /// <returns>Success response.</returns>
    [HttpDelete("current-date")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult RemoveCurrentDateOverride()
    {
        if (!IsTestingEnvironment())
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));
        }

        _memoryCache.Remove(CacheKeys.CurrentDateOverride);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "Current date override removed successfully"
        }));
    }

    private bool IsTestingEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return environment == "Development" || environment == "Test" || environment == "Testing";
    }
}

/// <summary>
///     Request model for setting current user override.
/// </summary>
public class SetCurrentUserOverrideRequest
{
    /// <summary>
    ///     The user ID (Guid) to override with.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     The user email/username to override with.
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    ///     Optional: The user role to override with (e.g., "Admin", "Manager", "Employee").
    /// </summary>
    public string? Role { get; set; }
}

/// <summary>
///     Request model for setting current date override.
/// </summary>
public class SetCurrentDateOverrideRequest
{
    /// <summary>
    ///     The UTC date/time to override with.
    /// </summary>
    public DateTime UtcDate { get; set; }
}


