using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Queries.GetDashboardStats;

/// <summary>
///     Handler for getting dashboard statistics.
/// </summary>
public class GetDashboardStatsQueryHandler(TaskManagementDbContext context) : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly TaskManagementDbContext _context = context;

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var nearDueDateThreshold = now.AddDays(3); // Tasks due within 3 days

        // Tasks created by user
        var tasksCreatedByUser = await _context.Tasks
            .CountAsync(t => t.CreatedById == request.UserId, cancellationToken);

        // Tasks completed (assigned to or created by user)
        var tasksCompleted = await _context.Tasks
            .CountAsync(t => t.Status == TaskStatus.Completed &&
                             (t.AssignedUserId == request.UserId || t.CreatedById == request.UserId ||
                              t.Assignments.Any(a => a.UserId == request.UserId)), cancellationToken);

        // Tasks near due date (within 3 days)
        var tasksNearDueDate = await _context.Tasks
            .CountAsync(t => t.DueDate.HasValue &&
                             t.DueDate.Value >= now &&
                             t.DueDate.Value <= nearDueDateThreshold &&
                             t.Status != TaskStatus.Completed &&
                             t.Status != TaskStatus.Cancelled &&
                             (t.AssignedUserId == request.UserId || t.CreatedById == request.UserId ||
                              t.Assignments.Any(a => a.UserId == request.UserId)), cancellationToken);

        // Tasks delayed (past due date)
        var tasksDelayed = await _context.Tasks
            .CountAsync(t => t.DueDate.HasValue &&
                             t.DueDate.Value < now &&
                             t.Status != TaskStatus.Completed &&
                             t.Status != TaskStatus.Cancelled &&
                             (t.AssignedUserId == request.UserId || t.CreatedById == request.UserId ||
                              t.Assignments.Any(a => a.UserId == request.UserId)), cancellationToken);

        // Tasks in progress (assigned and accepted)
        // Note: Accepted tasks are actively being worked on, Assigned tasks are awaiting acceptance
        var tasksInProgress = await _context.Tasks
            .Where(t => (t.Status == TaskStatus.Assigned || t.Status == TaskStatus.Accepted) &&
                        (t.AssignedUserId == request.UserId ||
                         _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == request.UserId)))
            .CountAsync(cancellationToken);

        // Tasks under review (includes both UnderReview and PendingManagerReview)
        // UnderReview: tasks/progress updates awaiting manager review
        // PendingManagerReview: tasks completed by employee, awaiting manager review with rating
        var tasksUnderReview = await _context.Tasks
            .Where(t => (t.Status == TaskStatus.UnderReview || t.Status == TaskStatus.PendingManagerReview) &&
                        (t.AssignedUserId == request.UserId ||
                         _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == request.UserId) ||
                         t.CreatedById == request.UserId))
            .CountAsync(cancellationToken);

        // Tasks pending acceptance (created with assigned user, awaiting acceptance)
        var tasksPendingAcceptance = await _context.Tasks
            .CountAsync(t => t.Status == TaskStatus.Created &&
                             (t.AssignedUserId == request.UserId ||
                              t.Assignments.Any(a => a.UserId == request.UserId)), cancellationToken);

        return new DashboardStatsDto
        {
            TasksCreatedByUser = tasksCreatedByUser,
            TasksCompleted = tasksCompleted,
            TasksNearDueDate = tasksNearDueDate,
            TasksDelayed = tasksDelayed,
            TasksInProgress = tasksInProgress,
            TasksUnderReview = tasksUnderReview,
            TasksPendingAcceptance = tasksPendingAcceptance
        };
    }
}