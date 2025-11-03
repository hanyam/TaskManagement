using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.DTOs;
using TaskManagement.Tests.Unit.TestHelpers;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Unit.Application.Common;

/// <summary>
/// Tests for CommandMediator to verify command handling with pipeline behaviors.
/// </summary>
public class CommandMediatorTests : InMemoryDatabaseTestBase
{
    private readonly CommandMediator _commandMediator;
    private readonly TestServiceLocator _serviceLocator;
    private readonly ILogger<CommandMediator> _logger;

    public CommandMediatorTests()
    {
        // Create a real service provider with logging
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var serviceProvider = services.BuildServiceProvider();
        
        // Create real service locator that provides actual services
        _serviceLocator = new TestServiceLocator(serviceProvider, Context);
        
        // Create real logger
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger<CommandMediator>();
        
        // Create real command mediator with real services
        _commandMediator = new CommandMediator(_serviceLocator, _logger);
    }

    [Fact]
    public async Task Send_WithValidCommand_ShouldSucceed()
    {
        // Arrange
        var testUser = GetTestUser("john.doe@example.com"); // John Doe
        var command = new CreateTaskCommand
        {
            Title = "Valid Task",
            Description = "Valid Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = testUser.Id,
            CreatedBy = "test@example.com"
        };

        // Act
        var result = await _commandMediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(command.Title);
    }

    [Fact]
    public async Task Send_WithInvalidCommand_ShouldReturnValidationErrors()
    {
        // Arrange
        var testUser = GetTestUser("john.doe@example.com"); // John Doe
        var command = new CreateTaskCommand
        {
            Title = "", // Invalid: empty title
            Description = "Valid description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(-1), // Invalid: past date
            AssignedUserId = testUser.Id,
            CreatedBy = "test@example.com"
        };

        // Act
        var result = await _commandMediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().HaveCount(2); // Expecting 2 validation errors (Title and DueDate)
        
        // Verify specific validation errors
        result.Errors.Should().Contain(e => e.Code == "VALIDATION_ERROR" && e.Field == "Title");
        result.Errors.Should().Contain(e => e.Code == "VALIDATION_ERROR" && e.Field == "DueDate");
    }

    [Fact]
    public async Task Send_WithNonExistentUser_ShouldReturnUserNotFoundError()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var command = new CreateTaskCommand
        {
            Title = "Valid Task",
            Description = "Valid Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = nonExistentUserId,
            CreatedBy = "test@example.com"
        };

        // Act
        var result = await _commandMediator.Send(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Code == "NOT_FOUND");
    }
}
