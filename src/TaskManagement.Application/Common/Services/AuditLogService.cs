using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Extensions;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Common.Services;

/// <summary>
///     Service for logging audit events for business-critical operations.
/// </summary>
public interface IAuditLogService
{
    void LogTaskCreated(Guid taskId, string userId, string userEmail, string? correlationId = null);
    void LogTaskUpdated(Guid taskId, string userId, string userEmail, string? correlationId = null);
    void LogTaskAssigned(Guid taskId, Guid assignedUserId, string userId, string userEmail, string? correlationId = null);
    void LogTaskCompleted(Guid taskId, string userId, string userEmail, string? correlationId = null);
    void LogTaskStatusChanged(Guid taskId, string oldStatus, string newStatus, string userId, string userEmail, string? correlationId = null);
    void LogFileUploaded(Guid taskId, Guid attachmentId, string fileName, string userId, string userEmail, string? correlationId = null);
    void LogFileDownloaded(Guid taskId, Guid attachmentId, string userId, string userEmail, string? correlationId = null);
    void LogExtensionRequestApproved(Guid taskId, Guid extensionRequestId, string userId, string userEmail, string? correlationId = null);
    void LogExtensionRequestRejected(Guid taskId, Guid extensionRequestId, string userId, string userEmail, string? correlationId = null);
    void LogAuthenticationSuccess(string userId, string userEmail, string? correlationId = null);
    void LogAuthenticationFailure(string userEmail, string reason, string? correlationId = null);
    void LogTaskReassigned(Guid taskId, Guid oldUserId, Guid newUserId, string userId, string userEmail, string? correlationId = null);
}

/// <summary>
///     Implementation of audit logging service.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly ILogger<AuditLogService> _logger;
    private readonly ICurrentUserService? _currentUserService;

    public AuditLogService(ILogger<AuditLogService> logger, ICurrentUserService? currentUserService = null)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public void LogTaskCreated(Guid taskId, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit("TaskCreated", "Task", taskId.ToString(), userId, userEmail, correlationId);
    }

    public void LogTaskUpdated(Guid taskId, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit("TaskUpdated", "Task", taskId.ToString(), userId, userEmail, correlationId);
    }

    public void LogTaskAssigned(Guid taskId, Guid assignedUserId, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit(
            "TaskAssigned",
            "Task",
            taskId.ToString(),
            userId,
            userEmail,
            correlationId,
            new Dictionary<string, object> { { "AssignedUserId", assignedUserId } });
    }

    public void LogTaskCompleted(Guid taskId, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit("TaskCompleted", "Task", taskId.ToString(), userId, userEmail, correlationId);
    }

    public void LogTaskStatusChanged(Guid taskId, string oldStatus, string newStatus, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit(
            "TaskStatusChanged",
            "Task",
            taskId.ToString(),
            userId,
            userEmail,
            correlationId,
            new Dictionary<string, object>
            {
                { "OldStatus", oldStatus },
                { "NewStatus", newStatus }
            });
    }

    public void LogFileUploaded(Guid taskId, Guid attachmentId, string fileName, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit(
            "FileUploaded",
            "TaskAttachment",
            attachmentId.ToString(),
            userId,
            userEmail,
            correlationId,
            new Dictionary<string, object>
            {
                { "TaskId", taskId },
                { "FileName", fileName }
            });
    }

    public void LogFileDownloaded(Guid taskId, Guid attachmentId, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit(
            "FileDownloaded",
            "TaskAttachment",
            attachmentId.ToString(),
            userId,
            userEmail,
            correlationId,
            new Dictionary<string, object> { { "TaskId", taskId } });
    }

    public void LogExtensionRequestApproved(Guid taskId, Guid extensionRequestId, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit(
            "ExtensionRequestApproved",
            "ExtensionRequest",
            extensionRequestId.ToString(),
            userId,
            userEmail,
            correlationId,
            new Dictionary<string, object> { { "TaskId", taskId } });
    }

    public void LogExtensionRequestRejected(Guid taskId, Guid extensionRequestId, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit(
            "ExtensionRequestRejected",
            "ExtensionRequest",
            extensionRequestId.ToString(),
            userId,
            userEmail,
            correlationId,
            new Dictionary<string, object> { { "TaskId", taskId } });
    }

    public void LogAuthenticationSuccess(string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit("AuthenticationSuccess", "User", userId, userId, userEmail, correlationId);
    }

    public void LogAuthenticationFailure(string userEmail, string reason, string? correlationId = null)
    {
        _logger.LogAudit(
            "AuthenticationFailure",
            "User",
            userEmail,
            "Unknown",
            userEmail,
            correlationId,
            new Dictionary<string, object> { { "Reason", reason } });
    }

    public void LogTaskReassigned(Guid taskId, Guid oldUserId, Guid newUserId, string userId, string userEmail, string? correlationId = null)
    {
        _logger.LogAudit(
            "TaskReassigned",
            "Task",
            taskId.ToString(),
            userId,
            userEmail,
            correlationId,
            new Dictionary<string, object>
            {
                { "OldUserId", oldUserId },
                { "NewUserId", newUserId }
            });
    }
}

