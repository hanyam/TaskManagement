using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Interface for User EF Core command repository.
/// </summary>
public interface IUserEfCommandRepository : ICommandRepository<User>
{
}


