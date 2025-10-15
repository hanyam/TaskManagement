namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Marker interface for requests that return a response.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
///     Marker interface for requests that don't return a response.
/// </summary>
public interface IRequest
{
}