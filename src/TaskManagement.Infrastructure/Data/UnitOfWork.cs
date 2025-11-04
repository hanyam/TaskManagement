using Microsoft.EntityFrameworkCore.Storage;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Infrastructure.Data;

/// <summary>
///     Unit of Work implementation using Entity Framework.
///     Provides explicit transaction management for complex operations.
///     
///     Note: The DbContext is managed by the DI container (scoped lifetime) and will be disposed automatically.
///     This UnitOfWork should NOT dispose the DbContext - it only manages transactions.
///     
///     Usage Guidelines:
///     - For simple operations: Use DbContext directly with SaveChangesAsync() (implicit transaction)
///     - For complex operations: Use UnitOfWork with explicit transaction control (BeginTransaction/Commit/Rollback)
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TaskManagementDbContext _context;
    private IRepository<Task>? _tasks;
    private IDbContextTransaction? _transaction;
    private IRepository<User>? _users;

    public UnitOfWork(TaskManagementDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Gets the Users repository.
    /// </summary>
    public IRepository<User> Users => _users ??= new Repository<User>(_context);

    /// <summary>
    ///     Gets the Tasks repository.
    /// </summary>
    public IRepository<Task> Tasks => _tasks ??= new Repository<Task>(_context);

    /// <summary>
    ///     Saves all changes made in this unit of work to the database.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Begins a new database transaction.
    ///     Use this for complex operations that need explicit transaction control.
    /// </summary>
    public async System.Threading.Tasks.Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress. Commit or rollback the current transaction before starting a new one.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    ///     Commits the current transaction.
    /// </summary>
    public async System.Threading.Tasks.Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress. Call BeginTransactionAsync first.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    ///     Rolls back the current transaction.
    /// </summary>
    public async System.Threading.Tasks.Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return; // No transaction to rollback - this is safe to call if no transaction was started
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    ///     Disposes the UnitOfWork. Only disposes the transaction if one exists.
    ///     The DbContext is managed by DI and will be disposed automatically.
    /// </summary>
    public void Dispose()
    {
        // Only dispose the transaction, NOT the DbContext
        // The DbContext is scoped by DI and will be disposed automatically when the scope ends
        if (_transaction != null)
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }
}