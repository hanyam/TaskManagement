namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Service for accessing the current date and time.
///     Supports override mechanism for testing via IMemoryCache.
/// </summary>
public interface ICurrentDateService
{
    /// <summary>
    ///     Gets the current UTC date and time.
    /// </summary>
    /// <returns>The current UTC DateTime.</returns>
    DateTime UtcNow { get; }

    /// <summary>
    ///     Gets the current local date and time.
    /// </summary>
    /// <returns>The current local DateTime.</returns>
    DateTime Now { get; }
}