using Dapper;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Data.Repositories;

/// <summary>
///     Dapper-based repository for TaskAttachment read operations.
/// </summary>
public class TaskAttachmentDapperRepository(IConfiguration configuration) : DapperQueryRepository<TaskAttachment>(configuration)
{
    /// <summary>
    ///     Gets all attachments for a task with user information.
    /// </summary>
    public virtual async Task<IEnumerable<TaskAttachmentDto>> GetTaskAttachmentsAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                TA.Id, TA.TaskId, TA.FileName, TA.OriginalFileName, TA.ContentType, 
                TA.FileSize, TA.Type, TA.UploadedById, TA.CreatedAt,
                U.Email AS UploadedByEmail,
                U.DisplayName AS UploadedByDisplayName
            FROM [Tasks].[TaskAttachments] AS TA
            LEFT JOIN [Tasks].[Users] AS U ON TA.UploadedById = U.Id
            WHERE TA.TaskId = @TaskId
            ORDER BY TA.CreatedAt DESC";

        using var connection = CreateConnection();
        return await connection.QueryAsync<TaskAttachmentDto>(
            new CommandDefinition(sql, new { TaskId = taskId }, cancellationToken: cancellationToken));
    }
}

