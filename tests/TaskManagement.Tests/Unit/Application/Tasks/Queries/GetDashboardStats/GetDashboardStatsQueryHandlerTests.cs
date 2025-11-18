using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Tasks.Queries.GetDashboardStats;
using TaskManagement.Domain.Entities;
using TaskManagement.Tests.Unit.TestHelpers;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Unit.Application.Tasks.Queries.GetDashboardStats;

/// <summary>
///     Unit tests for GetDashboardStatsQueryHandler using real in-memory database.
/// </summary>
public class GetDashboardStatsQueryHandlerTests : InMemoryDatabaseTestBase
{
    private readonly PipelineMediator _mediator;
    private readonly TestServiceLocator _serviceLocator;

    public GetDashboardStatsQueryHandlerTests()
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
    public async Task Handle_ShouldReturnCorrectTaskCounts()
    {
        // Arrange
        var user = GetTestUser("john.doe@example.com");

        // Create tasks with different statuses
        var completedTask = CreateTestTask("Completed", "Desc", TaskPriority.High, DateTime.UtcNow.AddDays(-1), user.Id,
            TaskType.Simple, user.Id);
        completedTask.Complete();
        Context.Tasks.Update(completedTask);

        var nearDueTask = CreateTestTask("Near Due", "Desc", TaskPriority.High, DateTime.UtcNow.AddDays(2), user.Id,
            TaskType.Simple, user.Id);

        var inProgressTask = CreateTestTask("In Progress", "Desc", TaskPriority.Medium, DateTime.UtcNow.AddDays(5),
            user.Id, TaskType.Simple, user.Id);
        inProgressTask.Assign();
        Context.Tasks.Update(inProgressTask);

        Context.SaveChanges();

        var query = new GetDashboardStatsQuery
        {
            UserId = user.Id
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TasksCreatedByUser.Should().BeGreaterThan(0);
        result.Value.TasksCompleted.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_ShouldCountTasksCreatedByUser()
    {
        // Arrange
        var user1 = GetTestUser("john.doe@example.com");
        var user2 = GetTestUser("jane.smith@example.com");

        // Create tasks for user1
        CreateTestTask("Task 1", "Desc", TaskPriority.Low, null, user1.Id, TaskType.Simple, user1.Id);
        CreateTestTask("Task 2", "Desc", TaskPriority.Medium, null, user1.Id, TaskType.Simple, user1.Id);

        // Create task for user2
        CreateTestTask("Task 3", "Desc", TaskPriority.High, null, user2.Id, TaskType.Simple, user2.Id);

        var query = new GetDashboardStatsQuery
        {
            UserId = user1.Id
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TasksCreatedByUser.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Handle_ShouldCountNearDueDateTasks()
    {
        // Arrange
        var user = GetTestUser("john.doe@example.com");

        // Create task due within 3 days
        CreateTestTask("Near Due", "Desc", TaskPriority.High, DateTime.UtcNow.AddDays(2), user.Id, TaskType.Simple,
            user.Id);

        // Create task due after 3 days
        CreateTestTask("Far Due", "Desc", TaskPriority.High, DateTime.UtcNow.AddDays(10), user.Id, TaskType.Simple,
            user.Id);

        var query = new GetDashboardStatsQuery
        {
            UserId = user.Id
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TasksNearDueDate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_ShouldCountDelayedTasks()
    {
        // Arrange
        var user = GetTestUser("john.doe@example.com");

        // Create task past due date
        var delayedTask = CreateTestTask("Delayed", "Desc", TaskPriority.High, DateTime.UtcNow.AddDays(-5), user.Id,
            TaskType.Simple, user.Id);
        delayedTask.Assign();
        Context.Tasks.Update(delayedTask);
        Context.SaveChanges();

        var query = new GetDashboardStatsQuery
        {
            UserId = user.Id
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TasksDelayed.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyStatsForUserWithNoTasks()
    {
        // Arrange
        var newUser = CreateTestUser("newuser@example.com", "New", "User", "azure-oid-new");

        var query = new GetDashboardStatsQuery
        {
            UserId = newUser.Id
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TasksCreatedByUser.Should().Be(0);
        result.Value.TasksCompleted.Should().Be(0);
        result.Value.TasksNearDueDate.Should().Be(0);
        result.Value.TasksDelayed.Should().Be(0);
    }
}