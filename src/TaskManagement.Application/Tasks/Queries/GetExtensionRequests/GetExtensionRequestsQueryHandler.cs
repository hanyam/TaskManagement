using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Tasks.Queries.GetExtensionRequests;

/// <summary>
///     Handler for getting extension requests.
/// </summary>
public class GetExtensionRequestsQueryHandler : IRequestHandler<GetExtensionRequestsQuery, List<ExtensionRequestDto>>
{
    private readonly TaskManagementDbContext _context;

    public GetExtensionRequestsQueryHandler(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ExtensionRequestDto>>> Handle(GetExtensionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate pagination
        if (request.Page < 1) errors.Add(TaskErrors.InvalidPageNumber);

        if (request.PageSize < 1 || request.PageSize > 100) errors.Add(TaskErrors.InvalidPageSize);

        if (errors.Any()) return Result<List<ExtensionRequestDto>>.Failure(errors);

        var query = _context.Set<DeadlineExtensionRequest>()
            .Include(er => er.Task)
            .Include(er => er.RequestedBy)
            .Include(er => er.ReviewedBy)
            .AsQueryable();

        if (request.TaskId.HasValue) query = query.Where(er => er.TaskId == request.TaskId.Value);

        if (request.Status.HasValue) query = query.Where(er => er.Status == request.Status.Value);

        if (request.UserId.HasValue) query = query.Where(er => er.RequestedById == request.UserId.Value);

        var extensionRequests = await query
            .OrderByDescending(er => er.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(er => new ExtensionRequestDto
            {
                Id = er.Id,
                TaskId = er.TaskId,
                TaskTitle = er.Task != null ? er.Task.Title : string.Empty,
                RequestedById = er.RequestedById,
                RequestedByEmail = er.RequestedBy != null ? er.RequestedBy.Email : null,
                RequestedDueDate = er.RequestedDueDate,
                Reason = er.Reason,
                Status = er.Status,
                ReviewedById = er.ReviewedById,
                ReviewedByEmail = er.ReviewedBy != null ? er.ReviewedBy.Email : null,
                ReviewedAt = er.ReviewedAt,
                ReviewNotes = er.ReviewNotes,
                CreatedAt = er.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return extensionRequests;
    }
}