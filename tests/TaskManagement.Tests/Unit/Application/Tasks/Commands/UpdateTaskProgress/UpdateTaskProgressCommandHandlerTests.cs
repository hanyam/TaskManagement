using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Commands.UpdateTaskProgress;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Tests.Unit.TestHelpers;
using static TaskManagement.Tests.Unit.TestHelpers.ErrorAssertionExtensions;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Unit.Application.Tasks.Commands.UpdateTaskProgress;

/// <summary>
///     Unit tests for UpdateTaskProgressCommandHandler using real in-memory database.
/// </summary>
public class UpdateTaskProgressCommandHandlerTests : InMemoryDatabaseTestBase
{
    private readonly PipelineMediator _mediator;
    private readonly TestServiceLocator _serviceLocator;

    public UpdateTaskProgressCommandHandlerTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var serviceProvider = services.BuildServiceProvider();
        
        _serviceLocator = new TestServiceLocator(serviceProvider, Context);
        
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<PipelineMediator>();
        
        _mediator = new PipelineMediator(_serviceLocator, logger);
    }

    [Fact]
    public async Task Handle_WithValidProgress_ShouldUpdateProgressSuccessfully()
    {
        // Arrange
        var employee = GetTestUser("john.doe@example.com");
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), employee.Id, TaskType.WithProgress, employee.Id);
        
        var command = new UpdateTaskProgressCommand
        {
            TaskId = task.Id,
            ProgressPercentage = 50,
            Notes = "Halfway complete",
            UpdatedById = employee.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProgressPercentage.Should().Be(50);
        result.Value.Notes.Should().Be("Halfway complete");
        
        // Verify task progress updated
        var updatedTask = await Context.Tasks.FindAsync(task.Id);
        updatedTask!.ProgressPercentage.Should().Be(50);
        
        // Verify progress history created
        var progressHistory = await Context.Set<TaskProgressHistory>()
            .FirstOrDefaultAsync(ph => ph.TaskId == task.Id);
        progressHistory.Should().NotBeNull();
        progressHistory!.ProgressPercentage.Should().Be(50);
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ShouldReturnFailure()
    {
        // Arrange
        var employee = GetTestUser("john.doe@example.com");
        var nonExistentTaskId = Guid.NewGuid();
        
        var command = new UpdateTaskProgressCommand
        {
            TaskId = nonExistentTaskId,
            ProgressPercentage = 50,
            UpdatedById = employee.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainError(TaskErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithInvalidProgressPercentage_ShouldReturnFailure()
    {
        // Arrange
        var employee = GetTestUser("john.doe@example.com");
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), employee.Id, TaskType.WithProgress, employee.Id);
        
        var command = new UpdateTaskProgressCommand
        {
            TaskId = task.Id,
            ProgressPercentage = 150, // Invalid: > 100
            UpdatedById = employee.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ShouldContainValidationError("ProgressPercentage");
    }

    [Fact]
    public async Task Handle_WithSimpleTaskType_ShouldReturnFailure()
    {
        // Arrange
        var employee = GetTestUser("john.doe@example.com");
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), employee.Id, TaskType.Simple, employee.Id);
        
        var command = new UpdateTaskProgressCommand
        {
            TaskId = task.Id,
            ProgressPercentage = 50,
            UpdatedById = employee.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.GetError().Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCreateProgressHistoryEntry()
    {
        // Arrange
        var employee = GetTestUser("john.doe@example.com");
        var task = CreateTestTask("Test Task", "Description", TaskPriority.High, DateTime.UtcNow.AddDays(7), employee.Id, TaskType.WithProgress, employee.Id);
        
        var command = new UpdateTaskProgressCommand
        {
            TaskId = task.Id,
            ProgressPercentage = 75,
            Notes = "Making good progress",
            UpdatedById = employee.Id
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var progressHistory = await Context.Set<TaskProgressHistory>()
            .Where(ph => ph.TaskId == task.Id)
            .OrderByDescending(ph => ph.CreatedAt)
            .FirstOrDefaultAsync();
        
        progressHistory.Should().NotBeNull();
        progressHistory!.ProgressPercentage.Should().Be(75);
        progressHistory.Notes.Should().Be("Making good progress");
        progressHistory.UpdatedById.Should().Be(employee.Id);
        progressHistory.Status.Should().Be(ProgressStatus.Pending);
    }
}

