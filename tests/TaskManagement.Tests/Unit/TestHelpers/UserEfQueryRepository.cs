using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using ManagerEmployee = TaskManagement.Domain.Entities.ManagerEmployee;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
///     EF-based user query repository for testing purposes.
/// </summary>
public class UserEfQueryRepository : IQueryRepository<User>
{
    private readonly TaskManagementDbContext _context;

    public UserEfQueryRepository(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> FindAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
    {
        // For testing purposes, we'll just return all users
        // In a real scenario, you'd parse the SQL and convert it to EF queries
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<User?> FirstOrDefaultAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
    {
        // For testing purposes, we'll just return the first user
        // In a real scenario, you'd parse the SQL and convert it to EF queries
        return await _context.Users.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        // For testing purposes, we'll just return the count of all users
        // In a real scenario, you'd parse the SQL and convert it to EF queries
        return await _context.Users.CountAsync(cancellationToken);
    }

    // Additional helper method for testing
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsManagerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await _context.Set<ManagerEmployee>()
            .CountAsync(me => me.ManagerId == userId, cancellationToken);
        return count > 0;
    }

    public async Task<IEnumerable<User>> SearchManagedUsersAsync(Guid managerId, string searchQuery,
        CancellationToken cancellationToken = default)
    {
        // For in-memory database, use case-insensitive comparison
        // Note: In-memory database doesn't support ToLower() in LINQ queries, so we filter in memory
        var allManagedUsers = await _context.Set<ManagerEmployee>()
            .Where(me => me.ManagerId == managerId)
            .Join(
                _context.Users.Where(u => u.IsActive),
                me => me.EmployeeId,
                u => u.Id,
                (me, u) => u)
            .ToListAsync(cancellationToken);

        var searchQueryLower = searchQuery.ToLowerInvariant();
        return allManagedUsers
            .Where(u =>
                u.DisplayName.ToLower().Contains(searchQueryLower) ||
                u.Email.ToLower().Contains(searchQueryLower))
            .OrderBy(u => u.DisplayName)
            .Take(10)
            .ToList();
    }
}