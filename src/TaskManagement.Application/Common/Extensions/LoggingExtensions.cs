using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace TaskManagement.Application.Common.Extensions;

/// <summary>
///     Extension methods for enhanced logging with context.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    ///     Logs an information message with correlation ID and user context.
    /// </summary>
    public static void LogWithContext(
        this ILogger logger,
        string message,
        string? userId = null,
        string? correlationId = null,
        params object[] args)
    {
        using (LogContext.PushProperty("UserId", userId ?? "Unknown"))
        using (LogContext.PushProperty("CorrelationId", correlationId ?? "Unknown"))
        {
            logger.LogInformation(message, args);
        }
    }

    /// <summary>
    ///     Logs a warning message with correlation ID and user context.
    /// </summary>
    public static void LogWarningWithContext(
        this ILogger logger,
        string message,
        string? userId = null,
        string? correlationId = null,
        params object[] args)
    {
        using (LogContext.PushProperty("UserId", userId ?? "Unknown"))
        using (LogContext.PushProperty("CorrelationId", correlationId ?? "Unknown"))
        {
            logger.LogWarning(message, args);
        }
    }

    /// <summary>
    ///     Logs an error message with correlation ID and user context.
    /// </summary>
    public static void LogErrorWithContext(
        this ILogger logger,
        Exception exception,
        string message,
        string? userId = null,
        string? correlationId = null,
        params object[] args)
    {
        using (LogContext.PushProperty("UserId", userId ?? "Unknown"))
        using (LogContext.PushProperty("CorrelationId", correlationId ?? "Unknown"))
        {
            logger.LogError(exception, message, args);
        }
    }

    /// <summary>
    ///     Logs performance metrics for an operation.
    /// </summary>
    public static void LogPerformance(
        this ILogger logger,
        string operation,
        long elapsedMilliseconds,
        string? userId = null,
        string? correlationId = null,
        Dictionary<string, object>? additionalProperties = null)
    {
        var logLevel = elapsedMilliseconds switch
        {
            > 1000 => LogLevel.Error,
            > 500 => LogLevel.Warning,
            _ => LogLevel.Information
        };

        using (LogContext.PushProperty("Operation", operation))
        using (LogContext.PushProperty("DurationMs", elapsedMilliseconds))
        using (LogContext.PushProperty("UserId", userId ?? "Unknown"))
        using (LogContext.PushProperty("CorrelationId", correlationId ?? "Unknown"))
        {
            if (additionalProperties != null)
            {
                foreach (var prop in additionalProperties)
                {
                    LogContext.PushProperty(prop.Key, prop.Value);
                }
            }

            logger.Log(
                logLevel,
                "Performance: {Operation} completed in {DurationMs}ms",
                operation,
                elapsedMilliseconds);
        }
    }

    /// <summary>
    ///     Logs an audit event for business-critical operations.
    /// </summary>
    public static void LogAudit(
        this ILogger logger,
        string action,
        string entityType,
        string entityId,
        string? userId = null,
        string? userEmail = null,
        string? correlationId = null,
        Dictionary<string, object>? details = null)
    {
        using (LogContext.PushProperty("AuditAction", action))
        using (LogContext.PushProperty("EntityType", entityType))
        using (LogContext.PushProperty("EntityId", entityId))
        using (LogContext.PushProperty("UserId", userId ?? "Unknown"))
        using (LogContext.PushProperty("UserEmail", userEmail ?? "Unknown"))
        using (LogContext.PushProperty("CorrelationId", correlationId ?? "Unknown"))
        {
            if (details != null)
            {
                foreach (var detail in details)
                {
                    LogContext.PushProperty($"AuditDetail_{detail.Key}", detail.Value);
                }
            }

            logger.LogInformation(
                "AUDIT: {Action} on {EntityType} {EntityId} by user {UserId}",
                action,
                entityType,
                entityId,
                userId);
        }
    }
}

