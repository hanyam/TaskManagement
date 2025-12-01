using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;
using TaskHistory = TaskManagement.Domain.Entities.TaskHistory;

namespace TaskManagement.Application.Tasks.Queries.GetTaskHistory;

/// <summary>
///     Handler for getting task history.
/// </summary>
public class GetTaskHistoryQueryHandler(
    TaskManagementDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<GetTaskHistoryQuery, List<TaskHistoryDto>>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<List<TaskHistoryDto>>> Handle(GetTaskHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue) return Result<List<TaskHistoryDto>>.Failure(Error.Forbidden("User not authenticated"));

        // Verify task exists and user has access
        var task = await _context.Set<Task>()
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null) return Result<List<TaskHistoryDto>>.Failure(TaskErrors.NotFound);

        // Check if user has access (creator, assignee, or admin/manager)
        var userRoleClaim = _currentUserService.GetUserPrincipal()?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userRole = Enum.TryParse<UserRole>(userRoleClaim, true, out var role) ? role : UserRole.Employee;
        var hasAccess = task.CreatedById == userId.Value ||
                        (task.AssignedUserId.HasValue && task.AssignedUserId.Value == userId.Value) ||
                        userRole == UserRole.Admin ||
                        userRole == UserRole.Manager;

        if (!hasAccess)
            return Result<List<TaskHistoryDto>>.Failure(Error.Forbidden("User does not have access to this task"));

        // Get history with user information
        var history = await _context.Set<TaskHistory>()
            .Include(h => h.PerformedByUser)
            .Where(h => h.TaskId == request.TaskId)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new TaskHistoryDto
            {
                Id = h.Id,
                TaskId = h.TaskId,
                FromStatus = (int)h.FromStatus,
                ToStatus = (int)h.ToStatus,
                Action = h.Action,
                PerformedById = h.PerformedById,
                PerformedByEmail = h.PerformedByUser != null ? h.PerformedByUser.Email : null,
                PerformedByName = h.PerformedByUser != null
                    ? $"{h.PerformedByUser.FirstName} {h.PerformedByUser.LastName}".Trim()
                    : null,
                Notes = h.Notes,
                CreatedAt = h.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<TaskHistoryDto>>.Success(history);
    }
}