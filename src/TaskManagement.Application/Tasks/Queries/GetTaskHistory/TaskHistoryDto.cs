namespace TaskManagement.Application.Tasks.Queries.GetTaskHistory;

/// <summary>
///     DTO for task history entry.
/// </summary>
public class TaskHistoryDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public int FromStatus { get; set; }
    public int ToStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid PerformedById { get; set; }
    public string? PerformedByEmail { get; set; }
    public string? PerformedByName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}