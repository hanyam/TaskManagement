using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.UpdateTask;

/// <summary>
///     Handler for updating an existing task.
/// </summary>
public class UpdateTaskCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    UserDapperRepository userQueryRepository,
    TaskManagementDbContext context,
    ILogger<UpdateTaskCommandHandler> logger,
    IAuditLogService auditLogService) : ICommandHandler<UpdateTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;
    private readonly ILogger<UpdateTaskCommandHandler> _logger = logger;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task<Result<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating task {TaskId} by user {UpdatedById}. Title: {Title}, AssignedUserId: {AssignedUserId}",
            request.TaskId,
            request.UpdatedById,
            request.Title,
            request.AssignedUserId);

        var errors = new List<Error>();

        // Find the task
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for update", request.TaskId);
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result<TaskDto>.Failure(errors);
        }

        // Validate assigned user exists if provided
        User? assignedUser = null;
        User? updater = null;
        if (request.AssignedUserId.HasValue && request.AssignedUserId.Value != Guid.Empty)
        {
            assignedUser = await _userQueryRepository.GetByIdAsync(request.AssignedUserId.Value, cancellationToken);
            if (assignedUser == null)
            {
                errors.Add(TaskErrors.AssignedUserNotFound);
            }
            else
            {
                // Get updater to check role and manager relationship
                updater = await _userQueryRepository.GetByIdAsync(request.UpdatedById, cancellationToken);
                if (updater == null)
                {
                    errors.Add(TaskErrors.CreatedByNotFound); // Reuse error, or create new one
                }
                else if (updater.Role != UserRole.Admin)
                {
                    // Only Admin can bypass manager check. Managers must be manager of the assignee.
                    if (updater.Role == UserRole.Manager)
                    {
                        var isManager = await _userQueryRepository.IsManagerOfEmployeeAsync(
                            request.UpdatedById,
                            request.AssignedUserId.Value,
                            cancellationToken);
                        if (!isManager)
                        {
                            errors.Add(TaskErrors.AssignerMustBeManagerOfAssignee);
                        }
                    }
                    else
                    {
                        // Employees cannot assign tasks
                        errors.Add(TaskErrors.AssignerMustBeManagerOfAssignee);
                    }
                }
            }
        }

        // Additional validations
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add(TaskErrors.TitleRequired);
        }

        if (request.DueDate.HasValue && request.DueDate < DateTime.UtcNow)
        {
            errors.Add(TaskErrors.DueDateInPast);
        }

        // If there are any validation errors, return them all
        if (errors.Any())
        {
            _logger.LogWarning(
                "Task update failed validation. Errors: {ErrorCount}, TaskId: {TaskId}, UpdatedById: {UpdatedById}",
                errors.Count,
                request.TaskId,
                request.UpdatedById);
            return Result<TaskDto>.Failure(errors);
        }

        // Update task properties
        task.UpdateTitle(request.Title);
        task.UpdateDescription(request.Description);
        task.UpdatePriority(request.Priority);
        task.UpdateDueDate(request.DueDate);
        // Note: Type is read-only and cannot be changed after task creation

        // Update assignment if changed
        if (request.AssignedUserId.HasValue && request.AssignedUserId.Value != Guid.Empty)
        {
            if (task.AssignedUserId != request.AssignedUserId.Value)
            {
                task.AssignToUser(request.AssignedUserId.Value);
            }
        }
        // Note: Unassigning (setting to null) would require a new method in the entity
        // For now, we only allow reassignment, not unassignment

        task.SetUpdatedBy(request.UpdatedBy);

        // Update using EF Core
        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully updated task {TaskId} with title {Title} by user {UpdatedById}",
            task.Id,
            task.Title,
            request.UpdatedById);

        // Audit log
        _auditLogService.LogTaskUpdated(
            task.Id,
            request.UpdatedById.ToString(),
            request.UpdatedBy);

        // Return the updated task as DTO
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            OriginalDueDate = task.OriginalDueDate,
            ExtendedDueDate = task.ExtendedDueDate,
            AssignedUserId = task.AssignedUserId,
            AssignedUserEmail = assignedUser?.Email,
            Type = task.Type,
            ReminderLevel = task.ReminderLevel,
            ProgressPercentage = task.ProgressPercentage,
            CreatedById = task.CreatedById,
            CreatedAt = task.CreatedAt,
            CreatedBy = task.CreatedBy,
            UpdatedAt = task.UpdatedAt
        };
    }
}

