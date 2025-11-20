using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using DomainTask = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;
using TaskType = TaskManagement.Domain.Entities.TaskType;
using static TaskManagement.Domain.Constants.RoleNames;

namespace TaskManagement.Application.Tasks.Services;

/// <summary>
///     Implementation of ITaskActionService for determining available HATEOAS actions.
/// </summary>
public class TaskActionService : ITaskActionService
{
    public List<ApiActionLink> GetAvailableActions(DomainTask task, Guid currentUserId, string currentUserRole)
    {
        var links = new List<ApiActionLink>();
        var isManager = currentUserRole == Manager || currentUserRole == Admin;
        var isAssignedUser = task.AssignedUserId.HasValue && task.AssignedUserId.Value == currentUserId;
        var isCreator = task.CreatedById == currentUserId;

        // Self link - always present
        links.Add(new ApiActionLink
        {
            Rel = "self",
            Href = $"/tasks/{task.Id}",
            Method = "GET"
        });

        switch (task.Status)
        {
            case TaskStatus.Created:
                // Managers/Admins can assign tasks
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "assign",
                        Href = $"/tasks/{task.Id}/assign",
                        Method = "POST"
                    });

                // Assigned user can accept or reject the task
                if (isAssignedUser)
                {
                    links.Add(new ApiActionLink
                    {
                        Rel = "accept",
                        Href = $"/tasks/{task.Id}/accept",
                        Method = "POST"
                    });

                    links.Add(new ApiActionLink
                    {
                        Rel = "reject",
                        Href = $"/tasks/{task.Id}/reject",
                        Method = "POST"
                    });
                }

                // Creator, Manager, or Admin can update
                if (isCreator || isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "update",
                        Href = $"/tasks/{task.Id}",
                        Method = "PUT"
                    });

                // Creator, Manager, or Admin can cancel
                if (isCreator || isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "cancel",
                        Href = $"/tasks/{task.Id}/cancel",
                        Method = "POST"
                    });
                break;

            case TaskStatus.Assigned:
                // Assigned user can update progress (if task type supports it)
                if (isAssignedUser &&
                    (task.Type == TaskType.WithProgress || task.Type == TaskType.WithAcceptedProgress))
                    links.Add(new ApiActionLink
                    {
                        Rel = "update-progress",
                        Href = $"/tasks/{task.Id}/update-progress",
                        Method = "POST"
                    });

                // Assigned user can mark as completed
                if (isAssignedUser)
                    links.Add(new ApiActionLink
                    {
                        Rel = "mark-completed",
                        Href = $"/tasks/{task.Id}/mark-completed",
                        Method = "POST"
                    });

                // Managers/Admins can update
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "update",
                        Href = $"/tasks/{task.Id}",
                        Method = "PUT"
                    });

                // Managers/Admins can cancel
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "cancel",
                        Href = $"/tasks/{task.Id}/cancel",
                        Method = "POST"
                    });
                break;

            case TaskStatus.UnderReview:
                // Managers/Admins can accept
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "accept",
                        Href = $"/tasks/{task.Id}/accept",
                        Method = "POST"
                    });

                // Managers/Admins can reject
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "reject",
                        Href = $"/tasks/{task.Id}/reject",
                        Method = "POST"
                    });

                // Managers/Admins can cancel
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "cancel",
                        Href = $"/tasks/{task.Id}/cancel",
                        Method = "POST"
                    });
                break;

            case TaskStatus.Accepted:
                // Assigned user can update progress
                if (isAssignedUser &&
                    (task.Type == TaskType.WithProgress || task.Type == TaskType.WithAcceptedProgress))
                    links.Add(new ApiActionLink
                    {
                        Rel = "update-progress",
                        Href = $"/tasks/{task.Id}/update-progress",
                        Method = "POST"
                    });

                // Assigned user can mark as completed
                if (isAssignedUser)
                    links.Add(new ApiActionLink
                    {
                        Rel = "mark-completed",
                        Href = $"/tasks/{task.Id}/mark-completed",
                        Method = "POST"
                    });

                // Managers/Admins can update
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "update",
                        Href = $"/tasks/{task.Id}",
                        Method = "PUT"
                    });

                // Managers/Admins can cancel
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "cancel",
                        Href = $"/tasks/{task.Id}/cancel",
                        Method = "POST"
                    });
                break;

            case TaskStatus.Rejected:
                // Managers/Admins can reassign
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "reassign",
                        Href = $"/tasks/{task.Id}/reassign",
                        Method = "POST"
                    });

                // Managers/Admins can update
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "update",
                        Href = $"/tasks/{task.Id}",
                        Method = "PUT"
                    });

                // Managers/Admins can cancel
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "cancel",
                        Href = $"/tasks/{task.Id}/cancel",
                        Method = "POST"
                    });
                break;

            case TaskStatus.PendingManagerReview:
                // Managers/Admins can review completed task
                if (isManager)
                    links.Add(new ApiActionLink
                    {
                        Rel = "review-completed",
                        Href = $"/tasks/{task.Id}/review-completed",
                        Method = "POST"
                    });
                break;

            case TaskStatus.Completed:
            case TaskStatus.RejectedByManager:
            case TaskStatus.Cancelled:
                // Terminal states - only self link
                break;
        }

        return links;
    }
}