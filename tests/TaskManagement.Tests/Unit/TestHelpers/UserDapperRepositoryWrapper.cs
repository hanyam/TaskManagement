using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
/// Wrapper that implements the same interface as UserDapperRepository but uses EF Core internally.
/// This allows the original CreateTaskCommandHandler to work with EF Core in tests.
/// </summary>
public class UserDapperRepositoryWrapper : UserDapperRepository
{
    private readonly UserEfQueryRepository _efRepository;

    public UserDapperRepositoryWrapper(UserEfQueryRepository efRepository) 
        : base(Microsoft.Extensions.Configuration.ConfigurationManager.CreateBuilder().Build()) // Dummy config
    {
        _efRepository = efRepository;
    }

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _efRepository.GetByIdAsync(id, cancellationToken);
    }

    public override async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _efRepository.GetAllAsync(cancellationToken);
    }

    public override async Task<IEnumerable<User>> FindAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        return await _efRepository.FindAsync(sql, param, cancellationToken);
    }

    public override async Task<User?> FirstOrDefaultAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        return await _efRepository.FirstOrDefaultAsync(sql, param, cancellationToken);
    }

    public override async Task<int> CountAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        return await _efRepository.CountAsync(sql, param, cancellationToken);
    }

    public override async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _efRepository.GetByEmailAsync(email, cancellationToken);
    }
}
