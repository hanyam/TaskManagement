using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Mediator interface for handling requests (queries).
/// </summary>
public interface IRequestMediator
{
    /// <summary>
    /// Sends a request to its handler.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the request execution.</returns>
    Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request to its handler.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the request execution.</returns>
    Task<Result> Send(IRequest request, CancellationToken cancellationToken = default);
}
