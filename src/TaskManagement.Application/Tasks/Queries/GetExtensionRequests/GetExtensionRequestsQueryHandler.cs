using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Errors.Tasks;

namespace TaskManagement.Application.Tasks.Queries.GetExtensionRequests;

/// <summary>
///     Handler for getting extension requests.
///     Follows CQRS pattern: Queries use Dapper only (no EF Core).
/// </summary>
public class GetExtensionRequestsQueryHandler(TaskDapperRepository taskRepository) : IRequestHandler<GetExtensionRequestsQuery, List<ExtensionRequestDto>>
{
    private readonly TaskDapperRepository _taskRepository = taskRepository;

    public async Task<Result<List<ExtensionRequestDto>>> Handle(GetExtensionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate pagination
        if (request.Page < 1) errors.Add(TaskErrors.InvalidPageNumber);

        if (request.PageSize < 1 || request.PageSize > 100) errors.Add(TaskErrors.InvalidPageSize);

        if (errors.Any()) return Result<List<ExtensionRequestDto>>.Failure(errors);

        // Get extension requests using Dapper (optimized query)
        var extensionRequests = await _taskRepository.GetExtensionRequestsAsync(
            request.TaskId,
            request.Status,
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result<List<ExtensionRequestDto>>.Success(extensionRequests.ToList());
    }
}