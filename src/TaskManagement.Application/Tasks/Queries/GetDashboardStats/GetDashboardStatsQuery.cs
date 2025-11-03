using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Queries.GetDashboardStats;

/// <summary>
///     Query for getting dashboard statistics for the current user.
/// </summary>
public record GetDashboardStatsQuery : IQuery<DashboardStatsDto>
{
    public Guid UserId { get; init; }
}

