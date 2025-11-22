using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Tests.TestHelpers;
using TaskManagement.Tests.Unit.TestHelpers;
using Xunit;
using DomainTask = TaskManagement.Domain.Entities.Task;
using SystemTask = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Unit.Application.Tasks.Commands;

/// <summary>
///     Unit tests for the CreateTaskCommandHandler.
/// </summary>
public class CreateTaskCommandHandlerTests
{
    private readonly CreateTaskCommandHandler _handler;
    private readonly Mock<TaskManagementDbContext> _mockContext;
    private readonly Mock<TaskEfCommandRepository> _mockTaskCommandRepository;
    private readonly Mock<UserDapperRepository> _mockUserQueryRepository;

    public CreateTaskCommandHandlerTests()
    {
        _mockContext = DbContextTestHelper.CreateMockDbContext();
        _mockTaskCommandRepository = new Mock<TaskEfCommandRepository>(_mockContext.Object);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb;Trusted_Connection=true;"
            })
            .Build();
        _mockUserQueryRepository = new Mock<UserDapperRepository>(configuration);
        _handler = new CreateTaskCommandHandler(_mockTaskCommandRepository.Object, _mockUserQueryRepository.Object,
            _mockContext.Object);
    }

    [Fact]
    public async SystemTask Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        var assignedUser = new User("assigned@example.com", "John", "Doe", "test-object-id");

        _mockUserQueryRepository
            .Setup(r => r.GetByIdAsync(command.AssignedUserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)assignedUser);

        _mockTaskCommandRepository.Setup(r => r.AddAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<DomainTask>());

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(command.Title);
        result.Value.Description.Should().Be(command.Description);
        result.Value.Priority.Should().Be(command.Priority);
        result.Value.DueDate.Should().Be(command.DueDate);
        result.Value.AssignedUserId.Should().Be(command.AssignedUserId);
        result.Value.AssignedUserEmail.Should().Be(assignedUser.Email);
        result.Value.CreatedBy.Should().Be(command.CreatedBy);
    }

    [Fact]
    public async SystemTask Handle_WithAssignedUserNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        _mockUserQueryRepository
            .Setup(r => r.GetByIdAsync(command.AssignedUserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.ShouldContainError(TaskErrors.AssignedUserNotFound);
    }

    [Fact]
    public async SystemTask Handle_WithEmptyTitle_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        var assignedUser = new User("assigned@example.com", "John", "Doe", "test-object-id");

        _mockUserQueryRepository
            .Setup(r => r.GetByIdAsync(command.AssignedUserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)assignedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.ShouldContainError(TaskErrors.TitleRequired);
    }

    [Fact]
    public async SystemTask Handle_WithPastDueDate_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(-1), // Past date
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        var assignedUser = new User("assigned@example.com", "John", "Doe", "test-object-id");

        _mockUserQueryRepository
            .Setup(r => r.GetByIdAsync(command.AssignedUserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)assignedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.ShouldContainError(TaskErrors.DueDateInPast);
    }

    [Fact]
    public async SystemTask Handle_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "", // Empty title
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(-1), // Past date
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        var assignedUser = new User("assigned@example.com", "John", "Doe", "test-object-id");

        _mockUserQueryRepository
            .Setup(r => r.GetByIdAsync(command.AssignedUserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)assignedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.ShouldContainErrorInCollection(TaskErrors.TitleRequired);
        result.ShouldContainErrorInCollection(TaskErrors.DueDateInPast);
    }

    [Fact]
    public async SystemTask Handle_ShouldCallRepositoryMethods()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        var assignedUser = new User("assigned@example.com", "John", "Doe", "test-object-id");

        _mockUserQueryRepository
            .Setup(r => r.GetByIdAsync(command.AssignedUserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)assignedUser);

        _mockTaskCommandRepository.Setup(r => r.AddAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<DomainTask>());

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUserQueryRepository.Verify(
            r => r.GetByIdAsync(command.AssignedUserId!.Value, It.IsAny<CancellationToken>()), Times.Once);
        _mockTaskCommandRepository.Verify(r => r.AddAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask Handle_ShouldSetCreatedByOnTask()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        var assignedUser = new User("assigned@example.com", "John", "Doe", "test-object-id");

        _mockUserQueryRepository
            .Setup(r => r.GetByIdAsync(command.AssignedUserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)assignedUser);

        _mockTaskCommandRepository.Setup(r => r.AddAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<DomainTask>());

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockTaskCommandRepository.Verify(r => r.AddAsync(
            It.Is<DomainTask>(t => t.CreatedBy == command.CreatedBy),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}