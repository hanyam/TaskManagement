namespace TaskManagement.Domain.Options;

/// <summary>
///     Configuration options for deadline extension policies.
/// </summary>
public class ExtensionPolicyOptions
{
    public const string SectionName = "ExtensionPolicy";

    /// <summary>
    ///     Maximum number of extension requests allowed per task.
    /// </summary>
    public int MaxExtensionRequestsPerTask { get; set; } = 3;

    /// <summary>
    ///     Minimum days before due date when extension can be requested.
    ///     Set to 0 to allow extensions at any time.
    /// </summary>
    public int MinDaysBeforeDueDate { get; set; } = 1;

    /// <summary>
    ///     Maximum extension days allowed (from current due date).
    /// </summary>
    public int MaxExtensionDays { get; set; } = 30;

    /// <summary>
    ///     Whether manager approval is required for extensions.
    /// </summary>
    public bool RequiresManagerApproval { get; set; } = true;

    /// <summary>
    ///     Whether extension requests are automatically approved if conditions are met.
    /// </summary>
    public bool AutoApproveIfConditionsMet { get; set; } = false;
}