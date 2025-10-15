using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Tasks.Queries.GetTasks;
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
/// Unit tests for the GetTasksQueryHandler.
/// </summary>
public class GetTasksQueryHandlerTests
{
    private readonly Mock<TaskDapperRepository> _mockTaskRepository;
    private readonly GetTasksQueryHandler _handler;

    public GetTasksQueryHandlerTests()
    {
        _mockTaskRepository = new Mock<TaskDapperRepository>(Mock.Of<IConfiguration>());
        _handler = new GetTasksQueryHandler(_mockTaskRepository.Object);
    }

    [Fact]
    public async SystemTask Handle_WithValidQuery_ShouldReturnTasksResponse()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Status = DomainTaskStatus.Pending,
            Priority = TaskPriority.High,
            AssignedUserId = Guid.NewGuid(),
            DueDateFrom = DateTime.UtcNow,
            DueDateTo = DateTime.UtcNow.AddDays(7),
            Page = 1,
            PageSize = 10
        };

        var tasks = new List<TaskDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Task 1",
                Description = "Description 1",
                Status = DomainTaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(1),
                AssignedUserId = query.AssignedUserId.Value,
                AssignedUserEmail = "user1@example.com",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin@example.com"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Task 2",
                Description = "Description 2",
                Status = DomainTaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(2),
                AssignedUserId = query.AssignedUserId.Value,
                AssignedUserEmail = "user1@example.com",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin@example.com"
            }
        };

        var totalCount = 2;

        _mockTaskRepository.Setup(r => r.GetTasksWithPaginationAsync(
                query.Status,
                query.Priority,
                query.AssignedUserId,
                query.DueDateFrom,
                query.DueDateTo,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(query.Page);
        result.Value.PageSize.Should().Be(query.PageSize);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async SystemTask Handle_WithInvalidPage_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 0, // Invalid page
            PageSize = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(TaskErrors.InvalidPageNumber);
    }

    [Fact]
    public async SystemTask Handle_WithInvalidPageSize_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 1,
            PageSize = 0 // Invalid page size
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(TaskErrors.InvalidPageSize);
    }

    [Fact]
    public async SystemTask Handle_WithPageSizeTooLarge_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 1,
            PageSize = 101 // Too large
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(TaskErrors.InvalidPageSize);
    }

    [Fact]
    public async SystemTask Handle_WithInvalidDateRange_ShouldReturnValidationError()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 1,
            PageSize = 10,
            DueDateFrom = DateTime.UtcNow.AddDays(7), // From date after To date
            DueDateTo = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Be(TaskErrors.InvalidDateRange);
    }

    [Fact]
    public async SystemTask Handle_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 0, // Invalid page
            PageSize = 101, // Invalid page size
            DueDateFrom = DateTime.UtcNow.AddDays(7), // Invalid date range
            DueDateTo = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(TaskErrors.InvalidPageNumber);
        result.Errors.Should().Contain(TaskErrors.InvalidPageSize);
        result.Errors.Should().Contain(TaskErrors.InvalidDateRange);
    }

    [Fact]
    public async SystemTask Handle_WithValidDateRange_ShouldNotReturnValidationError()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 1,
            PageSize = 10,
            DueDateFrom = DateTime.UtcNow,
            DueDateTo = DateTime.UtcNow.AddDays(7)
        };

        var tasks = new List<TaskDto>();
        var totalCount = 0;

        _mockTaskRepository.Setup(r => r.GetTasksWithPaginationAsync(
                query.Status,
                query.Priority,
                query.AssignedUserId,
                query.DueDateFrom,
                query.DueDateTo,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, totalCount));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Tasks.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async SystemTask Handle_ShouldCallRepositoryMethod()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Status = DomainTaskStatus.Pending,
            Priority = TaskPriority.High,
            AssignedUserId = Guid.NewGuid(),
            DueDateFrom = DateTime.UtcNow,
            DueDateTo = DateTime.UtcNow.AddDays(7),
            Page = 1,
            PageSize = 10
        };

        var tasks = new List<TaskDto>();
        var totalCount = 0;

        _mockTaskRepository.Setup(r => r.GetTasksWithPaginationAsync(
                query.Status,
                query.Priority,
                query.AssignedUserId,
                query.DueDateFrom,
                query.DueDateTo,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, totalCount));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockTaskRepository.Verify(r => r.GetTasksWithPaginationAsync(
            query.Status,
            query.Priority,
            query.AssignedUserId,
            query.DueDateFrom,
            query.DueDateTo,
            query.Page,
            query.PageSize,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask Handle_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var query = new GetTasksQuery
        {
            Page = 1,
            PageSize = 10
        };

        var exception = new InvalidOperationException("Repository exception");

        _mockTaskRepository.Setup(r => r.GetTasksWithPaginationAsync(
                query.Status,
                query.Priority,
                query.AssignedUserId,
                query.DueDateFrom,
                query.DueDateTo,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var exceptionThrown = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(query, CancellationToken.None));
        
        exceptionThrown.Message.Should().Be("Repository exception");
    }
}
