namespace TaskManagement.Application.Common.Constants;

/// <summary>
///     Centralized cache keys used across the application.
///     These keys are used for storing override values in IMemoryCache for testing purposes.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    ///     Cache key for current user override (used by ICurrentUserService and TestingController).
    /// </summary>
    public const string CurrentUserOverride = "CurrentUser_Override";

    /// <summary>
    ///     Cache key for current date/time override (used by ICurrentDateService and TestingController).
    /// </summary>
    public const string CurrentDateOverride = "CurrentDate_Override";

    /// <summary>
    ///     Cache key for user settings override (used by IUserSettingsService and TestingController).
    /// </summary>
    public const string UserSettingsOverride = "UserSettings_Override";
}