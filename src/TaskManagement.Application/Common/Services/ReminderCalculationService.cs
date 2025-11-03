using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Options;
using Microsoft.Extensions.Options;

namespace TaskManagement.Application.Common.Services;

/// <summary>
///     Service for calculating task reminder levels based on due date proximity.
/// </summary>
public interface IReminderCalculationService
{
    ReminderLevel CalculateReminderLevel(DateTime? dueDate, DateTime? createdAt);
}

/// <summary>
///     Implementation of reminder calculation service.
/// </summary>
public class ReminderCalculationService : IReminderCalculationService
{
    private readonly ReminderOptions _options;

    public ReminderCalculationService(IOptions<ReminderOptions> options)
    {
        _options = options.Value;
    }

    public ReminderLevel CalculateReminderLevel(DateTime? dueDate, DateTime? createdAt)
    {
        if (!dueDate.HasValue)
        {
            return ReminderLevel.None;
        }

        var now = DateTime.UtcNow;
        var dueDateUtc = dueDate.Value;

        // If due date has passed, it's critical
        if (dueDateUtc < now)
        {
            return ReminderLevel.Critical;
        }

        if (_options.UseDayThresholds && _options.DayThresholds.Any())
        {
            return CalculateByDayThresholds(dueDateUtc, now);
        }

        // Calculate by percentage if created date is available
        if (createdAt.HasValue)
        {
            return CalculateByPercentage(dueDateUtc, createdAt.Value, now);
        }

        // Fallback to day thresholds if no created date
        return CalculateByDayThresholds(dueDateUtc, now);
    }

    private ReminderLevel CalculateByPercentage(DateTime dueDate, DateTime createdAt, DateTime now)
    {
        var totalDuration = (dueDate - createdAt).TotalDays;
        if (totalDuration <= 0) return ReminderLevel.Critical;

        var elapsedDuration = (now - createdAt).TotalDays;
        var percentageElapsed = elapsedDuration / totalDuration;

        if (percentageElapsed >= _options.Thresholds.GetValueOrDefault("Critical", 0.90))
            return ReminderLevel.Critical;

        if (percentageElapsed >= _options.Thresholds.GetValueOrDefault("High", 0.75))
            return ReminderLevel.High;

        if (percentageElapsed >= _options.Thresholds.GetValueOrDefault("Medium", 0.50))
            return ReminderLevel.Medium;

        if (percentageElapsed >= _options.Thresholds.GetValueOrDefault("Low", 0.25))
            return ReminderLevel.Low;

        return ReminderLevel.None;
    }

    private ReminderLevel CalculateByDayThresholds(DateTime dueDate, DateTime now)
    {
        var daysRemaining = (dueDate - now).TotalDays;

        var criticalDays = _options.DayThresholds.GetValueOrDefault("Critical", 1);
        var highDays = _options.DayThresholds.GetValueOrDefault("High", 3);
        var mediumDays = _options.DayThresholds.GetValueOrDefault("Medium", 7);
        var lowDays = _options.DayThresholds.GetValueOrDefault("Low", 14);

        if (daysRemaining <= criticalDays)
            return ReminderLevel.Critical;

        if (daysRemaining <= highDays)
            return ReminderLevel.High;

        if (daysRemaining <= mediumDays)
            return ReminderLevel.Medium;

        if (daysRemaining <= lowDays)
            return ReminderLevel.Low;

        return ReminderLevel.None;
    }
}

