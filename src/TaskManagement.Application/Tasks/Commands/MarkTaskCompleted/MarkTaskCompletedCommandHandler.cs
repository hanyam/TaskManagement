using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.MarkTaskCompleted;

/// <summary>
///     Handler for marking a task as completed (manager).
/// </summary>
public class MarkTaskCompletedCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    UserDapperRepository userQueryRepository,
    TaskManagementDbContext context) : ICommandHandler<MarkTaskCompletedCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;

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

        // Mark task as completed by employee (moves to PendingManagerReview)
        try
        {
            task.MarkCompletedByEmployee();
            task.SetUpdatedBy(request.CompletedById.ToString());
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
            CreatedBy = task.CreatedBy,
            ManagerRating = task.ManagerRating,
            ManagerFeedback = task.ManagerFeedback
        };
    }
}