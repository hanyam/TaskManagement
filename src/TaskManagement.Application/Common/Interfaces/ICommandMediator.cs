using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Mediator interface for handling commands.
/// </summary>
public interface ICommandMediator
{
    /// <summary>
    /// Sends a command to its handler.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the command execution.</returns>
    Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command to its handler.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the command execution.</returns>
    Task<Result> Send(ICommand command, CancellationToken cancellationToken = default);
}
