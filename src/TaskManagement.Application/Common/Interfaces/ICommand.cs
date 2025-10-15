namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Base interface for commands in the CQRS pattern.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>
{
}

/// <summary>
///     Base interface for commands that don't return a value.
/// </summary>
public interface ICommand : IRequest
{
}