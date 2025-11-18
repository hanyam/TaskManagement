# Current User and Date Services - Implementation Summary

**Date:** November 18, 2025  
**Status:** ✅ Implemented  
**Purpose:** QA and Testing Override Mechanism

---

## Overview

This document describes the implementation of `ICurrentUserService` and `ICurrentDateService` with override support via `IMemoryCache` for testing purposes.

---

## Architecture

### Services Created

1. **`ICurrentUserService`** (`src/TaskManagement.Application/Common/Interfaces/ICurrentUserService.cs`)
   - Interface for accessing current user information
   - Methods: `GetUserId()`, `GetUserEmail()`, `GetUserPrincipal()`, `GetClaimValue()`, `IsAuthenticated()`

2. **`CurrentUserService`** (`src/TaskManagement.Application/Common/Services/CurrentUserService.cs`)
   - Default implementation using `HttpContext.User`
   - Override support via `IMemoryCache` (key: `"CurrentUser_Override"`)
   - Falls back to HttpContext if no override is set

3. **`ICurrentDateService`** (`src/TaskManagement.Application/Common/Interfaces/ICurrentDateService.cs`)
   - Interface for accessing current date/time
   - Properties: `UtcNow`, `Now`

4. **`CurrentDateService`** (`src/TaskManagement.Application/Common/Services/CurrentDateService.cs`)
   - Default implementation using `DateTime.UtcNow`/`DateTime.Now`
   - Override support via `IMemoryCache` (key: `"CurrentDate_Override"`)
   - Falls back to actual current time if no override is set

### Testing Controller

**`TestingController`** (`src/TaskManagement.Api/Controllers/TestingController.cs`)
- Endpoints for setting/getting/removing overrides
- Only available in Development/Test environments
- Returns 403 Forbidden in Production

**Endpoints:**
- `POST /api/testing/current-user` - Set user override
- `GET /api/testing/current-user` - Get user override
- `DELETE /api/testing/current-user` - Remove user override
- `POST /api/testing/current-date` - Set date override
- `GET /api/testing/current-date` - Get date override
- `DELETE /api/testing/current-date` - Remove date override

### BaseController Updates

**`BaseController`** (`src/TaskManagement.Api/Controllers/BaseController.cs`)
- Added `ICurrentUserService` injection (optional for backward compatibility)
- Added helper methods:
  - `GetCurrentUserId()` - Gets current user ID (respects override)
  - `GetCurrentUserEmail()` - Gets current user email (respects override)
- Falls back to `HttpContext.User` if service not injected

---

## Dependency Injection Registration

**File:** `src/TaskManagement.Application/DependencyInjection.cs`

```csharp
// Register current user and date services (with override support for testing)
services.AddHttpContextAccessor(); // Required for CurrentUserService
services.AddMemoryCache(); // Required for override mechanism
services.AddScoped<ICurrentUserService, CurrentUserService>();
services.AddScoped<ICurrentDateService, CurrentDateService>();
```

---

## Usage Examples

### In Controllers (Using BaseController Helpers)

```csharp
public class MyController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        // Get current user ID (respects override)
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return BadRequest("User not authenticated");
        }

        // Get current user email (respects override)
        var userEmail = GetCurrentUserEmail() ?? "system";

        var command = new CreateTaskCommand
        {
            CreatedById = userId.Value,
            CreatedBy = userEmail
        };

        // ...
    }
}
```

### In Handlers (Injecting Services)

```csharp
public class MyCommandHandler : ICommandHandler<MyCommand, MyResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentDateService _currentDateService;

    public MyCommandHandler(
        ICurrentUserService currentUserService,
        ICurrentDateService currentDateService)
    {
        _currentUserService = currentUserService;
        _currentDateService = currentDateService;
    }

    public async Task<Result<MyResponse>> Handle(MyCommand command, CancellationToken cancellationToken)
    {
        // Get current user (respects override)
        var userId = _currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Result<MyResponse>.Failure(Error.Unauthorized("User not authenticated"));
        }

        // Get current date/time (respects override)
        var now = _currentDateService.UtcNow;

        var entity = new MyEntity
        {
            CreatedById = userId.Value,
            CreatedAt = now
        };

        // ...
    }
}
```

---

## Migration Guide

### Step 1: Replace HttpContext.User in Controllers

**Before:**
```csharp
var userIdClaim = User.FindFirst("user_id")?.Value;
if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
{
    return BadRequest("User ID not found");
}
```

**After:**
```csharp
var userId = GetCurrentUserId(); // From BaseController
if (!userId.HasValue)
{
    return BadRequest("User ID not found");
}
```

### Step 2: Replace DateTime.UtcNow in Handlers

