# Testing Overrides Guide

## Overview

The Task Management API provides a mechanism to override the **current user** and **current date/time** for QA and testing purposes. This is essential for:

- **Automated Testing**: Set specific users and dates without authentication
- **Integration Testing**: Test time-sensitive scenarios (deadlines, reminders)
- **QA Testing**: Reproduce bugs at specific points in time
- **Manual Testing**: Test different user roles without switching accounts

---

## Architecture

### Services

1. **`ICurrentUserService`** - Provides current user information with override support
2. **`ICurrentDateService`** - Provides current date/time with override support

Both services check `IMemoryCache` first for override values, then fall back to actual values (HttpContext.User or DateTime.UtcNow).

### Override Mechanism

- Overrides are stored in `IMemoryCache` with fixed cache keys
- Overrides persist until manually cleared or application restart
- Overrides are **only available in Development and Test environments**

---

## API Endpoints

All testing endpoints are prefixed with `/api/testing` and are **only available in Development/Test environments**.

### Current User Override

#### Set Current User Override
```http
POST /api/testing/current-user
Content-Type: application/json

{
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "userEmail": "test.user@example.com",
  "role": "Admin"
}
```

**Request Body:**
- `userId` (Guid?, optional): The user ID to override with
- `userEmail` (string?, optional): The user email/username to override with
- `role` (string?, optional): The user role (e.g., "Admin", "Manager", "Employee")

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Current user override set successfully",
    "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "userEmail": "test.user@example.com",
    "role": "Admin"
  }
}
```

#### Get Current User Override
```http
GET /api/testing/current-user
```

**Response (if override is set):**
```json
{
  "success": true,
  "data": {
    "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "userEmail": "test.user@example.com",
    "role": "Admin"
  }
}
```

**Response (if no override):**
```json
{
  "success": true,
  "data": {
    "message": "No current user override is set"
  }
}
```

#### Remove Current User Override
```http
DELETE /api/testing/current-user
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Current user override removed successfully"
  }
}
```

---

### Current Date/Time Override

#### Set Current Date Override
```http
POST /api/testing/current-date
Content-Type: application/json

{
  "utcDate": "2025-12-25T10:30:00Z"
}
```

**Request Body:**
- `utcDate` (DateTime, required): The UTC date/time to override with

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Current date override set successfully",
    "utcDate": "2025-12-25T10:30:00Z",
    "localDate": "2025-12-25T12:30:00+02:00"
  }
}
```

#### Get Current Date Override
```http
GET /api/testing/current-date
```

**Response (if override is set):**
```json
{
  "success": true,
  "data": {
    "utcDate": "2025-12-25T10:30:00Z",
    "localDate": "2025-12-25T12:30:00+02:00"
  }
}
```

**Response (if no override):**
```json
{
  "success": true,
  "data": {
    "message": "No current date override is set",
    "currentUtcDate": "2025-11-18T14:30:00Z",
    "currentLocalDate": "2025-11-18T16:30:00+02:00"
  }
}
```

#### Remove Current Date Override
```http
DELETE /api/testing/current-date
```

**Response:**
```json
{
  "success": true,
  "data": {
    "message": "Current date override removed successfully"
  }
}
```

---

## Usage Examples

### cURL Examples

#### Set User Override
```bash
curl -X POST "http://localhost:5000/api/testing/current-user" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "userEmail": "test.admin@example.com",
    "role": "Admin"
  }'
```

#### Set Date Override
```bash
curl -X POST "http://localhost:5000/api/testing/current-date" \
  -H "Content-Type: application/json" \
  -d '{
    "utcDate": "2025-12-25T10:30:00Z"
  }'
```

#### Remove Overrides
```bash
curl -X DELETE "http://localhost:5000/api/testing/current-user"
curl -X DELETE "http://localhost:5000/api/testing/current-date"
```

### PowerShell Examples

