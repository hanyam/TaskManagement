using FluentAssertions;
using FluentValidation.TestHelper;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Domain.Entities;
using Xunit;

namespace TaskManagement.Tests.Unit.Application.Tasks.Commands;

/// <summary>
/// Unit tests for the CreateTaskCommandValidator.
/// </summary>
public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator;

    public CreateTaskCommandValidatorTests()
    {
        _validator = new CreateTaskCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
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

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldHaveValidationError()
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

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_WithNullTitle_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = null!,
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = new string('A', 201), // 201 characters
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = new string('A', 1001), // 1001 characters
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 1000 characters");
    }

    [Fact]
    public void Validate_WithEmptyAssignedUserId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.Empty,
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AssignedUserId)
            .WithErrorMessage("Assigned user ID is required");
    }

    [Fact]
    public void Validate_WithEmptyCreatedBy_ShouldHaveValidationError()
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
            CreatedBy = ""
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy)
            .WithErrorMessage("Created by is required");
    }

    [Fact]
    public void Validate_WithNullCreatedBy_ShouldHaveValidationError()
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
            CreatedBy = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy)
            .WithErrorMessage("Created by is required");
    }

    [Fact]
    public void Validate_WithPastDueDate_ShouldHaveValidationError()
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

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("Due date must be in the future");
    }

    [Fact]
    public void Validate_WithNullDueDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = null, // Null is allowed
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Validate_WithFutureDueDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1), // Future date
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Validate_WithAllFieldsInvalid_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "", // Empty title
            Description = new string('A', 1001), // Too long description
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(-1), // Past date
            AssignedUserId = Guid.Empty, // Empty GUID
            CreatedById = Guid.Empty, // Empty created by id
            CreatedBy = "" // Empty created by
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
        result.ShouldHaveValidationErrorFor(x => x.AssignedUserId);
        result.ShouldHaveValidationErrorFor(x => x.CreatedById);
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy);
    }

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Medium)]
    [InlineData(TaskPriority.High)]
    [InlineData(TaskPriority.Critical)]
    public void Validate_WithValidPriorities_ShouldNotHaveValidationError(TaskPriority priority)
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = priority,
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedById = Guid.NewGuid(),
            CreatedBy = "test@example.com"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }
}
