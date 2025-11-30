namespace TaskManagement.Domain.Common;

/// <summary>
///     Represents an error in the system.
/// </summary>
public class Error
{
    private Error(string code, string message, string? field = null, string? messageKey = null)
    {
        Code = code;
        Message = message;
        Field = field;
        MessageKey = messageKey;
    }

    public string Code { get; }
    public string Message { get; }
    public string? Field { get; }
    public string? MessageKey { get; }

    public static Error Create(string code, string message, string? field = null, string? messageKey = null)
    {
        return new Error(code, message, field, messageKey);
    }

    public static Error NotFound(string resource, string? field = null, string? messageKey = null)
    {
        return new Error("NOT_FOUND", $"{resource} not found", field, messageKey);
    }

    public static Error Validation(string message, string? field = null, string? messageKey = null)
    {
        return new Error("VALIDATION_ERROR", message, field, messageKey);
    }

    public static Error Unauthorized(string message = "Unauthorized access", string? messageKey = null)
    {
        return new Error("UNAUTHORIZED", message, null, messageKey);
    }

    public static Error Forbidden(string message = "Forbidden access", string? messageKey = null)
    {
        return new Error("FORBIDDEN", message, null, messageKey);
    }

    public static Error Conflict(string message, string? field = null, string? messageKey = null)
    {
        return new Error("CONFLICT", message, field, messageKey);
    }

    public static Error Internal(string message = "An internal error occurred", string? messageKey = null)
    {
        return new Error("INTERNAL_ERROR", message, null, messageKey);
    }

    public override string ToString()
    {
        return $"{Code}: {Message}";
    }
}