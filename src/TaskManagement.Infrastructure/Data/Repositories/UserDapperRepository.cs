using Dapper;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.Repositories;

/// <summary>
///     Dapper-based repository for User read operations.
/// </summary>
public class UserDapperRepository(IConfiguration configuration) : DapperQueryRepository<User>(configuration)
{

    /// <summary>
    ///     Gets a user by email.
    /// </summary>
    public virtual async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT * FROM [Tasks].[Users] WHERE Email = @Email";
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { Email = email },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    ///     Checks if a manager-employee relationship exists.
    /// </summary>
    public virtual async Task<bool> IsManagerOfEmployeeAsync(Guid managerId, Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT COUNT(1) 
            FROM [Tasks].[ManagerEmployees] 
            WHERE ManagerId = @ManagerId AND EmployeeId = @EmployeeId";
        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql,
            new { ManagerId = managerId, EmployeeId = employeeId },
            cancellationToken: cancellationToken));
        return count > 0;
    }

    /// <summary>
    ///     Checks if a user is a manager (exists in ManagerEmployee table as ManagerId).
    /// </summary>
    public virtual async Task<bool> IsManagerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT COUNT(1) 
            FROM [Tasks].[ManagerEmployees] 
            WHERE ManagerId = @UserId";
        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql,
            new { UserId = userId },
            cancellationToken: cancellationToken));
        return count > 0;
    }

    /// <summary>
    ///     Searches for users managed by a specific manager (manager-employee relationship).
    /// </summary>
    public virtual async Task<IEnumerable<User>> SearchManagedUsersAsync(Guid managerId, string searchQuery,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT TOP 10
                U.Id, U.Email, U.FirstName, U.LastName, U.DisplayName, 
                U.AzureAdObjectId, U.IsActive, U.LastLoginAt, U.Role,
                U.CreatedAt, U.UpdatedAt, U.CreatedBy, U.UpdatedBy
            FROM [Tasks].[ManagerEmployees] AS ME
            INNER JOIN [Tasks].[Users] AS U ON ME.EmployeeId = U.Id
            WHERE ME.ManagerId = @ManagerId
                AND U.IsActive = 1
                AND (U.DisplayName LIKE @SearchQuery OR U.Email LIKE @SearchQuery)
            ORDER BY U.DisplayName";

        var searchPattern = $"%{searchQuery.ToLower()}%";
        using var connection = CreateConnection();
        return await connection.QueryAsync<User>(new CommandDefinition(sql,
            new { ManagerId = managerId, SearchQuery = searchPattern },
            cancellationToken: cancellationToken));
    }
}

