using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using TaskManagement.Application.Common.Extensions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common.Behaviors;

/// <summary>
///     Pipeline behavior that logs request processing.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<TResponse>> Handle(TRequest request, Func<Task<Result<TResponse>>> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("RequestType", requestName))
        {
            _logger.LogInformation("Starting to process request {RequestName}", requestName);

            try
            {
                var result = await next();
                stopwatch.Stop();

                if (result.IsSuccess)
                    _logger.LogPerformance(
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        null,
                        null,
                        new Dictionary<string, object> { { "Success", true } });
                else
                    _logger.LogWarning(
                        "Failed to process request {RequestName} in {ElapsedMilliseconds}ms. Error: {Error}",
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        result.Error);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Exception occurred while processing request {RequestName} in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}

/// <summary>
///     Pipeline behavior that logs request processing (no response).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public class LoggingBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger<LoggingBehavior<TRequest>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(TRequest request, Func<Task<Result>> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("RequestType", requestName))
        {
            _logger.LogInformation("Starting to process request {RequestName}", requestName);

            try
            {
                var result = await next();
                stopwatch.Stop();

                if (result.IsSuccess)
                    _logger.LogPerformance(
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        null,
                        null,
                        new Dictionary<string, object> { { "Success", true } });
                else
                    _logger.LogWarning(
                        "Failed to process request {RequestName} in {ElapsedMilliseconds}ms. Error: {Error}",
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        result.Error);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Exception occurred while processing request {RequestName} in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}