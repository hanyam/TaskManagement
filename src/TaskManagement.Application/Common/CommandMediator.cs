using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Common;

/// <summary>
///     Command mediator implementation that handles commands with pipeline behaviors.
/// </summary>
public class CommandMediator : ICommandMediator
{
    private readonly ILogger<CommandMediator> _logger;
    private readonly IServiceLocator _serviceLocator;

    public CommandMediator(IServiceLocator serviceLocator, ILogger<CommandMediator> logger)
    {
        _serviceLocator = serviceLocator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing command of type {CommandType}", command.GetType().Name);

            // Get the handler type dynamically
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
            var handler = _serviceLocator.GetRequiredService(handlerType);

            // Build the pipeline with behaviors
            var pipeline = BuildPipeline<ICommand<TResponse>, TResponse>(command, handler, cancellationToken);

            // Execute the pipeline
            var result = await pipeline();

            if (result.IsSuccess)
                _logger.LogInformation("Command of type {CommandType} processed successfully", command.GetType().Name);
            else
                _logger.LogWarning("Command of type {CommandType} failed: {Error}", command.GetType().Name,
                    result.Error);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command of type {CommandType}", command.GetType().Name);
            return Result<TResponse>.Failure(
                Error.Internal($"An error occurred while processing the command: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing command of type {CommandType}", command.GetType().Name);

            // Get the handler type dynamically
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
            var handler = _serviceLocator.GetRequiredService(handlerType);

            // Build the pipeline with behaviors
            var pipeline = BuildPipeline<ICommand>(command, handler, cancellationToken);

            // Execute the pipeline
            var result = await pipeline();

            if (result.IsSuccess)
                _logger.LogInformation("Command of type {CommandType} processed successfully", command.GetType().Name);
            else
                _logger.LogWarning("Command of type {CommandType} failed: {Error}", command.GetType().Name,
                    result.Error);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing command of type {CommandType}", command.GetType().Name);
            return Result.Failure(Error.Internal($"An error occurred while processing the command: {ex.Message}"));
        }
    }

    private Func<Task<Result<TResponse>>> BuildPipeline<TCommand, TResponse>(TCommand command, object handler,
        CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        // Get the handle method
        var handleMethod = handler.GetType().GetMethod("Handle");
        if (handleMethod == null)
            throw new InvalidOperationException($"Handler {handler.GetType().Name} does not have a Handle method");

        // Create the base handler function
        Func<Task<Result<TResponse>>> handlerFunc = async () =>
        {
            var task = (Task<Result<TResponse>>)handleMethod.Invoke(handler,
                new object[] { command, cancellationToken })!;
            return await task;
        };

        // Build pipeline with built-in behaviors: Logging -> Validation -> Command Handler
        var pipeline = handlerFunc;

        // 1. Add Exception Handling (outermost)
        pipeline = WrapWithExceptionHandling(command, pipeline, cancellationToken);

        // 2. Add Validation (middle)
        pipeline = WrapWithValidation(command, pipeline, cancellationToken);

        // 3. Add Logging (innermost, closest to handler)
        pipeline = WrapWithLogging(command, pipeline, cancellationToken);

        return pipeline;
    }

    private Func<Task<Result>> BuildPipeline<TCommand>(TCommand command, object handler,
        CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        // Get the handle method
        var handleMethod = handler.GetType().GetMethod("Handle");
        if (handleMethod == null)
            throw new InvalidOperationException($"Handler {handler.GetType().Name} does not have a Handle method");

        // Create the base handler function
        Func<Task<Result>> handlerFunc = async () =>
        {
            var task = (Task<Result>)handleMethod.Invoke(handler, new object[] { command, cancellationToken })!;
            return await task;
        };

        // Build pipeline with built-in behaviors: Logging -> Validation -> Command Handler
        var pipeline = handlerFunc;

        // 1. Add Exception Handling (outermost)
        pipeline = WrapWithExceptionHandling(command, pipeline, cancellationToken);

        // 2. Add Validation (middle)
        pipeline = WrapWithValidation(command, pipeline, cancellationToken);

        // 3. Add Logging (innermost, closest to handler)
        pipeline = WrapWithLogging(command, pipeline, cancellationToken);

        return pipeline;
    }

