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
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT * FROM Users WHERE Email = @Email";
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { Email = email },
            cancellationToken: cancellationToken));
    }
}