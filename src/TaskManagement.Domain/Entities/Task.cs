using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents a task in the system.
/// </summary>
public class Task : BaseEntity
{
    private Task()
    {
    }

    public Task(string title, string? description, TaskPriority priority, DateTime? dueDate, Guid? assignedUserId,
        TaskType type, Guid createdById)
    {
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        OriginalDueDate = dueDate;
        AssignedUserId = assignedUserId; // Null for unassigned tasks (draft state)
        Type = type;
        CreatedById = createdById;
        Status = TaskStatus.Created;
        ReminderLevel = ReminderLevel.None;
        ProgressPercentage = null;
    }

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? OriginalDueDate { get; private set; }
    public DateTime? ExtendedDueDate { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public User? AssignedUser { get; private set; }
    public TaskType Type { get; }
    public ReminderLevel ReminderLevel { get; private set; }
    public int? ProgressPercentage { get; private set; }
    public Guid CreatedById { get; private set; }
    public User? CreatedByUser { get; private set; }
    public int? ManagerRating { get; private set; }
    public string? ManagerFeedback { get; private set; }

    // Navigation properties
    public ICollection<TaskAssignment> Assignments { get; private set; } = new List<TaskAssignment>();
    public ICollection<TaskProgressHistory> ProgressHistory { get; private set; } = new List<TaskProgressHistory>();
    public ICollection<TaskHistory> History { get; private set; } = new List<TaskHistory>();

    public ICollection<DeadlineExtensionRequest> ExtensionRequests { get; private set; } =
        new List<DeadlineExtensionRequest>();

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        Title = title;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdatePriority(TaskPriority priority)
    {
        Priority = priority;
    }

    public void UpdateDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
    }

    public void AssignToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        AssignedUserId = userId;
    }


    public void Cancel()
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed task");

        Status = TaskStatus.Cancelled;
    }

    public void Assign()
    {
        if (Status != TaskStatus.Created)
            throw new InvalidOperationException("Only created tasks can be assigned");

        Status = TaskStatus.Assigned;
    }

    public void SetUnderReview()
    {
        if (Status != TaskStatus.Assigned && Status != TaskStatus.Accepted)
            throw new InvalidOperationException("Task must be assigned or accepted to be under review");

        Status = TaskStatus.UnderReview;
    }

    public void Accept()
    {
        if (Status != TaskStatus.Created && Status != TaskStatus.Assigned && Status != TaskStatus.UnderReview)
            throw new InvalidOperationException("Task must be created, assigned, or under review to be accepted");

        // If task is in Created status, transition to Assigned first, then to Accepted
        if (Status == TaskStatus.Created)
        {
            if (!AssignedUserId.HasValue)
                throw new InvalidOperationException("Task must have an assigned user to be accepted");

            Status = TaskStatus.Assigned;
        }

        Status = TaskStatus.Accepted;
    }

    public void Reject()
    {
        if (Status != TaskStatus.Created && Status != TaskStatus.Assigned && Status != TaskStatus.UnderReview)
            throw new InvalidOperationException("Task must be created, assigned, or under review to be rejected");

        // If task is in Created status, transition to Assigned first, then to Rejected
        if (Status == TaskStatus.Created)
        {
            if (!AssignedUserId.HasValue)
                throw new InvalidOperationException("Task must have an assigned user to be rejected");

            Status = TaskStatus.Assigned;
        }

        Status = TaskStatus.Rejected;
    }

    public void UpdateProgress(int percentage, bool requiresAcceptance)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Progress percentage must be between 0 and 100", nameof(percentage));

        if (Type == TaskType.Simple && percentage > 0)
            throw new InvalidOperationException("Simple tasks cannot have progress tracking");

        if (Type != TaskType.WithProgress && Type != TaskType.WithAcceptedProgress && percentage > 0)
            throw new InvalidOperationException("This task type does not support progress tracking");

        ProgressPercentage = percentage;

        if (!requiresAcceptance && Type == TaskType.WithAcceptedProgress)
            // If it's a task with accepted progress but no acceptance required, it's still pending
            SetUnderReview();
    }

    public void AcceptProgress()
    {
        if (Type != TaskType.WithAcceptedProgress)
            throw new InvalidOperationException("This task type does not require progress acceptance");

        if (Status != TaskStatus.UnderReview)
            throw new InvalidOperationException("Task must be under review to accept progress");

        Status = TaskStatus.Accepted;
    }

    public void UpdateReminderLevel(ReminderLevel level)
    {
        ReminderLevel = level;
    }

    public void ExtendDeadline(DateTime newDueDate, string? reason = null)
    {
        if (newDueDate <= DateTime.UtcNow)
            throw new ArgumentException("Extended due date must be in the future", nameof(newDueDate));

        if (DueDate.HasValue && newDueDate <= DueDate.Value)
            throw new ArgumentException("Extended due date must be after the current due date", nameof(newDueDate));

        ExtendedDueDate = newDueDate;
        OriginalDueDate = DueDate;
        DueDate = newDueDate;
    }

    public void Complete()
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Task is already completed");

        if (Status == TaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled task");

        Status = TaskStatus.Completed;

        if (Type == TaskType.WithProgress || Type == TaskType.WithAcceptedProgress) ProgressPercentage = 100;
    }

    public void MarkCompletedByEmployee()
    {
        if (Status != TaskStatus.Assigned && Status != TaskStatus.Accepted)
            throw new InvalidOperationException("Task must be assigned or accepted to be marked as completed");

        if (Status == TaskStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled task");

        Status = TaskStatus.PendingManagerReview;

        if (Type == TaskType.WithProgress || Type == TaskType.WithAcceptedProgress) ProgressPercentage = 100;
    }

    public void ReviewByManager(bool accepted, int? rating, string? feedback, bool sendBackForRework = false)
    {
        if (Status != TaskStatus.PendingManagerReview)
            throw new InvalidOperationException("Task must be in PendingManagerReview status to be reviewed");

        if (rating.HasValue && (rating.Value < 1 || rating.Value > 5))
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        if (feedback != null && feedback.Length > 1000)
            throw new ArgumentException("Feedback cannot exceed 1000 characters", nameof(feedback));

        if (sendBackForRework && accepted)
            throw new ArgumentException("Cannot accept and send back for rework at the same time");

        ManagerRating = rating;
        ManagerFeedback = feedback;

        if (sendBackForRework)
            Status = TaskStatus.Assigned;
        else if (accepted)
            Status = TaskStatus.Accepted;
        else
            // When manager rejects, return to Accepted status since user already accepted the task
            Status = TaskStatus.Accepted;
    }
}

/// <summary>
///     Represents the status of a task.
/// </summary>
public enum TaskStatus
{
    Created = 0,
    Assigned = 1,
    UnderReview = 2,
    Accepted = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6,
    PendingManagerReview = 7,
    RejectedByManager = 8,

    // Legacy statuses for backward compatibility
    Pending = 0, // Maps to Created
    InProgress = 1 // Maps to Assigned
}

/// <summary>
///     Represents the priority of a task.
/// </summary>
public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
///     Represents the type of a task.
/// </summary>
public enum TaskType
{
    Simple = 0,
    WithDueDate = 1,
    WithProgress = 2,
    WithAcceptedProgress = 3
}

/// <summary>
///     Represents the reminder level based on due date proximity.
/// </summary>
public enum ReminderLevel
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
///     Represents the status of a deadline extension request.
/// </summary>
public enum ExtensionRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}