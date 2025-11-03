using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Tests.Unit.TestHelpers;
using static TaskManagement.Tests.Unit.TestHelpers.ErrorAssertionExtensions;
using Xunit;
using Task = System.Threading.Tasks.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

//using TaskStatus = Microsoft.Graph.TaskStatus;

namespace TaskManagement.Tests.Unit.Application.Tasks.Commands.CreateTask;

/// <summary>
///     Unit tests for CreateTaskCommandHandler using real in-memory database.
/// </summary>
public class CreateTaskCommandHandlerTests : InMemoryDatabaseTestBase
{
    private readonly PipelineMediator _mediator;
    private readonly TestServiceLocator _serviceLocator;

    public CreateTaskCommandHandlerTests()
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
    public async Task Handle_WhenUserExists_ShouldCreateTaskSuccessfully()
    {
        // Arrange - Use a real test user from the database
        var assignedUser = GetTestUser("john.doe@example.com"); // John Doe
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = assignedUser.Id,
            CreatedBy = assignedUser.Email
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(command.Title);
        result.Value.Description.Should().Be(command.Description);
        result.Value.Priority.Should().Be(command.Priority);
        result.Value.AssignedUserId.Should().Be(command.AssignedUserId);
        result.Value.AssignedUserEmail.Should().Be(assignedUser.Email);

        // Verify that the task was actually added to the in-memory database
        var createdTask = await Context.Tasks.FindAsync(result.Value.Id);
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be(command.Title);
        createdTask.AssignedUserId.Should().Be(assignedUser.Id);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnFailure()
    {
        // Arrange - Use a non-existent user ID
        var nonExistentUserId = Guid.NewGuid();
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = nonExistentUserId,
            CreatedBy = "nonexistent@example.com"
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainError(TaskErrors.AssignedUserNotFound);

        // Verify that no task was added to the database
        var taskCount = await Context.Tasks.CountAsync();
        taskCount.Should().Be(TestTaskIds.Count); // Should still be the initial seeded tasks
    }

    [Fact]
    public async Task Handle_WithInvalidData_ShouldReturnValidationErrors()
    {
        // Arrange - Use invalid data
        var assignedUser = GetTestUser("john.doe@example.com"); // John Doe
        var command = new CreateTaskCommand
        {
            Title = "", // Invalid: empty title
            Description = "Short",
            Priority = TaskPriority.High, // Valid priority
            DueDate = DateTime.UtcNow.AddDays(-1), // Invalid: past due date
            AssignedUserId = assignedUser.Id,
            CreatedBy = "", // Invalid: empty created by
            CreatedById = Guid.Empty // Invalid: empty created by ID
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        // Expecting validation errors: Title and DueDate at minimum
        result.Errors.Count.Should().BeGreaterThanOrEqualTo(2);

        // Use centralized error objects instead of hardcoded strings
        result.ShouldContainErrorInCollection(TaskErrors.TitleRequired);
        result.ShouldContainErrorInCollection(TaskErrors.DueDateInPast);

        // Verify that no task was added to the database
        var taskCount = await Context.Tasks.CountAsync();
        taskCount.Should().Be(TestTaskIds.Count);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateTaskWithCorrectProperties()
    {
        // Arrange
        var assignedUser = GetTestUser("jane.smith@example.com"); // Jane Smith
        var command = new CreateTaskCommand
        {
            Title = "New Valid Task",
            Description = "This is a new valid task description.",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(10),
            AssignedUserId = assignedUser.Id,
            CreatedBy = assignedUser.Email
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(command.Title);
        result.Value.Description.Should().Be(command.Description);
        result.Value.Priority.Should().Be(command.Priority);
        result.Value.AssignedUserId.Should().Be(command.AssignedUserId);
        result.Value.AssignedUserEmail.Should().Be(assignedUser.Email);
        result.Value.Status.Should().Be(TaskStatus.Pending); // Default status

        var createdTask = await Context.Tasks.FindAsync(result.Value.Id);
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be(command.Title);
        createdTask.Description.Should().Be(command.Description);
        createdTask.Priority.Should().Be(command.Priority);
        createdTask.DueDate.Should().BeCloseTo(command.DueDate!.Value, TimeSpan.FromSeconds(1));
        createdTask.AssignedUserId.Should().Be(command.AssignedUserId);
        createdTask.CreatedBy.Should().Be(command.CreatedBy);
        createdTask.Status.Should().Be(TaskStatus.Pending);
    }

    [Fact]
    public async Task Handle_WithMultipleTasks_ShouldCreateAllTasksSuccessfully()
    {
        // Arrange
        var user1 = GetTestUser("john.doe@example.com"); // John Doe
        var user2 = GetTestUser("jane.smith@example.com"); // Jane Smith

        var command1 = new CreateTaskCommand
        {
            Title = "Task 1 for John",
            Description = "Description 1",
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow.AddDays(5),
            AssignedUserId = user1.Id,
            CreatedBy = user1.Email
        };

        var command2 = new CreateTaskCommand
        {
            Title = "Task 2 for Jane",
            Description = "Description 2",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(10),
            AssignedUserId = user2.Id,
            CreatedBy = user2.Email
        };

        var initialTaskCount = await Context.Tasks.CountAsync();

        // Act
        var result1 = await _mediator.Send(command1);
        var result2 = await _mediator.Send(command2);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        var finalTaskCount = await Context.Tasks.CountAsync();
        finalTaskCount.Should().Be(initialTaskCount + 2);

        var createdTask1 = await Context.Tasks.FindAsync(result1.Value!.Id);
        createdTask1.Should().NotBeNull();
        createdTask1!.Title.Should().Be(command1.Title);

        var createdTask2 = await Context.Tasks.FindAsync(result2.Value!.Id);
        createdTask2.Should().NotBeNull();
        createdTask2!.Title.Should().Be(command2.Title);
    }
}