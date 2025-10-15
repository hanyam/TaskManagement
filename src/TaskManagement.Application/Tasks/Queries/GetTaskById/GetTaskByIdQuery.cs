using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Queries.GetTaskById;

/// <summary>
///     Query for getting a task by its ID.
/// </summary>
public record GetTaskByIdQuery : IQuery<TaskDto>
{
    public Guid Id { get; init; }
}