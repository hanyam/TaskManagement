using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Tasks.Queries.GetTaskProgressHistory;

/// <summary>
///     Handler for getting task progress history.
/// </summary>
public class GetTaskProgressHistoryQueryHandler : IRequestHandler<GetTaskProgressHistoryQuery, List<TaskProgressDto>>
{
    private readonly ApplicationDbContext _context;

    public GetTaskProgressHistoryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TaskProgressDto>>> Handle(GetTaskProgressHistoryQuery request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate pagination
        if (request.Page < 1)
        {
            errors.Add(TaskErrors.InvalidPageNumber);
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            errors.Add(TaskErrors.InvalidPageSize);
        }

        // Validate task exists
        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == request.TaskId, cancellationToken);
        if (!taskExists)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
        }

        if (errors.Any())
        {
            return Result<List<TaskProgressDto>>.Failure(errors);
        }

        var progressHistory = await _context.Set<TaskProgressHistory>()
            .Include(ph => ph.UpdatedByUser)
            .Include(ph => ph.AcceptedByUser)
            .Where(ph => ph.TaskId == request.TaskId)
            .OrderByDescending(ph => ph.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ph => new TaskProgressDto
            {
                Id = ph.Id,
                TaskId = ph.TaskId,
                UpdatedById = ph.UpdatedById,
                UpdatedByEmail = ph.UpdatedByUser != null ? ph.UpdatedByUser.Email : null,
                ProgressPercentage = ph.ProgressPercentage,
                Notes = ph.Notes,
                Status = ph.Status,
                AcceptedById = ph.AcceptedById,
                AcceptedByEmail = ph.AcceptedByUser != null ? ph.AcceptedByUser.Email : null,
                AcceptedAt = ph.AcceptedAt,
                UpdatedAt = ph.UpdatedAt ?? ph.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return progressHistory;
    }
}

