using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Commands.RejectTask;

/// <summary>
///     Handler for rejecting an assigned task (employee).
/// </summary>
public class RejectTaskCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    UserDapperRepository userQueryRepository,
    TaskManagementDbContext context) : ICommandHandler<RejectTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;

    public async Task<Result<TaskDto>> Handle(RejectTaskCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate task exists
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result<TaskDto>.Failure(errors);
        }

        // Validate user is assigned to the task
        var assignments = await _context.Set<TaskAssignment>()
            .Where(ta => ta.TaskId == request.TaskId)
            .ToListAsync(cancellationToken);

        var isAssigned = (task.AssignedUserId.HasValue && task.AssignedUserId.Value == request.RejectedById) ||
                         assignments.Any(a => a.UserId == request.RejectedById);


        // Check if already rejected
        if (task.Status == TaskStatus.Rejected)
        {
            errors.Add(TaskErrors.TaskAlreadyCompleted);
            return Result<TaskDto>.Failure(errors);
        }

        if (!isAssigned)
        {
            errors.Add(Error.Forbidden("User is not assigned to this task"));
            return Result<TaskDto>.Failure(errors);
        }

        // Reject task
        try
        {
            task.Reject();
            task.SetUpdatedBy(request.RejectedById.ToString());
        }
        catch (Exception ex)
        {
            errors.Add(Error.Validation(ex.Message, "Status"));
            return Result<TaskDto>.Failure(errors);
        }

        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Get assigned user for DTO
        User? assignedUser = null;
        if (task.AssignedUserId.HasValue)
            assignedUser = await _userQueryRepository.GetByIdAsync(task.AssignedUserId.Value, cancellationToken);

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
            CreatedBy = task.CreatedBy
        };
    }
}