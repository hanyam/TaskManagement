using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Infrastructure.Data.Repositories;

/// <summary>
///     Specialized EF Core command repository for Task entities.
/// </summary>
public class TaskEfCommandRepository(TaskManagementDbContext context) : EfCommandRepository<DomainTask>(context), ITaskEfCommandRepository
{
}