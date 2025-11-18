using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Errors.System;

/// <summary>
///     Centralized error definitions for System-related operations.
/// </summary>
public static class SystemErrors
{
    // General system errors
    public static Error InternalServerError => Error.Internal("An internal server error occurred");
    public static Error ServiceUnavailable => Error.Internal("Service is temporarily unavailable");
    public static Error DatabaseConnectionFailed => Error.Internal("Database connection failed");
    public static Error DatabaseTimeout => Error.Internal("Database operation timed out");
    public static Error ExternalServiceUnavailable => Error.Internal("External service is unavailable");

    // Validation errors
    public static Error InvalidInput => Error.Validation("Invalid input provided");

    // Business logic errors
    public static Error OperationNotAllowed => Error.Forbidden("Operation is not allowed");
    public static Error ResourceInUse => Error.Conflict("Resource is currently in use");
    public static Error ResourceLocked => Error.Conflict("Resource is locked by another operation");
    public static Error ConcurrentModification => Error.Conflict("Resource was modified by another user");

    // Network errors
    public static Error NetworkTimeout => Error.Internal("Network operation timed out");
    public static Error NetworkUnavailable => Error.Internal("Network is unavailable");
    public static Error ConnectionRefused => Error.Internal("Connection was refused");

    // File system errors
    public static Error FileNotFound => Error.NotFound("File", "Path");
    public static Error FileAccessDenied => Error.Forbidden("File access denied");
    public static Error FileTooLarge => Error.Validation("File size exceeds maximum allowed", "FileSize");
    public static Error InvalidFileFormat => Error.Validation("Invalid file format", "FileType");

    // Performance errors
    public static Error OperationTimeout => Error.Internal("Operation timed out");
    public static Error ResourceExhausted => Error.Internal("System resources exhausted");
    public static Error TooManyRequests => Error.Forbidden("Too many requests");

    // Maintenance errors
    public static Error SystemMaintenance => Error.Internal("System is under maintenance");
    public static Error FeatureDisabled => Error.Forbidden("Feature is currently disabled");
    public static Error ServiceDeprecated => Error.Forbidden("Service is deprecated");

    public static Error RequiredFieldMissing(string fieldName)
    {
        return Error.Validation($"Required field '{fieldName}' is missing", fieldName);
    }

    public static Error InvalidFormat(string fieldName)
    {
        return Error.Validation($"Invalid format for field '{fieldName}'", fieldName);
    }

    public static Error ValueTooLong(string fieldName, int maxLength)
    {
        return Error.Validation($"Field '{fieldName}' cannot exceed {maxLength} characters", fieldName);
    }

    public static Error ValueTooShort(string fieldName, int minLength)
    {
        return Error.Validation($"Field '{fieldName}' must be at least {minLength} characters", fieldName);
    }

    // Configuration errors
    public static Error ConfigurationMissing(string configKey)
    {
        return Error.Internal($"Configuration '{configKey}' is missing");
    }

    public static Error InvalidConfiguration(string configKey)
    {
        return Error.Internal($"Invalid configuration for '{configKey}'");
    }

    public static Error EnvironmentVariableMissing(string variableName)
    {
        return Error.Internal($"Environment variable '{variableName}' is missing");
    }
}