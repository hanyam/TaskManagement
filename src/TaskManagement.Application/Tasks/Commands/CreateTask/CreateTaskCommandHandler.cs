using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.CreateTask;

/// <summary>
///     Handler for creating a new task using EF Core for commands and Dapper for queries.
/// </summary>
public class CreateTaskCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    UserDapperRepository userQueryRepository,
    TaskManagementDbContext context,
    ILogger<CreateTaskCommandHandler> logger,
    IAuditLogService auditLogService,
    Domain.Interfaces.ITaskHistoryService taskHistoryService) : ICommandHandler<CreateTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;
    private readonly ILogger<CreateTaskCommandHandler> _logger = logger;
    private readonly IAuditLogService _auditLogService = auditLogService;
    private readonly Domain.Interfaces.ITaskHistoryService _taskHistoryService = taskHistoryService;

    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating task with title {Title} by user {CreatedById}. AssignedUserId: {AssignedUserId}, Type: {Type}",
            request.Title,
            request.CreatedById,
            request.AssignedUserId,
            request.Type);

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
            _logger.LogWarning(
                "Task creation failed validation. Errors: {ErrorCount}, CreatedById: {CreatedById}",
                errors.Count,
                request.CreatedById);
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
        
        // Record history: Task created
        await _taskHistoryService.RecordStatusChangeAsync(
            task.Id,
            Domain.Entities.TaskStatus.Created,
            Domain.Entities.TaskStatus.Created,
            "Created",
            request.CreatedById,
            request.AssignedUserId.HasValue ? $"Assigned to {assignedUser?.Email}" : "Draft task",
            cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully created task {TaskId} with title {Title} by user {CreatedById}",
            task.Id,
            task.Title,
            request.CreatedById);

        // Audit log
        _auditLogService.LogTaskCreated(
            task.Id,
            request.CreatedById.ToString(),
            request.CreatedBy);

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