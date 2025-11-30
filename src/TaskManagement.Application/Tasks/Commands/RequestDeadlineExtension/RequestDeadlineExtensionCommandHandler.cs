using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.RequestDeadlineExtension;

/// <summary>
///     Handler for requesting a deadline extension (employee).
/// </summary>
public class RequestDeadlineExtensionCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    UserDapperRepository userQueryRepository,
    TaskManagementDbContext context) : ICommandHandler<RequestDeadlineExtensionCommand, ExtensionRequestDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;

    public async Task<Result<ExtensionRequestDto>> Handle(RequestDeadlineExtensionCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate task exists
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result<ExtensionRequestDto>.Failure(errors);
        }

        // Validate user is assigned to the task
        var assignments = await _context.Set<TaskAssignment>()
            .Where(ta => ta.TaskId == request.TaskId)
            .ToListAsync(cancellationToken);

        var isAssigned = (task.AssignedUserId.HasValue && task.AssignedUserId.Value == request.RequestedById) ||
                         assignments.Any(a => a.UserId == request.RequestedById);

        if (!isAssigned)
        {
            errors.Add(Error.Forbidden("User is not assigned to this task", "Errors.Tasks.UserNotAssigned"));
        }

        // Validate requested due date
        if (request.RequestedDueDate <= DateTime.UtcNow)
        {
            errors.Add(Error.Validation("Requested due date must be in the future", "RequestedDueDate", "Errors.Tasks.RequestedDueDateMustBeFuture"));
        }

        if (task.DueDate.HasValue && request.RequestedDueDate <= task.DueDate.Value)
        {
            errors.Add(Error.Validation("Requested due date must be after the current due date", "RequestedDueDate", "Errors.Tasks.RequestedDueDateMustBeAfterCurrent"));
        }

        // Check all errors once before database operations
        if (errors.Any())
        {
            return Result<ExtensionRequestDto>.Failure(errors);
        }

        // Create extension request
        var extensionRequest = new DeadlineExtensionRequest(
            request.TaskId,
            request.RequestedById,
            request.RequestedDueDate,
            request.Reason);

        extensionRequest.SetCreatedBy(request.RequestedById.ToString());
        await _context.Set<DeadlineExtensionRequest>().AddAsync(extensionRequest, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Get user information
        var requestedByUser = await _userQueryRepository.GetByIdAsync(request.RequestedById, cancellationToken);

        return new ExtensionRequestDto
        {
            Id = extensionRequest.Id,
            TaskId = extensionRequest.TaskId,
            TaskTitle = task!.Title,
            RequestedById = extensionRequest.RequestedById,
            RequestedByEmail = requestedByUser?.Email,
            RequestedDueDate = extensionRequest.RequestedDueDate,
            Reason = extensionRequest.Reason,
            Status = extensionRequest.Status,
            CreatedAt = extensionRequest.CreatedAt
        };
    }
}