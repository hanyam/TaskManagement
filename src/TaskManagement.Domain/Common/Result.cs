namespace TaskManagement.Domain.Common;

/// <summary>
///     Represents the result of an operation that can either succeed or fail.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T>
{
    private Result(bool isSuccess, T? value, Error? error, List<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Errors = errors ?? new List<Error>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error? Error { get; }
    public List<Error> Errors { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Failure(Error error)
    {
        return new Result<T>(false, default, error);
    }

    public static Result<T> Failure(List<Error> errors)
    {
        return new Result<T>(false, default, null, errors);
    }

    public static Result<T> Failure(Error error, List<Error> additionalErrors)
    {
        return new Result<T>(false, default, error, additionalErrors);
    }

    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    public static implicit operator Result<T>(Error error)
    {
        return Failure(error);
    }

    public static implicit operator Result<T>(string errorMessage)
    {
        return Failure(Error.Validation(errorMessage));
    }
}

/// <summary>
///     Represents the result of an operation that can either succeed or fail without a return value.
/// </summary>
public class Result
{
    private Result(bool isSuccess, Error? error, List<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? new List<Error>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public List<Error> Errors { get; }

    public static Result Success()
    {
        return new Result(true, null);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    public static Result Failure(List<Error> errors)
    {
        return new Result(false, null, errors);
    }

    public static Result Failure(Error error, List<Error> additionalErrors)
    {
        return new Result(false, error, additionalErrors);
    }

    public static implicit operator Result(Error error)
    {
        return Failure(error);
    }

    public static implicit operator Result(string errorMessage)
    {
        return Failure(Error.Validation(errorMessage));
    }
}