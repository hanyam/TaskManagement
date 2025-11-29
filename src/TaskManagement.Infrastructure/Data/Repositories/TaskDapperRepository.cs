using Dapper;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;
using ExtensionRequestStatus = TaskManagement.Domain.Entities.ExtensionRequestStatus;
using ProgressStatus = TaskManagement.Domain.Entities.ProgressStatus;

namespace TaskManagement.Infrastructure.Data.Repositories;

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
                T.Id,
                T.Title,
                T.Description,
                T.Status,
                T.Priority,
                T.DueDate,
                T.AssignedUserId,
                T.CreatedAt,
                T.UpdatedAt,
                T.CreatedBy,
                T.ManagerRating,
                T.ManagerFeedback,
                U.Email AS AssignedUserEmail,
                U.DisplayName AS AssignedUserDisplayName
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

    /// <summary>
    ///     Checks if a user has access to a task.
    ///     User has access if:
    ///     1. User created the task (CreatedById)
    ///     2. User is assigned to the task (AssignedUserId)
    ///     3. User is in the assignment chain (TaskAssignments)
    /// </summary>
    public virtual async Task<bool> HasUserAccessToTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT CASE 
                WHEN EXISTS (
                    SELECT 1 
                    FROM [Tasks].[Tasks] AS T
                    WHERE T.Id = @TaskId
                      AND (T.CreatedById = @UserId 
                           OR T.AssignedUserId = @UserId
                           OR EXISTS (
                               SELECT 1 
                               FROM [Tasks].[TaskAssignments] AS TA 
                               WHERE TA.TaskId = @TaskId 
                                 AND TA.UserId = @UserId
                           ))
                ) THEN 1 
                ELSE 0 
            END AS HasAccess";

        using var connection = CreateConnection();
        var hasAccess = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { TaskId = taskId, UserId = userId }, cancellationToken: cancellationToken));
        
        return hasAccess == 1;
    }

    /// <summary>
    ///     Gets tasks assigned to a user (via AssignedUserId or TaskAssignments) with pagination.
    /// </summary>
    public virtual async Task<(IEnumerable<TaskDto> Tasks, int TotalCount)> GetAssignedTasksAsync(
        Guid userId,
        TaskStatus? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var conditions = new List<string> { "(T.AssignedUserId = @UserId OR EXISTS (SELECT 1 FROM [Tasks].[TaskAssignments] TA WHERE TA.TaskId = T.Id AND TA.UserId = @UserId))" };
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId);

        if (status.HasValue)
        {
            conditions.Add("T.Status = @Status");
            parameters.Add("Status", status.Value);
        }

        var whereClause = "WHERE " + string.Join(" AND ", conditions);

        var countSql = $@"
            SELECT COUNT(T.Id) 
            FROM [Tasks].[Tasks] AS T
            {whereClause}";

        var dataSql = $@"
            SELECT 
                T.Id, T.Title, T.Description, T.Status, T.Priority, T.DueDate, 
                T.OriginalDueDate, T.ExtendedDueDate, T.Type, T.ReminderLevel, T.ProgressPercentage,
                T.AssignedUserId, T.CreatedById, T.CreatedAt, T.UpdatedAt, T.CreatedBy,
                U.Email AS AssignedUserEmail
            FROM [Tasks].[Tasks] AS T
            LEFT JOIN [Tasks].[Users] AS U ON T.AssignedUserId = U.Id
            {whereClause}
            ORDER BY T.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var connection = CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var tasks = await connection.QueryAsync<TaskDto>(
            new CommandDefinition(dataSql, parameters, cancellationToken: cancellationToken));

        return (tasks, totalCount);
    }

    /// <summary>
    ///     Gets task progress history with user information.
    /// </summary>
    public virtual async Task<IEnumerable<TaskProgressDto>> GetTaskProgressHistoryAsync(
        Guid taskId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                PH.Id,
                PH.TaskId,
                PH.UpdatedById,
                U1.Email AS UpdatedByEmail,
                PH.ProgressPercentage,
                PH.Notes,
                PH.Status,
                PH.AcceptedById,
                U2.Email AS AcceptedByEmail,
                PH.AcceptedAt,
                COALESCE(PH.UpdatedAt, PH.CreatedAt) AS UpdatedAt
            FROM [Tasks].[TaskProgressHistory] AS PH
            LEFT JOIN [Tasks].[Users] AS U1 ON PH.UpdatedById = U1.Id
            LEFT JOIN [Tasks].[Users] AS U2 ON PH.AcceptedById = U2.Id
            WHERE PH.TaskId = @TaskId
            ORDER BY PH.UpdatedAt DESC, PH.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new
        {
            TaskId = taskId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        using var connection = CreateConnection();
        return await connection.QueryAsync<TaskProgressDto>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    /// <summary>
    ///     Checks if a task exists.
    /// </summary>
    public virtual async Task<bool> TaskExistsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT COUNT(1) FROM [Tasks].[Tasks] WHERE Id = @TaskId";
        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { TaskId = taskId }, cancellationToken: cancellationToken));
        return count > 0;
    }

    /// <summary>
    ///     Gets extension requests with related information.
    /// </summary>
    public virtual async Task<IEnumerable<ExtensionRequestDto>> GetExtensionRequestsAsync(
        Guid? taskId = null,
        ExtensionRequestStatus? status = null,
        Guid? userId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (taskId.HasValue)
        {
            conditions.Add("ER.TaskId = @TaskId");
            parameters.Add("TaskId", taskId.Value);
        }

        if (status.HasValue)
        {
            conditions.Add("ER.Status = @Status");
            parameters.Add("Status", status.Value);
        }

        if (userId.HasValue)
        {
            conditions.Add("ER.RequestedById = @UserId");
            parameters.Add("UserId", userId.Value);
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        var sql = $@"
            SELECT 
                ER.Id,
                ER.TaskId,
                T.Title AS TaskTitle,
                ER.RequestedById,
                U1.Email AS RequestedByEmail,
                ER.RequestedDueDate,
                ER.Reason,
                ER.Status,
                ER.ReviewedById,
                U2.Email AS ReviewedByEmail,
                ER.ReviewedAt,
                ER.ReviewNotes,
                ER.CreatedAt
            FROM [Tasks].[DeadlineExtensionRequests] AS ER
            LEFT JOIN [Tasks].[Tasks] AS T ON ER.TaskId = T.Id
            LEFT JOIN [Tasks].[Users] AS U1 ON ER.RequestedById = U1.Id
            LEFT JOIN [Tasks].[Users] AS U2 ON ER.ReviewedById = U2.Id
            {whereClause}
            ORDER BY ER.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var connection = CreateConnection();
        return await connection.QueryAsync<ExtensionRequestDto>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    /// <summary>
    ///     Gets tasks filtered by reminder level calculation.
    ///     Note: Reminder level is calculated in memory after fetching tasks, as it requires business logic.
    /// </summary>
    public virtual async Task<IEnumerable<TaskDto>> GetTasksForReminderLevelCalculationAsync(
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var conditions = new List<string> { "T.DueDate IS NOT NULL", "T.Status NOT IN (5, 6)" }; // Not Completed or Cancelled
        var parameters = new DynamicParameters();

        if (userId.HasValue)
        {
            conditions.Add("(T.AssignedUserId = @UserId OR T.CreatedById = @UserId OR EXISTS (SELECT 1 FROM [Tasks].[TaskAssignments] TA WHERE TA.TaskId = T.Id AND TA.UserId = @UserId))");
            parameters.Add("UserId", userId.Value);
        }

        var whereClause = "WHERE " + string.Join(" AND ", conditions);

        var sql = $@"
            SELECT 
                T.Id, T.Title, T.Description, T.Status, T.Priority, T.DueDate, 
                T.OriginalDueDate, T.ExtendedDueDate, T.Type, T.ReminderLevel, T.ProgressPercentage,
                T.AssignedUserId, T.CreatedById, T.CreatedAt, T.UpdatedAt, T.CreatedBy,
                U.Email AS AssignedUserEmail
            FROM [Tasks].[Tasks] AS T
            LEFT JOIN [Tasks].[Users] AS U ON T.AssignedUserId = U.Id
            {whereClause}
            ORDER BY T.CreatedAt DESC";

        using var connection = CreateConnection();
        return await connection.QueryAsync<TaskDto>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }
}

