using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Tasks.Commands.AcceptTask;
using TaskManagement.Application.Tasks.Commands.AcceptTaskProgress;
using TaskManagement.Application.Tasks.Commands.ApproveExtensionRequest;
using TaskManagement.Application.Tasks.Commands.AssignTask;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Application.Tasks.Commands.MarkTaskCompleted;
using TaskManagement.Application.Tasks.Commands.ReassignTask;
using TaskManagement.Application.Tasks.Commands.RejectTask;
using TaskManagement.Application.Tasks.Commands.RequestDeadlineExtension;
using TaskManagement.Application.Tasks.Commands.RequestMoreInfo;
using TaskManagement.Application.Tasks.Commands.UpdateTaskProgress;
using TaskManagement.Application.Tasks.Queries.GetAssignedTasks;
using TaskManagement.Application.Tasks.Queries.GetDashboardStats;
using TaskManagement.Application.Tasks.Queries.GetExtensionRequests;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Application.Tasks.Queries.GetTaskProgressHistory;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Application.Tasks.Queries.GetTasksByReminderLevel;
using TaskManagement.Application.Users.Queries.SearchManagedUsers;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Options;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
///     Real service locator implementation for testing that provides actual services instead of mocks.
///     Pipeline behaviors are now handled internally by PipelineMediator.
/// </summary>
public class TestServiceLocator : IServiceLocator
{
    private readonly TaskManagementDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public TestServiceLocator(IServiceProvider serviceProvider, TaskManagementDbContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
    }

    public T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetService<T>() ??
               throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
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
        // Create shared repositories
        var taskCommandRepository = new TaskEfCommandRepository(_context);
        var taskEfRepository = new TaskEfQueryRepository(_context);
        var userEfRepository = new UserEfQueryRepository(_context);
        var userQueryRepository = new UserDapperRepositoryWrapper(userEfRepository);
        var taskRepository = new TaskDapperRepositoryWrapper(taskEfRepository, _context);

        // Create ReminderCalculationService with default options
        var reminderOptions = Options.Create(new ReminderOptions());
        var reminderCalculationService = new ReminderCalculationService(reminderOptions);

