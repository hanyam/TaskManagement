# Centralized Constants - Implementation Guide

**Date:** November 18, 2025  
**Status:** ✅ Implemented  
**Purpose:** Eliminate hardcoded strings and magic values across the solution

---

## Overview

All hardcoded strings, cache keys, claim types, role names, and Azure AD claim types have been centralized into constants classes in the Domain layer. This ensures:

- **Single Source of Truth**: Change values in one place
- **Type Safety**: Compile-time checking prevents typos
- **Consistency**: All code uses the same values
- **Maintainability**: Easy to update and refactor

---

## Constants Classes

All constants are located in `src/TaskManagement.Domain/Constants/`:

### 1. CacheKeys.cs

**Purpose:** Memory cache keys for testing overrides

```csharp
public static class CacheKeys
{
    public const string CurrentUserOverride = "CurrentUser_Override";
    public const string CurrentDateOverride = "CurrentDate_Override";
}
```

**Used By:**
- `CurrentUserService` - User override cache key
- `CurrentDateService` - Date override cache key
- `TestingController` - Setting/getting/removing overrides

**Before:**
```csharp
// In CurrentUserService.cs
private const string OverrideCacheKey = "CurrentUser_Override";

// In TestingController.cs
private const string UserOverrideCacheKey = "CurrentUser_Override";
```

**After:**
```csharp
// In CurrentUserService.cs
_memoryCache.TryGetValue(CacheKeys.CurrentUserOverride, ...)

// In TestingController.cs
_memoryCache.Set(CacheKeys.CurrentUserOverride, ...)
```

---

### 2. CustomClaimTypes.cs

**Purpose:** Custom claim type names used in JWT tokens and HttpContext

```csharp
public static class CustomClaimTypes
{
    public const string UserId = "user_id";
    public const string Email = "email";
    public const string Role = "role";
}
```

**Used By:**
- `CurrentUserService` - Extracting user ID and email from claims
- `EnsureUserIdAttribute` - Validating user ID claim
- `BaseController` - Getting user ID and email
- `AuthenticateUserCommandHandler` - Creating JWT claims
- `AuthenticationService` - Mapping role claim to ClaimTypes.Role
- `UsersController` - Getting user email from claims

**Before:**
```csharp
var userIdClaim = User.FindFirst("user_id")?.Value;
var email = User.FindFirst("email")?.Value;
{ "role", user.Role.ToString() }
```

**After:**
```csharp
var userIdClaim = User.FindFirst(CustomClaimTypes.UserId)?.Value;
var email = User.FindFirst(CustomClaimTypes.Email)?.Value;
{ CustomClaimTypes.Role, user.Role.ToString() }
```

---

### 3. RoleNames.cs

**Purpose:** Role names as strings for use in [Authorize] attributes

```csharp
public static class RoleNames
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string Admin = "Admin";
    public const string Default = Employee;
    public const string EmployeeOrManager = "Employee,Manager";
    public const string ManagerOrAdmin = "Manager,Admin";
}
```

**Used By:**
- All controllers with `[Authorize(Roles = "...")]` attributes
- `TaskActionService` - Role-based permission checks
- `TasksController` - Default role fallback

**Before:**
```csharp
[Authorize(Roles = "Manager")]
[Authorize(Roles = "Employee,Manager")]
var isManager = currentUserRole == "Manager" || currentUserRole == "Admin";
string userRole = "Employee";
```

**After:**
```csharp
[Authorize(Roles = RoleNames.Manager)]
[Authorize(Roles = RoleNames.EmployeeOrManager)]
var isManager = currentUserRole == RoleNames.Manager || currentUserRole == RoleNames.Admin;
string userRole = RoleNames.Default;
```

---

### 4. AzureAdClaimTypes.cs

**Purpose:** Azure AD claim type names when extracting claims from Azure AD tokens

```csharp
public static class AzureAdClaimTypes
{
    public const string ObjectId = "oid";
    public const string GivenName = "given_name";
    public const string FamilyName = "family_name";
}
```

**Used By:**
- `AuthenticateUserCommandHandler` - Extracting user info from Azure AD tokens

**Before:**
```csharp
var firstName = claimsPrincipal.FindFirst("given_name")?.Value;
var lastName = claimsPrincipal.FindFirst("family_name")?.Value;
var objectId = claimsPrincipal.FindFirst("oid")?.Value;
```

**After:**
```csharp
var firstName = claimsPrincipal.FindFirst(AzureAdClaimTypes.GivenName)?.Value;
var lastName = claimsPrincipal.FindFirst(AzureAdClaimTypes.FamilyName)?.Value;
var objectId = claimsPrincipal.FindFirst(AzureAdClaimTypes.ObjectId)?.Value;
```

---

## Why Domain Layer?

Constants are placed in the **Domain layer** (`TaskManagement.Domain/Constants/`) because:

1. **Both Application and Infrastructure** need access to these constants
2. **Domain layer** is the lowest layer that both depend on
3. **No circular dependencies** - Domain doesn't depend on Application or Infrastructure
4. **Follows Clean Architecture** - Shared constants belong in the core layer

---

## Files Created

### Domain Layer (3 files)
1. `src/TaskManagement.Domain/Constants/CacheKeys.cs`
2. `src/TaskManagement.Domain/Constants/CustomClaimTypes.cs`
3. `src/TaskManagement.Domain/Constants/RoleNames.cs`
4. `src/TaskManagement.Domain/Constants/AzureAdClaimTypes.cs`

### Application Layer (1 file)
1. `src/TaskManagement.Application/Common/Constants/CacheKeys.cs` (kept for backward compatibility)

---

## Files Modified

### Updated to Use Constants (15 files)

