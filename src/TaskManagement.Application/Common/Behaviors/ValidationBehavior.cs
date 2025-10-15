using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common.Behaviors;

/// <summary>
///     Pipeline behavior that validates requests before processing.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<TResponse>> Handle(TRequest request, Func<Task<Result<TResponse>>> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            _logger.LogDebug("No validators found for request type {RequestType}", typeof(TRequest).Name);
            return await next();
        }

        _logger.LogDebug("Validating request of type {RequestType}", typeof(TRequest).Name);

        var context = new ValidationContext<TRequest>(request);
        var validationResults =
            await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Any())
        {
            var errors = failures.Select(f => f.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for request type {RequestType}. Errors: {Errors}",
                typeof(TRequest).Name, string.Join(", ", errors));

            var errorList = errors.Select(e => Error.Validation(e)).ToList();
            return Result<TResponse>.Failure(errorList);
        }

        _logger.LogDebug("Validation passed for request type {RequestType}", typeof(TRequest).Name);
        return await next();
    }
}

/// <summary>
///     Pipeline behavior that validates requests before processing (no response).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public class ValidationBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger<ValidationBehavior<TRequest>> _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(TRequest request, Func<Task<Result>> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            _logger.LogDebug("No validators found for request type {RequestType}", typeof(TRequest).Name);
            return await next();
        }

        _logger.LogDebug("Validating request of type {RequestType}", typeof(TRequest).Name);

        var context = new ValidationContext<TRequest>(request);
        var validationResults =
            await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

        if (failures.Any())
        {
            var errors = failures.Select(f => f.ErrorMessage).ToList();
            _logger.LogWarning("Validation failed for request type {RequestType}. Errors: {Errors}",
                typeof(TRequest).Name, string.Join(", ", errors));

            var errorList = errors.Select(e => Error.Validation(e)).ToList();
            return Result.Failure(errorList);
        }

        _logger.LogDebug("Validation passed for request type {RequestType}", typeof(TRequest).Name);
        return await next();
    }
}