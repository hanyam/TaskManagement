using FluentAssertions;
using FluentValidation.TestHelper;
using TaskManagement.Application.Tasks.Commands.AssignTask;
using Xunit;

namespace TaskManagement.Tests.Unit.Application.Tasks.Commands;

/// <summary>
/// Unit tests for the AssignTaskCommandValidator.
/// </summary>
public class AssignTaskCommandValidatorTests
{
    private readonly AssignTaskCommandValidator _validator;

    public AssignTaskCommandValidatorTests()
    {
        _validator = new AssignTaskCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new AssignTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            AssignedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTaskId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AssignTaskCommand
        {
            TaskId = Guid.Empty,
            UserIds = new List<Guid> { Guid.NewGuid() },
            AssignedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
            .WithErrorMessage("Task ID is required");
    }

    [Fact]
    public void Validate_WithEmptyUserIds_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AssignTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserIds = new List<Guid>(),
            AssignedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserIds)
            .WithErrorMessage("At least one user must be assigned");
    }

    [Fact]
    public void Validate_WithNullUserIds_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AssignTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserIds = null!,
            AssignedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserIds)
            .WithErrorMessage("User IDs are required");
    }

    [Fact]
    public void Validate_WithEmptyUserIdInList_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AssignTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserIds = new List<Guid> { Guid.Empty, Guid.NewGuid() },
            AssignedById = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserIds)
            .WithErrorMessage("User ID cannot be empty");
    }

    [Fact]
    public void Validate_WithEmptyAssignedById_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AssignTaskCommand
        {
            TaskId = Guid.NewGuid(),
            UserIds = new List<Guid> { Guid.NewGuid() },
            AssignedById = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AssignedById)
            .WithErrorMessage("Assigned by user ID is required");
    }

    [Fact]
    public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new AssignTaskCommand
        {
            TaskId = Guid.Empty,
            UserIds = new List<Guid>(),
            AssignedById = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaskId);
        result.ShouldHaveValidationErrorFor(x => x.UserIds);
        result.ShouldHaveValidationErrorFor(x => x.AssignedById);
    }
}

