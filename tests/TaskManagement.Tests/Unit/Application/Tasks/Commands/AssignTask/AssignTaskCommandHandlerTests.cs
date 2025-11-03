using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Commands.AssignTask;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Tests.Unit.TestHelpers;
using static TaskManagement.Tests.Unit.TestHelpers.ErrorAssertionExtensions;
using Xunit;
using Task = System.Threading.Tasks.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.Application.Tasks.Commands.AssignTask;

/// <summary>
///     Unit tests for AssignTaskCommandHandler using real in-memory database.
/// </summary>
public class AssignTaskCommandHandlerTests : InMemoryDatabaseTestBase
{
    private readonly PipelineMediator _mediator;
    private readonly TestServiceLocator _serviceLocator;

    public AssignTaskCommandHandlerTests()
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
    public async Task Handle_WhenTaskAndUsersExist_ShouldAssignTaskSuccessfully()
    {
        // Arrange
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var employee1 = GetTestUser("jane.smith@example.com");
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), manager.Id, TaskType.Simple, manager.Id);
        
        var command = new AssignTaskCommand
        {
            TaskId = task.Id,
            UserIds = new List<Guid> { employee1.Id },
            AssignedById = manager.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AssignedUserId.Should().Be(employee1.Id);
        
        // Verify task status
        var updatedTask = await Context.Tasks.FindAsync(task.Id);
        updatedTask.Should().NotBeNull();
        updatedTask!.Status.Should().Be(TaskStatus.Assigned);
        
        // Verify assignment created
        var assignment = await Context.Set<TaskAssignment>()
            .FirstOrDefaultAsync(ta => ta.TaskId == task.Id && ta.UserId == employee1.Id);
        assignment.Should().NotBeNull();
        assignment!.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithMultipleUsers_ShouldAssignToAllUsers()
    {
        // Arrange
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var employee1 = GetTestUser("jane.smith@example.com");
        var employee2 = GetTestUser("bob.wilson@example.com");
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), manager.Id, TaskType.Simple, manager.Id);
        
        var command = new AssignTaskCommand
        {
            TaskId = task.Id,
            UserIds = new List<Guid> { employee1.Id, employee2.Id },
            AssignedById = manager.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify both assignments created
        var assignments = await Context.Set<TaskAssignment>()
            .Where(ta => ta.TaskId == task.Id)
            .ToListAsync();
        assignments.Should().HaveCount(2);
        
        // First user should be primary
        var primaryAssignment = assignments.First(ta => ta.UserId == employee1.Id);
        primaryAssignment.IsPrimary.Should().BeTrue();
        
        // Second user should not be primary
        var secondaryAssignment = assignments.First(ta => ta.UserId == employee2.Id);
        secondaryAssignment.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ShouldReturnFailure()
    {
        // Arrange
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var nonExistentTaskId = Guid.NewGuid();
        
        var command = new AssignTaskCommand
        {
            TaskId = nonExistentTaskId,
            UserIds = new List<Guid> { manager.Id },
            AssignedById = manager.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainError(TaskErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), manager.Id, TaskType.Simple, manager.Id);
        var nonExistentUserId = Guid.NewGuid();
        
        var command = new AssignTaskCommand
        {
            TaskId = task.Id,
            UserIds = new List<Guid> { nonExistentUserId },
            AssignedById = manager.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainError(TaskErrors.AssignedUserNotFound);
    }

    [Fact]
    public async Task Handle_WithEmptyUserList_ShouldReturnFailure()
    {
        // Arrange
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), manager.Id, TaskType.Simple, manager.Id);
        
        var command = new AssignTaskCommand
        {
            TaskId = task.Id,
            UserIds = new List<Guid>(),
            AssignedById = manager.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainValidationError("UserIds");
    }

    [Fact]
    public async Task Handle_WhenReassigning_ShouldClearOldAssignments()
    {
        // Arrange
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var employee1 = GetTestUser("jane.smith@example.com");
        var employee2 = GetTestUser("bob.wilson@example.com");
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), manager.Id, TaskType.Simple, manager.Id);
        
        // Create initial assignment
        CreateTestAssignment(task.Id, employee1.Id, isPrimary: true);
        
        var command = new AssignTaskCommand
        {
            TaskId = task.Id,
            UserIds = new List<Guid> { employee2.Id },
            AssignedById = manager.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify old assignment removed
        var oldAssignment = await Context.Set<TaskAssignment>()
            .FirstOrDefaultAsync(ta => ta.TaskId == task.Id && ta.UserId == employee1.Id);
        oldAssignment.Should().BeNull();
        
        // Verify new assignment created
        var newAssignment = await Context.Set<TaskAssignment>()
            .FirstOrDefaultAsync(ta => ta.TaskId == task.Id && ta.UserId == employee2.Id);
        newAssignment.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldUpdateTaskStatusToAssigned()
    {
        // Arrange
        var manager = GetTestUserWithRole("john.doe@example.com", UserRole.Manager);
        var employee1 = GetTestUser("jane.smith@example.com");
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), manager.Id, TaskType.Simple, manager.Id);
        
        // Verify initial status
        task.Status.Should().Be(TaskStatus.Created);
        
        var command = new AssignTaskCommand
        {
            TaskId = task.Id,
            UserIds = new List<Guid> { employee1.Id },
            AssignedById = manager.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var updatedTask = await Context.Tasks.FindAsync(task.Id);
        updatedTask!.Status.Should().Be(TaskStatus.Assigned);
    }
}

