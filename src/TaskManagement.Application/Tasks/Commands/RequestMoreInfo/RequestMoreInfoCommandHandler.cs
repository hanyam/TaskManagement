using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.RequestMoreInfo;

/// <summary>
///     Handler for requesting more information on a task (employee).
/// </summary>
public class RequestMoreInfoCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    UserDapperRepository userQueryRepository,
    TaskManagementDbContext context,
    ITaskHistoryService taskHistoryService) : ICommandHandler<RequestMoreInfoCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly ITaskHistoryService _taskHistoryService = taskHistoryService;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;

    public async Task<Result<TaskDto>> Handle(RequestMoreInfoCommand request, CancellationToken cancellationToken)
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

        var isAssigned = (task.AssignedUserId.HasValue && task.AssignedUserId.Value == request.RequestedById) ||
                         assignments.Any(a => a.UserId == request.RequestedById);

        if (!isAssigned)
            errors.Add(Error.Forbidden("User is not assigned to this task", "Errors.Tasks.UserNotAssigned"));

        // Store previous status for history
        var previousStatus = task.Status;

        // Set task under review (may throw exceptions)
        try
        {
            task.SetUnderReview();
            task.SetUpdatedBy(request.RequestedById.ToString());
        }
        catch (Exception ex)
        {
            errors.Add(Error.Validation(ex.Message, "Status"));
        }

        // Check all errors once before database operations
        if (errors.Any()) return Result<TaskDto>.Failure(errors);

        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Record history: Request more information with the employee's message
        await _taskHistoryService.RecordStatusChangeAsync(
            task.Id,
            previousStatus,
            task.Status,
            "Requested More Information",
            request.RequestedById,
            request.RequestMessage, // Store the employee's request message in notes
            cancellationToken);

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