namespace TaskManagement.Domain.Common;

/// <summary>
///     Represents an error in the system.
/// </summary>
public class Error
{
    private Error(string code, string message, string? field = null)
    {
        Code = code;
        Message = message;
        Field = field;
    }

    public string Code { get; }
    public string Message { get; }
    public string? Field { get; }

    public static Error Create(string code, string message, string? field = null)
    {
        return new Error(code, message, field);
    }

    public static Error NotFound(string resource, string? field = null)
    {
        return new Error("NOT_FOUND", $"{resource} not found", field);
    }

    public static Error Validation(string message, string? field = null)
    {
        return new Error("VALIDATION_ERROR", message, field);
    }

    public static Error Unauthorized(string message = "Unauthorized access")
    {
        return new Error("UNAUTHORIZED", message);
    }

    public static Error Forbidden(string message = "Forbidden access")
    {
        return new Error("FORBIDDEN", message);
    }

    public static Error Conflict(string message, string? field = null)
    {
        return new Error("CONFLICT", message, field);
    }

    public static Error Internal(string message = "An internal error occurred")
    {
        return new Error("INTERNAL_ERROR", message);
    }

    public override string ToString()
    {
        return $"{Code}: {Message}";
    }
}