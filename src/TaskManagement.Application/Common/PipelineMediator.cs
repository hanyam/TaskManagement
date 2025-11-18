using FluentValidation;
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
            return Result<TResponse>.Failure(
                Error.Internal($"An error occurred while processing the request: {ex.Message}"));
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

    private Func<Task<Result<TResponse>>> BuildPipeline<TRequest, TResponse>(TRequest request, object handler,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        // Get the handle method
        var handleMethod = handler.GetType().GetMethod("Handle");
        if (handleMethod == null)
            throw new InvalidOperationException($"Handler {handler.GetType().Name} does not have a Handle method");

        // Create the base handler function
        Func<Task<Result<TResponse>>> handlerFunc = async () =>
        {
            var task = (Task<Result<TResponse>>)handleMethod.Invoke(handler,
                new object[] { request, cancellationToken })!;
            return await task;
        };

        // Build pipeline with built-in behaviors: Logging -> Validation -> Command Handler
        var pipeline = handlerFunc;

        // 1. Add Exception Handling (outermost)
        pipeline = WrapWithExceptionHandling(request, pipeline, cancellationToken);

        // 2. Add Validation (middle)
        pipeline = WrapWithValidation(request, pipeline, cancellationToken);

        // 3. Add Logging (innermost, closest to handler)
        pipeline = WrapWithLogging(request, pipeline, cancellationToken);

        return pipeline;
    }

    private Func<Task<Result>> BuildPipeline<TRequest>(TRequest request, object handler,
        CancellationToken cancellationToken)
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

        // Build pipeline with built-in behaviors: Logging -> Validation -> Command Handler
        var pipeline = handlerFunc;

        // 1. Add Exception Handling (outermost)
        pipeline = WrapWithExceptionHandling(request, pipeline, cancellationToken);

        // 2. Add Validation (middle)
        pipeline = WrapWithValidation(request, pipeline, cancellationToken);

        // 3. Add Logging (innermost, closest to handler)
        pipeline = WrapWithLogging(request, pipeline, cancellationToken);

        return pipeline;
    }

    private Func<Task<Result<TResponse>>> WrapWithLogging<TRequest, TResponse>(TRequest request,
        Func<Task<Result<TResponse>>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        return async () =>
        {
            _logger.LogInformation("Executing {RequestType} with {Request}", typeof(TRequest).Name, request);
            var result = await next();
            _logger.LogInformation("Executed {RequestType} with result: {IsSuccess}", typeof(TRequest).Name,
                result.IsSuccess);
            return result;
        };
    }

    private Func<Task<Result>> WrapWithLogging<TRequest>(TRequest request, Func<Task<Result>> next,
        CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        return async () =>
        {
            _logger.LogInformation("Executing {RequestType} with {Request}", typeof(TRequest).Name, request);
            var result = await next();
            _logger.LogInformation("Executed {RequestType} with result: {IsSuccess}", typeof(TRequest).Name,
                result.IsSuccess);
            return result;
        };
    }

    private Func<Task<Result<TResponse>>> WrapWithValidation<TRequest, TResponse>(TRequest request,
        Func<Task<Result<TResponse>>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        return async () =>
        {
            // Get validators for this request type
            var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TRequest));
            var validators = _serviceLocator.GetServices(validatorType).Cast<IValidator<TRequest>>().ToList();

            if (validators.Any())
            {
                _logger.LogDebug("Validating request of type {RequestType}", typeof(TRequest).Name);

                var context = new ValidationContext<TRequest>(request);
                var validationResults =
                    await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Any())
                {
                    var errors = failures.Select(f => f.ErrorMessage).ToList();
                    _logger.LogWarning("Validation failed for request type {RequestType}. Errors: {Errors}",
                        typeof(TRequest).Name, string.Join(", ", errors));

                    var errorList = errors.Select(e => Error.Validation(e)).ToList();
                    return Result<TResponse>.Failure(errorList);
                }

                _logger.LogDebug("Validation passed for request type {RequestType}", typeof(TRequest).Name);
            }

            return await next();
        };
    }

    private Func<Task<Result>> WrapWithValidation<TRequest>(TRequest request, Func<Task<Result>> next,
        CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        return async () =>
        {
            // Get validators for this request type
            var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TRequest));
            var validators = _serviceLocator.GetServices(validatorType).Cast<IValidator<TRequest>>().ToList();

            if (validators.Any())
            {
                _logger.LogDebug("Validating request of type {RequestType}", typeof(TRequest).Name);

                var context = new ValidationContext<TRequest>(request);
                var validationResults =
                    await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Any())
                {
                    var errors = failures.Select(f => f.ErrorMessage).ToList();
                    _logger.LogWarning("Validation failed for request type {RequestType}. Errors: {Errors}",
                        typeof(TRequest).Name, string.Join(", ", errors));

                    var errorList = errors.Select(e => Error.Validation(e)).ToList();
                    return Result.Failure(errorList);
                }

                _logger.LogDebug("Validation passed for request type {RequestType}", typeof(TRequest).Name);
            }

            return await next();
        };
    }

    private Func<Task<Result<TResponse>>> WrapWithExceptionHandling<TRequest, TResponse>(TRequest request,
        Func<Task<Result<TResponse>>> next, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        return async () =>
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing {RequestType}", typeof(TRequest).Name);
                return Result<TResponse>.Failure(
                    Error.Internal($"An error occurred while processing the request: {ex.Message}"));
            }
        };
    }

    private Func<Task<Result>> WrapWithExceptionHandling<TRequest>(TRequest request, Func<Task<Result>> next,
        CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        return async () =>
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing {RequestType}", typeof(TRequest).Name);
                return Result.Failure(Error.Internal($"An error occurred while processing the request: {ex.Message}"));
            }
        };
    }
}