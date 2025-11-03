# Task Management API - Testing Documentation

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Testing Architecture](#testing-architecture)
2. [Unit Testing](#unit-testing)
3. [Integration Testing](#integration-testing)
4. [Test Data Management](#test-data-management)
5. [Error Testing Patterns](#error-testing-patterns)
6. [Running Tests](#running-tests)

---

## Testing Architecture

### Test Project Structure

```
TaskManagement.Tests/
├── Unit/
│   ├── Application/
│   │   ├── Tasks/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateTask/
│   │   │   │   │   └── CreateTaskCommandHandlerTests.cs
│   │   │   │   └── ...
│   │   │   └── Queries/
│   │   └── ...
│   ├── Domain/
│   │   └── Entities/
│   └── TestHelpers/
│       ├── InMemoryDatabaseTestBase.cs
│       ├── TestServiceLocator.cs
│       └── ErrorAssertionExtensions.cs
└── Integration/
    ├── Controllers/
    │   ├── TasksControllerIntegrationTests.cs
    │   └── ...
    └── TestWebApplicationFactory.cs
```

### Test Base Classes

**InMemoryDatabaseTestBase:**
- Provides in-memory database for unit tests
- Seeds test data
- Helper methods for creating entities
- Disposes database after tests

**TestServiceLocator:**
- Real service locator implementation
- Registers all handlers and validators
- Used by mediator for handler resolution

---

## Unit Testing

### Handler Testing Patterns

**Command Handler Test Structure:**
```csharp
public class CreateTaskCommandHandlerTests : InMemoryDatabaseTestBase
{
    private readonly CreateTaskCommandHandler _handler;
    
    public CreateTaskCommandHandlerTests()
    {
        _handler = new CreateTaskCommandHandler(Context);
    }
    
    [Fact]
    public async Task Handle_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            AssignedUserId = TestUserIds[0],
            Type = TaskType.Simple,
            CreatedById = TestUserIds[0]
        };
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Test Task");
    }
}
```

**Query Handler Test Structure:**
```csharp
public class GetTaskByIdQueryHandlerTests : InMemoryDatabaseTestBase
{
    private readonly GetTaskByIdQueryHandler _handler;
    
    [Fact]
    public async Task Handle_WithValidId_ShouldReturnTask()
    {
        // Arrange
        var query = new GetTaskByIdQuery { Id = TestTaskIds[0] };
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
```

### Validator Testing Patterns

**Validator Test Structure:**
```csharp
public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator;
    
    public CreateTaskCommandValidatorTests()
    {
        _validator = new CreateTaskCommandValidator();
    }
    
    [Fact]
    public void Validate_WithEmptyTitle_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CreateTaskCommand { Title = string.Empty };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }
}
```

### Domain Entity Testing

**Entity Method Test:**
```csharp
[Fact]
public void Accept_WithAssignedStatus_ShouldTransitionToAccepted()
{
    // Arrange
    var task = CreateTestTask(status: TaskStatus.Assigned);
    
    // Act
    task.Accept();
    
    // Assert
    task.Status.Should().Be(TaskStatus.Accepted);
}

[Fact]
public void Accept_WithCompletedStatus_ShouldThrowException()
{
    // Arrange
    var task = CreateTestTask(status: TaskStatus.Completed);
    
    // Act & Assert
    var act = () => task.Accept();
    act.Should().Throw<InvalidOperationException>();
}
```

### Service Testing

**Business Service Test:**
```csharp
public class ReminderCalculationServiceTests
{
    private readonly IReminderCalculationService _service;
    
    [Fact]
    public void CalculateReminderLevel_WithPassedDueDate_ShouldReturnCritical()
    {
        // Arrange
        var dueDate = DateTime.UtcNow.AddDays(-1);
        var createdAt = DateTime.UtcNow.AddDays(-10);
        
        // Act
        var result = _service.CalculateReminderLevel(dueDate, createdAt);
        
        // Assert
        result.Should().Be(ReminderLevel.Critical);
    }
}
```

---

## Integration Testing

### API Endpoint Testing

**Test Structure:**
```csharp
public class TasksControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public TasksControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        // Setup authentication token
    }
    
    [Fact]
    public async Task CreateTask_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Integration Test Task",
            AssignedUserId = Guid.NewGuid()
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Database Testing

**Test Database Setup:**
- Use `TestWebApplicationFactory` for integration tests
- In-memory database or test SQL Server instance
- Seed test data as needed
- Clean up after tests

---

## Test Data Management

### InMemoryDatabaseTestBase

**Features:**
- In-memory database per test class
- Pre-seeded test users and tasks
- Helper methods for creating entities
- Automatic cleanup

**Helper Methods:**
```csharp
protected Task CreateTestTask(
    string title = "Test Task",
    TaskType type = TaskType.Simple,
    Guid? createdById = null);

protected User CreateTestUser(
    string email = "test@example.com");

protected TaskAssignment CreateTestAssignment(
    Guid taskId,
    Guid userId,
    bool isPrimary = false);
```

### Seed Data Patterns

**Pre-seeded Data:**
- Test users (5 users with known IDs)
- Test tasks (sample tasks for testing)
- Test assignments (if needed)

**Creating Test Data:**
```csharp
// Use helper methods
var task = CreateTestTask(title: "My Test Task", type: TaskType.WithProgress);

// Or create directly
var user = new User("test@example.com", "Test", "User", "azure-oid");
Context.Users.Add(user);
await Context.SaveChangesAsync();
```

### Test User Creation

**Setting User Roles:**
```csharp
var manager = GetTestUserWithRole(TestUserIds[0], UserRole.Manager);
var employee = GetTestUserWithRole(TestUserIds[1], UserRole.Employee);
```

---

## Error Testing Patterns

### ErrorAssertionExtensions Usage

**Single Error Assertion:**
```csharp
[Fact]
public async Task Handle_WhenTaskNotFound_ShouldReturnFailure()
{
    // Arrange
    var command = new UpdateTaskProgressCommand
    {
        TaskId = Guid.NewGuid(), // Non-existent task
        ProgressPercentage = 50
    };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.ShouldContainError(TaskErrors.NotFound);
}
```

**Multiple Errors Assertion:**
```csharp
[Fact]
public async Task Handle_WithInvalidData_ShouldReturnValidationErrors()
{
    // Arrange
    var command = new CreateTaskCommand
    {
        Title = string.Empty, // Invalid
        DueDate = DateTime.UtcNow.AddDays(-1) // Invalid
    };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.ShouldContainValidationError("Title");
    result.ShouldContainValidationError("DueDate");
}
```

### Centralized Error Testing

**Using TaskErrors:**
```csharp
result.ShouldContainError(TaskErrors.NotFound);
result.ShouldContainError(TaskErrors.TitleRequired);
result.ShouldContainError(TaskErrors.DueDateInPast);
```

**Validation Error Testing:**
```csharp
result.ShouldContainValidationError("Title");
result.ShouldContainValidationError("DueDate");
```

### Error Collection Testing

**Multiple Errors:**
```csharp
result.Errors.Should().HaveCount(2);
result.ShouldContainErrorInCollection(TaskErrors.TitleRequired);
result.ShouldContainErrorInCollection(TaskErrors.DueDateInPast);
```

---

## Running Tests

### Test Execution Commands

**Run All Tests:**
```bash
dotnet test
```

**Run Specific Test Project:**
```bash
dotnet test tests/TaskManagement.Tests/
```

**Run Specific Test Class:**
```bash
dotnet test --filter "FullyQualifiedName~CreateTaskCommandHandlerTests"
```

**Run Specific Test:**
```bash
dotnet test --filter "FullyQualifiedName~CreateTaskCommandHandlerTests.Handle_WithValidData_ShouldReturnSuccess"
```

**Run with Verbose Output:**
```bash
dotnet test --verbosity normal
```

**Run with Code Coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Debugging Tests

**Visual Studio:**
1. Set breakpoints in test or code
2. Right-click test → Debug Test
3. Step through code

**VS Code:**
1. Set breakpoints
2. Use "Run Test" with debug option
3. Use debugger to step through

**Command Line:**
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## See Also

- [Developer Guide](DEVELOPER_GUIDE.md) - Development workflow
- [Architecture Documentation](ARCHITECTURE.md) - System architecture
- [Error Handling](ERROR_HANDLING.md) - Error handling patterns

