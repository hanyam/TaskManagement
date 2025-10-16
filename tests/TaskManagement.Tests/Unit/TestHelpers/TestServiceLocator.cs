using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Behaviors;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Domain.DTOs;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Tests.Unit.TestHelpers;
using FluentValidation;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
/// Real service locator implementation for testing that provides actual services instead of mocks.
/// </summary>
public class TestServiceLocator : IServiceLocator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _context;

    public TestServiceLocator(IServiceProvider serviceProvider, ApplicationDbContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
    }

    public T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetService<T>() ?? throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public object? GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }

    public object GetRequiredService(Type serviceType)
    {
        // Handle specific types that need custom resolution
        if (serviceType == typeof(IRequestHandler<CreateTaskCommand, TaskDto>))
        {
            var taskCommandRepository = new TaskEfCommandRepository(_context);
            var userEfRepository = new UserEfQueryRepository(_context);
            var userQueryRepository = new UserDapperRepositoryWrapper(userEfRepository);
            return new CreateTaskCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        if (serviceType == typeof(IRequestHandler<GetTasksQuery, GetTasksResponse>))
        {
            var taskEfRepository = new TaskEfQueryRepository(_context);
            var taskRepository = new TaskDapperRepositoryWrapper(taskEfRepository);
            return new GetTasksQueryHandler(taskRepository);
        }

        if (serviceType == typeof(IRequestHandler<GetTaskByIdQuery, TaskDto>))
        {
            var taskEfRepository = new TaskEfQueryRepository(_context);
            var taskRepository = new TaskDapperRepositoryWrapper(taskEfRepository);
            return new GetTaskByIdQueryHandler(taskRepository);
        }

        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
        {
            var genericArgs = serviceType.GetGenericArguments();
            var requestType = genericArgs[0];
            var responseType = genericArgs[1];

            if (requestType == typeof(CreateTaskCommand) && responseType == typeof(TaskDto))
            {
                return GetCreateTaskPipelineBehaviors();
            }
        }

        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IValidator<>))
        {
            var requestType = serviceType.GetGenericArguments()[0];
            if (requestType == typeof(CreateTaskCommand))
            {
                return new CreateTaskCommandValidator();
            }
        }

        return _serviceProvider.GetRequiredService(serviceType);
    }

    public IEnumerable<object> GetServices(Type serviceType)
    {
        // Handle pipeline behaviors
        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
        {
            var genericArgs = serviceType.GetGenericArguments();
            var requestType = genericArgs[0];
            var responseType = genericArgs[1];

            if (requestType == typeof(CreateTaskCommand) && responseType == typeof(TaskDto))
            {
                return GetCreateTaskPipelineBehaviors();
            }
        }

        // Handle validators
        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IValidator<>))
        {
            var requestType = serviceType.GetGenericArguments()[0];
            if (requestType == typeof(CreateTaskCommand))
            {
                return new List<object> { new CreateTaskCommandValidator() };
            }
        }

        return _serviceProvider.GetServices(serviceType).Where(s => s != null)!;
    }

    private List<object> GetCreateTaskPipelineBehaviors()
    {
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        
        return new List<object>
        {
            new ValidationBehavior<CreateTaskCommand, TaskDto>(
                new List<IValidator<CreateTaskCommand>> { new CreateTaskCommandValidator() },
                loggerFactory.CreateLogger<ValidationBehavior<CreateTaskCommand, TaskDto>>()),
            new LoggingBehavior<CreateTaskCommand, TaskDto>(
                loggerFactory.CreateLogger<LoggingBehavior<CreateTaskCommand, TaskDto>>()),
            new ExceptionHandlingBehavior<CreateTaskCommand, TaskDto>(
                loggerFactory.CreateLogger<ExceptionHandlingBehavior<CreateTaskCommand, TaskDto>>())
        };
    }
}
