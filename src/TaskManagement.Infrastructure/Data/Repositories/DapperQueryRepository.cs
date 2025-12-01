using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Data.Repositories;

/// <summary>
///     Generic Dapper-based query repository implementation.
/// </summary>
/// <typeparam name="T">The type of entity to query.</typeparam>
public class DapperQueryRepository<T>(IConfiguration configuration) : IQueryRepository<T> where T : BaseEntity
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
                                                ?? throw new InvalidOperationException(
                                                    "DefaultConnection is not configured.");

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM [Tasks].[{typeof(T).Name}s] WHERE Id = @Id";
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, new { Id = id },
            cancellationToken: cancellationToken));
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM [Tasks].[{typeof(T).Name}s]";
        using var connection = CreateConnection();
        return await connection.QueryAsync<T>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public virtual async Task<IEnumerable<T>> FindAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        return await connection.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: cancellationToken));
    }

    public virtual async Task<T?> FirstOrDefaultAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(sql, param,
            cancellationToken: cancellationToken));
    }

    public virtual async Task<int> CountAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
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