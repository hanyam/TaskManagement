namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Service for accessing the current user's settings, including language preference.
///     Supports override mechanism for testing via IMemoryCache.
/// </summary>
public interface IUserSettingsService
{
    /// <summary>
    ///     Gets the current user's language preference (e.g., "en", "ar").
    ///     Defaults to "en" if not specified.
    /// </summary>
    /// <returns>The language code (e.g., "en" or "ar"). Defaults to "en".</returns>
    string GetLanguage();
}