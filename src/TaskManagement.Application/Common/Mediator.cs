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
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing request of type {RequestType}", request.GetType().Name);

            var handler = _serviceProvider.GetRequiredService<IRequestHandler<IRequest<TResponse>, TResponse>>();

            // For now, just call the handler directly without pipeline behaviors
            // TODO: Implement pipeline behaviors later
            var result = await handler.Handle((IRequest<TResponse>)request, cancellationToken);

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

            var handler = _serviceProvider.GetRequiredService<IRequestHandler<IRequest>>();

            // For now, just call the handler directly without pipeline behaviors
            // TODO: Implement pipeline behaviors later
            var result = await handler.Handle(request, cancellationToken);

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