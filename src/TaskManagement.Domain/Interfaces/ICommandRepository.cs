using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Generic interface for Entity Framework Core-based command repositories.
/// </summary>
/// <typeparam name="T">The type of entity for command operations.</typeparam>
public interface ICommandRepository<T> where T : BaseEntity
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}



