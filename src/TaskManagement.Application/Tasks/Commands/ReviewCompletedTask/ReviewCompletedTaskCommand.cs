using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.ReviewCompletedTask;

/// <summary>
///     Command for a manager to review a completed task with rating and feedback.
/// </summary>
public class ReviewCompletedTaskCommand : ICommand<TaskDto>
{
    public Guid TaskId { get; set; }
    public bool Accepted { get; set; }
    public int Rating { get; set; }
    public string? Feedback { get; set; }
    public bool SendBackForRework { get; set; }
}