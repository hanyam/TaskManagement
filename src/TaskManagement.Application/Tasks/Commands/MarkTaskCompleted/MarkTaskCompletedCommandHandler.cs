using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Commands.MarkTaskCompleted;

/// <summary>
///     Handler for marking a task as completed by employee.
/// </summary>
public class MarkTaskCompletedCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    UserDapperRepository userQueryRepository,
    TaskManagementDbContext context,
    ITaskHistoryService taskHistoryService) : ICommandHandler<MarkTaskCompletedCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;
    private readonly ITaskHistoryService _taskHistoryService = taskHistoryService;

    public async Task<Result<TaskDto>> Handle(MarkTaskCompletedCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate task exists
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result<TaskDto>.Failure(errors);
        }

        // Check if task is already accepted by manager (terminal state - no more actions allowed)
        var isAcceptedByManager = task.Status == TaskStatus.Accepted && task.ManagerRating.HasValue;
        if (isAcceptedByManager)
        {
            errors.Add(TaskErrors.TaskAlreadyAcceptedByManager);
        }
        
        // Check if task is rejected by manager (terminal state - no more actions allowed)
        if (task.Status == TaskStatus.RejectedByManager)
        {
            errors.Add(TaskErrors.TaskRejectedByManager);
        }
        
        // Mark task as completed by employee (moves to PendingManagerReview) - may throw exceptions
        var previousStatus = task.Status;
        try
        {
            task.MarkCompletedByEmployee();
            task.SetUpdatedBy(request.CompletedById.ToString());
            
            // Record history: Task marked as completed
            var notes = string.IsNullOrWhiteSpace(request.Comment) 
                ? "Task marked as completed" 
                : request.Comment;
            
            await _taskHistoryService.RecordStatusChangeAsync(
                task.Id,
                previousStatus,
                task.Status,
                "Marked as Completed",
                request.CompletedById,
                notes,
                cancellationToken);
        }
        catch (Exception ex)
        {
            errors.Add(Error.Validation(ex.Message, "Status"));
        }

        // Check all errors once before database operations
        if (errors.Any())
        {
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
            CreatedBy = task.CreatedBy,
            ManagerRating = task.ManagerRating,
            ManagerFeedback = task.ManagerFeedback
        };
    }
}