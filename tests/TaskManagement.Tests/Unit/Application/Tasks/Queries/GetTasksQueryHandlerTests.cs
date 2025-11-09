using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Tests.Unit.TestHelpers;
using Xunit;
using Task = System.Threading.Tasks.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.Application.Tasks.Queries;

/// <summary>
///     Unit tests for GetTasksQueryHandler using real in-memory database.
/// </summary>
public class GetTasksQueryHandlerTests : InMemoryDatabaseTestBase
{
    private readonly PipelineMediator _mediator;
    private readonly TestServiceLocator _serviceLocator;

    public GetTasksQueryHandlerTests()
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
    public async Task Handle_WithValidQuery_ShouldReturnTasksSuccessfully()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().NotBeEmpty();
        result.Value.TotalCount.Should().BeGreaterThan(0);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Status = TaskStatus.Pending,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().AllSatisfy(t => t.Status.Should().Be(TaskStatus.Pending));
    }

    [Fact]
    public async Task Handle_WithPriorityFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Priority = TaskPriority.High,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().AllSatisfy(t => t.Priority.Should().Be(TaskPriority.High));
    }

    [Fact]
    public async Task Handle_WithAssignedUserFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var testUser = GetTestUser("john.doe@example.com");
        var query = new GetTasksQuery
        {
            AssignedUserId = testUser.Id,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().AllSatisfy(t => t.AssignedUserId.Should().Be(testUser.Id));
    }

    [Fact]
    public async Task Handle_WithDateRangeFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-1);
        var toDate = DateTime.UtcNow.AddDays(30);
        var query = new GetTasksQuery
        {
            DueDateFrom = fromDate,
            DueDateTo = toDate,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().AllSatisfy(t => 
            t.DueDate.Should().BeOnOrAfter(fromDate).And.BeOnOrBefore(toDate));
    }

    [Fact]
    public async Task Handle_WithInvalidPagination_ShouldReturnValidationErrors()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 0, // Invalid: page must be >= 1
            PageSize = 0 // Invalid: page size must be >= 1
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().HaveCount(2); // Expecting 2 validation errors
    }

    [Fact]
    public async Task Handle_WithInvalidDateRange_ShouldReturnValidationErrors()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            DueDateFrom = DateTime.UtcNow.AddDays(10),
            DueDateTo = DateTime.UtcNow.AddDays(5), // Invalid: from date is after to date
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.ShouldContainError(TaskErrors.InvalidDateRange);
    }
}