**Before:**
```csharp
var now = DateTime.UtcNow;
var entity = new MyEntity { CreatedAt = now };
```

**After:**
```csharp
// Inject ICurrentDateService in constructor
private readonly ICurrentDateService _currentDateService;

// Use in handler
var now = _currentDateService.UtcNow;
var entity = new MyEntity { CreatedAt = now };
```

### Step 3: Update Validators (if needed)

**Before:**
```csharp
RuleFor(x => x.DueDate)
    .GreaterThan(DateTime.UtcNow)
    .WithMessage("Due date must be in the future");
```

**After:**
```csharp
// Validators can still use DateTime.UtcNow for validation rules
// Override mechanism is primarily for business logic, not validation
// If needed, inject ICurrentDateService into validator
```

---

## Known Limitations

### BaseEntity Constructor

**Issue:** `BaseEntity` constructor uses `DateTime.UtcNow` directly:

```csharp
protected BaseEntity()
{
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow; // ← Cannot be overridden
}
```

**Reason:** `BaseEntity` is in Domain layer and cannot depend on Application layer services.

**Workaround Options:**
1. **Accept Date Parameter** (Breaking Change):
   ```csharp
   protected BaseEntity(DateTime createdAt)
   {
       CreatedAt = createdAt;
   }
   ```

2. **Factory Pattern** (Complex):
   ```csharp
   public static Task Create(ICurrentDateService dateService, ...)
   {
       return new Task(dateService.UtcNow, ...);
   }
   ```

3. **Post-Construction Update** (Current Approach):
   ```csharp
   var task = new Task(...);
   // CreatedAt is set to actual DateTime.UtcNow
   // Can be updated if needed via SetUpdatedBy (uses DateTime.UtcNow)
   ```

**Recommendation:** For now, `BaseEntity` continues using `DateTime.UtcNow`. Override mechanism works for:
- Handler logic that uses `ICurrentDateService`
- `SetUpdatedBy()` method (if updated to use `ICurrentDateService`)
- New entities created with explicit date parameters

**Future Enhancement:** Consider refactoring `BaseEntity` to accept date in constructor or use a factory pattern.

---

## Files Created/Modified

### New Files (6)
1. `src/TaskManagement.Application/Common/Interfaces/ICurrentUserService.cs`
2. `src/TaskManagement.Application/Common/Services/CurrentUserService.cs`
3. `src/TaskManagement.Application/Common/Interfaces/ICurrentDateService.cs`
4. `src/TaskManagement.Application/Common/Services/CurrentDateService.cs`
5. `src/TaskManagement.Api/Controllers/TestingController.cs`
6. `docs/TESTING_OVERRIDES.md`

### Modified Files (4)
1. `src/TaskManagement.Application/DependencyInjection.cs` - Service registration
2. `src/TaskManagement.Api/Controllers/BaseController.cs` - Added helper methods
3. `src/TaskManagement.Api/Controllers/TasksController.cs` - Updated to use helpers (partial)
4. `src/TaskManagement.Api/Controllers/DashboardController.cs` - Updated to use helpers

### Documentation (2)
1. `docs/TESTING_OVERRIDES.md` - Comprehensive usage guide
2. `docs/CURRENT_USER_DATE_SERVICES.md` - This file

---

## Testing Checklist

- [x] Services compile without errors
- [x] DI registration works
- [x] Testing endpoints return 403 in Production
- [x] Testing endpoints work in Development
- [x] User override works in controllers
- [x] Date override works in handlers
- [x] Overrides persist until removed
- [x] Overrides cleared on app restart
- [ ] Integration tests with overrides (recommended)
- [ ] Update remaining `User.FindFirst` usages in TasksController
- [ ] Update `DateTime.UtcNow` usages in handlers/services

---

## Next Steps

1. **Update Remaining Controllers**: Replace all `User.FindFirst` calls with `GetCurrentUserId()`/`GetCurrentUserEmail()`
2. **Update Handlers**: Inject `ICurrentDateService` where `DateTime.UtcNow` is used
3. **Update Services**: Inject `ICurrentDateService` in `ReminderCalculationService`, etc.
4. **Write Integration Tests**: Test override mechanism end-to-end
5. **Consider BaseEntity Refactoring**: Evaluate factory pattern for date injection

---

## Related Documentation

- **[Testing Overrides Guide](TESTING_OVERRIDES.md)** - Complete usage guide
- **[Architecture Documentation](ARCHITECTURE.md)** - System architecture
- **[Developer Guide](DEVELOPER_GUIDE.md)** - Adding features

---

**Last Updated:** November 18, 2025  
**Feature Status:** ✅ Production Ready (Development/Test Only)  
**Breaking Changes:** None (backward compatible)

