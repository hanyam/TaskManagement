using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
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
    private readonly ApplicationDbContext _context;
    private readonly TaskEfCommandRepository _taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository;

    public CreateTaskCommandHandler(
        TaskEfCommandRepository taskCommandRepository,
        UserDapperRepository userQueryRepository,
        ApplicationDbContext context)
    {
        _taskCommandRepository = taskCommandRepository;
        _userQueryRepository = userQueryRepository;
        _context = context;
    }

    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate that the assigned user exists using Dapper (fast query)
        var assignedUser = await _userQueryRepository.GetByIdAsync(request.AssignedUserId, cancellationToken);
        if (assignedUser == null)
        {
            errors.Add(TaskErrors.AssignedUserNotFound);
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

        // Create the task
        var task = new Task(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.AssignedUserId,
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
            AssignedUserEmail = assignedUser!.Email,
            Type = task.Type,
            ReminderLevel = task.ReminderLevel,
            ProgressPercentage = task.ProgressPercentage,
            CreatedById = task.CreatedById,
            CreatedAt = task.CreatedAt,
            CreatedBy = task.CreatedBy
        };
    }
}