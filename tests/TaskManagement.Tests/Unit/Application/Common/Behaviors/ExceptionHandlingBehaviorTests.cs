using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.Common.Behaviors;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using Xunit;

namespace TaskManagement.Tests.Unit.Application.Common.Behaviors;

/// <summary>
///     Unit tests for the ExceptionHandlingBehavior class.
/// </summary>
public class ExceptionHandlingBehaviorTests
{
    private readonly ExceptionHandlingBehavior<TestRequest, string> _behavior;
    private readonly Mock<ILogger<ExceptionHandlingBehavior<TestRequest, string>>> _mockLogger;

    public ExceptionHandlingBehaviorTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingBehavior<TestRequest, string>>>();
        _behavior = new ExceptionHandlingBehavior<TestRequest, string>(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithSuccessfulNext_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new TestRequest { TestProperty = "Test" };
        var expectedResult = Result<string>.Success("Success");

        // Act
        var result = await _behavior.Handle(request, () => Task.FromResult(expectedResult), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithFailedNext_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new TestRequest { TestProperty = "Test" };
        var expectedError = Error.Validation("Test validation error");
        var expectedResult = Result<string>.Failure(expectedError);

        // Act
        var result = await _behavior.Handle(request, () => Task.FromResult(expectedResult), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnInternalError()
    {
        // Arrange
        var request = new TestRequest { TestProperty = "Test" };
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = await _behavior.Handle(request, () => throw exception, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INTERNAL_ERROR");
        result.Error.Message.Should().Contain("An unexpected error occurred while processing your request");
    }

    [Fact]
    public async Task Handle_WithException_ShouldLogError()
    {
        // Arrange
        var request = new TestRequest { TestProperty = "Test" };
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _behavior.Handle(request, () => throw exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An exception occurred while processing request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentExceptionTypes_ShouldReturnInternalError()
    {
        // Arrange
        var request = new TestRequest { TestProperty = "Test" };
        var exceptions = new Exception[]
        {
            new ArgumentException("Argument exception"),
            new NullReferenceException("Null reference exception"),
            new TimeoutException("Timeout exception"),
            new UnauthorizedAccessException("Unauthorized access exception")
        };

        foreach (var exception in exceptions)
        {
            // Act
            var result = await _behavior.Handle(request, () => throw exception, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
            result.Error!.Code.Should().Be("INTERNAL_ERROR");
            result.Error.Message.Should().Contain("An unexpected error occurred while processing your request");
        }
    }

    [Fact]
    public async Task Handle_WithAggregateException_ShouldReturnInternalError()
    {
        // Arrange
        var request = new TestRequest { TestProperty = "Test" };
        var innerException1 = new InvalidOperationException("Inner exception 1");
        var innerException2 = new ArgumentException("Inner exception 2");
        var aggregateException = new AggregateException("Aggregate exception", innerException1, innerException2);

        // Act
        var result = await _behavior.Handle(request, () => Task.FromException<Result<string>>(aggregateException),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INTERNAL_ERROR");
        result.Error.Message.Should().Contain("An unexpected error occurred while processing your request");
    }
}

/// <summary>
///     Unit tests for the ExceptionHandlingBehavior class with void requests.
/// </summary>
public class ExceptionHandlingBehaviorVoidTests
{
    private readonly ExceptionHandlingBehavior<TestVoidRequest> _behavior;
    private readonly Mock<ILogger<ExceptionHandlingBehavior<TestVoidRequest>>> _mockLogger;

    public ExceptionHandlingBehaviorVoidTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingBehavior<TestVoidRequest>>>();
        _behavior = new ExceptionHandlingBehavior<TestVoidRequest>(_mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithSuccessfulNext_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new TestVoidRequest { TestProperty = "Test" };
        var expectedResult = Result.Success();

        // Act
        var result = await _behavior.Handle(request, () => Task.FromResult(expectedResult), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithFailedNext_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new TestVoidRequest { TestProperty = "Test" };
        var expectedError = Error.Validation("Test validation error");
        var expectedResult = Result.Failure(expectedError);

        // Act
        var result = await _behavior.Handle(request, () => Task.FromResult(expectedResult), CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnInternalError()
    {
        // Arrange
        var request = new TestVoidRequest { TestProperty = "Test" };
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = await _behavior.Handle(request, () => throw exception, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INTERNAL_ERROR");
        result.Error.Message.Should().Contain("An unexpected error occurred while processing your request");
    }

    [Fact]
    public async Task Handle_WithException_ShouldLogError()
    {
        // Arrange
        var request = new TestVoidRequest { TestProperty = "Test" };
        var exception = new InvalidOperationException("Test exception");

        // Act
        await _behavior.Handle(request, () => throw exception, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An exception occurred while processing request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

// Test request classes
public class TestRequest : IRequest<string>
{
    public string TestProperty { get; set; } = string.Empty;
}

public class TestVoidRequest : IRequest
{
    public string TestProperty { get; set; } = string.Empty;
}