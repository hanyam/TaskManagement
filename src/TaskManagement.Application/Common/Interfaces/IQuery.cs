namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Base interface for queries in the CQRS pattern.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the query.</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>
{
}