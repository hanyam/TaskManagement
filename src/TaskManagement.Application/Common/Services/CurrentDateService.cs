using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Common.Services;

/// <summary>
///     Implementation of ICurrentDateService that returns the current date/time
///     with support for override via IMemoryCache for testing purposes.
/// </summary>
public class CurrentDateService(IMemoryCache memoryCache) : ICurrentDateService
{
    private readonly IMemoryCache _memoryCache = memoryCache;

    public DateTime UtcNow
    {
        get
        {
            // Check for override first (for testing)
            if (_memoryCache.TryGetValue(CacheKeys.CurrentDateOverride, out DateTime? overrideValue) && overrideValue.HasValue)
            {
                return overrideValue.Value;
            }

            // Fall back to actual current time
            return DateTime.UtcNow;
        }
    }

    public DateTime Now
    {
        get
        {
            // Check for override first (for testing)
            if (_memoryCache.TryGetValue(CacheKeys.CurrentDateOverride, out DateTime? overrideValue) && overrideValue.HasValue)
            {
                // Convert UTC override to local time
                return overrideValue.Value.ToLocalTime();
            }

            // Fall back to actual current time
            return DateTime.Now;
        }
    }
}

