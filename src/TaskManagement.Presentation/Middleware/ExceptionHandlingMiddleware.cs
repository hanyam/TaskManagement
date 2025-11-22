using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using TaskManagement.Domain.Common;

namespace TaskManagement.Presentation.Middleware;

/// <summary>
///     Global exception handling middleware for standardized error responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            var requestPath = context.Request.Path.Value ?? string.Empty;
            var requestMethod = context.Request.Method;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("RequestPath", requestPath))
            using (LogContext.PushProperty("RequestMethod", requestMethod))
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception in {RequestMethod} {RequestPath} for user {UserId}. Exception: {ExceptionType} - {ExceptionMessage}",
                    requestMethod,
                    requestPath,
                    userId,
                    ex.GetType().Name,
                    ex.Message);

                // Log inner exception if present
                if (ex.InnerException != null)
                {
                    _logger.LogError(
                        ex.InnerException,
                        "Inner exception: {InnerExceptionType} - {InnerExceptionMessage}",
                        ex.InnerException.GetType().Name,
                        ex.InnerException.Message);
                }
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "An error occurred while processing your request",
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = argEx.Message;
                break;
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized access";
                break;
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "Resource not found";
                break;
            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = invalidOpEx.Message;
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An internal server error occurred";
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
