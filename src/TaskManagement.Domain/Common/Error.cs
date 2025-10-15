namespace TaskManagement.Domain.Common;

/// <summary>
/// Represents an error in the system.
/// </summary>
public class Error
{
    public string Code { get; }
    public string Message { get; }
    public string? Field { get; }

    private Error(string code, string message, string? field = null)
    {
        Code = code;
        Message = message;
        Field = field;
    }

    public static Error Create(string code, string message, string? field = null) => new(code, message, field);

    public static Error NotFound(string resource, string? field = null) => 
        new("NOT_FOUND", $"{resource} not found", field);

    public static Error Validation(string message, string? field = null) => 
        new("VALIDATION_ERROR", message, field);

    public static Error Unauthorized(string message = "Unauthorized access") => 
        new("UNAUTHORIZED", message);

    public static Error Forbidden(string message = "Forbidden access") => 
        new("FORBIDDEN", message);

    public static Error Conflict(string message, string? field = null) => 
        new("CONFLICT", message, field);

    public static Error Internal(string message = "An internal error occurred") => 
        new("INTERNAL_ERROR", message);

    public override string ToString() => $"{Code}: {Message}";
}
