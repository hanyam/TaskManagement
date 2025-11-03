using FluentAssertions;
using TaskManagement.Domain.Common;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
/// Extension methods for asserting errors in test results.
/// </summary>
public static class ErrorAssertionExtensions
{
    /// <summary>
    /// Gets the error from a result (checks both Error property and Errors collection).
    /// </summary>
    public static Error? GetError<T>(this Result<T> result)
    {
        return result.Error ?? result.Errors.FirstOrDefault();
    }

    /// <summary>
    /// Gets the error from a result (checks both Error property and Errors collection).
    /// </summary>
    public static Error? GetError(this Result result)
    {
        return result.Error ?? result.Errors.FirstOrDefault();
    }

    /// <summary>
    /// Asserts that the result contains the specified error by comparing code and optionally field.
    /// </summary>
    public static void ShouldContainError(this Result result, Error expectedError)
    {
        var actualError = result.GetError();
        actualError.Should().NotBeNull();
        actualError!.Code.Should().Be(expectedError.Code, "because error codes should match");
        
        if (!string.IsNullOrEmpty(expectedError.Field))
        {
            actualError.Field.Should().Be(expectedError.Field, "because error fields should match");
        }
    }

    /// <summary>
    /// Asserts that the result contains the specified error by comparing code and optionally field.
    /// </summary>
    public static void ShouldContainError<T>(this Result<T> result, Error expectedError)
    {
        var actualError = result.GetError();
        actualError.Should().NotBeNull();
        actualError!.Code.Should().Be(expectedError.Code, "because error codes should match");
        
        if (!string.IsNullOrEmpty(expectedError.Field))
        {
            actualError.Field.Should().Be(expectedError.Field, "because error fields should match");
        }
    }

    /// <summary>
    /// Asserts that the result contains a validation error for the specified field.
    /// </summary>
    public static void ShouldContainValidationError(this Result result, string fieldName)
    {
        var validationErrorCode = Error.Validation("test", fieldName).Code;
        result.Errors.Should().NotBeEmpty("because validation errors should be present");
        result.Errors.Should().Contain(e => e.Code == validationErrorCode && e.Field == fieldName,
            $"because a validation error for field '{fieldName}' should be present");
    }

    /// <summary>
    /// Asserts that the result contains a validation error for the specified field.
    /// </summary>
    public static void ShouldContainValidationError<T>(this Result<T> result, string fieldName)
    {
        var validationErrorCode = Error.Validation("test", fieldName).Code;
        result.Errors.Should().NotBeEmpty("because validation errors should be present");
        result.Errors.Should().Contain(e => e.Code == validationErrorCode && e.Field == fieldName,
            $"because a validation error for field '{fieldName}' should be present");
    }

    /// <summary>
    /// Asserts that the result contains the specified error in the Errors collection.
    /// </summary>
    public static void ShouldContainErrorInCollection<T>(this Result<T> result, Error expectedError)
    {
        result.Errors.Should().Contain(e => 
            e.Code == expectedError.Code && 
            (string.IsNullOrEmpty(expectedError.Field) || e.Field == expectedError.Field),
            $"because the error {expectedError.Code} should be in the errors collection");
    }

    /// <summary>
    /// Asserts that the result contains the specified error in the Errors collection.
    /// </summary>
    public static void ShouldContainErrorInCollection(this Result result, Error expectedError)
    {
        result.Errors.Should().Contain(e => 
            e.Code == expectedError.Code && 
            (string.IsNullOrEmpty(expectedError.Field) || e.Field == expectedError.Field),
            $"because the error {expectedError.Code} should be in the errors collection");
    }
}