#### Set User Override
```powershell
$body = @{
    userId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    userEmail = "test.admin@example.com"
    role = "Admin"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/testing/current-user" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

#### Set Date Override
```powershell
$body = @{
    utcDate = "2025-12-25T10:30:00Z"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/testing/current-date" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

---

## Testing Scenarios

### Scenario 1: Test Task Creation as Different User

```bash
# 1. Set user override
curl -X POST "http://localhost:5000/api/testing/current-user" \
  -H "Content-Type: application/json" \
  -d '{"userId": "user-guid-here", "userEmail": "test@example.com"}'

# 2. Create task (will use overridden user)
curl -X POST "http://localhost:5000/api/tasks" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "Test Task", "priority": 1}'

# 3. Remove override
curl -X DELETE "http://localhost:5000/api/testing/current-user"
```

### Scenario 2: Test Deadline Reminder Logic

```bash
# 1. Set date to 3 days before deadline
curl -X POST "http://localhost:5000/api/testing/current-date" \
  -H "Content-Type: application/json" \
  -d '{"utcDate": "2025-12-22T10:00:00Z"}'

# 2. Create task with deadline 3 days from now
curl -X POST "http://localhost:5000/api/tasks" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "Urgent Task", "dueDate": "2025-12-25T10:00:00Z"}'

# 3. Check reminder level (should be High/Critical)
curl -X GET "http://localhost:5000/api/tasks/{taskId}" \
  -H "Authorization: Bearer YOUR_TOKEN"

# 4. Remove date override
curl -X DELETE "http://localhost:5000/api/testing/current-date"
```

### Scenario 3: Test Role-Based Access Control

```bash
# 1. Set user override with Admin role
curl -X POST "http://localhost:5000/api/testing/current-user" \
  -H "Content-Type: application/json" \
  -d '{"userId": "admin-guid", "role": "Admin"}'

# 2. Test admin-only endpoint
curl -X POST "http://localhost:5000/api/database/seed" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'

# 3. Change to Employee role
curl -X POST "http://localhost:5000/api/testing/current-user" \
  -H "Content-Type: application/json" \
  -d '{"userId": "employee-guid", "role": "Employee"}'

# 4. Test same endpoint (should fail with 403)
curl -X POST "http://localhost:5000/api/database/seed" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'
```

---

## Integration with Code

### Using ICurrentUserService in Handlers

```csharp
public class MyCommandHandler : ICommandHandler<MyCommand, MyResponse>
{
    private readonly ICurrentUserService _currentUserService;

    public MyCommandHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<Result<MyResponse>> Handle(MyCommand command, CancellationToken cancellationToken)
    {
        // Get current user ID (respects override)
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Result<MyResponse>.Failure(Error.Unauthorized("User not authenticated"));
        }

        // Get user email (respects override)
        var userEmail = _currentUserService.GetUserEmail();

        // Use userId and userEmail in your logic
        // ...
    }
}
```

### Using ICurrentDateService in Handlers

```csharp
public class MyCommandHandler : ICommandHandler<MyCommand, MyResponse>
{
    private readonly ICurrentDateService _currentDateService;

    public MyCommandHandler(ICurrentDateService currentDateService)
    {
        _currentDateService = currentDateService;
    }

    public async Task<Result<MyResponse>> Handle(MyCommand command, CancellationToken cancellationToken)
    {
        // Get current UTC date/time (respects override)
        var now = _currentDateService.UtcNow;

        // Use now in your logic
        var task = new Task
        {
            CreatedAt = now,
            DueDate = now.AddDays(7)
        };

        // ...
    }
}
```

### Using in Controllers (BaseController Helpers)

```csharp
public class MyController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateSomething()
    {
        // Use helper methods from BaseController
        var userId = GetCurrentUserId(); // Respects override
        var userEmail = GetCurrentUserEmail(); // Respects override

        if (!userId.HasValue)
        {
            return BadRequest("User not authenticated");
        }

        // Use userId and userEmail
        // ...
    }
}
```

---

## Replacing Existing Code

### Replace HttpContext.User Access

**Before:**
```csharp
var userIdClaim = User.FindFirst("user_id")?.Value;
if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
{
    return BadRequest("User ID not found");
}
```

**After (in Controllers):**
```csharp
var userId = GetCurrentUserId(); // From BaseController
if (!userId.HasValue)
{
    return BadRequest("User ID not found");
}
```

**After (in Handlers):**
```csharp
var userId = _currentUserService.GetUserId();
if (!userId.HasValue)
{
    return Result.Failure(Error.Unauthorized("User not authenticated"));
}
```

### Replace DateTime.UtcNow

**Before:**
```csharp
var now = DateTime.UtcNow;
var task = new Task
{
    CreatedAt = now
};
```

**After:**
```csharp
var now = _currentDateService.UtcNow;
var task = new Task
{
    CreatedAt = now
};
```

### Replace DateTime.Now

**Before:**
```csharp
var localTime = DateTime.Now;
```

**After:**
```csharp
var localTime = _currentDateService.Now;
```

---

## Important Notes

### Environment Restrictions

⚠️ **Testing endpoints are ONLY available in Development and Test environments.**

- In **Production**, all `/api/testing/*` endpoints return `403 Forbidden`
- This prevents accidental override usage in production
- Environment is checked via `ASPNETCORE_ENVIRONMENT` variable

### Override Persistence

- Overrides persist in memory until:
  - Manually removed via DELETE endpoint
  - Application restarts
  - Cache is cleared

- Overrides are **shared across all requests** (not per-request)
- Use with caution in multi-user testing scenarios

### Cache Keys

- User Override: `"CurrentUser_Override"`
- Date Override: `"CurrentDate_Override"`

These are internal implementation details but documented for reference.

---

## Troubleshooting

### Issue: 403 Forbidden on Testing Endpoints

**Cause:** Environment is not Development or Test.

**Solution:**
```bash
# Set environment variable
export ASPNETCORE_ENVIRONMENT=Development

# Or in appsettings.json
{
  "Environment": "Development"
}
```

### Issue: Override Not Working

**Possible Causes:**
1. Override not set (check with GET endpoint)
2. Service not injected (ensure DI registration)
3. Wrong cache key (should be automatic)

**Solution:**
1. Verify override is set: `GET /api/testing/current-user`
2. Check service registration in `DependencyInjection.cs`
3. Ensure `IMemoryCache` is registered

### Issue: Override Affects Other Tests

**Cause:** Overrides are shared across requests.

**Solution:**
- Always remove overrides after tests
- Use test isolation (separate test runs)
- Consider per-test cache keys (future enhancement)

---

## Best Practices

1. **Always Clean Up**: Remove overrides after tests
   ```bash
   # At end of test script
   curl -X DELETE "http://localhost:5000/api/testing/current-user"
   curl -X DELETE "http://localhost:5000/api/testing/current-date"
   ```

2. **Use in Test Setup/Teardown**: Set overrides in test initialization, remove in cleanup

3. **Document Override Usage**: Comment in test code why override is needed

4. **Verify Override**: Check override is set before relying on it

5. **Isolate Tests**: Don't share overrides between unrelated tests

---

## Future Enhancements

Potential improvements:
- Per-request override keys (isolate by request ID)
- Override expiration (auto-clear after X minutes)
- Override history/logging
- Bulk override operations
- Override templates/presets

---

## Related Documentation

- **[Architecture Documentation](ARCHITECTURE.md)** - System architecture
- **[Developer Guide](DEVELOPER_GUIDE.md)** - Adding features
- **[API Reference](API_REFERENCE.md)** - Complete API documentation

---

**Last Updated:** November 18, 2025  
**Feature Status:** ✅ Production Ready (Development/Test Only)  
**Breaking Changes:** None (backward compatible)

