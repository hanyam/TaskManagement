# Technical Decisions Log

This document records significant technical decisions made during the development of the Task Management system, with rationale and alternatives considered.

---

## Table of Contents

1. [Automatic Database Migrations](#automatic-database-migrations)
2. [Backend Proxy for Microsoft Graph API](#backend-proxy-for-microsoft-graph-api)
3. [HATEOAS for Dynamic UI](#hateoas-for-dynamic-ui)
4. [Unified Error Response Format](#unified-error-response-format)
5. [User Search Autocomplete Pattern](#user-search-autocomplete-pattern)

---

## Automatic Database Migrations

**Date:** November 15, 2025  
**Status:** ✅ Implemented  
**Impact:** High

### Decision

Database migrations are now automatically applied on API startup using an extension method that runs during application initialization.

### Context

Previously, migrations had to be manually applied using:
```bash
dotnet ef database update
```

This caused issues:
- Developers forgetting to run migrations after pulling new code
- Docker containers failing when database schema was outdated
- CI/CD pipelines requiring manual migration steps

### Solution

Created `DatabaseExtensions.cs` with `ApplyMigrations()` method:

```csharp
public static WebApplication ApplyMigrations(this WebApplication app)
{
    if (app.Environment.IsEnvironment("Testing"))
        return app;

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
    
    var pendingMigrations = context.Database.GetPendingMigrations().ToList();
    if (pendingMigrations.Any())
    {
        logger.LogInformation("Applying {Count} pending migration(s)", pendingMigrations.Count);
        context.Database.Migrate();
    }
    
    return app;
}
```

Called in `Program.cs`:
```csharp
app.ApplyMigrations();
```

### Alternatives Considered

1. **Manual Migrations**
   - ❌ Prone to human error
   - ❌ Doesn't work well with Docker
   - ❌ Requires documentation and training

2. **Separate Migration Service**
   - ❌ Additional complexity
   - ❌ Another service to deploy and monitor
   - ✅ Better for very large migrations with downtime

3. **Database Initialization Scripts**
   - ❌ Doesn't leverage EF Core migrations
   - ❌ Loses migration history
   - ❌ SQL scripts harder to maintain

### Trade-offs

**Pros:**
- ✅ Zero manual intervention
- ✅ Docker-friendly
- ✅ CI/CD friendly
- ✅ Idempotent (safe to run multiple times)
- ✅ Detailed logging

**Cons:**
- ⚠️ Migrations run synchronously on startup (increases startup time)
- ⚠️ For large migrations, may cause downtime
- ⚠️ All instances apply migrations (use leader election for multi-instance)

### Future Considerations

For production deployments with:
- Very large migrations (>1 minute)
- Blue-green deployments
- High availability requirements

Consider:
- Running migrations manually before deployment
- Using database deployment slots
- Implementing leader election for multi-instance scenarios

### References
- Implementation: `src/TaskManagement.Api/Extensions/DatabaseExtensions.cs`
- Documentation: `docs/SESSION_NOVEMBER_15_2025.md`

---

## Backend Proxy for Microsoft Graph API

**Date:** November 15, 2025  
**Status:** ✅ Implemented  
**Impact:** High

### Decision

Microsoft Graph API calls are proxied through backend endpoints rather than being called directly from the frontend.

### Context

Implementing user search autocomplete required accessing Azure AD user data via Microsoft Graph API. Two approaches were considered:

1. **Direct Frontend Access:** Frontend gets a Graph API token and calls directly
2. **Backend Proxy:** Backend proxies Graph API requests using its own credentials

### Solution

Backend proxy using Client Credentials Flow:

**Backend Controller:**
```csharp
[HttpGet("search")]
public async Task<IActionResult> SearchUsers([FromQuery] string query)
{
    var users = await _graphClient.Users.GetAsync(requestConfiguration =>
    {
        requestConfiguration.QueryParameters.Filter = 
            $"startswith(displayName,'{query}') or startswith(mail,'{query}')";
    });
    
    return Ok(ApiResponse<List<UserSearchResult>>.SuccessResponse(userDtos));
}
```

**Frontend Service:**
```typescript
export async function searchGraphUsers(searchQuery: string): Promise<GraphUser[]> {
  const { data } = await apiClient.request<GraphUser[]>({
    path: "/users/search",  // Backend proxy
    method: "GET",
    query: { query: searchQuery }
  });
  return data;
}
```

### Alternatives Considered

1. **Direct Frontend Access with On-Behalf-Of Flow**
   - ❌ JWT token audience mismatch (API token ≠ Graph token)
   - ❌ Requires additional token exchange logic
   - ❌ Exposes Graph API errors directly to frontend
   - ❌ No server-side caching possible

2. **Separate Graph API Microservice**
   - ❌ Over-engineering for current scale
   - ✅ Would be better for large-scale systems

3. **Azure AD B2C Custom Policies**
   - ❌ Only works with B2C (we use organizational Azure AD)
   - ❌ Very complex setup

### Rationale

**Security:**
- ✅ Client secret stays server-side
- ✅ Backend can implement rate limiting
- ✅ Backend can audit all Graph API calls
- ✅ Centralized error handling

**Maintainability:**
- ✅ Single point of change for Graph API logic
- ✅ Easier to add business rules (filter by department, etc.)
- ✅ Can implement result caching
- ✅ Backend can transform/enrich data

**Token Management:**
- ✅ Avoids JWT audience mismatch issues
- ✅ Backend manages its own token lifecycle
- ✅ No frontend token refresh logic needed

### Trade-offs

**Pros:**
- ✅ More secure (credentials server-side)
- ✅ Better performance (backend caching possible)
- ✅ Simpler frontend code
- ✅ Centralized business logic

**Cons:**
- ⚠️ Additional backend endpoints to maintain
- ⚠️ Extra network hop (frontend → backend → Graph)
- ⚠️ Backend becomes single point of failure

### Configuration Required

Azure AD Application must have:
- **Permission:** `User.Read.All` (Application permission)
- **Admin Consent:** Must be granted by tenant administrator
- **Client Secret:** Valid and not expired

See: `docs/AZURE_AD_USER_SEARCH_SETUP.md`

### References
- Implementation: `src/TaskManagement.Api/Controllers/UsersController.cs`
- Configuration: `src/TaskManagement.Api/DependencyInjection.cs`
- Documentation: `docs/AZURE_AD_USER_SEARCH_SETUP.md`

---

## HATEOAS for Dynamic UI

**Date:** November 14-15, 2025  
**Status:** ✅ Implemented  
**Impact:** Very High

### Decision

The UI dynamically shows/hides action buttons based on HATEOAS links returned by the API, rather than implementing client-side permission logic.

### Context

Task detail page needed to show different actions based on:
- Current task state (Created, Assigned, Accepted, etc.)
- User role (Employee, Manager, Admin)
- User relationship to task (creator, assignee, etc.)
- Business rules (can't accept your own task, etc.)

Traditional approach would be to implement these rules in both backend AND frontend.

### Solution

**Backend generates HATEOAS links:**
```csharp
var links = await _taskActionService.GetAvailableActions(task, userId, userRole);
return HandleResultWithLinks(result, links);
```

**API Response:**
```json
{
  "success": true,
  "data": { /* task data */ },
  "links": [
    { "rel": "self", "href": "/tasks/123", "method": "GET" },
    { "rel": "accept", "href": "/tasks/123/accept", "method": "POST" },
    { "rel": "reject", "href": "/tasks/123/reject", "method": "POST" }
  ]
}
```

**Frontend conditionally renders:**
```typescript
const hasLink = (rel: string) => links.some(link => link.rel === rel);

{hasLink("accept") && (
  <Button onClick={handleAccept}>Accept</Button>
)}

{hasLink("reject") && (
  <Button onClick={handleReject}>Reject</Button>
)}
```

### Alternatives Considered

1. **Duplicate Permission Logic in Frontend**
   ```typescript
   // ❌ BAD: Business logic in frontend
   const canAccept = task.status === 'Assigned' && 
                     task.assignedUserId === currentUserId &&
                     userRole === 'Employee';
   ```
   - ❌ Business rules duplicated
   - ❌ Frontend/backend can get out of sync
   - ❌ Changes require updates in both places

2. **Role-Based UI Only**
   ```typescript
   // ❌ INCOMPLETE: Doesn't account for task state
   {userRole === 'Manager' && <ReviewButton />}
   ```
   - ❌ Ignores task state
   - ❌ Ignores user relationship to task
   - ❌ Too coarse-grained

3. **Separate "Permissions" Endpoint**
   ```typescript
   // ❌ INEFFICIENT: Extra API call
   const { canAccept, canReject } = await fetchPermissions(taskId);
   ```
   - ❌ Additional API request
   - ❌ Permissions could change between calls
   - ❌ More complex caching

### Rationale

**Single Source of Truth:**
- ✅ Business rules only in backend
- ✅ Frontend automatically adapts
- ✅ Impossible for frontend/backend to be inconsistent

**Maintainability:**
- ✅ Add new actions by adding link relations
- ✅ Change permissions by updating backend only
- ✅ No frontend changes needed for permission changes

**RESTful Design:**
- ✅ Follows HATEOAS principles (REST Level 3)
- ✅ Self-documenting API
- ✅ Clients discover available actions

**Security:**
- ✅ Backend always validates permissions
- ✅ Hiding buttons is UX, not security
- ✅ Backend rejects unauthorized actions regardless

### Trade-offs

**Pros:**
- ✅ DRY (Don't Repeat Yourself)
- ✅ Always consistent
- ✅ Very maintainable
- ✅ Follows REST best practices

**Cons:**
- ⚠️ Slightly larger API responses (links array)
- ⚠️ Frontend developers must understand HATEOAS
- ⚠️ Can't show/hide buttons before API call completes

### Link Relations Used

| Relation | Action | Description |
|----------|--------|-------------|
| `self` | View details | Get task by ID |
| `update` | Edit task | Update task properties |
| `cancel` | Cancel task | Cancel task (terminal) |
| `assign` | Assign task | Assign to user |
| `reassign` | Reassign task | Change assignee |
| `accept` | Accept task | Employee accepts assignment |
| `reject` | Reject task | Employee rejects assignment |
| `update-progress` | Update progress | Report progress |
| `mark-completed` | Mark completed | Employee marks done |
| `review-completed` | Review & rate | Manager reviews completed task |
| `request-extension` | Request extension | Ask for deadline extension |
| `approve-extension` | Approve extension | Manager approves extension |
| `request-more-info` | Request info | Manager requests more info |

### References
- Implementation: `src/TaskManagement.Application/Tasks/Services/TaskActionService.cs`
- Frontend: `web/src/features/tasks/components/TaskDetailsView.tsx`
- Documentation: `docs/HATEOAS.md`, `docs/STATE_MACHINE.md`

---

## Unified Error Response Format

**Date:** November 15, 2025  
**Status:** ✅ Implemented  
**Impact:** Medium

### Decision

All API errors are returned in a consistent `errors[]` array, regardless of whether the source is a single error or multiple validation errors.

### Context

Previously, errors were inconsistent:
- Single errors went to `message` field
- Multiple errors went to `errors[]` array
- Frontend had to check both locations

Example problematic response:
```json
{
  "success": false,
  "message": "An error occurred",  // ← Single error here
  "errors": []                     // ← Empty!
}
```

Frontend couldn't reliably display errors.

### Solution

Modified `BaseController.HandleResult()` to aggregate all errors:

```csharp
protected IActionResult HandleResult<T>(Result<T> result, int successStatusCode = 200)
{
    if (result.IsSuccess) 
        return StatusCode(successStatusCode, ApiResponse<T>.SuccessResponse(result.Value!));

    // Collect ALL errors into single list
    var allErrors = new List<Error>();
    
    if (result.Errors.Any())
        allErrors.AddRange(result.Errors);
    
    if (result.Error != null)
        allErrors.Add(result.Error);

    return allErrors.Any()
        ? BadRequest(ApiResponse<T>.ErrorResponse(allErrors, HttpContext.TraceIdentifier))
        : BadRequest(ApiResponse<T>.ErrorResponse("An error occurred", HttpContext.TraceIdentifier));
}
```

Now ALL responses are consistent:
```json
{
  "success": false,
  "errors": [
    {
      "code": "NOT_FOUND",
      "message": "Task not found",
      "field": null
    }
  ]
}
```

### Alternatives Considered

1. **Keep Dual Format**
   - ❌ Frontend complexity remains
   - ❌ Easy to forget which property to check

2. **Move to `message` Only**
   - ❌ Loses field-specific validation errors
   - ❌ Can't show multiple errors at once

3. **Nested Error Objects**
   ```json
   {
     "error": {
       "type": "validation",
       "details": [...]
     }
   }
   ```
   - ❌ More complex parsing
   - ❌ Harder to type in TypeScript

### Frontend Error Handling

Created reusable error display function:

```typescript
function displayApiError(error: unknown, fallbackMessage: string) {
  if (error && typeof error === "object") {
    const apiError = error as ApiErrorResponse;
    
    // Priority: details > message > rawMessage > fallback
    if (apiError.details?.length > 0) {
      apiError.details.forEach(detail => {
        toast.error(detail.field 
          ? `${detail.field}: ${detail.message}` 
          : detail.message
        );
      });
      return;
    }
    
    if (apiError.message) {
      toast.error(apiError.message);
      return;
    }
  }
  
  toast.error(fallbackMessage);
}
```

### Trade-offs

**Pros:**
- ✅ Single location for all errors
- ✅ Consistent frontend parsing
- ✅ Supports field-specific errors
- ✅ Supports multiple errors

**Cons:**
- ⚠️ Single errors become single-item arrays (minor verbosity)

### References
- Implementation: `src/TaskManagement.Api/Controllers/BaseController.cs`
- Frontend: `web/src/features/tasks/components/TaskDetailsView.tsx`
- Documentation: `docs/SESSION_NOVEMBER_15_2025.md`

---

## User Search Autocomplete Pattern

**Date:** November 15, 2025  
**Status:** ✅ Implemented  
**Impact:** Medium-High

### Decision

User selection uses an autocomplete component with real-time search, rather than a dropdown or manual GUID entry.

### Context

Previously, creating a task required entering a user's GUID manually:
```typescript
<Input name="assignedUserId" placeholder="Enter user GUID..." />
```

This was:
- ❌ Not user-friendly
- ❌ Prone to errors
- ❌ Required copying GUIDs from Azure AD portal

### Solution

Created `UserSearchInput` component with:

**Features:**
- Real-time search as you type (300ms debounce)
- Keyboard navigation (arrow keys, enter, escape)
- Loading indicator during search
- Selected user display with clear button
- Click outside to close
- Error state support

**Usage:**
```typescript
<Controller
  name="assignedUserId"
  control={form.control}
  render={({ field }) => (
    <UserSearchInput
      value={field.value}
      onChange={field.onChange}
      placeholder="Type name or email to search..."
    />
  )}
/>
```

### Alternatives Considered

1. **Simple Dropdown with All Users**
   ```typescript
   <Select>
     {allUsers.map(user => <option value={user.id}>{user.name}</option>)}
   </Select>
   ```
   - ❌ Doesn't scale (100+ users = slow)
   - ❌ Still requires loading all users upfront
   - ✅ Simple implementation

2. **Modal with Search**
   ```typescript
   <Button onClick={() => setSearchModalOpen(true)}>Select User</Button>
   <SearchModal onSelect={handleSelect} />
   ```
   - ⚠️ Extra clicks (open modal, search, select, close)
   - ⚠️ Interrupts form flow
   - ✅ More screen space for results

3. **Manual GUID Entry with Validation**
   ```typescript
   <Input type="text" validate={isValidGuid} />
   ```
   - ❌ Still requires copying GUID
   - ❌ No user-friendly names
   - ❌ Error-prone

4. **Recent/Favorite Users List**
   - ✅ Great for power users
   - ⚠️ Still needs search for first-time users
   - 💡 Good future enhancement

### Implementation Details

**Debouncing:**
- 300ms delay prevents excessive API calls
- Uses `@uidotdev/usehooks` for clean implementation

**Keyboard Accessibility:**
- Arrow Up/Down: Navigate results
- Enter: Select highlighted result
- Escape: Close dropdown

**Search Algorithm:**
- Backend filters by `displayName`, `mail`, or `userPrincipalName`
- Returns top 10 results ordered by display name
- Uses Graph API `startswith` filter (fast index scan)

### Trade-offs

**Pros:**
- ✅ Excellent UX (search as you type)
- ✅ Scales to large organizations
- ✅ Shows user context (name, email, job title)
- ✅ Keyboard accessible
- ✅ Mobile-friendly

**Cons:**
- ⚠️ More complex than simple dropdown
- ⚠️ Requires backend API
- ⚠️ Slight delay from debouncing

### Future Enhancements

1. **Result Caching**
   - Cache search results in frontend (TanStack Query)
   - Cache in backend (Redis) for 5-10 minutes

2. **Profile Pictures**
   - Fetch user photos from Graph API
   - Display avatar in autocomplete

3. **Recent Users**
   - Store recently assigned users in localStorage
   - Show as quick selection above search results

4. **Department Filtering**
   - Add department dropdown
   - Filter search results by department

### References
- Implementation: `web/src/features/tasks/components/UserSearchInput.tsx`
- Integration: `web/src/features/tasks/components/TaskCreateView.tsx`
- Service: `web/src/core/services/graph-api.ts`

---

## Decision Template

Use this template for future technical decisions:

```markdown
## [Decision Title]

**Date:** YYYY-MM-DD  
**Status:** 🚧 Proposed | ✅ Implemented | ❌ Rejected | 🔄 Superseded  
**Impact:** Low | Medium | High | Very High

### Decision
Brief statement of what was decided.

### Context
Background information, problem statement, constraints.

### Solution
Description of implemented solution with code examples.

### Alternatives Considered
1. **Alternative 1**
   - ❌ Why rejected
   - ✅ Any pros

2. **Alternative 2**
   - Evaluation

### Rationale
Why this solution was chosen. Key benefits.

### Trade-offs
**Pros:**
- ✅ List advantages

**Cons:**
- ⚠️ List disadvantages

### References
- Implementation files
- Related documentation
```

---

**Last Updated:** November 15, 2025  
**Total Decisions:** 5  
**Active Decisions:** 5  
**Superseded Decisions:** 0

