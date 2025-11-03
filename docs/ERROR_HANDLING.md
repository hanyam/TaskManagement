# Task Management API - Error Handling Documentation

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Error Model](#error-model)
2. [Error Categories](#error-categories)
3. [Centralized Error Definitions](#centralized-error-definitions)
4. [Error Code Reference](#error-code-reference)
5. [Error Response Format](#error-response-format)
6. [Global Exception Handling](#global-exception-handling)

---

## Error Model

### Error Class Structure

**Error Class:**
```csharp
public class Error
{
    public string Code { get; }
    public string Message { get; }
    public string? Field { get; }
}
```

**Properties:**
- `Code`: Error code identifier (e.g., "NOT_FOUND", "VALIDATION_ERROR")
- `Message`: Human-readable error message
- `Field`: Optional field name for validation errors

**Factory Methods:**
- `Error.Create(code, message, field)`: Create custom error
- `Error.NotFound(resource, field)`: Resource not found
- `Error.Validation(message, field)`: Validation error
- `Error.Unauthorized(message)`: Unauthorized access
- `Error.Forbidden(message)`: Forbidden access
- `Error.Conflict(message, field)`: Business rule conflict
- `Error.Internal(message)`: Internal server error

### Result Pattern Usage

**Result<T> Class:**
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
    public List<Error> Errors { get; }
}
```

**Success:**
```csharp
Result<TaskDto>.Success(taskDto)
```

**Single Error:**
```csharp
Result<TaskDto>.Failure(TaskErrors.NotFound)
```

**Multiple Errors:**
```csharp
Result<TaskDto>.Failure(new List<Error> { 
    TaskErrors.TitleRequired, 
    TaskErrors.DueDateInPast 
})
```

**Result (non-generic):**
```csharp
public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public List<Error> Errors { get; }
}
```

### ApiResponse Structure

**ApiResponse<T> Class:**
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<Error> Errors { get; set; }
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; }
}
```

**Success Response:**
```json
{
  "success": true,
  "data": { /* data */ },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**Error Response:**
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Title is required",
      "field": "Title"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

---

## Error Categories

### Validation Errors

**Code:** `VALIDATION_ERROR`

**Sources:**
- FluentValidation validators
- Domain entity validation
- Business rule validation

**Examples:**
- Title is required
- Due date cannot be in the past
- Progress percentage must be between 0 and 100
- Invalid status transition

**Response Format:**
```json
{
  "code": "VALIDATION_ERROR",
  "message": "Title is required",
  "field": "Title"
}
```

### Not Found Errors

**Code:** `NOT_FOUND`

**Sources:**
- Entity not found in database
- User not found
- Task not found

**Examples:**
- Task with ID not found
- User not found
- Assigned user not found

**Response Format:**
```json
{
  "code": "NOT_FOUND",
  "message": "Task not found",
  "field": "Id"
}
```

### Authorization Errors

**Code:** `UNAUTHORIZED` or `FORBIDDEN`

**Sources:**
- Invalid or missing JWT token
- Insufficient permissions
- Role-based access control violations

**Examples:**
- Unauthorized access
- Invalid token
- Insufficient permissions

**Response Format:**
```json
{
  "code": "FORBIDDEN",
  "message": "Insufficient permissions",
  "field": null
}
```

### Business Rule Errors

**Code:** `CONFLICT` or `VALIDATION_ERROR`

**Sources:**
- Status transition violations
- Business logic violations
- Rule enforcement failures

**Examples:**
- Cannot update completed task
- Invalid status transition
- Extension limit exceeded

**Response Format:**
```json
{
  "code": "CONFLICT",
  "message": "Task is already completed",
  "field": "Status"
}
```

### System Errors

**Code:** `INTERNAL_ERROR`

**Sources:**
- Unhandled exceptions
- Database errors
- External service failures

**Examples:**
- Database connection failure
- Unexpected exception
- Service unavailable

**Response Format:**
```json
{
  "code": "INTERNAL_ERROR",
  "message": "An internal error occurred",
  "field": null
}
```

---

## Centralized Error Definitions

### TaskErrors Class

**Location:** `src/TaskManagement.Domain/Errors/Tasks/TaskErrors.cs`

**Error Definitions:**
```csharp
public static class TaskErrors
{
    // Not found errors
    public static Error NotFound => Error.NotFound("Task", "Id");
    public static Error NotFoundById(Guid id) => Error.NotFound($"Task with ID '{id}'", "Id");
    public static Error AssignedUserNotFound => Error.NotFound("Assigned user", "AssignedUserId");
    
    // Validation errors
    public static Error TitleRequired => Error.Validation("Title is required", "Title");
    public static Error DueDateInPast => Error.Validation("Due date cannot be in the past", "DueDate");
    
    // Business logic errors
    public static Error TaskAlreadyCompleted => Error.Conflict("Task is already completed", "Status");
    public static Error CannotUpdateCompletedTask => Error.Validation("Cannot update a completed task", "Status");
}
```

**Usage:**
```csharp
return Result<TaskDto>.Failure(TaskErrors.NotFound);
return Result<TaskDto>.Failure(TaskErrors.TitleRequired);
```

### UserErrors Class

**Location:** `src/TaskManagement.Domain/Errors/Users/UserErrors.cs`

**Error Definitions:**
- User not found
- User validation errors
- User business logic errors

### AuthenticationErrors Class

**Location:** `src/TaskManagement.Domain/Errors/Authentication/AuthenticationErrors.cs`

**Error Definitions:**
- Invalid Azure AD token
- Authentication failures
- Token validation errors

### SystemErrors Class

**Location:** `src/TaskManagement.Domain/Errors/System/SystemErrors.cs`

**Error Definitions:**
- Internal server errors
- System-level failures
- Unexpected exceptions

---

## Error Code Reference

### Complete Error Code List

**Validation Errors:**
- `VALIDATION_ERROR`: General validation failure

**Not Found Errors:**
- `NOT_FOUND`: Resource not found

**Authorization Errors:**
- `UNAUTHORIZED`: Authentication required or invalid token
- `FORBIDDEN`: Insufficient permissions

**Business Rule Errors:**
- `CONFLICT`: Business rule violation or conflict

**System Errors:**
- `INTERNAL_ERROR`: Internal server error

### Error Code Meanings

**VALIDATION_ERROR:**
- Input validation failed
- Business rule validation failed
- Invalid parameter values

**NOT_FOUND:**
- Requested resource does not exist
- Entity not found in database
- Reference to non-existent entity

**UNAUTHORIZED:**
- Authentication required
- Invalid or expired token
- Token validation failed

**FORBIDDEN:**
- Insufficient permissions
- Role-based access denied
- Action not allowed for current user

**CONFLICT:**
- Business rule violation
- State conflict
- Operation conflicts with current state

**INTERNAL_ERROR:**
- Unexpected server error
- Database error
- External service failure

### When Errors Occur

**Validation Errors:**
- During request validation (FluentValidation)
- During entity method calls
- During business rule checks

**Not Found Errors:**
- Entity lookup failures
- Foreign key references
- Related entity not found

**Authorization Errors:**
- Missing or invalid JWT token
- Insufficient role permissions
- User ID not found in claims

**Business Rule Errors:**
- Invalid status transitions
- Extension limit exceeded
- Task type constraints violated

**System Errors:**
- Unhandled exceptions
- Database connection failures
- External service timeouts

---

## Error Response Format

### Success Responses

**Single Resource:**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Task Title",
    // ... other properties
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**List Resource:**
```json
{
  "success": true,
  "data": {
    "tasks": [ /* task list */ ],
    "totalCount": 10,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**No Content (Result without data):**
```json
{
  "success": true,
  "data": null,
  "message": "Operation completed successfully",
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### Error Responses

**Single Error:**
```json
{
  "success": false,
  "data": null,
  "message": "Task not found",
  "errors": [
    {
      "code": "NOT_FOUND",
      "message": "Task not found",
      "field": "Id"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**Multiple Errors:**
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Title is required",
      "field": "Title"
    },
    {
      "code": "VALIDATION_ERROR",
      "message": "Due date cannot be in the past",
      "field": "DueDate"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**Internal Error:**
```json
{
  "success": false,
  "data": null,
  "message": "An error occurred while processing your request",
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### Multiple Error Handling

**Validation Errors:**
- Multiple validation errors can be returned in single response
- Each error includes field name for client-side handling
- All validation errors collected before response

**Business Rule Errors:**
- Single error typically returned
- Clear message indicating rule violation
- Field name indicates affected property

---

## Global Exception Handling

### ExceptionHandlingMiddleware

**Location:** `src/TaskManagement.Api/Middleware/ExceptionHandlingMiddleware.cs`

**Purpose:**
- Catch all unhandled exceptions
- Log exception details
- Return standardized error response
- Prevent sensitive information leakage

**Implementation:**
```csharp
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        await HandleExceptionAsync(context, ex);
    }
}
```

**Exception Handling:**
- Logs full exception with Serilog
- Returns generic error message to client
- Includes trace ID for correlation
- No stack traces in response

### Unhandled Exception Strategy

**Logged Information:**
- Full exception details (server-side)
- Stack trace
- Request path and method
- User ID (if available)
- Timestamp

**Client Response:**
- Generic error message
- Trace ID for support
- No sensitive information
- Appropriate HTTP status code (500)

**Example Log:**
```
[ERROR] Unhandled exception occurred
Exception: System.InvalidOperationException: Task must be assigned
   at TaskManagement.Domain.Entities.Task.Accept()
   at TaskManagement.Application.Tasks.Commands.AcceptTask.AcceptTaskCommandHandler.Handle()
Request Path: /api/tasks/123/accept
User ID: 456e7890-e89b-12d3-a456-426614174001
Trace ID: 0HMQ8VQJQJQJQ
```

**Example Response:**
```json
{
  "success": false,
  "data": null,
  "message": "An error occurred while processing your request",
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### Logging Strategy

**Serilog Configuration:**
- Structured logging
- Multiple sinks (Console, File)
- Enriched with context
- Log levels: Information, Warning, Error

**Log Levels:**
- `Information`: Normal operations
- `Warning`: Recoverable errors
- `Error`: Exception caught and handled
- `Fatal`: Application crash

**Log Format:**
```
[Timestamp] [Level] [Message] {Properties} {Exception}
```

---

## See Also

- [API Reference](API_REFERENCE.md) - Error response examples
- [Architecture Documentation](ARCHITECTURE.md) - Result pattern usage
- [Domain Model](DOMAIN_MODEL.md) - Entity error handling

