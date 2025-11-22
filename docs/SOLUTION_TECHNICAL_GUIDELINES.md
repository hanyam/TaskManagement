# Task Management Solution - Technical Guidelines

**Version:** 1.0  
**Last Updated:** November 15, 2025

## Table of Contents

1. [Overview](#overview)
2. [Backend Architecture](#backend-architecture)
3. [Frontend Architecture](#frontend-architecture)
4. [Code Style & Conventions](#code-style--conventions)
5. [Design Patterns](#design-patterns)
6. [Testing Standards](#testing-standards)
7. [API Design](#api-design)
8. [Database Patterns](#database-patterns)
9. [Error Handling](#error-handling)
10. [Security & Authentication](#security--authentication)
11. [Internationalization](#internationalization)
12. [Development Workflow](#development-workflow)

---

## Overview

This solution combines a .NET 8 Task Management API with a Next.js 14 frontend, implementing enterprise-grade patterns and practices.

### Technology Stack

**Backend:**
- .NET 8.0
- Entity Framework Core (for writes)
- Dapper (for reads)
- Custom CQRS Mediator
- FluentValidation
- Serilog
- xUnit + FluentAssertions
- Azure AD Authentication

**Frontend:**
- Next.js 14 (App Router)
- TypeScript (strict mode)
- TanStack Query (server state)
- React Hook Form + Zod (forms)
- Tailwind CSS
- react-i18next (i18n with Arabic/English + RTL)
- Sonner (toast notifications)

---

## Backend Architecture

### Vertical Slice Architecture

Features are organized as **vertical slices** by business capability, not by technical layers.

**Structure:**
```
Application/
└── Tasks/
    ├── Commands/
    │   └── CreateTask/
    │       ├── CreateTaskCommand.cs
    │       ├── CreateTaskCommandHandler.cs
    │       └── CreateTaskCommandValidator.cs
    └── Queries/
        └── GetTaskById/
            ├── GetTaskByIdQuery.cs
            └── GetTaskByIdQueryHandler.cs
```

**Benefits:**
- Feature isolation
- Easy navigation
- Independent development
- Simplified testing

### Clean Architecture Layers

#### 1. Domain Layer (`TaskManagement.Domain`)
- **Entities**: Immutable domain models with private setters
- **DTOs**: Data transfer objects
- **Interfaces**: Domain contracts (IRepository, IUnitOfWork, IQueryRepository, ICommandRepository, etc.)
- **Common Patterns**: Result, Error, BaseEntity
- **Errors**: Centralized static error definitions
- **Constants**: RoleNames, CustomClaimTypes, AzureAdClaimTypes
- **NO dependencies** on other layers

**Example Entity:**
```csharp
public class Task : BaseEntity
{
    public string Title { get; private set; }
    
    public Task(string title, ...)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty");
        Title = title;
    }
    
    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty");
        Title = title;
    }
}
```

#### 2. Application Layer (`TaskManagement.Application`)
- **Feature Slices**: Commands/Queries organized by feature
- **Handlers**: Business logic orchestration
- **Validators**: FluentValidation validators
- **Behaviors**: Pipeline behaviors (Validation, Logging, Exception Handling)
- **Services**: Application-level services (ICurrentUserService, ICurrentDateService)
- **Depends ONLY** on Domain layer

#### 3. Infrastructure Layer (`TaskManagement.Infrastructure`)
- **Data Access**: EF Core DbContext, Repository implementations (Dapper and EF Core), UnitOfWork
- **External Services**: Authentication (Azure AD), Email, etc.
- **Repository Implementations**: 
  - `TaskDapperRepository`, `UserDapperRepository` (Dapper-based query repositories)
  - `TaskEfCommandRepository`, `UserEfCommandRepository` (EF Core-based command repositories)
  - `DapperQueryRepository<T>`, `EfCommandRepository<T>` (base repository classes)
- **Depends** on Domain layer (for interfaces) and Application layer (for DbContext usage)

#### 4. API Layer (`TaskManagement.Api`)
- **Controllers**: Thin controllers delegating to mediator
- **Middleware**: Exception handling, logging
- **Configuration**: Program.cs, DependencyInjection.cs
- **Depends** on Application and Infrastructure layers

### CQRS Pattern

**Commands** (Write Operations):
- Use `ICommand<TResponse>` or `ICommand`
- Return `Result<T>` or `Result`
- Use **EF Core** repositories (change tracking, transactions)

**Queries** (Read Operations):
- Use `IQuery<TResponse>`
- Return `Result<T>`
- Use **Dapper** repositories (performance, raw SQL)

**Example Command Handler:**
```csharp
public class CreateTaskCommandHandler(
    TaskEfCommandRepository taskRepository,
    TaskManagementDbContext context) 
    : ICommandHandler<CreateTaskCommand, TaskDto>
{
    public async Task<Result<TaskDto>> Handle(
        CreateTaskCommand request, 
        CancellationToken cancellationToken)
    {
        var task = new Task(request.Title, ...);
        await taskRepository.AddAsync(task, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return Result<TaskDto>.Success(taskDto);
    }
}
```

**Example Query Handler:**
```csharp
public class GetTaskByIdQueryHandler(
    TaskDapperRepository taskRepository) 
    : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    public async Task<Result<TaskDto>> Handle(
        GetTaskByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetTaskWithUserAsync(
            request.Id, 
            cancellationToken);
        
        if (task == null)
            return Result<TaskDto>.Failure(TaskErrors.NotFound);
        
        return Result<TaskDto>.Success(taskDto);
    }
}
```

### Custom Mediator Pattern

**Pipeline Execution Order:**
1. Exception Handling (outermost)
2. Validation (middle)
3. Logging (innermost)
4. Handler

**Service Registration:**
- Handlers registered via reflection in `DependencyInjection.cs`
- Command handlers registered for **both** `ICommandHandler` and `IRequestHandler`
- Query handlers registered for `IRequestHandler` only

---

## Frontend Architecture

### Feature-Based Organization

```
src/
├── features/
│   └── tasks/
│       ├── api/          # TanStack Query hooks
│       ├── components/   # Feature-specific components
│       ├── hooks/        # Custom hooks
│       ├── types.ts      # TypeScript types
│       └── i18n/         # i18n resources (if feature-specific)
├── core/
│   ├── api/              # API client
│   ├── auth/             # Authentication
│   ├── providers/        # React providers
│   └── hooks/            # Shared hooks
├── ui/
│   └── components/       # Reusable UI primitives
└── i18n/                 # Global i18n configuration
```

### Server State Management (TanStack Query)

**Query Keys:**
```typescript
// web/src/features/tasks/api/taskKeys.ts
export const taskKeys = {
  all: ["tasks"] as const,
  lists: () => [...taskKeys.all, "list"] as const,
  list: (filters: TaskFilters) => [...taskKeys.lists(), filters] as const,
  details: () => [...taskKeys.all, "detail"] as const,
  detail: (id: string) => [...taskKeys.details(), id] as const,
};
```

**Query Hook:**
```typescript
export function useTaskDetailsQuery(taskId: string, enabled = true) {
  const locale = useCurrentLocale();
  return useQuery({
    queryKey: taskKeys.detail(taskId),
    enabled,
    queryFn: async ({ signal }) => {
      const response = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}`,
        method: "GET",
        signal,
        locale
      });
      return response; // Includes data and links (HATEOAS)
    }
  });
}
```

**Mutation Hook:**
```typescript
export function useCreateTaskMutation() {
  const queryClient = useQueryClient();
  const locale = useCurrentLocale();
  
  return useMutation({
    mutationFn: async (command: CreateTaskCommand) => {
      return apiClient.request<TaskDto>({
        path: "/tasks",
        method: "POST",
        body: command,
        locale
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
    }
  });
}
```

### Form Management (React Hook Form + Zod)

**Schema Definition:**
```typescript
import { z } from "zod";

export const createTaskSchema = z.object({
  title: z.string().min(1, "Title is required"),
  description: z.string().optional(),
  assignedUserId: z.string().uuid("Invalid user ID"),
  dueDate: z.date(),
  priority: z.nativeEnum(TaskPriority),
  type: z.nativeEnum(TaskType),
});
```

**Form Component:**
```typescript
export function TaskCreateView() {
  const form = useForm<CreateTaskFormData>({
    resolver: zodResolver(createTaskSchema),
  });
  
  const createMutation = useCreateTaskMutation();
  
  const onSubmit = async (data: CreateTaskFormData) => {
    try {
      await createMutation.mutateAsync(data);
      toast.success(t("tasks:forms.create.success"));
      router.push("/tasks");
    } catch (error) {
      displayApiError(error, t("tasks:forms.create.error"));
    }
  };
  
  return (
    <form onSubmit={form.handleSubmit(onSubmit)}>
      {/* Form fields */}
    </form>
  );
}
```

### HATEOAS Integration

**API Response with Links:**
```typescript
interface ApiSuccessResponse<T> {
  data: T;
  links?: ApiActionLink[];
}

interface ApiActionLink {
  rel: string;
  href: string;
  method: string;
}
```

**Conditional UI Rendering:**
```typescript
const { data: response } = useTaskDetailsQuery(taskId);
const links = response?.links ?? [];

const hasLink = (rel: string) => 
  links.some(link => link.rel === rel);

{hasLink("update") && (
  <Button onClick={handleEditTask}>
    {t("common:actions.edit")}
  </Button>
)}
{hasLink("cancel") && (
  <Button onClick={handleCancelTask}>
    {t("common:actions.cancel")}
  </Button>
)}
```

---

## Code Style & Conventions

### C# Naming Conventions

- **Classes**: PascalCase (`CreateTaskCommand`)
- **Methods**: PascalCase (`Handle`)
- **Properties**: PascalCase (`TaskId`)
- **Private Fields**: camelCase (`_context`)
- **Constants**: PascalCase (`RoleNames.Admin`)

### Primary Constructors (C# 12)

**All classes** (except auto-generated migrations) use primary constructors:

```csharp
// ✅ Correct
public class CreateTaskCommandHandler(
    TaskEfCommandRepository taskRepository,
    TaskManagementDbContext context) 
    : ICommandHandler<CreateTaskCommand, TaskDto>
{
    private readonly TaskEfCommandRepository _taskRepository = taskRepository;
    private readonly TaskManagementDbContext _context = context;
}

// ❌ Avoid (old style)
public class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, TaskDto>
{
    private readonly TaskEfCommandRepository _taskRepository;
    private readonly TaskManagementDbContext _context;
    
    public CreateTaskCommandHandler(
        TaskEfCommandRepository taskRepository,
        TaskManagementDbContext context)
    {
        _taskRepository = taskRepository;
        _context = context;
    }
}
```

**Complex Initialization:**
For classes with complex initialization logic, use static helper methods:

```csharp
public class AuthenticationService(
    IOptions<JwtOptions> jwtOptions,
    IOptions<AzureAdOptions> azureAdOptions,
    ILogger<AuthenticationService> logger) 
    : IAuthenticationService
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration> 
        _configurationManager = InitializeConfigurationManager(
            azureAdOptions.Value, 
            logger);
    
    private static ConfigurationManager<OpenIdConnectConfiguration> 
        InitializeConfigurationManager(
            AzureAdOptions options, 
            ILogger logger)
    {
        // Complex initialization logic
    }
}
```

### TypeScript Naming Conventions

- **Components**: PascalCase (`TaskDetailsView`)
- **Functions**: camelCase (`handleSubmit`)
- **Constants**: UPPER_SNAKE_CASE (`API_BASE_URL`)
- **Types/Interfaces**: PascalCase (`TaskDto`, `ApiResponse`)
- **Files**: kebab-case (`task-details-view.tsx`)

### Import Order (ESLint Enforced)

```typescript
// 1. Built-in modules
import { useState, useEffect } from "react";

// 2. External packages
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";

// 3. Internal imports (@/...)
import { apiClient } from "@/core/api/client.browser";
import { Button } from "@/ui/components/Button";

// 4. Relative imports
import { useTaskDetailsQuery } from "../api/queries";
import { TaskForm } from "./TaskForm";
```

---

## Design Patterns

### Result Pattern

**Purpose**: Standardized API responses, no exceptions for business logic.

**Usage:**
```csharp
// Success
return Result<TaskDto>.Success(taskDto);

// Single Error
return Result<TaskDto>.Failure(TaskErrors.NotFound);

// Multiple Errors (Validation)
var errors = new List<Error> { 
    TaskErrors.TitleRequired, 
    TaskErrors.DueDateInPast 
};
return Result<TaskDto>.Failure(errors);
```

**Controller Mapping:**
```csharp
protected IActionResult HandleResult<T>(Result<T> result, int successStatusCode = 200)
{
    if (result.IsSuccess) 
        return StatusCode(successStatusCode, ApiResponse<T>.SuccessResponse(result.Value!));
    
    // Collect all errors (both Error and Errors collection)
    var allErrors = new List<Error>();
    if (result.Errors.Any()) allErrors.AddRange(result.Errors);
    if (result.Error != null) allErrors.Add(result.Error);
    
    return allErrors.Any()
        ? BadRequest(ApiResponse<T>.ErrorResponse(allErrors, HttpContext.TraceIdentifier))
        : BadRequest(ApiResponse<T>.ErrorResponse("An error occurred", HttpContext.TraceIdentifier));
}
```

### Centralized Constants

**Cache Keys:**
```csharp
// src/TaskManagement.Application/Common/Constants/CacheKeys.cs
public static class CacheKeys
{
    public const string CurrentUserOverride = "CurrentUser_Override";
    public const string CurrentDateOverride = "CurrentDate_Override";
}
```

**Claim Types:**
```csharp
// src/TaskManagement.Domain/Constants/CustomClaimTypes.cs
public static class CustomClaimTypes
{
    public const string UserId = "user_id";
    public const string Email = "email";
    public const string Role = "role";
    public const string DisplayName = "display_name";
}
```

**Role Names:**
```csharp
// src/TaskManagement.Domain/Constants/RoleNames.cs
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
    public const string Default = Employee;
    
    public const string EmployeeOrManager = $"{Employee},{Manager}";
    public const string ManagerOrAdmin = $"{Manager},{Admin}";
}
```

**Usage:**
```csharp
// ✅ Correct
[Authorize(Roles = RoleNames.ManagerOrAdmin)]
public async Task<IActionResult> CreateTask(...)

// ❌ Avoid
[Authorize(Roles = "Manager,Admin")]
```

### Repository Pattern

**Architecture:**
- **Interfaces**: Defined in `TaskManagement.Domain/Interfaces/` (IQueryRepository, ICommandRepository, ITaskEfCommandRepository, IUserEfCommandRepository)
- **Implementations**: Located in `TaskManagement.Infrastructure/Data/Repositories/`
- **Registration**: Repositories registered in `Infrastructure/DependencyInjection.cs`

**EF Core for Commands:**
```csharp
// Location: TaskManagement.Infrastructure/Data/Repositories/TaskEfCommandRepository.cs
public class TaskEfCommandRepository : EfCommandRepository<Task>, ITaskEfCommandRepository
{
    public TaskEfCommandRepository(TaskManagementDbContext context) 
        : base(context) { }
    
    // Add custom command methods here
}
```

**Dapper for Queries:**
```csharp
// Location: TaskManagement.Infrastructure/Data/Repositories/TaskDapperRepository.cs
public class TaskDapperRepository : DapperQueryRepository<Task>
{
    public TaskDapperRepository(IConfiguration configuration) 
        : base(configuration) { }
    
    public async Task<TaskDto?> GetTaskWithUserAsync(
        Guid taskId, 
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT T.*, U.Email as AssignedUserEmail
            FROM [Tasks].[Tasks] T
            LEFT JOIN [Tasks].[Users] U ON T.AssignedUserId = U.Id
            WHERE T.Id = @TaskId";
        
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TaskDto>(
            new CommandDefinition(sql, new { TaskId = taskId }, cancellationToken: cancellationToken));
    }
}
```

**Using Repositories in Handlers:**
```csharp
// Application layer handlers use repositories from Infrastructure
using TaskManagement.Infrastructure.Data.Repositories;

public class GetTaskByIdQueryHandler(TaskDapperRepository taskRepository) 
    : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    // Uses Dapper repository for optimized queries
}
```

### Current User & Date Services

**ICurrentUserService:**
```csharp
public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetUserEmail();
    ClaimsPrincipal? GetUserPrincipal();
    bool IsAuthenticated();
}
```

**ICurrentDateService:**
```csharp
public interface ICurrentDateService
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}
```

**Override Mechanism (Testing):**
```csharp
// Set override via TestingController
POST /api/testing/current-user
{
  "userId": "guid-here",
  "email": "test@example.com"
}

// Use in handlers
public class SomeHandler(ICurrentUserService currentUserService)
{
    public async Task<Result> Handle(...)
    {
        var userId = currentUserService.GetUserId();
        // ...
    }
}
```

### EnsureUserId Attribute

**Purpose**: Encapsulate user ID validation in controllers.

**Usage:**
```csharp
[HttpGet("{id}")]
[EnsureUserId]  // Ensures user ID exists, returns 400 if not
public async Task<IActionResult> GetTaskById(Guid id)
{
    var userId = GetRequiredUserId(); // Guaranteed to exist
    // ...
}
```

**Implementation:**
```csharp
public class EnsureUserIdAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = GetUserIdFromServiceOrContext(context);
        if (!userId.HasValue)
        {
            context.Result = new BadRequestObjectResult(...);
            return;
        }
        
        // Store in HttpContext.Items for easy access
        context.HttpContext.Items["CurrentUserId"] = userId.Value;
    }
}
```

---

## Testing Standards

### Unit Test Structure

**Test Base:**
```csharp
public abstract class InMemoryDatabaseTestBase : IDisposable
{
    protected readonly TaskManagementDbContext Context;
    
    protected InMemoryDatabaseTestBase()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Context = new TaskManagementDbContext(options);
    }
    
    protected Task CreateTestTask(...) { }
    protected User CreateTestUser(...) { }
    protected void SetUserRole(Guid userId, string role) { }
}
```

**Test Example:**
```csharp
public class CreateTaskCommandHandlerTests : InMemoryDatabaseTestBase
{
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateTaskCommand 
        { 
            AssignedUserId = Guid.NewGuid() 
        };
        var handler = new CreateTaskCommandHandler(Context, ...);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.ShouldContainError(TaskErrors.AssignedUserNotFound);
    }
}
```

**Error Assertions:**
```csharp
// Use ErrorAssertionExtensions helper methods
result.ShouldContainError(TaskErrors.NotFound);
result.ShouldContainValidationError("FieldName");
result.GetError(); // Checks both Error and Errors collection