    private Func<Task<Result<TResponse>>> WrapWithLogging<TCommand, TResponse>(TCommand command,
        Func<Task<Result<TResponse>>> next, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        return async () =>
        {
            _logger.LogInformation("Executing {CommandType} with {Command}", typeof(TCommand).Name, command);
            var result = await next();
            _logger.LogInformation("Executed {CommandType} with result: {IsSuccess}", typeof(TCommand).Name,
                result.IsSuccess);
            return result;
        };
    }

    private Func<Task<Result>> WrapWithLogging<TCommand>(TCommand command, Func<Task<Result>> next,
        CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        return async () =>
        {
            _logger.LogInformation("Executing {CommandType} with {Command}", typeof(TCommand).Name, command);
            var result = await next();
            _logger.LogInformation("Executed {CommandType} with result: {IsSuccess}", typeof(TCommand).Name,
                result.IsSuccess);
            return result;
        };
    }

    private Func<Task<Result<TResponse>>> WrapWithValidation<TCommand, TResponse>(TCommand command,
        Func<Task<Result<TResponse>>> next, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        return async () =>
        {
            // Get validators for this command type
            var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TCommand));
            var validators = _serviceLocator.GetServices(validatorType).Cast<IValidator<TCommand>>().ToList();

            if (validators.Any())
            {
                _logger.LogDebug("Validating command of type {CommandType}", typeof(TCommand).Name);

                var context = new ValidationContext<TCommand>(command);
                var validationResults =
                    await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Any())
                {
                    var errors = failures.Select(f => f.ErrorMessage).ToList();
                    _logger.LogWarning("Validation failed for command type {CommandType}. Errors: {Errors}",
                        typeof(TCommand).Name, string.Join(", ", errors));

                    var errorList = errors.Select(e => Error.Validation(e)).ToList();
                    return Result<TResponse>.Failure(errorList);
                }

                _logger.LogDebug("Validation passed for command type {CommandType}", typeof(TCommand).Name);
            }

            return await next();
        };
    }

    private Func<Task<Result>> WrapWithValidation<TCommand>(TCommand command, Func<Task<Result>> next,
        CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        return async () =>
        {
            // Get validators for this command type
            var validatorType = typeof(IValidator<>).MakeGenericType(typeof(TCommand));
            var validators = _serviceLocator.GetServices(validatorType).Cast<IValidator<TCommand>>().ToList();

            if (validators.Any())
            {
                _logger.LogDebug("Validating command of type {CommandType}", typeof(TCommand).Name);

                var context = new ValidationContext<TCommand>(command);
                var validationResults =
                    await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Any())
                {
                    var errors = failures.Select(f => f.ErrorMessage).ToList();
                    _logger.LogWarning("Validation failed for command type {CommandType}. Errors: {Errors}",
                        typeof(TCommand).Name, string.Join(", ", errors));

                    var errorList = errors.Select(e => Error.Validation(e)).ToList();
                    return Result.Failure(errorList);
                }

                _logger.LogDebug("Validation passed for command type {CommandType}", typeof(TCommand).Name);
            }

            return await next();
        };
    }

    private Func<Task<Result<TResponse>>> WrapWithExceptionHandling<TCommand, TResponse>(TCommand command,
        Func<Task<Result<TResponse>>> next, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        return async () =>
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing {CommandType}", typeof(TCommand).Name);
                return Result<TResponse>.Failure(
                    Error.Internal($"An error occurred while processing the command: {ex.Message}"));
            }
        };
    }

    private Func<Task<Result>> WrapWithExceptionHandling<TCommand>(TCommand command, Func<Task<Result>> next,
        CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        return async () =>
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing {CommandType}", typeof(TCommand).Name);
                return Result.Failure(Error.Internal($"An error occurred while processing the command: {ex.Message}"));
            }
        };
    }
}