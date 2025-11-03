using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.ReassignTask;

/// <summary>
///     Handler for reassigning a task to different user(s) (manager).
/// </summary>
public class ReassignTaskCommandHandler : ICommandHandler<ReassignTaskCommand, TaskDto>
{
    private readonly ApplicationDbContext _context;
    private readonly TaskEfCommandRepository _taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository;

    public ReassignTaskCommandHandler(
        TaskEfCommandRepository taskCommandRepository,
        UserDapperRepository userQueryRepository,
        ApplicationDbContext context)
    {
        _taskCommandRepository = taskCommandRepository;
        _userQueryRepository = userQueryRepository;
        _context = context;
    }

    public async Task<Result<TaskDto>> Handle(ReassignTaskCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate task exists
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result<TaskDto>.Failure(errors);
        }

        // Validate user IDs
        if (!request.NewUserIds.Any())
        {
            errors.Add(Error.Validation("At least one user must be assigned", "NewUserIds"));
            return Result<TaskDto>.Failure(errors);
        }

        // Validate that all users exist
        foreach (var userId in request.NewUserIds)
        {
            var user = await _userQueryRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                errors.Add(TaskErrors.AssignedUserNotFound);
                return Result<TaskDto>.Failure(errors);
            }

            if (!user.IsActive)
            {
                errors.Add(TaskErrors.AssignedUserInactive);
                return Result<TaskDto>.Failure(errors);
            }
        }

        // Clear existing assignments
        var existingAssignments = await _context.Set<TaskAssignment>()
            .Where(ta => ta.TaskId == request.TaskId)
            .ToListAsync(cancellationToken);

        foreach (var assignment in existingAssignments)
        {
            _context.Set<TaskAssignment>().Remove(assignment);
        }

        // Create new assignments
        var primaryAssigned = true;
        foreach (var userId in request.NewUserIds)
        {
            var assignment = new TaskAssignment(request.TaskId, userId, primaryAssigned);
            assignment.SetCreatedBy(request.ReassignedById.ToString());
            task.Assignments.Add(assignment);
            await _context.Set<TaskAssignment>().AddAsync(assignment, cancellationToken);

            if (primaryAssigned)
            {
                task.AssignToUser(userId);
                primaryAssigned = false;
            }
        }

        // Reset task status to assigned
        task.Assign();
        task.SetUpdatedBy(request.ReassignedById.ToString());

        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Get assigned users for DTO
        var assignedUser = await _userQueryRepository.GetByIdAsync(task.AssignedUserId, cancellationToken);

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