// ❌ Never hardcode error strings
result.Error.Message.Should().Be("Task not found"); // WRONG
```

### Frontend Testing (Vitest + RTL)

**Test Structure:**
```typescript
import { render, screen } from "@testing-library/react";
import { describe, it, expect } from "vitest";

describe("TaskCreateView", () => {
  it("should display form fields", () => {
    render(<TaskCreateView />);
    expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
  });
});
```

---

## API Design

### Response Format

**Success:**
```json
{
  "success": true,
  "data": { ... },
  "message": null,
  "errors": [],
  "timestamp": "2025-11-15T10:00:00Z",
  "traceId": null,
  "links": [
    {
      "rel": "self",
      "href": "/tasks/123",
      "method": "GET"
    },
    {
      "rel": "update",
      "href": "/tasks/123",
      "method": "PUT"
    }
  ]
}
```

**Error:**
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Title is required",
      "field": "title"
    }
  ],
  "timestamp": "2025-11-15T10:00:00Z",
  "traceId": "0HNH42PG4LPUK:00000014",
  "links": null
}
```

### Status Codes

- **200 OK**: Successful operation
- **201 Created**: Resource created
- **400 Bad Request**: Validation errors, bad input
- **401 Unauthorized**: Missing/invalid authentication
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Business rule violation
- **500 Internal Server Error**: Unexpected errors