1. **Application Layer:**
   - `CurrentUserService.cs` - Uses `CacheKeys` and `CustomClaimTypes`
   - `CurrentDateService.cs` - Uses `CacheKeys`
   - `AuthenticateUserCommandHandler.cs` - Uses `CustomClaimTypes` and `AzureAdClaimTypes`
   - `TaskActionService.cs` - Uses `RoleNames`

2. **Infrastructure Layer:**
   - `AuthenticationService.cs` - Uses `CustomClaimTypes`

3. **API Layer:**
   - `EnsureUserIdAttribute.cs` - Uses `CustomClaimTypes`
   - `BaseController.cs` - Uses `CustomClaimTypes`
   - `TasksController.cs` - Uses `RoleNames` and `CustomClaimTypes`
   - `DashboardController.cs` - Uses `RoleNames` (via EnsureUserIdAttribute)
   - `DatabaseController.cs` - Uses `RoleNames`
   - `UsersController.cs` - Uses `CustomClaimTypes`
   - `TestingController.cs` - Uses `CacheKeys`

---

## Usage Examples

### Using Cache Keys

```csharp
using TaskManagement.Application.Common.Constants;

// Set override
_memoryCache.Set(CacheKeys.CurrentUserOverride, overrideValue);

// Get override
_memoryCache.TryGetValue(CacheKeys.CurrentDateOverride, out DateTime? date);
```

### Using Claim Types

```csharp
using TaskManagement.Domain.Constants;
using static TaskManagement.Domain.Constants.CustomClaimTypes;

// Extract claim
var userId = User.FindFirst(UserId)?.Value;
var email = User.FindFirst(Email)?.Value;

// Create claim
new Claim(UserId, userId.ToString())
```

### Using Role Names

```csharp
using TaskManagement.Domain.Constants;
using static TaskManagement.Domain.Constants.RoleNames;

// In [Authorize] attribute
[Authorize(Roles = Manager)]
[Authorize(Roles = EmployeeOrManager)]

// In code
if (userRole == Admin) { ... }
var defaultRole = Default;
```

### Using Azure AD Claim Types

```csharp
using TaskManagement.Domain.Constants;
using static TaskManagement.Domain.Constants.AzureAdClaimTypes;

// Extract from Azure AD token
var objectId = claimsPrincipal.FindFirst(ObjectId)?.Value;
var firstName = claimsPrincipal.FindFirst(GivenName)?.Value;
```

---

## Migration Summary

### Strings Centralized

| Category | Before | After | Files Updated |
|----------|--------|-------|---------------|
| Cache Keys | `"CurrentUser_Override"` (3 places) | `CacheKeys.CurrentUserOverride` | 3 |
| Cache Keys | `"CurrentDate_Override"` (3 places) | `CacheKeys.CurrentDateOverride` | 3 |
| Claim Types | `"user_id"` (5 places) | `CustomClaimTypes.UserId` | 5 |
| Claim Types | `"email"` (4 places) | `CustomClaimTypes.Email` | 4 |
| Claim Types | `"role"` (2 places) | `CustomClaimTypes.Role` | 2 |
| Role Names | `"Manager"` (6 places) | `RoleNames.Manager` | 6 |
| Role Names | `"Employee,Manager"` (5 places) | `RoleNames.EmployeeOrManager` | 5 |
| Role Names | `"Employee"` (3 places) | `RoleNames.Default` | 3 |
| Azure AD Claims | `"oid"`, `"given_name"`, `"family_name"` | `AzureAdClaimTypes.*` | 1 |

**Total:** ~30+ hardcoded strings centralized

---

## Benefits

### ✅ Single Source of Truth
- Change claim type name in one place
- All code automatically uses new value
- No risk of inconsistencies

### ✅ Type Safety
- Compile-time checking prevents typos
- IntelliSense autocomplete support
- Refactoring tools work correctly

### ✅ Maintainability
- Easy to find all usages (via IDE "Find References")
- Clear intent (constants vs magic strings)
- Self-documenting code

### ✅ Consistency
- Same values used everywhere
- No duplicate definitions
- Centralized documentation

---

## Best Practices

1. **Always use constants** instead of hardcoded strings
2. **Add new constants** to appropriate class in `Domain/Constants/`
3. **Use `using static`** for cleaner code when using multiple constants
4. **Document constants** with XML comments
5. **Group related constants** in same class

---

## Adding New Constants

### Example: Adding a New Cache Key

**Step 1:** Add to `CacheKeys.cs`:
```csharp
public static class CacheKeys
{
    // ... existing keys ...
    public const string MyNewCacheKey = "MyNewCache_Key";
}
```

**Step 2:** Use in code:
```csharp
using TaskManagement.Application.Common.Constants;

_memoryCache.Set(CacheKeys.MyNewCacheKey, value);
```

### Example: Adding a New Role

**Step 1:** Add to `RoleNames.cs`:
```csharp
public static class RoleNames
{
    // ... existing roles ...
    public const string SuperAdmin = "SuperAdmin";
}
```

**Step 2:** Use in [Authorize] attribute:
```csharp
using static TaskManagement.Domain.Constants.RoleNames;

[Authorize(Roles = SuperAdmin)]
```

---

## Related Documentation

- **[Testing Overrides Guide](TESTING_OVERRIDES.md)** - Cache keys usage
- **[EnsureUserIdAttribute Guide](ENSURE_USER_ID_ATTRIBUTE.md)** - Claim types usage
- **[Current User/Date Services](CURRENT_USER_DATE_SERVICES.md)** - Service implementation

---

**Last Updated:** November 18, 2025  
**Feature Status:** ✅ Production Ready  
**Breaking Changes:** None (backward compatible)

