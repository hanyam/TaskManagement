using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Domain.Entities;
using TaskManagement.Tests.Unit.TestHelpers;
using Xunit;
using Task = System.Threading.Tasks.Task;
using TaskManagement.Domain.Errors.Tasks;

namespace TaskManagement.Tests.Unit.Application.Tasks.Queries.GetTaskById;

/// <summary>
///     Unit tests for GetTaskByIdQueryHandler using real in-memory database.
/// </summary>
public class GetTaskByIdQueryHandlerTests : InMemoryDatabaseTestBase
{
    private readonly PipelineMediator _mediator;
    private readonly TestServiceLocator _serviceLocator;

    public GetTaskByIdQueryHandlerTests()
    {
        // Create a real service provider with logging
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var serviceProvider = services.BuildServiceProvider();
        
        // Create real service locator that provides actual services
        _serviceLocator = new TestServiceLocator(serviceProvider, Context);
        
        // Create real logger
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<PipelineMediator>();
        
        // Create real mediator with real services
        _mediator = new PipelineMediator(_serviceLocator, logger);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnTaskSuccessfully()
    {
        // Arrange - Use a test task from the seeded data
        var testTask = GetAllTestTasks().First();
        var query = new GetTaskByIdQuery 
        { 
            Id = testTask.Id,
            UserId = testTask.CreatedById // User who created the task should have access
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(testTask.Id);
        result.Value.Title.Should().Be(testTask.Title);
        result.Value.Description.Should().Be(testTask.Description);
        result.Value.Status.Should().Be(testTask.Status);
        result.Value.Priority.Should().Be(testTask.Priority);
        result.Value.AssignedUserId.Should().Be(testTask.AssignedUserId);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnNotFoundError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var userId = GetAllTestUsers().First().Id;
        var query = new GetTaskByIdQuery 
        { 
            Id = nonExistentId,
            UserId = userId
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainError(TaskErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnValidationError()
    {
        // Arrange
        var userId = GetAllTestUsers().First().Id;
        var query = new GetTaskByIdQuery 
        { 
            Id = Guid.Empty,
            UserId = userId
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainError(TaskErrors.InvalidTaskId);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnTaskWithUserInformation()
    {
        // Arrange - Use a test task from the seeded data
        var testTask = GetAllTestTasks().First();
        var query = new GetTaskByIdQuery 
        { 
            Id = testTask.Id,
            UserId = testTask.CreatedById // User who created the task should have access
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(testTask.Id);
        result.Value.AssignedUserEmail.Should().NotBeNullOrEmpty();
        result.Value.AssignedUserEmail.Should().Be(testTask.AssignedUser?.Email);
    }

    [Fact]
    public async Task Handle_WithMultipleTasks_ShouldReturnCorrectTask()
    {
        // Arrange - Get all test tasks and pick a specific one
        var allTasks = GetAllTestTasks().ToList();
        var targetTask = allTasks[1]; // Pick the second task
        var query = new GetTaskByIdQuery 
        { 
            Id = targetTask.Id,
            UserId = targetTask.CreatedById // User who created the task should have access
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(targetTask.Id);
        result.Value.Title.Should().Be(targetTask.Title);
        result.Value.Description.Should().Be(targetTask.Description);
    }

    [Fact]
    public async Task Handle_WhenUserIsAssignedToTask_ShouldReturnTaskSuccessfully()
    {
        // Arrange - Use a test task and access it as the assigned user
        var testTask = GetAllTestTasks().First(t => t.AssignedUserId.HasValue);
        var query = new GetTaskByIdQuery 
        { 
            Id = testTask.Id,
            UserId = testTask.AssignedUserId!.Value // User assigned to the task should have access
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(testTask.Id);
    }

    [Fact]
    public async Task Handle_WhenUserIsInAssignmentChain_ShouldReturnTaskSuccessfully()
    {
        // Arrange - Create a task and assign it to a user via TaskAssignment
        var testTask = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), GetAllTestUsers().First().Id);
        var assignedUser = GetAllTestUsers().Skip(1).First();
        CreateTestAssignment(testTask.Id, assignedUser.Id, isPrimary: true);
        
        var query = new GetTaskByIdQuery 
        { 
            Id = testTask.Id,
            UserId = assignedUser.Id // User in assignment chain should have access
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(testTask.Id);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoAccess_ShouldReturnAccessDeniedError()
    {
        // Arrange - Create a task and try to access it with an unrelated user
        var testTask = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), GetAllTestUsers().First().Id);
        var unrelatedUser = GetAllTestUsers().Last();
        
        var query = new GetTaskByIdQuery 
        { 
            Id = testTask.Id,
            UserId = unrelatedUser.Id // User with no relationship to task
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainError(TaskErrors.AccessDenied);
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnValidationError()
    {
        // Arrange
        var testTask = GetAllTestTasks().First();
        var query = new GetTaskByIdQuery 
        { 
            Id = testTask.Id,
            UserId = Guid.Empty // Invalid user ID
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainError(TaskErrors.InvalidUserId);
    }
}