### Authorization

**Role-Based:**
```csharp
[Authorize(Roles = RoleNames.ManagerOrAdmin)]
public async Task<IActionResult> CreateTask(...)
```

**User Context:**
```csharp
[EnsureUserId]
public async Task<IActionResult> GetTaskById(Guid id)
{
    var userId = GetRequiredUserId(); // Guaranteed to exist
    // ...
}
```

---

## Database Patterns

### Migrations

**Automatic Application:**
```csharp
// Program.cs
app.ApplyMigrations(); // Applies pending migrations on startup
```

**Manual Creation:**
```bash
dotnet ef migrations add MigrationName --project src/TaskManagement.Infrastructure --startup-project src/TaskManagement.Api
```

### Seeding

**SQL Scripts:**
- Location: `TaskManagement/scripts/Seeding/`
- Naming: `01_InitialData.sql`, `02_TestUsers.sql`, etc.
- Execution: Ordered by filename (ascending)

**API Endpoint:**
```http
POST /api/database/seed
Authorization: Bearer <admin-token>
```

**Handler:**
```csharp
public class SeedDatabaseCommandHandler(
    TaskManagementDbContext context,
    ILogger<SeedDatabaseCommandHandler> logger) 
    : ICommandHandler<SeedDatabaseCommand, SeedDatabaseResultDto>
{
    public async Task<Result<SeedDatabaseResultDto>> Handle(...)
    {
        // Discover scripts, execute in order, collect results
    }
}
```

