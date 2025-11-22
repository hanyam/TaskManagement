using Dapper;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Infrastructure.Data.Repositories;

/// <summary>
///     Dapper-based repository for Task read operations with optimized queries.
/// </summary>
public class TaskDapperRepository(IConfiguration configuration) : DapperQueryRepository<Task>(configuration)
{

    /// <summary>
    ///     Gets a task by ID with assigned user information.
    /// </summary>
    public virtual async Task<TaskDto?> GetTaskWithUserAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                T.Id, T.Title, T.Description, T.Status, T.Priority, T.DueDate, 
                T.OriginalDueDate, T.ExtendedDueDate, T.Type, T.ReminderLevel, T.ProgressPercentage,
                T.AssignedUserId, T.CreatedById, T.CreatedAt, T.UpdatedAt, T.CreatedBy,
                T.ManagerRating, T.ManagerFeedback,
                U.Email AS AssignedUserEmail
            FROM [Tasks].[Tasks] AS T
            LEFT JOIN [Tasks].[Users] AS U ON T.AssignedUserId = U.Id
            WHERE T.Id = @TaskId";

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TaskDto>(new CommandDefinition(sql, new { TaskId = taskId },
            cancellationToken: cancellationToken));
    }

    /// <summary>
    ///     Gets tasks with filtering and pagination.
    /// </summary>
    public virtual async Task<(IEnumerable<TaskDto> Tasks, int TotalCount)> GetTasksWithPaginationAsync(
        TaskStatus? status = null,
        TaskPriority? priority = null,
        Guid? assignedUserId = null,
        Guid? createdById = null,
        DateTime? dueDateFrom = null,
        DateTime? dueDateTo = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (status.HasValue)
        {
            conditions.Add("T.Status = @Status");
            parameters.Add("Status", status.Value);
        }

        if (priority.HasValue)
        {
            conditions.Add("T.Priority = @Priority");
            parameters.Add("Priority", priority.Value);
        }

        if (assignedUserId.HasValue)
        {
            conditions.Add("T.AssignedUserId = @AssignedUserId");
            parameters.Add("AssignedUserId", assignedUserId.Value);
        }

        if (createdById.HasValue)
        {
            conditions.Add("T.CreatedById = @CreatedById");
            parameters.Add("CreatedById", createdById.Value);
        }

        if (dueDateFrom.HasValue)
        {
            conditions.Add("T.DueDate >= @DueDateFrom");
            parameters.Add("DueDateFrom", dueDateFrom.Value);
        }

        if (dueDateTo.HasValue)
        {
            conditions.Add("T.DueDate <= @DueDateTo");
            parameters.Add("DueDateTo", dueDateTo.Value);
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        var countSql = $"SELECT COUNT(T.Id) FROM [Tasks].[Tasks] AS T {whereClause}";
        var dataSql = $@"
            SELECT 
                T.Id, T.Title, T.Description, T.Status, T.Priority, T.DueDate, 
                T.AssignedUserId, T.CreatedAt, T.UpdatedAt, T.CreatedBy,
                U.Email AS AssignedUserEmail
            FROM [Tasks].[Tasks] AS T
            LEFT JOIN [Tasks].[Users] AS U ON T.AssignedUserId = U.Id
            {whereClause}
            ORDER BY T.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var connection = CreateConnection();
        var totalCount =
            await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, parameters,
                cancellationToken: cancellationToken));
        var tasks = await connection.QueryAsync<TaskDto>(new CommandDefinition(dataSql, parameters,
            cancellationToken: cancellationToken));

        return (tasks, totalCount);
    }

    /// <summary>
    ///     Gets dashboard statistics for a user using a single optimized SQL query.
    ///     Calculates all 7 counts in one database round trip using conditional aggregation.
    /// </summary>
    public virtual async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var nearDueDateThreshold = now.AddDays(3);

        var sql = @"
            WITH TaskWithAssignment AS (
                SELECT 
                    T.Id,
                    T.CreatedById,
                    T.AssignedUserId,
                    T.Status,
                    T.DueDate,
                    CASE WHEN TA.TaskId IS NOT NULL THEN 1 ELSE 0 END AS HasAssignment
                FROM [Tasks].[Tasks] AS T
                LEFT JOIN [Tasks].[TaskAssignments] AS TA ON TA.TaskId = T.Id AND TA.UserId = @UserId
                WHERE T.CreatedById = @UserId
                   OR T.AssignedUserId = @UserId
                   OR TA.TaskId IS NOT NULL
            )
            SELECT 
                -- Tasks created by user
                COUNT(CASE WHEN CreatedById = @UserId THEN 1 END) AS TasksCreatedByUser,
                
                -- Tasks completed (assigned to or created by user, or has assignment)
                COUNT(CASE 
                    WHEN Status = 5 -- Completed
                        AND (AssignedUserId = @UserId 
                             OR CreatedById = @UserId 
                             OR HasAssignment = 1)
                    THEN 1 
                END) AS TasksCompleted,
                
                -- Tasks near due date (within 3 days)
                COUNT(CASE 
                    WHEN DueDate IS NOT NULL
                        AND DueDate >= @Now
                        AND DueDate <= @NearDueDateThreshold
                        AND Status NOT IN (5, 6) -- Not Completed or Cancelled
                        AND (AssignedUserId = @UserId 
                             OR CreatedById = @UserId 
                             OR HasAssignment = 1)
                    THEN 1 
                END) AS TasksNearDueDate,
                
                -- Tasks delayed (past due date)
                COUNT(CASE 
                    WHEN DueDate IS NOT NULL
                        AND DueDate < @Now
                        AND Status NOT IN (5, 6) -- Not Completed or Cancelled
                        AND (AssignedUserId = @UserId 
                             OR CreatedById = @UserId 
                             OR HasAssignment = 1)
                    THEN 1 
                END) AS TasksDelayed,
                
                -- Tasks in progress (Assigned or Accepted)
                COUNT(CASE 
                    WHEN Status IN (1, 3) -- Assigned or Accepted
                        AND (AssignedUserId = @UserId 
                             OR HasAssignment = 1)
                    THEN 1 
                END) AS TasksInProgress,
                
                -- Tasks under review (UnderReview or PendingManagerReview)
                COUNT(CASE 
                    WHEN Status IN (2, 7) -- UnderReview or PendingManagerReview
                        AND (AssignedUserId = @UserId 
                             OR CreatedById = @UserId 
                             OR HasAssignment = 1)
                    THEN 1 
                END) AS TasksUnderReview,
                
                -- Tasks pending acceptance (Created status)
                COUNT(CASE 
                    WHEN Status = 0 -- Created
                        AND (AssignedUserId = @UserId 
                             OR HasAssignment = 1)
                    THEN 1 
                END) AS TasksPendingAcceptance
            FROM TaskWithAssignment";

        var parameters = new
        {
            UserId = userId,
            Now = now,
            NearDueDateThreshold = nearDueDateThreshold
        };

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<DashboardStatsDto>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken)) 
            ?? new DashboardStatsDto();
    }
}