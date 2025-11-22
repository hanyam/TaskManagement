using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Queries.GetDashboardStats;

/// <summary>
///     Handler for getting dashboard statistics.
///     Optimized to use a single Dapper query instead of multiple EF Core queries.
///     Follows CQRS pattern: Queries use Dapper for performance.
/// </summary>
public class GetDashboardStatsQueryHandler(TaskDapperRepository taskRepository) : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly TaskDapperRepository _taskRepository = taskRepository;

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        // Single optimized query: Calculate all 7 counts in one database round trip
        // Uses conditional aggregation (CASE WHEN) for maximum performance
        var stats = await _taskRepository.GetDashboardStatsAsync(request.UserId, cancellationToken);

        return Result<DashboardStatsDto>.Success(stats);
    }
}