### Transaction Management

**Simple Operations (Implicit Transaction):**
```csharp
await repository.AddAsync(entity, cancellationToken);
await context.SaveChangesAsync(cancellationToken); // Implicit transaction
```

**Complex Operations (Explicit Transaction):**
```csharp
await unitOfWork.BeginTransactionAsync(cancellationToken);
try
{
    await unitOfWork.Tasks.AddAsync(task);
    await context.Set<TaskAssignment>().AddRangeAsync(assignments);
    await unitOfWork.SaveChangesAsync(cancellationToken);
    await unitOfWork.CommitTransactionAsync(cancellationToken);
}
catch
{
    await unitOfWork.RollbackTransactionAsync(cancellationToken);
    throw;
}
```

---

## Error Handling

### Centralized Error Definitions

**Domain Errors:**
```csharp
// src/TaskManagement.Domain/Errors/Tasks/TaskErrors.cs
public static class TaskErrors
{
    public static Error NotFound => 
        Error.NotFound("Task", "Id");
    
    public static Error AssignedUserNotFound => 
        Error.NotFound("Assigned user", "AssignedUserId");
    
    public static Error TitleRequired => 
        Error.Validation("Title is required", "Title");
}
```

**Error Factory Methods:**
```csharp
Error.NotFound(resource, field?)
Error.Validation(message, field?)
Error.Conflict(message, field?)
Error.Forbidden(message)
Error.Internal(message)
```

