using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Data.Repositories;

/// <summary>
///     Specialized EF Core command repository for User entities.
/// </summary>
public class UserEfCommandRepository(TaskManagementDbContext context)
    : EfCommandRepository<User>(context), IUserEfCommandRepository
{
}