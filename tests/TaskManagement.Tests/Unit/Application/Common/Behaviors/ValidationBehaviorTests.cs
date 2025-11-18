using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.Common.Behaviors;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using Xunit;

namespace TaskManagement.Tests.Unit.Application.Common.Behaviors;

/// <summary>
///     Unit tests for the ValidationBehavior class.
/// </summary>
public class ValidationBehaviorTests
{
    private readonly ValidationBehavior<ValidationTestRequest, string> _behavior;
    private readonly Mock<ILogger<ValidationBehavior<ValidationTestRequest, string>>> _mockLogger;
    private readonly List<IValidator<ValidationTestRequest>> _validators;

    public ValidationBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<ValidationBehavior<ValidationTestRequest, string>>>();
        _validators = new List<IValidator<ValidationTestRequest>>();
        _behavior = new ValidationBehavior<ValidationTestRequest, string>(_validators, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var request = new ValidationTestRequest { TestProperty = "Test" };
        var nextCalled = false;
        var expectedResult = Result<string>.Success("Success");

        // Act
        var result = await _behavior.Handle(request, () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        }, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var request = new ValidationTestRequest { TestProperty = "Valid" };
        var mockValidator = new Mock<IValidator<ValidationTestRequest>>();
        mockValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _validators.Add(mockValidator.Object);

        var nextCalled = false;
        var expectedResult = Result<string>.Success("Success");

        // Act
        var result = await _behavior.Handle(request, () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        }, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldReturnValidationErrors()
    {
        // Arrange
        var request = new ValidationTestRequest { TestProperty = "Invalid" };
        var mockValidator = new Mock<IValidator<ValidationTestRequest>>();
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("TestProperty", "Test property is invalid"));

        mockValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _validators.Add(mockValidator.Object);

        var nextCalled = false;

        // Act
        var result = await _behavior.Handle(request, () =>
        {
            nextCalled = true;
            return Task.FromResult(Result<string>.Success("Success"));
        }, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be("VALIDATION_ERROR");
        result.Errors[0].Message.Should().Be("Test property is invalid");
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldReturnAllErrors()
    {
        // Arrange
        var request = new ValidationTestRequest { TestProperty = "Invalid" };

        var mockValidator1 = new Mock<IValidator<ValidationTestRequest>>();
        var validationResult1 = new ValidationResult();
        validationResult1.Errors.Add(new ValidationFailure("TestProperty", "First error"));
        mockValidator1.Setup(v =>
                v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult1);

        var mockValidator2 = new Mock<IValidator<ValidationTestRequest>>();
        var validationResult2 = new ValidationResult();
        validationResult2.Errors.Add(new ValidationFailure("TestProperty", "Second error"));
        mockValidator2.Setup(v =>
                v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult2);

        _validators.Add(mockValidator1.Object);
        _validators.Add(mockValidator2.Object);

        var nextCalled = false;

        // Act
        var result = await _behavior.Handle(request, () =>
        {
            nextCalled = true;
            return Task.FromResult(Result<string>.Success("Success"));
        }, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Message.Should().Be("First error");
        result.Errors[1].Message.Should().Be("Second error");
    }

    [Fact]
    public async Task Handle_ShouldLogDebug_WhenNoValidators()
    {
        // Arrange
        var request = new ValidationTestRequest { TestProperty = "Test" };

        // Act
        await _behavior.Handle(request, () => Task.FromResult(Result<string>.Success("Success")),
            CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No validators found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogDebug_WhenValidationPasses()
    {
        // Arrange
        var request = new ValidationTestRequest { TestProperty = "Valid" };
        var mockValidator = new Mock<IValidator<ValidationTestRequest>>();
        mockValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _validators.Add(mockValidator.Object);

        // Act
        await _behavior.Handle(request, () => Task.FromResult(Result<string>.Success("Success")),
            CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validating request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation passed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogWarning_WhenValidationFails()
    {
        // Arrange
        var request = new ValidationTestRequest { TestProperty = "Invalid" };
        var mockValidator = new Mock<IValidator<ValidationTestRequest>>();
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("TestProperty", "Test property is invalid"));

        mockValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ValidationContext<ValidationTestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        _validators.Add(mockValidator.Object);

        // Act
        await _behavior.Handle(request, () => Task.FromResult(Result<string>.Success("Success")),
            CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

// Test request class
public class ValidationTestRequest : IRequest<string>
{
    public string TestProperty { get; set; } = string.Empty;
}