### Frontend Error Display

**Reusable Error Handler:**
```typescript
function displayApiError(error: unknown, fallbackMessage: string) {
  if (error && typeof error === "object") {
    const apiError = error as ApiErrorResponse;
    
    // Prioritize details array
    if (apiError.details && apiError.details.length > 0) {
      apiError.details.forEach((detail) => {
        const message = detail.field 
          ? `${detail.field}: ${detail.message}` 
          : detail.message;
        toast.error(message);
      });
      return;
    }
    
    // Fallback to top-level message
    if (apiError.message) {
      toast.error(apiError.message);
      return;
    }
  }
  
  toast.error(fallbackMessage);
}
```

**Usage:**
```typescript
try {
  await mutation.mutateAsync(data);
} catch (error) {
  displayApiError(error, t("forms.create.error"));
}
```

---

## Security & Authentication

### Azure AD Integration

**Backend Configuration:**
```json
{
  "AzureAd": {
    "TenantId": "...",
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

**Frontend Configuration:**
```typescript
// Environment variables
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=...
NEXT_PUBLIC_AZURE_AD_TENANT_ID=...
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=...
NEXT_PUBLIC_AZURE_AD_SCOPES=...
```

**Microsoft Graph API Proxy:**
- Backend endpoint: `/api/users/search`
- Uses Client Credentials flow
- Requires `User.Read.All` application permission

### JWT Token Claims

**Custom Claims:**
- `user_id`: User GUID
- `email`: User email
- `role`: User role (Admin, Manager, Employee)
- `display_name`: User display name

**Extraction:**
```csharp
var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;
var email = User.FindFirst(CustomClaimTypes.Email)?.Value;
var role = User.FindFirst(ClaimTypes.Role)?.Value;
```

---

## Internationalization

### Backend

**No i18n** - API returns English error messages. Frontend handles translation.

### Frontend

**Configuration:**
```typescript
// web/src/i18n/config.ts
export const i18nConfig = {
  supportedLocales: ["en", "ar"],
  defaultLocale: "en",
  resources: {
    en: { common: enCommon, tasks: enTasks },
    ar: { common: arCommon, tasks: arTasks },
  },
};
```

**Usage:**
```typescript
const { t } = useTranslation(["tasks", "common"]);

