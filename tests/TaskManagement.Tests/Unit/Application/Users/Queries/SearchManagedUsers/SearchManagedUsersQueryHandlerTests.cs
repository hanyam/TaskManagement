using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Users.Queries.SearchManagedUsers;
using TaskManagement.Tests.Unit.TestHelpers;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Unit.Application.Users.Queries.SearchManagedUsers;

/// <summary>
///     Unit tests for SearchManagedUsersQueryHandler using real in-memory database.
/// </summary>
public class SearchManagedUsersQueryHandlerTests : InMemoryDatabaseTestBase
{
    private readonly PipelineMediator _mediator;
    private readonly TestServiceLocator _serviceLocator;

    public SearchManagedUsersQueryHandlerTests()
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
    public async Task Handle_WithValidQuery_ShouldReturnManagedUsersSuccessfully()
    {
        // Arrange
        var manager = CreateTestUser("manager@example.com", "Manager", "User", "manager-oid");
        var employee1 = CreateTestUser("employee1@example.com", "John", "Doe", "emp1-oid");
        var employee2 = CreateTestUser("employee2@example.com", "Jane", "Smith", "emp2-oid");
        var otherEmployee = CreateTestUser("other@example.com", "Other", "User", "other-oid");

        // Create manager-employee relationships
        CreateManagerEmployeeRelationship(manager.Id, employee1.Id);
        CreateManagerEmployeeRelationship(manager.Id, employee2.Id);
        // otherEmployee is not managed by manager

        var query = new SearchManagedUsersQuery
        {
            ManagerId = manager.Id,
            SearchQuery = "John"
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Count.Should().Be(1);
        result.Value[0].DisplayName.Should().Be("John Doe");
        result.Value[0].Mail.Should().Be("employee1@example.com");
    }

    [Fact]
    public async Task Handle_WithSearchQueryMatchingEmail_ShouldReturnManagedUsers()
    {
        // Arrange
        var manager = CreateTestUser("manager@example.com", "Manager", "User", "manager-oid");
        var employee1 = CreateTestUser("employee1@example.com", "John", "Doe", "emp1-oid");
        var employee2 = CreateTestUser("employee2@example.com", "Jane", "Smith", "emp2-oid");

        CreateManagerEmployeeRelationship(manager.Id, employee1.Id);
        CreateManagerEmployeeRelationship(manager.Id, employee2.Id);

        var query = new SearchManagedUsersQuery
        {
            ManagerId = manager.Id,
            SearchQuery = "employee1"
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(1);
        result.Value[0].Mail.Should().Be("employee1@example.com");
    }

    [Fact]
    public async Task Handle_WithSearchQueryLessThanTwoCharacters_ShouldReturnEmptyList()
    {
        // Arrange
        var manager = CreateTestUser("manager@example.com", "Manager", "User", "manager-oid");
        var employee1 = CreateTestUser("employee1@example.com", "John", "Doe", "emp1-oid");

        CreateManagerEmployeeRelationship(manager.Id, employee1.Id);

        var query = new SearchManagedUsersQuery
        {
            ManagerId = manager.Id,
            SearchQuery = "J"
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNoManagedUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var manager = CreateTestUser("manager@example.com", "Manager", "User", "manager-oid");
        var employee1 = CreateTestUser("employee1@example.com", "John", "Doe", "emp1-oid");
        // No manager-employee relationship created

        var query = new SearchManagedUsersQuery
        {
            ManagerId = manager.Id,
            SearchQuery = "John"
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldNotReturnInactiveUser()
    {
        // Arrange
        var manager = CreateTestUser("manager@example.com", "Manager", "User", "manager-oid");
        var employee1 = CreateTestUser("employee1@example.com", "John", "Doe", "emp1-oid");
        var inactiveEmployee = CreateTestUser("inactive@example.com", "Inactive", "User", "inactive-oid");
        inactiveEmployee.Deactivate();
        Context.SaveChanges();

        CreateManagerEmployeeRelationship(manager.Id, employee1.Id);
        CreateManagerEmployeeRelationship(manager.Id, inactiveEmployee.Id);

        var query = new SearchManagedUsersQuery
        {
            ManagerId = manager.Id,
            SearchQuery = "John"
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(1); // Only active employee1, not inactiveEmployee
        result.Value[0].DisplayName.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_WithMultipleMatches_ShouldReturnOrderedByDisplayName()
    {
        // Arrange
        var manager = CreateTestUser("manager@example.com", "Manager", "User", "manager-oid");
        var employee1 = CreateTestUser("employee1@example.com", "Zebra", "User", "emp1-oid");
        var employee2 = CreateTestUser("employee2@example.com", "Alpha", "User", "emp2-oid");
        var employee3 = CreateTestUser("employee3@example.com", "Beta", "User", "emp3-oid");

        CreateManagerEmployeeRelationship(manager.Id, employee1.Id);
        CreateManagerEmployeeRelationship(manager.Id, employee2.Id);
        CreateManagerEmployeeRelationship(manager.Id, employee3.Id);

        var query = new SearchManagedUsersQuery
        {
            ManagerId = manager.Id,
            SearchQuery = "User"
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(3);
        result.Value[0].DisplayName.Should().Be("Alpha User");
        result.Value[1].DisplayName.Should().Be("Beta User");
        result.Value[2].DisplayName.Should().Be("Zebra User");
    }
}