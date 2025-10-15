using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common.Behaviors;

/// <summary>
///     Pipeline behavior that handles exceptions during request processing.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<TResponse>> Handle(TRequest request, Func<Task<Result<TResponse>>> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while processing request {RequestType}", typeof(TRequest).Name);

            // Return a generic error response
            return Result<TResponse>.Failure(
                Error.Internal("An unexpected error occurred while processing your request. Please try again later."));
        }
    }
}

/// <summary>
///     Pipeline behavior that handles exceptions during request processing (no response).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public class ExceptionHandlingBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(TRequest request, Func<Task<Result>> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while processing request {RequestType}", typeof(TRequest).Name);

            // Return a generic error response
            return Result.Failure(
                Error.Internal("An unexpected error occurred while processing your request. Please try again later."));
        }
    }
}