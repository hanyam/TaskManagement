using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.AcceptTask;

/// <summary>
///     Handler for accepting an assigned task (employee).
/// </summary>
public class AcceptTaskCommandHandler : ICommandHandler<AcceptTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context;
    private readonly TaskEfCommandRepository _taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository;

    public AcceptTaskCommandHandler(
        TaskEfCommandRepository taskCommandRepository,
        UserDapperRepository userQueryRepository,
        TaskManagementDbContext context)
    {
        _taskCommandRepository = taskCommandRepository;
        _userQueryRepository = userQueryRepository;
        _context = context;
    }

    public async Task<Result<TaskDto>> Handle(AcceptTaskCommand request, CancellationToken cancellationToken)
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

        var isAssigned = (task.AssignedUserId.HasValue && task.AssignedUserId.Value == request.AcceptedById) ||
                         assignments.Any(a => a.UserId == request.AcceptedById);

        if (!isAssigned)
        {
            errors.Add(Error.Forbidden("User is not assigned to this task"));
            return Result<TaskDto>.Failure(errors);
        }

        // Accept task
        try
        {
            task.Accept();
            task.SetUpdatedBy(request.AcceptedById.ToString());
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