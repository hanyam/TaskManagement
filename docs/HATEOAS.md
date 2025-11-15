# HATEOAS Implementation Guide

## Overview

HATEOAS (Hypermedia as the Engine of Application State) is a REST architectural constraint that enables clients to dynamically discover available actions based on the current state of a resource. Our implementation adds `links` to API responses that tell clients which operations they can perform.

## Benefits

1. **Decoupling**: Frontend doesn't need to know business rules about when actions are allowed
2. **Flexibility**: Backend can change permission logic without breaking clients
3. **Self-Documentation**: API responses show what's possible
4. **Better UX**: UI can show/hide buttons based on actual permissions
5. **Reduced Errors**: Clients won't attempt unauthorized operations

## Architecture

### Backend Components

#### 1. ApiResponse Envelope

All API responses use the `ApiResponse<T>` wrapper which includes a `links` property:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<Error> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public List<ApiActionLink>? Links { get; set; }  // HATEOAS links
}
```

#### 2. ApiActionLink Model

```csharp
public class ApiActionLink
{
    public string Rel { get; set; } = string.Empty;    // Relationship type
    public string Href { get; set; } = string.Empty;   // URI for the action
    public string Method { get; set; } = string.Empty; // HTTP method
}
```

#### 3. ITaskActionService

Service that determines available actions based on:
- Task state
- Current user ID
- Current user role

```csharp
public interface ITaskActionService
{
    List<ApiActionLink> GetAvailableActions(Task task, Guid currentUserId, string currentUserRole);
}
```

### Frontend Components

#### 1. ApiEnvelope Type

```typescript
export interface ApiEnvelope<T> {
  success: boolean;
  data?: T;
  message?: string | null;
  errors?: ApiErrorDetail[];
  traceId?: string | null;
  timestamp?: string;
  links?: ApiActionLink[];  // HATEOAS links
}

export interface ApiActionLink {
  rel: string;
  href: string;
  method: string;
}
```

#### 2. Helper Functions

```typescript
// Check if an action link exists
export function hasActionLink(
  links: ApiActionLink[] | undefined, 
  rel: string
): boolean {
  return links?.some(link => link.rel === rel) ?? false;
}

// Get a specific action link
export function getActionLink(
  links: ApiActionLink[] | undefined, 
  rel: string
): ApiActionLink | undefined {
  return links?.find(link => link.rel === rel);
}
```

## Standard Link Relations

### Common Relations

| Relation | Description | HTTP Method |
|----------|-------------|-------------|
| `self` | Link to the resource itself | GET |
| `update` | Update the resource | PUT |
| `delete` | Delete the resource | DELETE |

### Task-Specific Relations

| Relation | Description | HTTP Method | Required Role |
|----------|-------------|-------------|---------------|
| `assign` | Assign task to user(s) | POST | Manager/Admin |
| `accept` | Accept task/progress | POST | Manager/Admin |
| `reject` | Reject task/progress | POST | Manager/Admin |
| `update-progress` | Update task progress | POST | Assigned User |
| `mark-completed` | Mark task as completed | POST | Assigned User |
| `review-completed` | Review completed task with rating | POST | Manager/Admin |
| `reassign` | Reassign task to different user | POST | Manager/Admin |
| `cancel` | Cancel the task | POST | Manager/Admin |

## Implementation Examples

### Backend: Adding Links to Response

In controllers, after getting a task entity, call the service:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetTaskById(Guid id)
{
    // Get task from handler
    var result = await _requestMediator.Send(new GetTaskByIdQuery { Id = id });
    
    if (result.IsSuccess)
    {
        // Get current user context
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        
        // Generate links
        var task = GetTaskEntity(id); // Get the actual entity
        var links = _taskActionService.GetAvailableActions(task, userId, userRole);
        
        // Create response with links
        var response = ApiResponse<TaskDto>.SuccessResponse(result.Value);
        response.Links = links;
        
        return Ok(response);
    }
    
    return HandleResult(result);
}
```

### Backend: TaskActionService Logic

```csharp
public List<ApiActionLink> GetAvailableActions(Task task, Guid currentUserId, string currentUserRole)
{
    var links = new List<ApiActionLink>();
    var isManager = currentUserRole == "Manager" || currentUserRole == "Admin";
    var isAssignedUser = task.AssignedUserId == currentUserId;
    
    // Always include self link
    links.Add(new ApiActionLink
    {
        Rel = "self",
        Href = $"/tasks/{task.Id}",
        Method = "GET"
    });
    
    // State-specific actions
    switch (task.Status)
    {
        case TaskStatus.Assigned:
            if (isAssignedUser)
            {
                links.Add(new ApiActionLink
                {
                    Rel = "mark-completed",
                    Href = $"/tasks/{task.Id}/mark-completed",
                    Method = "POST"
                });
            }
            break;
            
        case TaskStatus.PendingManagerReview:
            if (isManager)
            {
                links.Add(new ApiActionLink
                {
                    Rel = "review-completed",
                    Href = $"/tasks/{task.Id}/review-completed",
                    Method = "POST"
                });
            }
            break;
    }
    
    return links;
}
```

### Frontend: Using Links in React Components

