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

        return result.Errors.Any()
            ? BadRequest(ApiResponse<T>.ErrorResponse(result.Errors, HttpContext.TraceIdentifier))
            : BadRequest(ApiResponse<T>.ErrorResponse(result.Error?.Message ?? "An error occurred",
                HttpContext.TraceIdentifier));
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

        return result.Errors.Any()
            ? BadRequest(ApiResponse.ErrorResponse(result.Errors, HttpContext.TraceIdentifier))
            : BadRequest(ApiResponse.ErrorResponse(result.Error?.Message ?? "An error occurred", HttpContext.TraceIdentifier));
    }
}