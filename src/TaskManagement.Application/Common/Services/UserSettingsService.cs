using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.Common.Constants;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Common.Services;

/// <summary>
///     Implementation of IUserSettingsService that retrieves user settings from HttpContext headers
///     with support for override via IMemoryCache for testing purposes.
/// </summary>
public class UserSettingsService(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache) : IUserSettingsService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private const string DefaultLanguage = "en";
    private static readonly string[] SupportedLanguages = { "en", "ar" };

    public string GetLanguage()
    {
        // Check for override first (for testing)
        if (_memoryCache.TryGetValue(CacheKeys.UserSettingsOverride, out UserSettingsOverride? overrideValue) && overrideValue != null)
        {
            return NormalizeLanguage(overrideValue.Language) ?? DefaultLanguage;
        }

        // Try to get from X-Locale header first (explicit preference)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var xLocaleHeader = httpContext.Request.Headers["X-Locale"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xLocaleHeader))
            {
                var normalized = NormalizeLanguage(xLocaleHeader);
                if (normalized != null)
                {
                    return normalized;
                }
            }

            // Fall back to Accept-Language header
            var acceptLanguageHeader = httpContext.Request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(acceptLanguageHeader))
            {
                var normalized = NormalizeLanguage(acceptLanguageHeader);
                if (normalized != null)
                {
                    return normalized;
                }
            }
        }

        // Default to English
        return DefaultLanguage;
    }

    private static string? NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        // Extract language code (e.g., "en-US" -> "en", "ar-SA" -> "ar")
        var languageCode = language.Split('-', ',', ';')[0]?.Trim().ToLowerInvariant();
        
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return null;
        }

        // Return if supported, otherwise null
        return SupportedLanguages.Contains(languageCode) ? languageCode : null;
    }
}

/// <summary>
///     Class for storing user settings override values in memory cache.
///     Made public so it can be used by TestingController.
/// </summary>
public class UserSettingsOverride
{
    public string? Language { get; set; }
}