```typescript
interface TaskDetailsViewProps {
  task: TaskDto;
  links?: ApiActionLink[];
}

export function TaskDetailsView({ task, links }: TaskDetailsViewProps) {
  const { t } = useTranslation("tasks");
  
  // Check if actions are available
  const canMarkComplete = hasActionLink(links, "mark-completed");
  const canReview = hasActionLink(links, "review-completed");
  
  return (
    <div>
      <h1>{task.title}</h1>
      
      <div className="actions">
        {/* Only show button if link exists */}
        {canMarkComplete && (
          <Button onClick={handleMarkComplete}>
            {t("actions.markCompleted")}
          </Button>
        )}
        
        {canReview && (
          <Button onClick={handleReview}>
            {t("actions.reviewCompleted")}
          </Button>
        )}
      </div>
    </div>
  );
}
```

### Frontend: Querying with Links

```typescript
export function useTaskQuery(taskId: string) {
  return useQuery({
    queryKey: ["task", taskId],
    queryFn: async () => {
      const response = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}`,
        method: "GET"
      });
      
      // Return both data and links
      return {
        task: response.data,
        links: response.links
      };
    }
  });
}

// Usage in component
function TaskDetails({ taskId }: Props) {
  const { data, isLoading } = useTaskQuery(taskId);
  
  if (isLoading) return <Spinner />;
  
  return <TaskDetailsView task={data.task} links={data.links} />;
}
```

## Best Practices

### Backend

1. **Always include `self` link** - clients need to know the canonical URL
2. **Generate links based on actual permissions** - don't expose actions user can't perform
3. **Use consistent relation names** - follow RFC 5988 and establish project conventions
4. **Include links in ALL responses** - lists, single items, after mutations
5. **Keep link generation centralized** - use services like `ITaskActionService`

### Frontend

1. **Never hardcode permission logic** - always use links to determine button visibility
2. **Cache link patterns** - extract base URL patterns for reuse
3. **Handle missing links gracefully** - links may be absent if no actions available
4. **Use TypeScript types** - define `ApiActionLink` interface consistently
5. **Test with different roles** - verify buttons show/hide correctly

## Testing

### Backend Tests

```csharp
[Fact]
public void GetAvailableActions_WhenTaskIsPendingManagerReview_AndUserIsManager_ShouldIncludeReviewLink()
{
    // Arrange
    var task = new Task(...) { Status = TaskStatus.PendingManagerReview };
    var userId = Guid.NewGuid();
    var userRole = "Manager";
    
    // Act
    var links = _taskActionService.GetAvailableActions(task, userId, userRole);
    
    // Assert
    links.Should().Contain(l => l.Rel == "review-completed");
}

[Fact]
public void GetAvailableActions_WhenTaskIsCompleted_ShouldOnlyIncludeSelfLink()
{
    // Arrange
    var task = new Task(...) { Status = TaskStatus.Completed };
    
    // Act
    var links = _taskActionService.GetAvailableActions(task, Guid.NewGuid(), "Employee");
    
    // Assert
    links.Should().HaveCount(1);
    links.Single().Rel.Should().Be("self");
}
```

### Frontend Tests

```typescript
describe("TaskDetailsView", () => {
  it("should show Mark Complete button when link exists", () => {
    const links = [
      { rel: "self", href: "/tasks/123", method: "GET" },
      { rel: "mark-completed", href: "/tasks/123/mark-completed", method: "POST" }
    ];
    
    render(<TaskDetailsView task={mockTask} links={links} />);
    
    expect(screen.getByText("Mark Completed")).toBeInTheDocument();
  });
  
  it("should hide Mark Complete button when link does not exist", () => {
    const links = [
      { rel: "self", href: "/tasks/123", method: "GET" }
    ];
    
    render(<TaskDetailsView task={mockTask} links={links} />);
    
    expect(screen.queryByText("Mark Completed")).not.toBeInTheDocument();
  });
});
```

## Extending the System

### Adding a New Action

1. **Define the relation name** (e.g., `"archive"`)
2. **Add to TaskActionService**:
   ```csharp
   if (canArchive)
   {
       links.Add(new ApiActionLink
       {
           Rel = "archive",
           Href = $"/tasks/{task.Id}/archive",
           Method = "POST"
       });
   }
   ```
3. **Create backend endpoint** (`POST /tasks/{id}/archive`)
4. **Update frontend**:
   ```typescript
   const canArchive = hasActionLink(links, "archive");
   ```
5. **Add i18n translations** for button labels
6. **Write tests** for new action

## Common Pitfalls

1. **❌ Hardcoding business rules in frontend**
   ```typescript
   // BAD
   const canComplete = task.status === "Assigned" && isAssignedUser;
   ```
   ```typescript
   // GOOD
   const canComplete = hasActionLink(links, "mark-completed");
   ```

2. **❌ Forgetting to pass links to child components**
   ```typescript
   // BAD
   <TaskActions task={task} />
   ```
   ```typescript
   // GOOD
   <TaskActions task={task} links={links} />
   ```

3. **❌ Not handling missing links array**
   ```typescript
   // BAD
   const canAccept = links.some(l => l.rel === "accept");
   ```
   ```typescript
   // GOOD
   const canAccept = hasActionLink(links, "accept");
   ```

## Resources

- [RFC 5988 - Web Linking](https://tools.ietf.org/html/rfc5988)
- [Richardson Maturity Model - Level 3](https://martinfowler.com/articles/richardsonMaturityModel.html#level3)
- [REST API Design - HATEOAS](https://restfulapi.net/hateoas/)

## Conclusion

HATEOAS provides a powerful way to decouple frontend and backend while maintaining security and flexibility. By following these patterns, we ensure that:

- Business rules are centralized in the backend
- Frontend adapts automatically to permission changes
- API consumers discover capabilities dynamically
- System is more maintainable and testable

