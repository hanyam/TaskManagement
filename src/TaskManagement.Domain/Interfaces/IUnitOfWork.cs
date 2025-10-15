using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Unit of Work pattern interface for managing transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Task> Tasks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}