using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using Xunit;
using Task = System.Threading.Tasks.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.Application.Tasks.Queries.GetTaskById;

/// <summary>
///     Unit tests for GetTaskByIdQueryHandler.
/// </summary>
public class GetTaskByIdQueryHandlerTests
{
    private readonly GetTaskByIdQueryHandler _handler;
    private readonly Mock<TaskDapperRepository> _taskRepositoryMock;

    public GetTaskByIdQueryHandlerTests()
    {
        // Create a mock configuration that returns a connection string
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["ConnectionStrings:DefaultConnection"])
            .Returns("Data Source=:memory:");
        
        _taskRepositoryMock = new Mock<TaskDapperRepository>(configurationMock.Object);
        _handler = new GetTaskByIdQueryHandler(_taskRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTaskExists_ShouldReturnTaskSuccessfully()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var assignedUserId = Guid.NewGuid();
        var taskDto = new TaskDto
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = TaskStatus.Pending,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = assignedUserId,
            AssignedUserEmail = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        var query = new GetTaskByIdQuery { Id = taskId };

        _taskRepositoryMock.Setup(r => r.GetTaskWithUserAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(taskDto.Id);
        result.Value.Title.Should().Be(taskDto.Title);
        result.Value.Description.Should().Be(taskDto.Description);
        result.Value.Status.Should().Be(taskDto.Status);
        result.Value.Priority.Should().Be(taskDto.Priority);
        result.Value.AssignedUserId.Should().Be(taskDto.AssignedUserId);
        result.Value.AssignedUserEmail.Should().Be(taskDto.AssignedUserEmail);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var query = new GetTaskByIdQuery { Id = taskId };

        _taskRepositoryMock.Setup(r => r.GetTaskWithUserAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Task not found");
    }
}