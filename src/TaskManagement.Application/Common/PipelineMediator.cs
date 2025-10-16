using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common;

/// <summary>
///     Pipeline-based mediator implementation that handles requests with pipeline behaviors.
/// </summary>
public class PipelineMediator : IMediator
{
    private readonly ILogger<PipelineMediator> _logger;
    private readonly IServiceLocator _serviceLocator;

    public PipelineMediator(IServiceLocator serviceLocator, ILogger<PipelineMediator> logger)
    {
        _serviceLocator = serviceLocator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing request of type {RequestType}", request.GetType().Name);

            // Get the handler type dynamically
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceLocator.GetRequiredService(handlerType);

            // Build the pipeline with behaviors
            var pipeline = BuildPipeline<IRequest<TResponse>, TResponse>(request, handler, cancellationToken);

            // Execute the pipeline
            var result = await pipeline();

            if (result.IsSuccess)
                _logger.LogInformation("Request of type {RequestType} processed successfully", request.GetType().Name);
            else
                _logger.LogWarning("Request of type {RequestType} failed: {Error}", request.GetType().Name,
                    result.Error);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request of type {RequestType}", request.GetType().Name);
            return Result<TResponse>.Failure(Error.Internal($"An error occurred while processing the request: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<Result> Send(IRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing request of type {RequestType}", request.GetType().Name);

            // Get the handler type dynamically
            var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
            var handler = _serviceLocator.GetRequiredService(handlerType);

            // Build the pipeline with behaviors
            var pipeline = BuildPipeline<IRequest>(request, handler, cancellationToken);

            // Execute the pipeline
            var result = await pipeline();

            if (result.IsSuccess)
                _logger.LogInformation("Request of type {RequestType} processed successfully", request.GetType().Name);
            else
                _logger.LogWarning("Request of type {RequestType} failed: {Error}", request.GetType().Name,
                    result.Error);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request of type {RequestType}", request.GetType().Name);
            return Result.Failure(Error.Internal($"An error occurred while processing the request: {ex.Message}"));
        }
    }

    private Func<Task<Result<TResponse>>> BuildPipeline<TRequest, TResponse>(TRequest request, object handler, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        // Get the handle method
        var handleMethod = handler.GetType().GetMethod("Handle");
        if (handleMethod == null)
            throw new InvalidOperationException($"Handler {handler.GetType().Name} does not have a Handle method");

        // Create the base handler function
        Func<Task<Result<TResponse>>> handlerFunc = async () =>
        {
            var task = (Task<Result<TResponse>>)handleMethod.Invoke(handler, new object[] { request, cancellationToken })!;
            return await task;
        };

        // Apply behaviors in reverse order (last behavior wraps the handler)
        var pipeline = handlerFunc;

        // Get pipeline behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));
        var behaviors = _serviceLocator.GetServices(behaviorType).Cast<IPipelineBehavior<TRequest, TResponse>>().ToList();

        // Apply behaviors in reverse order (last behavior wraps the handler)
        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            pipeline = WrapWithBehavior(request, pipeline, behavior, cancellationToken);
        }

        return pipeline;
    }

    private Func<Task<Result>> BuildPipeline<TRequest>(TRequest request, object handler, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        // Get the handle method
        var handleMethod = handler.GetType().GetMethod("Handle");
        if (handleMethod == null)
            throw new InvalidOperationException($"Handler {handler.GetType().Name} does not have a Handle method");

        // Create the base handler function
        Func<Task<Result>> handlerFunc = async () =>
        {
            var task = (Task<Result>)handleMethod.Invoke(handler, new object[] { request, cancellationToken })!;
            return await task;
        };

        // Apply behaviors in reverse order (last behavior wraps the handler)
        var pipeline = handlerFunc;

        // Get pipeline behaviors
        var behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(typeof(TRequest));
        var behaviors = _serviceLocator.GetServices(behaviorType).Cast<IPipelineBehavior<TRequest>>().ToList();

        // Apply behaviors in reverse order (last behavior wraps the handler)
        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            pipeline = WrapWithBehavior(request, pipeline, behavior, cancellationToken);
        }

        return pipeline;
    }

    private Func<Task<Result<TResponse>>> WrapWithBehavior<TRequest, TResponse>(TRequest request, Func<Task<Result<TResponse>>> next, IPipelineBehavior<TRequest, TResponse> behavior, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        return async () =>
        {
            return await behavior.Handle(request, next, cancellationToken);
        };
    }

    private Func<Task<Result>> WrapWithBehavior<TRequest>(TRequest request, Func<Task<Result>> next, IPipelineBehavior<TRequest> behavior, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        return async () =>
        {
            return await behavior.Handle(request, next, cancellationToken);
        };
    }

}
