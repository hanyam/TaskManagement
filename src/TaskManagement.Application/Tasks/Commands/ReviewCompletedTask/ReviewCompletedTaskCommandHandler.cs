using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Commands.ReviewCompletedTask;

/// <summary>
///     Handler for reviewing a completed task by a manager.
/// </summary>
public class ReviewCompletedTaskCommandHandler : ICommandHandler<ReviewCompletedTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context;

    public ReviewCompletedTaskCommandHandler(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TaskDto>> Handle(ReviewCompletedTaskCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Find the task
        var task = await _context.Set<Task>()
            .Include(t => t.AssignedUser)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            errors.Add(TaskErrors.NotFound);
            return Result<TaskDto>.Failure(errors);
        }

        // Verify task is in PendingManagerReview status
        if (task.Status != TaskStatus.PendingManagerReview)
        {
            errors.Add(Error.Validation("Task must be in PendingManagerReview status to be reviewed", "Status"));
            return Result<TaskDto>.Failure(errors);
        }

        // Review the task
        try
        {
            task.ReviewByManager(request.Accepted, request.Rating, request.Feedback, request.SendBackForRework);
            task.SetUpdatedBy("Manager"); // Set updated by current user

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            errors.Add(Error.Validation(ex.Message));
            return Result<TaskDto>.Failure(errors);
        }
        catch (ArgumentException ex)
        {
            errors.Add(Error.Validation(ex.Message));
            return Result<TaskDto>.Failure(errors);
        }

        // Map to DTO
        var taskDto = new TaskDto
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
            AssignedUserEmail = task.AssignedUser?.Email,
            Type = task.Type,
            ReminderLevel = task.ReminderLevel,
            ProgressPercentage = task.ProgressPercentage,
            CreatedById = task.CreatedById,
            CreatedBy = task.CreatedByUser?.Email ?? string.Empty,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            ManagerRating = task.ManagerRating,
            ManagerFeedback = task.ManagerFeedback
        };

        return Result<TaskDto>.Success(taskDto);
    }
}