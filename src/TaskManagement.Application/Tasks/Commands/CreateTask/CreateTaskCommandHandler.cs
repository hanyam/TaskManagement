using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Errors.Users;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.CreateTask;

/// <summary>
///     Handler for creating a new task using EF Core for commands and Dapper for queries.
/// </summary>
public class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context;
    private readonly TaskEfCommandRepository _taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository;

    public CreateTaskCommandHandler(
        TaskEfCommandRepository taskCommandRepository,
        UserDapperRepository userQueryRepository,
        TaskManagementDbContext context)
    {
        _taskCommandRepository = taskCommandRepository;
        _userQueryRepository = userQueryRepository;
        _context = context;
    }

    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate that the assigned user exists only if AssignedUserId is provided (not a draft)
        User? assignedUser = null;
        User? creator = null;
        if (request.AssignedUserId.HasValue && request.AssignedUserId.Value != Guid.Empty)
        {
            assignedUser = await _userQueryRepository.GetByIdAsync(request.AssignedUserId.Value, cancellationToken);
            if (assignedUser == null)
            {
                errors.Add(TaskErrors.AssignedUserNotFound);
            }
            else
            {
                // Get creator to check role and manager relationship
                creator = await _userQueryRepository.GetByIdAsync(request.CreatedById, cancellationToken);
                if (creator == null)
                {
                    errors.Add(TaskErrors.CreatedByNotFound);
                }
                else if (creator.Role != UserRole.Admin)
                {
                    // Only Admin can bypass manager check. Managers must be manager of the assignee.
                    if (creator.Role == UserRole.Manager)
                    {
                        var isManager = await _userQueryRepository.IsManagerOfEmployeeAsync(
                            request.CreatedById,
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

        // Additional validations can be added here
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add(TaskErrors.TitleRequired);
        }

        if (request.DueDate < DateTime.UtcNow)
        {
            errors.Add(TaskErrors.DueDateInPast);
        }

        // If there are any validation errors, return them all
        if (errors.Any())
        {
            return Result<TaskDto>.Failure(errors);
        }

        // Create the task (can be draft if AssignedUserId is null)
        var task = new Task(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.AssignedUserId, // Null for unassigned (draft) tasks
            request.Type,
            request.CreatedById);

        task.SetCreatedBy(request.CreatedBy);

        // Add to repository using EF Core (for change tracking and complex operations)
        await _taskCommandRepository.AddAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Return the created task as DTO
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