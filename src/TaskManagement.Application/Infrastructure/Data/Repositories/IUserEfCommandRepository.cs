using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Infrastructure.Data.Repositories;

/// <summary>
/// Interface for User EF Core command repository.
/// </summary>
public interface IUserEfCommandRepository : ICommandRepository<User>
{
}
