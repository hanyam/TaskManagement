# EnsureUserIdAttribute - Implementation Guide

**Date:** November 18, 2025  
**Status:** ✅ Implemented  
**Purpose:** Eliminate repeated user ID validation code in controllers

---

## Overview

The `EnsureUserIdAttribute` is a custom authorization filter that ensures the current user ID exists before executing controller actions. This eliminates the need for repeated validation code in every controller method.

---

## Problem Solved

**Before:** Every controller method had this repeated code:

```csharp
[HttpPost]
public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
{
    var userIdClaim = User.FindFirst("user_id")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        return BadRequest(
            ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));

    // Actual logic here...
}
```

**After:** Simply add the attribute:

```csharp
[HttpPost]
[EnsureUserId]
public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
{
    var userId = GetRequiredUserId(); // Guaranteed to exist

    // Actual logic here...
}
```

---

## Implementation

### Attribute Definition

**File:** `src/TaskManagement.Api/Attributes/EnsureUserIdAttribute.cs`

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class EnsureUserIdAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Get ICurrentUserService from DI
        var currentUserService = context.HttpContext.RequestServices.GetService<ICurrentUserService>();
        
        Guid? userId = currentUserService?.GetUserId() ?? /* fallback logic */;

        if (!userId.HasValue)
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.ErrorResponse(
                    "User ID not found in token",
                    context.HttpContext.TraceIdentifier));
        }
        else
        {
            // Store userId in HttpContext.Items for easy access
            context.HttpContext.Items["CurrentUserId"] = userId.Value;
        }
    }
}
```

### Key Features

1. **Works with ICurrentUserService**: Supports override mechanism for testing
2. **Backward Compatible**: Falls back to HttpContext.User if service not available
3. **Stores in HttpContext.Items**: Makes userId easily accessible in controllers
4. **Early Return**: Returns BadRequest before action executes if user ID missing

---

## Usage

### At Method Level

```csharp
[HttpPost]
[EnsureUserId]
public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
{
    var userId = GetRequiredUserId(); // Non-nullable Guid
    // Use userId...
}
```

### At Controller Level

```csharp
[ApiController]
[Route("tasks")]
[Authorize]
[EnsureUserId] // Applies to all actions in this controller
public class TasksController : BaseController
{
    // All methods automatically have user ID validation
}
```

### Combined with Other Attributes

```csharp
[HttpPost("{id}/assign")]
[Authorize(Roles = "Manager")] // Role-based authorization
[EnsureUserId]                  // User ID validation
public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
{
    var userId = GetRequiredUserId();
    // ...
}
```

---

## BaseController Helper Methods

### GetRequiredUserId()

Returns a non-nullable `Guid`. Use this when `[EnsureUserId]` is applied:

```csharp
[EnsureUserId]
public async Task<IActionResult> MyAction()
{
    var userId = GetRequiredUserId(); // Guid (not Guid?)
    // userId is guaranteed to exist
}
```

### GetCurrentUserId()

Returns a nullable `Guid?`. Use this when user ID might not exist:

```csharp
public async Task<IActionResult> MyAction()
{
    var userId = GetCurrentUserId(); // Guid?
    if (!userId.HasValue)
    {
        // Handle missing user ID
    }
}
```

---

## How It Works

### Execution Flow

1. **Request arrives** → ASP.NET Core pipeline
2. **Authorization filters execute** → `EnsureUserIdAttribute.OnAuthorization()` runs
3. **Check user ID**:
   - Try `ICurrentUserService.GetUserId()` (supports override)
   - Fallback to `HttpContext.User.FindFirst("user_id")`
4. **If user ID found**:
   - Store in `HttpContext.Items["CurrentUserId"]`
   - Continue to action method
5. **If user ID missing**:
   - Set `context.Result = BadRequest`
   - Action method never executes

### HttpContext.Items Storage

The attribute stores the validated user ID in `HttpContext.Items`:

```csharp
context.HttpContext.Items["CurrentUserId"] = userId.Value;
```

This allows `GetCurrentUserId()` and `GetRequiredUserId()` to retrieve it efficiently without re-validating.

---

## Benefits

### ✅ Code Reduction

- **Before**: ~5 lines of validation per method
- **After**: 1 attribute + 1 line to get userId
- **Savings**: ~4 lines per method × 12 methods = **48 lines removed**

### ✅ Consistency

- All methods use the same validation logic
- Same error message format
- Same error response structure

### ✅ Maintainability

- Change validation logic in one place
- No risk of forgetting validation in new methods
- Clear intent via attribute

### ✅ Testing Support

- Works with `ICurrentUserService` override mechanism
- Can set user override via `/api/testing/current-user`
- Attribute respects override values

---

## Migration Guide

### Step 1: Add Using Statement

```csharp
using TaskManagement.Api.Attributes;
```

### Step 2: Add Attribute to Method

```csharp
[HttpPost]
[EnsureUserId] // Add this
public async Task<IActionResult> MyAction()
```

### Step 3: Replace Validation Code

**Remove:**
```csharp
var userIdClaim = User.FindFirst("user_id")?.Value;
if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    return BadRequest(
        ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
```

**Replace with:**
```csharp
var userId = GetRequiredUserId();
```

### Step 4: Update Variable Usage

Change from `userId` (Guid) to `userId` (Guid) - no `.Value` needed:

```csharp
// Before
CreatedById = userId.Value

// After
CreatedById = userId
```

---

## Examples

### Example 1: Simple Action

**Before:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
{
    var userIdClaim = User.FindFirst("user_id")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        return BadRequest(
            ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));

    var command = new CreateTaskCommand
    {
        CreatedById = userId,
        // ...
    };
    // ...
}
```

**After:**
```csharp
[HttpPost]
[EnsureUserId]
public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
{
    var userId = GetRequiredUserId();

    var command = new CreateTaskCommand
    {
        CreatedById = userId,
        // ...
    };
    // ...
}
```

### Example 2: With Role Authorization

**Before:**
```csharp
[HttpPost("{id}/assign")]
[Authorize(Roles = "Manager")]
public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
{
    var userIdClaim = User.FindFirst("user_id")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        return BadRequest(
            ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));

    var command = new AssignTaskCommand
    {
        AssignedById = userId,
        // ...
    };
    // ...
}
```

**After:**
```csharp
[HttpPost("{id}/assign")]
[Authorize(Roles = "Manager")]
[EnsureUserId]
public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
{
    var userId = GetRequiredUserId();

    var command = new AssignTaskCommand
    {
        AssignedById = userId,
        // ...
    };
    // ...
}
```

---

## Error Response

When user ID is not found, the attribute returns:

```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "User ID not found in token",
      "field": null
    }
  ],
  "timestamp": "2025-11-18T10:30:00Z",
  "traceId": "0HNH42M4FEMFU:0000000A"
}
```

**HTTP Status:** `400 Bad Request`

---

## Testing

### Unit Testing

The attribute can be tested by mocking `ICurrentUserService`:

```csharp
[Fact]
public void OnAuthorization_WhenUserIdExists_ShouldContinue()
{
    // Arrange
    var mockService = new Mock<ICurrentUserService>();
    mockService.Setup(s => s.GetUserId()).Returns(Guid.NewGuid());
    
    var context = CreateAuthorizationContext(mockService.Object);
    var attribute = new EnsureUserIdAttribute();
    
    // Act
    attribute.OnAuthorization(context);
    
    // Assert
    Assert.Null(context.Result); // No error, continues
    Assert.True(context.HttpContext.Items.ContainsKey("CurrentUserId"));
}

[Fact]
public void OnAuthorization_WhenUserIdMissing_ShouldReturnBadRequest()
{
    // Arrange
    var mockService = new Mock<ICurrentUserService>();
    mockService.Setup(s => s.GetUserId()).Returns((Guid?)null);
    
    var context = CreateAuthorizationContext(mockService.Object);
    var attribute = new EnsureUserIdAttribute();
    
    // Act
    attribute.OnAuthorization(context);
    
    // Assert
    Assert.NotNull(context.Result);
    Assert.IsType<BadRequestObjectResult>(context.Result);
}
```

### Integration Testing

Test with override mechanism:

```bash
# 1. Set user override
curl -X POST "http://localhost:5000/api/testing/current-user" \
  -H "Content-Type: application/json" \
  -d '{"userId": "test-guid-here"}'

# 2. Call endpoint (EnsureUserIdAttribute will use override)
curl -X POST "http://localhost:5000/api/tasks" \
  -H "Authorization: Bearer TOKEN" \
  -d '{"title": "Test"}'

# 3. Remove override
curl -X DELETE "http://localhost:5000/api/testing/current-user"
```

---

## Files Modified

### New Files (1)
1. `src/TaskManagement.Api/Attributes/EnsureUserIdAttribute.cs`

### Modified Files (3)
1. `src/TaskManagement.Api/Controllers/BaseController.cs`
   - Added `GetRequiredUserId()` method
   - Updated `GetCurrentUserId()` to check `HttpContext.Items` first

2. `src/TaskManagement.Api/Controllers/TasksController.cs`
   - Added `[EnsureUserId]` to 12 action methods
   - Replaced validation code with `GetRequiredUserId()`
   - Updated `GenerateTaskLinks()` to use `ICurrentUserService`

3. `src/TaskManagement.Api/Controllers/DashboardController.cs`
   - Added `[EnsureUserId]` to `GetDashboardStats()`
   - Replaced validation code with `GetRequiredUserId()`

---

## Statistics

- **Methods Updated**: 13 (12 in TasksController, 1 in DashboardController)
- **Lines Removed**: ~52 lines of repeated validation code
- **Lines Added**: 1 attribute file (~50 lines) + helper method (~10 lines)
- **Net Reduction**: ~42 lines of code
- **Consistency**: 100% (all methods use same validation)

---

## Best Practices

1. **Always use `[EnsureUserId]`** when action requires authenticated user
2. **Use `GetRequiredUserId()`** when attribute is applied (non-nullable)
3. **Use `GetCurrentUserId()`** when user might not exist (nullable)
4. **Combine with `[Authorize]`** for role-based access control
5. **Don't duplicate validation** - let the attribute handle it

---

## Related Documentation

- **[Testing Overrides Guide](TESTING_OVERRIDES.md)** - User/date override mechanism
- **[Current User/Date Services](CURRENT_USER_DATE_SERVICES.md)** - Service implementation
- **[API Reference](API_REFERENCE.md)** - Complete API documentation

---

**Last Updated:** November 18, 2025  
**Feature Status:** ✅ Production Ready  
**Breaking Changes:** None (backward compatible)

