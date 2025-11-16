using Dapper;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Infrastructure.Data.Repositories;

/// <summary>
///     Dapper-based repository for User read operations.
/// </summary>
public class UserDapperRepository : DapperQueryRepository<User>
{
    public UserDapperRepository(IConfiguration configuration) : base(configuration)
    {
    }

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
    public virtual async Task<bool> IsManagerOfEmployeeAsync(Guid managerId, Guid employeeId, CancellationToken cancellationToken = default)
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
}