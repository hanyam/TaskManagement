using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using static TaskManagement.Domain.Constants.CustomClaimTypes;

namespace TaskManagement.Api.Controllers;

/// <summary>
///     Base controller with common functionality for all API controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController(
    ICommandMediator commandMediator,
    IRequestMediator requestMediator,
    ICurrentUserService? currentUserService = null)
    : ControllerBase
{
    protected readonly ICommandMediator _commandMediator = commandMediator;
    protected readonly IRequestMediator _requestMediator = requestMediator;
    protected readonly ICurrentUserService? _currentUserService = currentUserService;

    /// <summary>
    ///     Gets the current user ID from ICurrentUserService (with override support) or falls back to HttpContext.User.
    ///     If EnsureUserIdAttribute is used, this will always return a value (throws if not set).
    /// </summary>
    /// <returns>The user ID if available, otherwise null.</returns>
    protected Guid? GetCurrentUserId()
    {
        // Check if EnsureUserIdAttribute already set it in HttpContext.Items
        if (HttpContext.Items.TryGetValue("CurrentUserId", out var storedUserId) && storedUserId is Guid guid)
        {
            return guid;
        }

        if (_currentUserService != null)
        {
            return _currentUserService.GetUserId();
        }

        // Fallback to HttpContext.User for backward compatibility
        var userIdClaim = User.FindFirst(UserId)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }

    /// <summary>
    ///     Gets the current user ID, throwing an exception if not found.
    ///     Use this when EnsureUserIdAttribute is applied to ensure user ID exists.
    /// </summary>
    /// <returns>The user ID (never null).</returns>
    /// <exception cref="InvalidOperationException">Thrown if user ID is not found.</exception>
    protected Guid GetRequiredUserId()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            throw new InvalidOperationException("User ID not found. Ensure EnsureUserIdAttribute is applied.");
        }

        return userId.Value;
    }

    /// <summary>
    ///     Gets the current user email from ICurrentUserService (with override support) or falls back to HttpContext.User.
    /// </summary>
    /// <returns>The user email if available, otherwise null.</returns>
    protected string? GetCurrentUserEmail()
    {
        if (_currentUserService != null)
        {
            return _currentUserService.GetUserEmail();
        }

        // Fallback to HttpContext.User for backward compatibility
        return User.Identity?.Name ?? User.FindFirst(Email)?.Value;
    }

    /// <summary>
    ///     Handles the result of a command or query and returns appropriate HTTP response.
    /// </summary>
    /// <typeparam name="T">The type of data being returned.</typeparam>
    /// <param name="result">The result from the command/query handler.</param>
    /// <param name="successStatusCode">HTTP status code for successful operations.</param>
    /// <returns>HTTP response with standardized format.</returns>
    protected IActionResult HandleResult<T>(Result<T> result, int successStatusCode = 200)
    {
        if (result.IsSuccess) return StatusCode(successStatusCode, ApiResponse<T>.SuccessResponse(result.Value!));

        // Collect all errors (both single Error and Errors collection)
        var allErrors = new List<Error>();
        if (result.Errors.Any()) allErrors.AddRange(result.Errors);
        if (result.Error != null) allErrors.Add(result.Error);

        return allErrors.Any()
            ? BadRequest(ApiResponse<T>.ErrorResponse(allErrors, HttpContext.TraceIdentifier))
            : BadRequest(ApiResponse<T>.ErrorResponse("An error occurred", HttpContext.TraceIdentifier));
    }

    /// <summary>
    ///     Handles the result of a command that doesn't return data.
    /// </summary>
    /// <param name="result">The result from the command handler.</param>
    /// <param name="successStatusCode">HTTP status code for successful operations.</param>
    /// <returns>HTTP response with standardized format.</returns>
    protected IActionResult HandleResult(Result result, int successStatusCode = 200)
    {
        if (result.IsSuccess) return StatusCode(successStatusCode, ApiResponse.SuccessResponse());

        // Collect all errors (both single Error and Errors collection)
        var allErrors = new List<Error>();
        if (result.Errors.Any()) allErrors.AddRange(result.Errors);
        if (result.Error != null) allErrors.Add(result.Error);

        return allErrors.Any()
            ? BadRequest(ApiResponse.ErrorResponse(allErrors, HttpContext.TraceIdentifier))
            : BadRequest(ApiResponse.ErrorResponse("An error occurred", HttpContext.TraceIdentifier));
    }

    /// <summary>
    ///     Handles the result with HATEOAS links.
    /// </summary>
    /// <typeparam name="T">The type of data being returned.</typeparam>
    /// <param name="result">The result from the command/query handler.</param>
    /// <param name="links">HATEOAS links to include in the response.</param>
    /// <param name="successStatusCode">HTTP status code for successful operations.</param>
    /// <returns>HTTP response with standardized format including HATEOAS links.</returns>
    protected IActionResult HandleResultWithLinks<T>(Result<T> result, List<ApiActionLink>? links,
        int successStatusCode = 200)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.SuccessResponse(result.Value!);
            response.Links = links;
            return StatusCode(successStatusCode, response);
        }

        // Collect all errors (both single Error and Errors collection)
        var allErrors = new List<Error>();
        if (result.Errors.Any()) allErrors.AddRange(result.Errors);
        if (result.Error != null) allErrors.Add(result.Error);

        return allErrors.Any()
            ? BadRequest(ApiResponse<T>.ErrorResponse(allErrors, HttpContext.TraceIdentifier))
            : BadRequest(ApiResponse<T>.ErrorResponse("An error occurred", HttpContext.TraceIdentifier));
    }
}