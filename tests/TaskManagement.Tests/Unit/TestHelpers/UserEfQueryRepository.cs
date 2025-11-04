using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Common;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Application.Infrastructure.Data.Repositories;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
/// EF-based user query repository for testing purposes.
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

    public async Task<IEnumerable<User>> FindAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        // For testing purposes, we'll just return all users
        // In a real scenario, you'd parse the SQL and convert it to EF queries
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<User?> FirstOrDefaultAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
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
}
