using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Base controller with common functionality for all API controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController(
    ICommandMediator commandMediator,
    IRequestMediator requestMediator,
    ICurrentUserService currentUserService,
    Application.Common.Interfaces.ILocalizationService localizationService)
    : ControllerBase
{
    protected readonly ICommandMediator _commandMediator = commandMediator;
    protected readonly IRequestMediator _requestMediator = requestMediator;
    protected readonly ICurrentUserService _currentUserService = currentUserService;
    protected readonly Application.Common.Interfaces.ILocalizationService _localizationService = localizationService;

    /// <summary>
    ///     Gets the current user ID from ICurrentUserService (with override support for testing).
    /// </summary>
    /// <returns>The user ID if available, otherwise null.</returns>
    protected Guid? GetCurrentUserId()
    {
        return _currentUserService.GetUserId();
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
    ///     Gets the current user email from ICurrentUserService (with override support for testing).
    /// </summary>
    /// <returns>The user email if available, otherwise null.</returns>
    protected string? GetCurrentUserEmail()
    {
        return _currentUserService.GetUserEmail();
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

        // Localize error messages
        var localizedErrors = allErrors.Select(e => LocalizeError(e)).ToList();

        return allErrors.Any()
            ? BadRequest(ApiResponse<T>.ErrorResponse(localizedErrors, HttpContext.TraceIdentifier))
            : BadRequest(ApiResponse<T>.ErrorResponse(_localizationService.GetString("Errors.System.InternalServerError", "An error occurred"), HttpContext.TraceIdentifier));
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

        // Localize error messages
        var localizedErrors = allErrors.Select(e => LocalizeError(e)).ToList();

        return allErrors.Any()
            ? BadRequest(ApiResponse.ErrorResponse(localizedErrors, HttpContext.TraceIdentifier))
            : BadRequest(ApiResponse.ErrorResponse(_localizationService.GetString("Errors.System.InternalServerError", "An error occurred"), HttpContext.TraceIdentifier));
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

        // Localize error messages
        var localizedErrors = allErrors.Select(e => LocalizeError(e)).ToList();

        return allErrors.Any()
            ? BadRequest(ApiResponse<T>.ErrorResponse(localizedErrors, HttpContext.TraceIdentifier))
            : BadRequest(ApiResponse<T>.ErrorResponse(_localizationService.GetString("Errors.System.InternalServerError", "An error occurred"), HttpContext.TraceIdentifier));
    }

    /// <summary>
    ///     Localizes an error message if a message key is provided, otherwise returns the original error.
    /// </summary>
    private Error LocalizeError(Error error)
    {
        if (string.IsNullOrWhiteSpace(error.MessageKey))
        {
            return error; // No key, return as-is
        }

        // Try to extract format arguments from the original message for common patterns
        var formatArgs = ExtractFormatArguments(error.Message, error.MessageKey);
        
        var localizedMessage = formatArgs.Length > 0
            ? _localizationService.GetString(error.MessageKey, error.Message, formatArgs)
            : _localizationService.GetString(error.MessageKey, error.Message);
        
        return Error.Create(error.Code, localizedMessage, error.Field, error.MessageKey);
    }

    /// <summary>
    ///     Extracts format arguments from error messages for common patterns.
    /// </summary>
    private object[] ExtractFormatArguments(string originalMessage, string messageKey)
    {
        // Handle ProgressMinNotMet: "Progress must be at least {X}%..."
        if (messageKey == "Errors.Tasks.ProgressMinNotMet")
        {
            var match = System.Text.RegularExpressions.Regex.Match(originalMessage, @"at least (\d+)%");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var minProgress))
            {
                return new object[] { minProgress };
            }
        }

        // Handle NotFoundById: "Task with ID '{X}' not found"
        if (messageKey.Contains(".NotFoundById"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(originalMessage, @"ID '([^']+)'");
            if (match.Success)
            {
                return new object[] { match.Groups[1].Value };
            }
        }

        // Handle NotFoundByEmail: "User with email '{X}' not found"
        if (messageKey.Contains(".NotFoundByEmail"))
        {
            var match = System.Text.RegularExpressions.Regex.Match(originalMessage, @"email '([^']+)'");
            if (match.Success)
            {
                return new object[] { match.Groups[1].Value };
            }
        }

        return Array.Empty<object>();
    }
}
