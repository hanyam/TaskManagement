using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using Xunit;

namespace TaskManagement.Tests.Unit.Application.Common.Mediator;

/// <summary>
/// Unit tests for the Mediator class.
/// </summary>
public class MediatorTests
{
    private readonly Mock<IServiceLocator> _mockServiceLocator;
    private readonly Mock<ILogger<TaskManagement.Application.Common.Mediator>> _mockLogger;
    private readonly TaskManagement.Application.Common.Mediator _mediator;

    public MediatorTests()
    {
        _mockServiceLocator = new Mock<IServiceLocator>();
        _mockLogger = new Mock<ILogger<TaskManagement.Application.Common.Mediator>>();
        _mediator = new TaskManagement.Application.Common.Mediator(_mockServiceLocator.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Send_WithValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new TestRequest();
        var expectedResponse = "Test Response";
        var mockHandler = new Mock<IRequestHandler<TestRequest, string>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(expectedResponse));

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestRequest, string>>())
            .Returns(mockHandler.Object);

        // Act
        var result = await _mediator.Send(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedResponse);
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Send_WithHandlerFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new TestRequest();
        var expectedError = Error.Validation("Test validation error");
        var mockHandler = new Mock<IRequestHandler<TestRequest, string>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(expectedError));

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestRequest, string>>())
            .Returns(mockHandler.Object);

        // Act
        var result = await _mediator.Send(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task Send_WithHandlerException_ShouldReturnInternalError()
    {
        // Arrange
        var request = new TestRequest();
        var mockHandler = new Mock<IRequestHandler<TestRequest, string>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler exception"));

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestRequest, string>>())
            .Returns(mockHandler.Object);

        // Act
        var result = await _mediator.Send(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INTERNAL_ERROR");
        result.Error.Message.Should().Contain("Handler exception");
    }

    [Fact]
    public async Task Send_WithVoidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new TestVoidRequest();
        var mockHandler = new Mock<IRequestHandler<TestVoidRequest>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestVoidRequest>>())
            .Returns(mockHandler.Object);

        // Act
        var result = await _mediator.Send(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task Send_WithVoidRequestHandlerFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new TestVoidRequest();
        var expectedError = Error.Validation("Test validation error");
        var mockHandler = new Mock<IRequestHandler<TestVoidRequest>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(expectedError));

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestVoidRequest>>())
            .Returns(mockHandler.Object);

        // Act
        var result = await _mediator.Send(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expectedError);
    }

    [Fact]
    public async Task Send_WithVoidRequestHandlerException_ShouldReturnInternalError()
    {
        // Arrange
        var request = new TestVoidRequest();
        var mockHandler = new Mock<IRequestHandler<TestVoidRequest>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler exception"));

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestVoidRequest>>())
            .Returns(mockHandler.Object);

        // Act
        var result = await _mediator.Send(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("INTERNAL_ERROR");
        result.Error.Message.Should().Contain("Handler exception");
    }

    [Fact]
    public async Task Send_ShouldLogInformation_WhenRequestSucceeds()
    {
        // Arrange
        var request = new TestRequest();
        var mockHandler = new Mock<IRequestHandler<TestRequest, string>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("Success"));

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestRequest, string>>())
            .Returns(mockHandler.Object);

        // Act
        await _mediator.Send(request);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing request of type")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("processed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Send_ShouldLogWarning_WhenRequestFails()
    {
        // Arrange
        var request = new TestRequest();
        var expectedError = Error.Validation("Test error");
        var mockHandler = new Mock<IRequestHandler<TestRequest, string>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(expectedError));

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestRequest, string>>())
            .Returns(mockHandler.Object);

        // Act
        await _mediator.Send(request);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Send_ShouldLogError_WhenExceptionOccurs()
    {
        // Arrange
        var request = new TestRequest();
        var mockHandler = new Mock<IRequestHandler<TestRequest, string>>();
        mockHandler.Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        _mockServiceLocator.Setup(sp => sp.GetRequiredService<IRequestHandler<TestRequest, string>>())
            .Returns(mockHandler.Object);

        // Act
        await _mediator.Send(request);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error processing request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

// Test request classes
public class TestRequest : IRequest<string>
{
    public string TestProperty { get; set; } = "Test";
}

public class TestVoidRequest : IRequest
{
    public string TestProperty { get; set; } = "Test";
}
