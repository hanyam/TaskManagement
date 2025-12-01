using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Application.Common.Services;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Errors.Authentication;
using TaskManagement.Domain.Errors.Users;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using static TaskManagement.Domain.Constants.CustomClaimTypes;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Controller for testing overrides (current user and date/time).
///     Only available in Development and Test environments.
/// </summary>
[ApiController]
[Route("testing")]
public class TestingController(
    IMemoryCache memoryCache,
    UserDapperRepository userRepository,
    IAuthenticationService authenticationService) : ControllerBase
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly UserDapperRepository _userRepository = userRepository;

    /// <summary>
    ///     Sets the current user override for testing purposes.
    ///     Only available in Development and Test environments.
    ///     Accepts only email and fetches user information from the database.
    /// </summary>
    /// <param name="request">The user override request (email only).</param>
    /// <returns>Success or error response.</returns>
    [HttpPost("current-user")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetCurrentUserOverride([FromBody] SetCurrentUserOverrideRequest request)
    {
        if (!IsTestingEnvironment())
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));

        if (string.IsNullOrWhiteSpace(request.UserEmail))
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<Error> { UserErrors.EmailRequired },
                HttpContext.TraceIdentifier));

        // Query user from database by email
        var user = await _userRepository.GetByEmailAsync(request.UserEmail, CancellationToken.None);

        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse(
                new List<Error> { UserErrors.NotFoundByEmail(request.UserEmail) },
                HttpContext.TraceIdentifier));

        // Create override value with data from database
        var overrideValue = new CurrentUserOverride
        {
            UserId = user.Id,
            UserEmail = user.Email,
            Role = user.Role.ToString() // Converts UserRole enum to string (Employee, Manager, Admin)
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
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));

        if (_memoryCache.TryGetValue(CacheKeys.CurrentUserOverride, out CurrentUserOverride? overrideValue))
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                userId = overrideValue?.UserId,
                userEmail = overrideValue?.UserEmail,
                role = overrideValue?.Role
            }));

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
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));

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
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));

        // Ensure the date is UTC
        var utcDate = request.UtcDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.UtcDate, DateTimeKind.Utc)
            : request.UtcDate.ToUniversalTime();

        // Store in cache with no expiration (cleared manually or on app restart)
        _memoryCache.Set(CacheKeys.CurrentDateOverride, utcDate);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "Current date override set successfully",
            utcDate,
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
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));

        if (_memoryCache.TryGetValue(CacheKeys.CurrentDateOverride, out DateTime? overrideValue))
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                utcDate = overrideValue,
                localDate = overrideValue?.ToLocalTime()
            }));

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
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));

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

    /// <summary>
    ///     Generates a JWT token for the specified user email (testing only).
    /// </summary>
    /// <param name="request">The request containing the user email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token that can be used for testing other endpoints.</returns>
    [HttpPost("generate-jwt")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateJwtToken([FromBody] GenerateJwtTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsTestingEnvironment())
            return StatusCode(403, ApiResponse<object>.ErrorResponse(
                "Testing endpoints are only available in Development or Test environments.",
                HttpContext.TraceIdentifier));

        if (string.IsNullOrWhiteSpace(request.UserEmail))
            return BadRequest(ApiResponse<object>.ErrorResponse(
                new List<Error> { UserErrors.EmailRequired },
                HttpContext.TraceIdentifier));

        var user = await _userRepository.GetByEmailAsync(request.UserEmail, cancellationToken);
        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse(
                new List<Error> { UserErrors.NotFoundByEmail(request.UserEmail) },
                HttpContext.TraceIdentifier));

        var additionalClaims = new Dictionary<string, string>
        {
            { UserId, user.Id.ToString() },
            { "display_name", user.DisplayName },
            { "first_name", user.FirstName },
            { "last_name", user.LastName },
            { "role", user.Role.ToString() }
        };

        var tokenResult = await _authenticationService.GenerateJwtTokenAsync(
            user.Email,
            additionalClaims,
            cancellationToken);

        if (tokenResult.IsFailure)
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                new List<Error> { tokenResult.Error ?? AuthenticationErrors.JwtTokenGenerationFailed },
                HttpContext.TraceIdentifier));

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "JWT token generated successfully",
            token = tokenResult.Value
        }));
    }
}

/// <summary>
///     Request model for setting current user override.
///     Only email is required; user ID and role are fetched from the database.
/// </summary>
public class SetCurrentUserOverrideRequest
{
    /// <summary>
    ///     The user email to override with. User ID and role will be fetched from the database.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;
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

/// <summary>
///     Request model for generating JWT tokens in testing.
/// </summary>
public class GenerateJwtTokenRequest
{
    /// <summary>
    ///     The user email to generate a JWT token for.
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;
}