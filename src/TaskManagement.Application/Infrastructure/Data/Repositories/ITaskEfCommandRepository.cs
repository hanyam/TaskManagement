using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Infrastructure.Data.Repositories;

/// <summary>
///     Interface for Task EF Core command repository.
/// </summary>
public interface ITaskEfCommandRepository : ICommandRepository<DomainTask>
{
}