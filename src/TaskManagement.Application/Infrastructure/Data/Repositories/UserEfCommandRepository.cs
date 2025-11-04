using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Infrastructure.Data.Repositories;

/// <summary>
///     Specialized EF Core command repository for User entities.
/// </summary>
public class UserEfCommandRepository : EfCommandRepository<User>, IUserEfCommandRepository
{
    public UserEfCommandRepository(TaskManagementDbContext context) : base(context)
    {
    }
}