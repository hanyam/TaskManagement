using TaskManagement.Domain.Common;
using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Services;

/// <summary>
///     Service for determining available actions on a task based on state and user permissions (HATEOAS).
/// </summary>
public interface ITaskActionService
{
    /// <summary>
    ///     Gets the available actions for a task based on its current state, user ID, and user role.
    /// </summary>
    /// <param name="task">The task entity.</param>
    /// <param name="currentUserId">The ID of the current user.</param>
    /// <param name="currentUserRole">The role of the current user (Employee, Manager, Admin).</param>
    /// <returns>List of available action links.</returns>
    List<ApiActionLink> GetAvailableActions(DomainTask task, Guid currentUserId, string currentUserRole);
}