        // Handle command handlers as both ICommandHandler and IRequestHandler (since PipelineMediator uses IRequestHandler)
        if (serviceType == typeof(ICommandHandler<CreateTaskCommand, TaskDto>) ||
            serviceType == typeof(IRequestHandler<CreateTaskCommand, TaskDto>))
        {
            return new CreateTaskCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        // Handle command handlers as both ICommandHandler and IRequestHandler
        if (serviceType == typeof(ICommandHandler<AssignTaskCommand, TaskDto>) ||
            serviceType == typeof(IRequestHandler<AssignTaskCommand, TaskDto>))
        {
            return new AssignTaskCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<UpdateTaskProgressCommand, TaskProgressDto>) ||
            serviceType == typeof(IRequestHandler<UpdateTaskProgressCommand, TaskProgressDto>))
        {
            return new UpdateTaskProgressCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<AcceptTaskProgressCommand>) ||
            serviceType == typeof(IRequestHandler<AcceptTaskProgressCommand>))
        {
            return new AcceptTaskProgressCommandHandler(taskCommandRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<AcceptTaskCommand, TaskDto>) ||
            serviceType == typeof(IRequestHandler<AcceptTaskCommand, TaskDto>))
        {
            return new AcceptTaskCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<RejectTaskCommand, TaskDto>) ||
            serviceType == typeof(IRequestHandler<RejectTaskCommand, TaskDto>))
        {
            return new RejectTaskCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<RequestMoreInfoCommand, TaskDto>) ||
            serviceType == typeof(IRequestHandler<RequestMoreInfoCommand, TaskDto>))
        {
            return new RequestMoreInfoCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<ReassignTaskCommand, TaskDto>) ||
            serviceType == typeof(IRequestHandler<ReassignTaskCommand, TaskDto>))
        {
            return new ReassignTaskCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<RequestDeadlineExtensionCommand, ExtensionRequestDto>) ||
            serviceType == typeof(IRequestHandler<RequestDeadlineExtensionCommand, ExtensionRequestDto>))
        {
            return new RequestDeadlineExtensionCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<ApproveExtensionRequestCommand>) ||
            serviceType == typeof(IRequestHandler<ApproveExtensionRequestCommand>))
        {
            return new ApproveExtensionRequestCommandHandler(taskCommandRepository, _context);
        }

        if (serviceType == typeof(ICommandHandler<MarkTaskCompletedCommand, TaskDto>) ||
            serviceType == typeof(IRequestHandler<MarkTaskCompletedCommand, TaskDto>))
        {
            return new MarkTaskCompletedCommandHandler(taskCommandRepository, userQueryRepository, _context);
        }

        // Handle query handlers
        if (serviceType == typeof(IRequestHandler<GetTasksQuery, GetTasksResponse>))
        {
            return new GetTasksQueryHandler(taskRepository);
        }

        if (serviceType == typeof(IRequestHandler<GetTaskByIdQuery, TaskDto>))
        {
            return new GetTaskByIdQueryHandler(taskRepository, _context);
        }

        if (serviceType == typeof(IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>))
        {
            return new GetDashboardStatsQueryHandler(taskRepository);
        }

        if (serviceType == typeof(IRequestHandler<GetTaskProgressHistoryQuery, List<TaskProgressDto>>))
        {
            return new GetTaskProgressHistoryQueryHandler(_context);
        }

        if (serviceType == typeof(IRequestHandler<GetExtensionRequestsQuery, List<ExtensionRequestDto>>))
        {
            return new GetExtensionRequestsQueryHandler(_context);
        }

        if (serviceType == typeof(IRequestHandler<GetAssignedTasksQuery, GetTasksResponse>))
        {
            return new GetAssignedTasksQueryHandler(_context);
        }

        if (serviceType == typeof(IRequestHandler<GetTasksByReminderLevelQuery, GetTasksResponse>))
        {
            return new GetTasksByReminderLevelQueryHandler(_context, reminderCalculationService);
        }

        if (serviceType == typeof(IRequestHandler<SearchManagedUsersQuery, List<UserSearchResultDto>>))
        {
            return new SearchManagedUsersQueryHandler(userQueryRepository);
        }

        // Handle services
        if (serviceType == typeof(IReminderCalculationService))
        {
            return reminderCalculationService;
        }

        // Pipeline behaviors are now handled internally by PipelineMediator

        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IValidator<>))
        {
            var requestType = serviceType.GetGenericArguments()[0];
            if (requestType == typeof(CreateTaskCommand))
            {
                return new CreateTaskCommandValidator();
            }

            if (requestType == typeof(AssignTaskCommand))
            {
                return new AssignTaskCommandValidator();
            }

            if (requestType == typeof(UpdateTaskProgressCommand))
            {
                return new UpdateTaskProgressCommandValidator();
            }

            if (requestType == typeof(RequestDeadlineExtensionCommand))
            {
                return new RequestDeadlineExtensionCommandValidator();
            }
        }

        return _serviceProvider.GetRequiredService(serviceType);
    }

    public IEnumerable<object> GetServices(Type serviceType)
    {
        // Pipeline behaviors are now handled internally by PipelineMediator

        // Handle validators
        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IValidator<>))
        {
            var requestType = serviceType.GetGenericArguments()[0];
            if (requestType == typeof(CreateTaskCommand))
            {
                return new List<object> { new CreateTaskCommandValidator() };
            }

            if (requestType == typeof(AssignTaskCommand))
            {
                return new List<object> { new AssignTaskCommandValidator() };
            }

            if (requestType == typeof(UpdateTaskProgressCommand))
            {
                return new List<object> { new UpdateTaskProgressCommandValidator() };
            }

            if (requestType == typeof(RequestDeadlineExtensionCommand))
            {
                return new List<object> { new RequestDeadlineExtensionCommandValidator() };
            }
        }

        return _serviceProvider.GetServices(serviceType).Where(s => s != null)!;
    }
}