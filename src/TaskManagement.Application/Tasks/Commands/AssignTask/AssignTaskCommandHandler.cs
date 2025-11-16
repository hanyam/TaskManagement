using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.AssignTask;

/// <summary>
///     Handler for assigning a task to one or multiple users (manager only).
/// </summary>
public class AssignTaskCommandHandler : ICommandHandler<AssignTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context;
    private readonly TaskEfCommandRepository _taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository;

    public AssignTaskCommandHandler(
        TaskEfCommandRepository taskCommandRepository,
        UserDapperRepository userQueryRepository,
        TaskManagementDbContext context)
    {
        _taskCommandRepository = taskCommandRepository;
        _userQueryRepository = userQueryRepository;
        _context = context;
    }

    public async Task<Result<TaskDto>> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate that the task exists
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result<TaskDto>.Failure(errors);
        }

        // Validate user IDs
        if (!request.UserIds.Any())
        {
            errors.Add(Error.Validation("At least one user must be assigned", "UserIds"));
            return Result<TaskDto>.Failure(errors);
        }

        // Get the assigner to check role and manager relationships
        var assigner = await _userQueryRepository.GetByIdAsync(request.AssignedById, cancellationToken);
        if (assigner == null)
        {
            errors.Add(TaskErrors.CreatedByNotFound);
            return Result<TaskDto>.Failure(errors);
        }

        // Validate that all users exist and check manager relationships
        foreach (var userId in request.UserIds)
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

            // Check manager-employee relationship (Admin can bypass)
            if (assigner.Role != UserRole.Admin)
            {
                if (assigner.Role == UserRole.Manager)
                {
                    var isManager = await _userQueryRepository.IsManagerOfEmployeeAsync(
                        request.AssignedById,
                        userId,
                        cancellationToken);
                    if (!isManager)
                    {
                        errors.Add(TaskErrors.AssignerMustBeManagerOfAssignee);
                        return Result<TaskDto>.Failure(errors);
                    }
                }
                else
                {
                    // Employees cannot assign tasks
                    errors.Add(TaskErrors.AssignerMustBeManagerOfAssignee);
                    return Result<TaskDto>.Failure(errors);
                }
            }
        }

        // Clear existing assignments for this task
        var existingAssignments = await _context.Set<TaskAssignment>()
            .Where(ta => ta.TaskId == request.TaskId)
            .ToListAsync(cancellationToken);
        
        foreach (var assignment in existingAssignments)
        {
            _context.Set<TaskAssignment>().Remove(assignment);
        }

        // Create new assignments
        var primaryAssigned = true;
        foreach (var userId in request.UserIds)
        {
            var assignment = new TaskAssignment(request.TaskId, userId, primaryAssigned);
            assignment.SetCreatedBy(request.AssignedById.ToString());
            await _context.Set<TaskAssignment>().AddAsync(assignment, cancellationToken);

            if (primaryAssigned)
            {
                task.AssignToUser(userId);
                primaryAssigned = false;
            }
        }

        // Update task status
        task.Assign();
        task.SetUpdatedBy(request.AssignedById.ToString());

        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Get assigned users for DTO
        var assignedUsers = new List<(Guid Id, string Email)>();
        foreach (var userId in request.UserIds)
        {
            var user = await _userQueryRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                assignedUsers.Add((user.Id, user.Email));
            }
        }

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
            AssignedUserEmail = assignedUsers.FirstOrDefault().Email,
            Type = task.Type,
            ReminderLevel = task.ReminderLevel,
            ProgressPercentage = task.ProgressPercentage,
            CreatedById = task.CreatedById,
            CreatedAt = task.CreatedAt,
            CreatedBy = task.CreatedBy
        };
    }
}