<h1>{t("tasks:title")}</h1>
<Button>{t("common:actions.save")}</Button>
```

**RTL Support:**
- `I18nProvider` sets `<html dir={locale === "ar" ? "rtl" : "ltr"}>`
- Use Tailwind logical utilities (`start`, `end`, `ms`, `me`)
- Select dropdowns automatically flip arrow position (CSS in `globals.css`)
- DatePicker component fully supports RTL with proper calendar positioning

**UI Components:**
- **DatePicker** (`src/ui/components/DatePicker.tsx`): Beautiful calendar component using `react-day-picker` with RTL and i18n support. Use with React Hook Form `Controller` for form integration.
- **LocaleSwitcher** (`src/core/routing/LocaleSwitcher.tsx`): Button component showing opposite language indicator ("ع" for English, "en" for Arabic) for quick locale switching.
- All select dropdowns have automatic RTL arrow positioning via CSS.

---

## Development Workflow

### Adding a New Feature

1. **Domain Layer:**
   - Create/update entities
   - Define DTOs
   - Add error definitions

2. **Application Layer:**
   - Create command/query
   - Create handler
   - Create validator

3. **Infrastructure Layer:**
   - Update DbContext if needed
   - Create migration if schema changed

4. **API Layer:**
   - Add controller endpoint
   - Apply `[EnsureUserId]` if needed
   - Apply authorization attributes

5. **Frontend:**
   - Create API hooks (queries/mutations)
   - Create components
   - Add i18n resources
   - Update routes

6. **Testing:**
   - Write unit tests
   - Write integration tests (if needed)
   - Write frontend tests (if needed)

### Code Review Checklist

- ✅ Follows Vertical Slice Architecture
- ✅ Uses Result pattern, no business exceptions
- ✅ EF Core for writes, Dapper for reads
- ✅ FluentValidation for input validation
- ✅ Centralized error constants
- ✅ Primary constructors (C# 12)
- ✅ Constants centralized (no hardcoded strings)
- ✅ Tests use ErrorAssertionExtensions
- ✅ Frontend uses TanStack Query, RHF+Zod
- ✅ DatePicker used for date inputs (not native `<input type="date">`)
- ✅ i18n resources updated (en/ar)
- ✅ RTL support tested (selects, DatePicker, LocaleSwitcher)
- ✅ Tailwind classes use tokens
- ✅ ESLint/Prettier passes

### Anti-Patterns to Avoid

- ❌ Direct setter access on entities (use methods)
- ❌ Hardcoded strings (use constants/i18n)
- ❌ Mixing EF Core for queries or Dapper for commands
- ❌ Skipping handler registration
- ❌ Manual API calls bypassing TanStack Query
- ❌ Raw hex colors instead of Tailwind tokens
- ❌ Missing Arabic translations
- ❌ Old-style constructors (use primary constructors)

---

## Key Files Reference

### Backend

- **Mediator**: `src/TaskManagement.Application/Common/Mediator/PipelineMediator.cs`
- **Result**: `src/TaskManagement.Domain/Common/Result.cs`
- **Error**: `src/TaskManagement.Domain/Common/Error.cs`
- **BaseEntity**: `src/TaskManagement.Domain/Common/BaseEntity.cs`
- **BaseController**: `src/TaskManagement.Presentation/Controllers/BaseController.cs`
- **EnsureUserIdAttribute**: `src/TaskManagement.Presentation/Attributes/EnsureUserIdAttribute.cs`
- **Repository Interfaces**: `src/TaskManagement.Domain/Interfaces/` (IQueryRepository, ICommandRepository, ITaskEfCommandRepository, IUserEfCommandRepository)
- **Repository Implementations**: `src/TaskManagement.Infrastructure/Data/Repositories/` (TaskDapperRepository, TaskEfCommandRepository, UserDapperRepository, UserEfCommandRepository)
- **DI Files**: `DependencyInjection.cs` in each layer

### Frontend

- **API Client**: `web/src/core/api/client.browser.ts`
- **Providers**: `web/src/core/providers/AppProviders.tsx`
- **Auth**: `web/src/core/auth/session.ts`
- **i18n Config**: `web/src/i18n/config.ts`
- **UI Components**: `web/src/ui/components/**` (DatePicker, Button, Input, etc.)
- **LocaleSwitcher**: `web/src/core/routing/LocaleSwitcher.tsx`
- **Task Feature**: `web/src/features/tasks/**`

---

**Last Updated**: December 2025  
**Maintainer**: Update whenever architectural or tooling changes occur.

**Recent Changes (December 2025):**
- Repository implementations moved from `Application.Infrastructure.Data.Repositories` to `Infrastructure.Data.Repositories` (Clean Architecture compliance)
- Repository interfaces moved from `Application.Infrastructure.Data.Repositories` to `Domain.Interfaces` (proper dependency direction)
- All query handlers now use Dapper repositories from Infrastructure layer
- All command handlers now use EF Core repositories from Infrastructure layer

