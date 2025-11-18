using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Tests.Unit.TestHelpers;
using Xunit;
using Task = System.Threading.Tasks.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.Application.Common;

/// <summary>
///     Tests for RequestMediator to verify request handling with pipeline behaviors.
/// </summary>
public class RequestMediatorTests : InMemoryDatabaseTestBase
{
    private readonly ILogger<RequestMediator> _logger;
    private readonly RequestMediator _requestMediator;
    private readonly TestServiceLocator _serviceLocator;

    public RequestMediatorTests()
    {
        // Create a real service provider with logging
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var serviceProvider = services.BuildServiceProvider();

        // Create real service locator that provides actual services
        _serviceLocator = new TestServiceLocator(serviceProvider, Context);

        // Create real logger
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<RequestMediator>();

        // Create real request mediator with real services
        _requestMediator = new RequestMediator(_serviceLocator, _logger);
    }

    [Fact]
    public async Task Send_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var query = new GetTasksQuery();

        // Act
        var result = await _requestMediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().HaveCount(TestTaskIds.Count);
        result.Value.TotalCount.Should().Be(TestTaskIds.Count);
    }

    [Fact]
    public async Task Send_WithStatusFilter_ShouldReturnFilteredTasks()
    {
        // Arrange
        var query = new GetTasksQuery { Status = TaskStatus.Pending };

        // Act
        var result = await _requestMediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().NotBeEmpty();
        result.Value.Tasks.Should().OnlyContain(t => t.Status == TaskStatus.Pending);
    }

    [Fact]
    public async Task Send_WithInvalidPageNumber_ShouldReturnValidationErrors()
    {
        // Arrange
        var query = new GetTasksQuery { Page = 0 };

        // Act
        var result = await _requestMediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Code == "VALIDATION_ERROR" && e.Field == "Page");
    }
}