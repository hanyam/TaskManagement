using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;

namespace TaskManagement.Presentation.Middleware;

/// <summary>
///     Middleware that generates and propagates correlation IDs for request tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private const string CorrelationIdItemName = "CorrelationId";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request header, otherwise generate a new one
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault() 
                             ?? Guid.NewGuid().ToString();

        // Store in HttpContext.Items for access throughout the request pipeline
        context.Items[CorrelationIdItemName] = correlationId;

        // Add to response headers so clients can track requests
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        // Enrich Serilog context with correlation ID for all logs in this request
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    /// <summary>
    ///     Gets the correlation ID from HttpContext.Items.
    /// </summary>
    public static string? GetCorrelationId(HttpContext context)
    {
        return context.Items[CorrelationIdItemName]?.ToString();
    }
}

