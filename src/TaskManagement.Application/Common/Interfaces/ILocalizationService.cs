namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
///     Service for localizing error messages and other strings.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    ///     Gets a localized string by key.
    /// </summary>
    /// <param name="key">The resource key (e.g., "Errors.Tasks.NotFound").</param>
    /// <param name="defaultValue">Optional default value if key is not found.</param>
    /// <param name="args">Optional format arguments for string formatting.</param>
    /// <returns>The localized string, or default value, or key if not found.</returns>
    string GetString(string key, string? defaultValue = null, params object[] args);
}

