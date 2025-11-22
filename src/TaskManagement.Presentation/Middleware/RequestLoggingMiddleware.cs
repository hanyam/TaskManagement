using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace TaskManagement.Presentation.Middleware;

/// <summary>
///     Middleware that logs HTTP requests and responses with performance metrics.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private static readonly string[] SensitiveHeaders = { "Authorization", "Cookie", "X-Api-Key" };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context) ?? "Unknown";

        // Log incoming request
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var requestMethod = context.Request.Method;
        var queryString = context.Request.QueryString.Value ?? string.Empty;

        // Get user context if available
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var userEmail = context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Unknown";

        // Enrich log context
        using (LogContext.PushProperty("RequestPath", requestPath))
        using (LogContext.PushProperty("RequestMethod", requestMethod))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserEmail", userEmail))
        {
            _logger.LogInformation(
                "Incoming HTTP {RequestMethod} request to {RequestPath}{QueryString} from user {UserId}",
                requestMethod, requestPath, queryString, userId);

            try
            {
                await _next(context);
                stopwatch.Stop();

                var statusCode = context.Response.StatusCode;
                var logLevel = GetLogLevel(statusCode);

                // Log response
                _logger.Log(
                    logLevel,
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds}ms",
                    requestMethod, requestPath, statusCode, stopwatch.ElapsedMilliseconds);

                // Log performance warnings for slow requests
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "Slow request detected: {RequestMethod} {RequestPath} took {ElapsedMilliseconds}ms",
                        requestMethod, requestPath, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Unhandled exception in HTTP {RequestMethod} {RequestPath} after {ElapsedMilliseconds}ms",
                    requestMethod, requestPath, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }

    private static LogLevel GetLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }

    private static string SanitizeHeaders(IHeaderDictionary headers)
    {
        var sb = new StringBuilder();
        foreach (var header in headers)
        {
            if (SensitiveHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine($"{header.Key}: [REDACTED]");
            }
            else
            {
                var values = header.Value.Where(v => v != null).ToArray();
                sb.AppendLine($"{header.Key}: {string.Join(", ", values)}");
            }
        }
        return sb.ToString();
    }
}

