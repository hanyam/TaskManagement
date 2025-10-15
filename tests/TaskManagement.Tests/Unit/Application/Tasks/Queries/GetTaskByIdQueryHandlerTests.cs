using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using Xunit;
using DomainTask = TaskManagement.Domain.Entities.Task;
using SystemTask = System.Threading.Tasks.Task;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.Application.Tasks.Queries;

/// <summary>
/// Unit tests for the GetTaskByIdQueryHandler.
/// </summary>
public class GetTaskByIdQueryHandlerTests
{
    private readonly Mock<TaskDapperRepository> _mockTaskRepository;
    private readonly GetTaskByIdQueryHandler _handler;

    public GetTaskByIdQueryHandlerTests()
    {
        _mockTaskRepository = new Mock<TaskDapperRepository>(Mock.Of<IConfiguration>());
        _handler = new GetTaskByIdQueryHandler(_mockTaskRepository.Object);
    }

    [Fact]
    public async SystemTask Handle_WithValidId_ShouldReturnTaskDto()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var query = new GetTaskByIdQuery { Id = taskId };
        
        var expectedTaskDto = new TaskDto
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = DomainTaskStatus.Pending,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            AssignedUserEmail = "assigned@example.com",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test@example.com"
        };

        _mockTaskRepository.Setup(r => r.GetTaskWithUserAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTaskDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(expectedTaskDto.Id);
        result.Value.Title.Should().Be(expectedTaskDto.Title);
        result.Value.Description.Should().Be(expectedTaskDto.Description);
        result.Value.Status.Should().Be(expectedTaskDto.Status);
        result.Value.Priority.Should().Be(expectedTaskDto.Priority);
        result.Value.DueDate.Should().Be(expectedTaskDto.DueDate);
        result.Value.AssignedUserId.Should().Be(expectedTaskDto.AssignedUserId);
        result.Value.AssignedUserEmail.Should().Be(expectedTaskDto.AssignedUserEmail);
        result.Value.CreatedAt.Should().Be(expectedTaskDto.CreatedAt);
        result.Value.CreatedBy.Should().Be(expectedTaskDto.CreatedBy);
    }

    [Fact]
    public async SystemTask Handle_WithEmptyId_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetTaskByIdQuery { Id = Guid.Empty };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(TaskErrors.InvalidTaskId);
    }

    [Fact]
    public async SystemTask Handle_WithTaskNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var query = new GetTaskByIdQuery { Id = taskId };

        _mockTaskRepository.Setup(r => r.GetTaskWithUserAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(TaskErrors.NotFound);
    }

    [Fact]
    public async SystemTask Handle_ShouldCallRepositoryMethod()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var query = new GetTaskByIdQuery { Id = taskId };
        
        var expectedTaskDto = new TaskDto
        {
            Id = taskId,
            Title = "Test Task",
            Description = "Test Description",
            Status = DomainTaskStatus.Pending,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            AssignedUserEmail = "assigned@example.com",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test@example.com"
        };

        _mockTaskRepository.Setup(r => r.GetTaskWithUserAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTaskDto);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockTaskRepository.Verify(r => r.GetTaskWithUserAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask Handle_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var query = new GetTaskByIdQuery { Id = taskId };
        var exception = new InvalidOperationException("Repository exception");

        _mockTaskRepository.Setup(r => r.GetTaskWithUserAsync(taskId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var exceptionThrown = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(query, CancellationToken.None));
        
        exceptionThrown.Message.Should().Be("Repository exception");
    }
}
