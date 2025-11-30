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
public class ReviewCompletedTaskCommandHandler(
    TaskManagementDbContext context,
    Domain.Interfaces.ITaskHistoryService taskHistoryService,
    ICurrentUserService currentUserService) : ICommandHandler<ReviewCompletedTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly Domain.Interfaces.ITaskHistoryService _taskHistoryService = taskHistoryService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

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
        }

        // Review the task (may throw exceptions)
        var previousStatus = task.Status;
        var performedById = _currentUserService.GetUserId() ?? Guid.Empty;
        
        try
        {
            task.ReviewByManager(request.Accepted, request.Rating, request.Feedback, request.SendBackForRework);
            task.SetUpdatedBy("Manager"); // Set updated by current user

            // Record history based on review decision
            string action;
            string? notes = null;
            
            if (request.SendBackForRework)
            {
                action = "Sent Back for Rework";
                notes = request.Feedback;
            }
            else if (request.Accepted)
            {
                action = "Reviewed and Accepted";
                notes = request.Rating > 0 
                    ? $"Rating: {request.Rating}/5" + (request.Feedback != null ? $", Feedback: {request.Feedback}" : "")
                    : request.Feedback;
            }
            else
            {
                action = "Reviewed and Returned";
                notes = request.Rating > 0 
                    ? $"Rating: {request.Rating}/5" + (request.Feedback != null ? $", Feedback: {request.Feedback}" : "")
                    : request.Feedback;
            }

            await _taskHistoryService.RecordStatusChangeAsync(
                task.Id,
                previousStatus,
                task.Status,
                action,
                performedById,
                notes,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            errors.Add(Error.Validation(ex.Message));
        }
        catch (ArgumentException ex)
        {
            errors.Add(Error.Validation(ex.Message));
        }

        // Check all errors once before database operations
        if (errors.Any())
        {
            return Result<TaskDto>.Failure(errors);
        }

        // All validations passed - proceed with database operations
        await _context.SaveChangesAsync(cancellationToken);

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