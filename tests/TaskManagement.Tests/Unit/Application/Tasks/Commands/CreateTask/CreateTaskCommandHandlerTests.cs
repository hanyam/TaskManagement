using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace TaskManagement.Tests.Unit.Application.Tasks.Commands.CreateTask;

/// <summary>
///     Unit tests for CreateTaskCommandHandler.
/// </summary>
public class CreateTaskCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly CreateTaskCommandHandler _handler;
    private readonly Mock<TaskEfCommandRepository> _taskCommandRepositoryMock;
    private readonly Mock<UserDapperRepository> _userQueryRepositoryMock;

    public CreateTaskCommandHandlerTests()
    {
        // Create in-memory configuration that returns a connection string
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", "Data Source=:memory:"}
        };
        
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        // Create a mock DbContextOptions
        var optionsMock = new Mock<DbContextOptions<ApplicationDbContext>>();
        
        _contextMock = new Mock<ApplicationDbContext>(optionsMock.Object);
        _taskCommandRepositoryMock = new Mock<TaskEfCommandRepository>(_contextMock.Object);
        _userQueryRepositoryMock = new Mock<UserDapperRepository>(configuration);

        _handler = new CreateTaskCommandHandler(
            _taskCommandRepositoryMock.Object,
            _userQueryRepositoryMock.Object,
            _contextMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldCreateTaskSuccessfully()
    {
        // Arrange
        var assignedUserId = Guid.NewGuid();
        var assignedUser = new User("test@example.com", "John", "Doe", "azure-oid-123");
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = assignedUserId,
            CreatedBy = "test@example.com"
        };

        _userQueryRepositoryMock.Setup(r => r.GetByIdAsync(assignedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignedUser);

        _taskCommandRepositoryMock.Setup(r =>
                r.AddAsync(It.IsAny<TaskManagement.Domain.Entities.Task>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskManagement.Domain.Entities.Task task) => task);

        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(command.Title);
        result.Value.Description.Should().Be(command.Description);
        result.Value.Priority.Should().Be(command.Priority);
        result.Value.AssignedUserId.Should().Be(command.AssignedUserId);
        result.Value.AssignedUserEmail.Should().Be(assignedUser.Email);

        _taskCommandRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TaskManagement.Domain.Entities.Task>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var assignedUserId = Guid.NewGuid();
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = assignedUserId,
            CreatedBy = "test@example.com"
        };

        _userQueryRepositoryMock.Setup(r => r.GetByIdAsync(assignedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Assigned user not found");

        _taskCommandRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TaskManagement.Domain.Entities.Task>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}