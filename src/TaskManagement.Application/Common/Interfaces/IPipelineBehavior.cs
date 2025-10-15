using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Interface for pipeline behaviors that can be executed before and after request handling.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///     Handles the request in the pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response result.</returns>
    Task<Result<TResponse>> Handle(TRequest request, Func<Task<Result<TResponse>>> next,
        CancellationToken cancellationToken);
}

/// <summary>
///     Interface for pipeline behaviors that can be executed before and after request handling (no response).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public interface IPipelineBehavior<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    ///     Handles the request in the pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> Handle(TRequest request, Func<Task<Result>> next, CancellationToken cancellationToken);
}