# Task Management API - Developer Guide

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Getting Started](#getting-started)
2. [Development Workflow](#development-workflow)
3. [Adding New Features](#adding-new-features)
4. [Common Patterns](#common-patterns)
5. [Debugging](#debugging)
6. [Code Review Checklist](#code-review-checklist)

---

## Getting Started

### Environment Setup

**Prerequisites:**
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- SQL Server (LocalDB or full instance)
- Git

**IDE Recommendations:**
- **Visual Studio 2022**: Full IDE with IntelliSense, debugging, and testing tools
- **VS Code**: Lightweight with C# Dev Kit extension
- **JetBrains Rider**: Cross-platform IDE with advanced refactoring

### Project Structure Navigation

**Solution Structure:**
```
TaskManagement/
├── src/
│   ├── TaskManagement.Api/          # API Layer (Controllers, Middleware)
│   ├── TaskManagement.Application/  # Application Layer (Commands, Queries, Handlers)
│   ├── TaskManagement.Domain/       # Domain Layer (Entities, DTOs, Interfaces)
│   └── TaskManagement.Infrastructure/ # Infrastructure Layer (Data, Auth)
├── tests/
│   └── TaskManagement.Tests/       # Test Project (Unit, Integration)
└── docs/                            # Documentation
```

**Key Directories:**
- `Application/Tasks/Commands/`: Write operations
- `Application/Tasks/Queries/`: Read operations
- `Domain/Entities/`: Domain models
- `Domain/DTOs/`: Data transfer objects
- `Infrastructure/Data/`: Data access

---

## Development Workflow

### Feature Development Process

**1. Domain Layer:**
- Define/update entities in `Domain/Entities/`
- Define DTOs in `Domain/DTOs/`
- Define errors in `Domain/Errors/`

**2. Application Layer:**
- Create command/query in `Application/Tasks/Commands/` or `Queries/`
- Create handler in same directory
- Create validator in same directory

**3. Infrastructure Layer:**
- Update `ApplicationDbContext` if needed
- Create migrations if schema changed

**4. API Layer:**
- Add endpoint in `Controllers/`
- Map request to command/query
- Return appropriate response

**5. Testing:**
- Create unit tests in `tests/TaskManagement.Tests/Unit/`
- Create integration tests in `tests/TaskManagement.Tests/Integration/`
- Run tests to verify

### Code Style Guidelines

**Naming Conventions:**
- Classes: PascalCase (e.g., `CreateTaskCommand`)
- Methods: PascalCase (e.g., `Handle`)
- Properties: PascalCase (e.g., `TaskId`)
- Private fields: camelCase (e.g., `_context`)
- Constants: PascalCase (e.g., `SectionName`)

**File Organization:**
- One class per file
- File name matches class name
- Group related classes in folders

**Code Formatting:**
- Use default .NET formatting
- Follow existing code style
- Run format document in IDE

### Git Workflow

**Branch Strategy:**
- `main`: Production-ready code
- `dev`: Development branch
- Feature branches: `feature/feature-name`
- Bug fix branches: `fix/bug-description`

**Commit Messages:**
- Use descriptive commit messages
- Reference issue numbers if applicable
- Format: `[Type] Description`

**Example:**
```
[Feature] Add task assignment functionality
[Fix] Resolve progress update validation error
[Docs] Update API reference documentation
```

---

## Adding New Features

### Vertical Slice Creation

**1. Create Domain Entity (if needed):**
```csharp
// src/TaskManagement.Domain/Entities/Project.cs
public class Project : BaseEntity
{
    public string Name { get; private set; }
    // ... properties
    
    public Project(string name)
    {
        Name = name;
    }
}
```

**2. Create DTO:**
```csharp
// src/TaskManagement.Domain/DTOs/ProjectDto.cs
public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
```

**3. Create Command:**
```csharp
// src/TaskManagement.Application/Projects/Commands/CreateProject/CreateProjectCommand.cs
public record CreateProjectCommand : ICommand<ProjectDto>
{
    public string Name { get; init; } = string.Empty;
}
```

**4. Create Command Handler:**
```csharp
// src/TaskManagement.Application/Projects/Commands/CreateProject/CreateProjectCommandHandler.cs
public class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, ProjectDto>
{
    private readonly ApplicationDbContext _context;
    
    public CreateProjectCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project(request.Name);
        project.SetCreatedBy("system");
        
        _context.Set<Project>().Add(project);
        await _context.SaveChangesAsync(cancellationToken);
        
        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name
        };
        
        return Result<ProjectDto>.Success(dto);
    }
}
```

**5. Create Validator:**
```csharp
// src/TaskManagement.Application/Projects/Commands/CreateProject/CreateProjectCommandValidator.cs
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
    }
}
```

**6. Register Handler:**
```csharp
// src/TaskManagement.Api/Program.cs
builder.Services.AddScoped<ICommandHandler<CreateProjectCommand, ProjectDto>, CreateProjectCommandHandler>();
```

**7. Add Controller Endpoint:**
```csharp
// src/TaskManagement.Api/Controllers/ProjectsController.cs
[HttpPost]
[Authorize]
public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
{
    var command = new CreateProjectCommand { Name = request.Name };
    var result = await _commandMediator.Send(command);
    return HandleResult(result, 201);
}
```

### Command/Query Handler Creation

**Command Handler Pattern:**
```csharp
public class CreateEntityCommandHandler : ICommandHandler<CreateEntityCommand, EntityDto>
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Result<EntityDto>> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate business rules
        // 2. Create entity
        // 3. Save changes
        // 4. Return DTO
    }
}
```

**Query Handler Pattern:**
```csharp
public class GetEntityByIdQueryHandler : IRequestHandler<GetEntityByIdQuery, EntityDto>
{
    private readonly EntityDapperRepository _repository;
    
    public async Task<Result<EntityDto>> Handle(GetEntityByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Query data
        // 2. Map to DTO
        // 3. Return result
    }
}
```

### Validator Creation

**FluentValidation Pattern:**
```csharp
public class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
{
    public CreateEntityCommandValidator()
    {
        RuleFor(x => x.Property)
            .NotEmpty().WithMessage("Property is required")
            .MaximumLength(100).WithMessage("Property cannot exceed 100 characters");
    }
}
```

### Controller Endpoint Creation

**Controller Pattern:**
```csharp
[HttpPost]
[Authorize(Roles = "Manager")]
public async Task<IActionResult> CreateEntity([FromBody] CreateEntityRequest request)
{
    var userIdClaim = User.FindFirst("user_id")?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
    }
    
    var command = new CreateEntityCommand
    {
        // Map request to command
        CreatedById = userId
    };
    
    var result = await _commandMediator.Send(command);
    return HandleResult(result, 201);
}
```

### Testing New Features

**Unit Test Pattern:**
```csharp
public class CreateEntityCommandHandlerTests : InMemoryDatabaseTestBase
{
    private readonly CreateEntityCommandHandler _handler;
    
    public CreateEntityCommandHandlerTests()
    {
        var locator = new TestServiceLocator(Context);
        _handler = new CreateEntityCommandHandler(Context);
    }
    
    [Fact]
    public async Task Handle_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateEntityCommand { Name = "Test Entity" };
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
```

---

## Common Patterns

### Command Handler Pattern

**Standard Structure:**
```csharp
public class CommandHandler : ICommandHandler<Command, Response>
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
    {
        // 1. Validate inputs
        // 2. Load entities
        // 3. Apply business logic
        // 4. Save changes
        // 5. Return result
    }
}
```

### Query Handler Pattern

**Standard Structure:**
```csharp
public class QueryHandler : IRequestHandler<Query, Response>
{
    private readonly EntityDapperRepository _repository;
    
    public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        // 1. Validate inputs
        // 2. Query data
        // 3. Map to DTO
        // 4. Return result
    }
}
```

### Error Handling Pattern

**Error Creation:**
```csharp
// In domain errors class
public static Error EntityNotFound => Error.NotFound("Entity", "Id");

// In handler
if (entity == null)
    return Result<EntityDto>.Failure(EntityErrors.NotFound);
```

**Multiple Errors:**
```csharp
var errors = new List<Error>();
if (string.IsNullOrEmpty(name))
    errors.Add(EntityErrors.NameRequired);
if (invalidDate)
    errors.Add(EntityErrors.InvalidDate);

if (errors.Any())
    return Result<EntityDto>.Failure(errors);
```

### Result Pattern Usage

**Success:**
```csharp
return Result<EntityDto>.Success(dto);
```

**Failure:**
```csharp
return Result<EntityDto>.Failure(EntityErrors.NotFound);
```

**No Content:**
```csharp
return Result.Success();
```

---

## Debugging

### Logging Usage

**Structured Logging:**
```csharp
_logger.LogInformation("Processing request {RequestId}", request.Id);
_logger.LogWarning("Validation failed: {Errors}", errors);
_logger.LogError(ex, "Error processing request {RequestId}", request.Id);
```

**Log Levels:**
- `LogInformation`: Normal operations
- `LogWarning`: Recoverable errors
- `LogError`: Exceptions
- `LogDebug`: Debug information

### Breakpoint Strategies

**Key Breakpoints:**
- Controller entry points
- Handler entry points
- Entity method calls
- Repository queries
- Exception handlers

**Debugging Tips:**
- Use conditional breakpoints
- Inspect `Result<T>` values
- Check entity state in EF Core
- Verify claims in authentication

### Database Inspection

**Entity Framework Core:**
- Use `Context.ChangeTracker.Entries()` to inspect tracked entities
- Check `Context.Entry(entity).State` for entity state
- Use SQL Profiler to inspect generated SQL

**In-Memory Database:**
- Inspect `Context.Set<T>().ToList()` in debugger
- Check test database state in unit tests

---

## Code Review Checklist

### Architecture Compliance

- [ ] Follows Vertical Slice Architecture
- [ ] Commands/queries in correct directories
- [ ] Handlers follow established patterns
- [ ] DTOs defined in Domain layer
- [ ] Errors centralized in Domain layer

### Testing Requirements

- [ ] Unit tests created for new handlers
- [ ] Integration tests for new endpoints
- [ ] Test coverage adequate
- [ ] Tests use centralized error assertions
- [ ] Tests cover error scenarios

### Documentation Requirements

- [ ] XML comments on public APIs
- [ ] Complex logic documented
- [ ] API examples updated (if applicable)
- [ ] Architecture docs updated (if applicable)

### Code Quality

- [ ] No compiler warnings
- [ ] Follows naming conventions
- [ ] Error handling implemented
- [ ] Validation rules added
- [ ] Authorization checks in place

### Performance

- [ ] Async/await used correctly
- [ ] No unnecessary database queries
- [ ] Pagination implemented for lists
- [ ] Efficient queries (Dapper for reads)

---

## See Also

- [Architecture Documentation](ARCHITECTURE.md) - System architecture
- [API Reference](API_REFERENCE.md) - API endpoint documentation
- [Testing Documentation](TESTING.md) - Testing guidelines
- [Features Documentation](FEATURES.md) - Feature descriptions

