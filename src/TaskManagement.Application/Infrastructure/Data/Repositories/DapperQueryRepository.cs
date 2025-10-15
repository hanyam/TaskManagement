using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Infrastructure.Data.Repositories;

/// <summary>
///     Generic Dapper-based query repository implementation.
/// </summary>
/// <typeparam name="T">The type of entity to query.</typeparam>
public class DapperQueryRepository<T> : IQueryRepository<T> where T : BaseEntity
{
    private readonly string _connectionString;

    public DapperQueryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {typeof(T).Name}s WHERE Id = @Id";
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, new { Id = id },
            cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {typeof(T).Name}s";
        using var connection = CreateConnection();
        return await connection.QueryAsync<T>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<T>> FindAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));
    }

    public async Task<T?> FirstOrDefaultAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, param,
            cancellationToken: cancellationToken));
    }

    public async Task<int> CountAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, param,
            cancellationToken: cancellationToken));
    }

    protected SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}