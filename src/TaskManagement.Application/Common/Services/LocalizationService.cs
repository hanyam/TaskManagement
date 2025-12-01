using System.Text.Json;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Common.Services;

/// <summary>
///     Service for localizing error messages and other strings.
///     Loads resources from JSON files based on language preference.
/// </summary>
public class LocalizationService(IUserSettingsService userSettingsService) : ILocalizationService
{
    private static readonly Dictionary<string, Dictionary<string, string>> _resourceCache = new();
    private static readonly object _lockObject = new();
    private readonly IUserSettingsService _userSettingsService = userSettingsService;

    public string GetString(string key, string? defaultValue = null, params object[] args)
    {
        var language = _userSettingsService.GetLanguage();
        var resourceKey = $"Resources.{language}.json";

        // Load resources if not cached
        if (!_resourceCache.ContainsKey(resourceKey)) LoadResources(language);

        string? value = null;

        // Try to get from cache
        if (_resourceCache.TryGetValue(resourceKey, out var resources) && resources.TryGetValue(key, out value))
        {
            // Format with arguments if provided
            if (args.Length > 0 && !string.IsNullOrEmpty(value))
                try
                {
                    return string.Format(value, args);
                }
                catch
                {
                    // If formatting fails, return unformatted string
                    return value;
                }

            return value;
        }

        // Fall back to English if not found
        if (language != "en")
        {
            if (!_resourceCache.ContainsKey("Resources.en.json")) LoadResources("en");

            if (_resourceCache.TryGetValue("Resources.en.json", out var enResources) &&
                enResources.TryGetValue(key, out var enValue))
            {
                // Format with arguments if provided
                if (args.Length > 0 && !string.IsNullOrEmpty(enValue))
                    try
                    {
                        return string.Format(enValue, args);
                    }
                    catch
                    {
                        // If formatting fails, return unformatted string
                        return enValue;
                    }

                return enValue;
            }
        }

        // Return default or key if not found
        var result = defaultValue ?? key;

        // Format default value with arguments if provided
        if (args.Length > 0 && !string.IsNullOrEmpty(result))
            try
            {
                return string.Format(result, args);
            }
            catch
            {
                // If formatting fails, return unformatted string
                return result;
            }

        return result;
    }

    private static void LoadResources(string language)
    {
        lock (_lockObject)
        {
            var resourceKey = $"Resources.{language}.json";
            if (_resourceCache.ContainsKey(resourceKey)) return; // Already loaded

            var resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var assembly = typeof(LocalizationService).Assembly;
                var resourceName = $"TaskManagement.Application.Resources.{language}.json";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    var jsonDoc = JsonDocument.Parse(json);

                    // Flatten nested JSON structure
                    FlattenJson(jsonDoc.RootElement, "", resources);
                }
            }
            catch
            {
                // If resource file doesn't exist, use empty dictionary
            }

            _resourceCache[resourceKey] = resources;
        }
    }

    private static void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> result)
    {
        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            if (property.Value.ValueKind == JsonValueKind.Object)
                FlattenJson(property.Value, key, result);
            else
                result[key] = property.Value.GetString() ?? string.Empty;
        }
    }
}