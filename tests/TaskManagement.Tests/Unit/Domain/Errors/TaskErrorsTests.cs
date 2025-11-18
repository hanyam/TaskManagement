using FluentAssertions;
using TaskManagement.Domain.Errors.Tasks;
using Xunit;

namespace TaskManagement.Tests.Unit.Domain.Errors;

/// <summary>
///     Unit tests for TaskErrors centralized error management.
/// </summary>
public class TaskErrorsTests
{
    [Fact]
    public void TaskErrors_ShouldHaveConsistentErrorStructure()
    {
        // Act & Assert
        var notFound = TaskErrors.NotFound;
        notFound.Code.Should().Be("NOT_FOUND");
        notFound.Message.Should().Be("Task not found");
        notFound.Field.Should().Be("Id");
    }

    [Fact]
    public void TaskErrors_ShouldHaveValidationErrors()
    {
        // Act & Assert
        var titleRequired = TaskErrors.TitleRequired;
        titleRequired.Code.Should().Be("VALIDATION_ERROR");
        titleRequired.Message.Should().Be("Title is required");
        titleRequired.Field.Should().Be("Title");

        var dueDateInPast = TaskErrors.DueDateInPast;
        dueDateInPast.Code.Should().Be("VALIDATION_ERROR");
        dueDateInPast.Message.Should().Be("Due date cannot be in the past");
        dueDateInPast.Field.Should().Be("DueDate");
    }

    [Fact]
    public void TaskErrors_ShouldHaveBusinessLogicErrors()
    {
        // Act & Assert
        var cannotUpdateCompletedTask = TaskErrors.CannotUpdateCompletedTask;
        cannotUpdateCompletedTask.Code.Should().Be("VALIDATION_ERROR");
        cannotUpdateCompletedTask.Message.Should().Be("Cannot update a completed task");
        cannotUpdateCompletedTask.Field.Should().Be("Status");

        var taskAlreadyCompleted = TaskErrors.TaskAlreadyCompleted;
        taskAlreadyCompleted.Code.Should().Be("CONFLICT");
        taskAlreadyCompleted.Message.Should().Be("Task is already completed");
        taskAlreadyCompleted.Field.Should().Be("Status");
    }

    [Fact]
    public void TaskErrors_ShouldHaveAssignmentErrors()
    {
        // Act & Assert
        var assignedUserNotFound = TaskErrors.AssignedUserNotFound;
        assignedUserNotFound.Code.Should().Be("NOT_FOUND");
        assignedUserNotFound.Message.Should().Be("Assigned user not found");
        assignedUserNotFound.Field.Should().Be("AssignedUserId");

        var cannotAssignToSelf = TaskErrors.CannotAssignToSelf;
        cannotAssignToSelf.Code.Should().Be("VALIDATION_ERROR");
        cannotAssignToSelf.Message.Should().Be("Cannot assign task to yourself");
        cannotAssignToSelf.Field.Should().Be("AssignedUserId");
    }

    [Fact]
    public void TaskErrors_ShouldHavePaginationErrors()
    {
        // Act & Assert
        var invalidPageNumber = TaskErrors.InvalidPageNumber;
        invalidPageNumber.Code.Should().Be("VALIDATION_ERROR");
        invalidPageNumber.Message.Should().Be("Page must be greater than 0");
        invalidPageNumber.Field.Should().Be("Page");

        var invalidPageSize = TaskErrors.InvalidPageSize;
        invalidPageSize.Code.Should().Be("VALIDATION_ERROR");
        invalidPageSize.Message.Should().Be("Page size must be between 1 and 100");
        invalidPageSize.Field.Should().Be("PageSize");
    }

    [Fact]
    public void TaskErrors_ShouldHavePermissionErrors()
    {
        // Act & Assert
        var cannotUpdateOtherUserTask = TaskErrors.CannotUpdateOtherUserTask;
        cannotUpdateOtherUserTask.Code.Should().Be("FORBIDDEN");
        cannotUpdateOtherUserTask.Message.Should().Be("Cannot update task created by another user");
        cannotUpdateOtherUserTask.Field.Should().BeNull();

        var cannotDeleteOtherUserTask = TaskErrors.CannotDeleteOtherUserTask;
        cannotDeleteOtherUserTask.Code.Should().Be("FORBIDDEN");
        cannotDeleteOtherUserTask.Message.Should().Be("Cannot delete task created by another user");
        cannotDeleteOtherUserTask.Field.Should().BeNull();
    }
}