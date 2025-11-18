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
}