using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Errors.System;

/// <summary>
///     Centralized error definitions for System-related operations.
/// </summary>
public static class SystemErrors
{
    // General system errors
    public static Error InternalServerError => Error.Create("INTERNAL_ERROR", "An internal server error occurred", null, "Errors.System.InternalServerError");
    public static Error ServiceUnavailable => Error.Create("INTERNAL_ERROR", "Service is temporarily unavailable", null, "Errors.System.ServiceUnavailable");
    public static Error DatabaseConnectionFailed => Error.Create("INTERNAL_ERROR", "Database connection failed", null, "Errors.System.DatabaseConnectionFailed");
    public static Error DatabaseTimeout => Error.Create("INTERNAL_ERROR", "Database operation timed out", null, "Errors.System.DatabaseTimeout");
    public static Error ExternalServiceUnavailable => Error.Create("INTERNAL_ERROR", "External service is unavailable", null, "Errors.System.ExternalServiceUnavailable");

    // Validation errors
    public static Error InvalidInput => Error.Create("VALIDATION_ERROR", "Invalid input provided", null, "Errors.System.InvalidInput");

    // Business logic errors
    public static Error OperationNotAllowed => Error.Create("FORBIDDEN", "Operation is not allowed", null, "Errors.System.OperationNotAllowed");
    public static Error ResourceInUse => Error.Create("CONFLICT", "Resource is currently in use", null, "Errors.System.ResourceInUse");
    public static Error ResourceLocked => Error.Create("CONFLICT", "Resource is locked by another operation", null, "Errors.System.ResourceLocked");
    public static Error ConcurrentModification => Error.Create("CONFLICT", "Resource was modified by another user", null, "Errors.System.ConcurrentModification");

    // Network errors
    public static Error NetworkTimeout => Error.Create("INTERNAL_ERROR", "Network operation timed out", null, "Errors.System.NetworkTimeout");
    public static Error NetworkUnavailable => Error.Create("INTERNAL_ERROR", "Network is unavailable", null, "Errors.System.NetworkUnavailable");
    public static Error ConnectionRefused => Error.Create("INTERNAL_ERROR", "Connection was refused", null, "Errors.System.ConnectionRefused");

    // File system errors
    public static Error FileNotFound => Error.Create("NOT_FOUND", "File not found", "Path", "Errors.System.FileNotFound");
    public static Error FileAccessDenied => Error.Create("FORBIDDEN", "File access denied", null, "Errors.System.FileAccessDenied");
    public static Error FileTooLarge => Error.Create("VALIDATION_ERROR", "File size exceeds maximum allowed", "FileSize", "Errors.System.FileTooLarge");
    public static Error InvalidFileFormat => Error.Create("VALIDATION_ERROR", "Invalid file format", "FileType", "Errors.System.InvalidFileFormat");

    // Performance errors
    public static Error OperationTimeout => Error.Create("INTERNAL_ERROR", "Operation timed out", null, "Errors.System.OperationTimeout");
    public static Error ResourceExhausted => Error.Create("INTERNAL_ERROR", "System resources exhausted", null, "Errors.System.ResourceExhausted");
    public static Error TooManyRequests => Error.Create("FORBIDDEN", "Too many requests", null, "Errors.System.TooManyRequests");

    // Maintenance errors
    public static Error SystemMaintenance => Error.Create("INTERNAL_ERROR", "System is under maintenance", null, "Errors.System.SystemMaintenance");
    public static Error FeatureDisabled => Error.Create("FORBIDDEN", "Feature is currently disabled", null, "Errors.System.FeatureDisabled");
    public static Error ServiceDeprecated => Error.Create("FORBIDDEN", "Service is deprecated", null, "Errors.System.ServiceDeprecated");

    public static Error RequiredFieldMissing(string fieldName)
    {
        return Error.Create("VALIDATION_ERROR", $"Required field '{fieldName}' is missing", fieldName, "Errors.System.RequiredFieldMissing");
    }

    public static Error InvalidFormat(string fieldName)
    {
        return Error.Create("VALIDATION_ERROR", $"Invalid format for field '{fieldName}'", fieldName, "Errors.System.InvalidFormat");
    }

    public static Error ValueTooLong(string fieldName, int maxLength)
    {
        return Error.Create("VALIDATION_ERROR", $"Field '{fieldName}' cannot exceed {maxLength} characters", fieldName, "Errors.System.ValueTooLong");
    }

    public static Error ValueTooShort(string fieldName, int minLength)
    {
        return Error.Create("VALIDATION_ERROR", $"Field '{fieldName}' must be at least {minLength} characters", fieldName, "Errors.System.ValueTooShort");
    }

    // Configuration errors
    public static Error ConfigurationMissing(string configKey)
    {
        return Error.Create("INTERNAL_ERROR", $"Configuration '{configKey}' is missing", null, "Errors.System.ConfigurationMissing");
    }

    public static Error InvalidConfiguration(string configKey)
    {
        return Error.Create("INTERNAL_ERROR", $"Invalid configuration for '{configKey}'", null, "Errors.System.InvalidConfiguration");
    }

    public static Error EnvironmentVariableMissing(string variableName)
    {
        return Error.Create("INTERNAL_ERROR", $"Environment variable '{variableName}' is missing", null, "Errors.System.EnvironmentVariableMissing");
    }
}
