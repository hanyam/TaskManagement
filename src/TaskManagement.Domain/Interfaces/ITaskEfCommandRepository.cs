using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Interface for Task EF Core command repository.
/// </summary>
public interface ITaskEfCommandRepository : ICommandRepository<Task>
{
}





