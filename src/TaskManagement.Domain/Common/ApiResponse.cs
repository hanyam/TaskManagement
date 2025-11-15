namespace TaskManagement.Domain.Common;

/// <summary>
///     Standardized API response model for all endpoints.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<Error> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public List<ApiActionLink>? Links { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResponse(string error, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = error,
            TraceId = traceId
        };
    }

    public static ApiResponse<T> ErrorResponse(List<Error> errors, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors,
            TraceId = traceId
        };
    }
}

/// <summary>
///     Standardized API response model for endpoints that don't return data.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResponse(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public new static ApiResponse ErrorResponse(string error, string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = error,
            TraceId = traceId
        };
    }

    public new static ApiResponse ErrorResponse(List<Error> errors, string? traceId = null)
    {
        return new ApiResponse
        {
            Success = false,
            Errors = errors,
            TraceId = traceId
        };
    }
}