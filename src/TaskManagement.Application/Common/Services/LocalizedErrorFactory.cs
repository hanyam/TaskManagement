using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common.Services;

/// <summary>
///     Factory for creating localized Error instances.
/// </summary>
public class LocalizedErrorFactory(ILocalizationService localizationService)
{
    private readonly ILocalizationService _localizationService = localizationService;

    public Error NotFound(string resourceKey, string? field = null)
    {
        var message = _localizationService.GetString("Errors.Common.NotFound", $"{resourceKey} not found");
        return Error.Create("NOT_FOUND", string.Format(message, resourceKey), field);
    }

    public Error Validation(string messageKey, string? field = null, params object[] args)
    {
        var message = _localizationService.GetString(messageKey, messageKey);
        if (args.Length > 0) message = string.Format(message, args);
        return Error.Create("VALIDATION_ERROR", message, field);
    }

    public Error Unauthorized(string messageKey = "Errors.Common.Unauthorized", params object[] args)
    {
        var message = _localizationService.GetString(messageKey, "Unauthorized access");
        if (args.Length > 0) message = string.Format(message, args);
        return Error.Create("UNAUTHORIZED", message);
    }

    public Error Forbidden(string messageKey = "Errors.Common.Forbidden", params object[] args)
    {
        var message = _localizationService.GetString(messageKey, "Forbidden access");
        if (args.Length > 0) message = string.Format(message, args);
        return Error.Create("FORBIDDEN", message);
    }

    public Error Conflict(string messageKey, string? field = null, params object[] args)
    {
        var message = _localizationService.GetString(messageKey, messageKey);
        if (args.Length > 0) message = string.Format(message, args);
        return Error.Create("CONFLICT", message, field);
    }

    public Error Internal(string messageKey = "Errors.System.InternalServerError", params object[] args)
    {
        var message = _localizationService.GetString(messageKey, "An internal error occurred");
        if (args.Length > 0) message = string.Format(message, args);
        return Error.Create("INTERNAL_ERROR", message);
    }
}