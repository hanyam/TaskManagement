namespace TaskManagement.Domain.Options;

/// <summary>
///     Configuration options for task reminder levels based on due date proximity.
/// </summary>
public class ReminderOptions
{
    public const string SectionName = "Reminder";

    /// <summary>
    ///     Percentage thresholds for reminder levels (0-1.0).
    ///     Example: If threshold is 0.25, reminder triggers when 25% of time has passed.
    /// </summary>
    public Dictionary<string, double> Thresholds { get; set; } = new()
    {
        { "Critical", 0.90 }, // 90% of time passed
        { "High", 0.75 }, // 75% of time passed
        { "Medium", 0.50 }, // 50% of time passed
        { "Low", 0.25 } // 25% of time passed
    };

    /// <summary>
    ///     Fixed day thresholds as alternative to percentage-based calculation.
    ///     If set, these override percentage thresholds.
    /// </summary>
    public Dictionary<string, int> DayThresholds { get; set; } = new()
    {
        { "Critical", 1 }, // 1 day before due date
        { "High", 3 }, // 3 days before due date
        { "Medium", 7 }, // 7 days before due date
        { "Low", 14 } // 14 days before due date
    };

    /// <summary>
    ///     Whether to use day thresholds instead of percentage thresholds.
    /// </summary>
    public bool UseDayThresholds { get; set; } = false;
}