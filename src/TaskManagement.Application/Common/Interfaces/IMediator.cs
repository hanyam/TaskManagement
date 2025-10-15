using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Custom mediator interface for handling requests and responses.
/// </summary>
public interface IMediator
{
    /// <summary>
    ///     Sends a request and returns a result.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response result.</returns>
    Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends a request without expecting a response.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> Send(IRequest request, CancellationToken cancellationToken = default);
}