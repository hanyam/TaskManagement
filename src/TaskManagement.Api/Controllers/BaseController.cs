using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Api.Controllers;

/// <summary>
///     Base controller with common functionality for all API controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController(ICommandMediator commandMediator, IRequestMediator requestMediator)
    : ControllerBase
{
    protected readonly ICommandMediator _commandMediator = commandMediator;
    protected readonly IRequestMediator _requestMediator = requestMediator;

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