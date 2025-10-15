using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Common;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Infrastructure.Data.Repositories;

/// <summary>
///     Generic Entity Framework Core-based command repository implementation.
/// </summary>
/// <typeparam name="T">The type of entity for command operations.</typeparam>
public class EfCommandRepository<T> : ICommandRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfCommandRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask; // EF Core tracks changes, SaveChangesAsync will persist them
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask; // EF Core tracks changes, SaveChangesAsync will persist them
    }

    public virtual async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null) _dbSet.Remove(entity);
        await Task.CompletedTask; // EF Core tracks changes, SaveChangesAsync will persist them
    }
}