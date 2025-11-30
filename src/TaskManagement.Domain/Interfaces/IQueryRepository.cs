using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Generic interface for Dapper-based query repositories.
/// </summary>
/// <typeparam name="T">The type of entity to query.</typeparam>
public interface IQueryRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(string sql, object? param = null, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(string sql, object? param = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string sql, object? param = null, CancellationToken cancellationToken = default);
}









