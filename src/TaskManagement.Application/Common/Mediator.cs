using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common;

/// <summary>
///     Custom mediator implementation that handles requests with pipeline behaviors.
/// </summary>
public class Mediator : IMediator
{
    private readonly ILogger<Mediator> _logger;
    private readonly IServiceLocator _serviceLocator;

    public Mediator(IServiceLocator serviceLocator, ILogger<Mediator> logger)
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

            // Use reflection to call the Handle method
            var handleMethod = handlerType.GetMethod("Handle");
            var task = (Task<Result<TResponse>>)handleMethod!.Invoke(handler, new object[] { request, cancellationToken })!;
            var result = await task;

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

            // Use reflection to call the Handle method
            var handleMethod = handlerType.GetMethod("Handle");
            var task = (Task<Result>)handleMethod!.Invoke(handler, new object[] { request, cancellationToken })!;
            var result = await task;

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
}