using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Behaviors;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Application.Tasks.Services;
using TaskManagement.Domain.Options;

namespace TaskManagement.Application;

/// <summary>
///     Dependency injection configuration for Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers all Application layer services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assembly = typeof(AssemblyReference).Assembly;

        // Register custom mediators
        services.AddScoped<IServiceLocator, ServiceLocator>();
        services.AddScoped<ICommandMediator, CommandMediator>();
        services.AddScoped<IRequestMediator, RequestMediator>();

        // Note: Dapper and EF Core command repositories are registered in Infrastructure layer
        // (TaskManagement.Infrastructure.DependencyInjection.AddInfrastructure)

        // Register all command handlers (both ICommandHandler and IRequestHandler interfaces)
        // This is necessary because PipelineMediator uses IRequestHandler internally
        RegisterCommandHandlers(services, assembly);

        // Register all query handlers (IRequestHandler only, not command handlers)
        RegisterQueryHandlers(services, assembly);

        // Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register options
        services.Configure<ReminderOptions>(configuration.GetSection(ReminderOptions.SectionName));
        services.Configure<ExtensionPolicyOptions>(configuration.GetSection(ExtensionPolicyOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

        // Register business services
        services.AddScoped<IReminderCalculationService, ReminderCalculationService>();
        services.AddScoped<ITaskActionService,
            TaskActionService>();

        // Register audit logging service
        services.AddScoped<IAuditLogService, AuditLogService>();

        // Register current user, date, and settings services (with override support for testing)
        services.AddHttpContextAccessor(); // Required for CurrentUserService and UserSettingsService
        services.AddMemoryCache(); // Required for override mechanism
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentDateService, CurrentDateService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();

        // Register localization services
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<LocalizedErrorFactory>();

        return services;
    }

    /// <summary>
    ///     Registers command handlers for both ICommandHandler and IRequestHandler interfaces.
    /// </summary>
    private static void RegisterCommandHandlers(IServiceCollection services, Assembly assembly)
    {
        // Find all classes that implement ICommandHandler<,> or ICommandHandler<>
        var commandHandlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                           i.GetGenericTypeDefinition() == typeof(ICommandHandler<>))));

        foreach (var handlerType in commandHandlerTypes)
        {
            // Get all interfaces implemented by this handler
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                             i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                             i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                             i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
                .ToList();

            foreach (var interfaceType in interfaces) services.AddScoped(interfaceType, handlerType);
        }
    }

    /// <summary>
    ///     Registers query handlers (IRequestHandler only, excluding command handlers).
    /// </summary>
    private static void RegisterQueryHandlers(IServiceCollection services, Assembly assembly)
    {
        // Find all classes that implement IRequestHandler<,> but are NOT command handlers
        var queryHandlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .Where(t => !t.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                           i.GetGenericTypeDefinition() == typeof(ICommandHandler<>))));

        foreach (var handlerType in queryHandlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .ToList();

            foreach (var interfaceType in interfaces)
                // Only register if not already registered (shouldn't happen for pure query handlers)
                if (!services.Any(s => s.ServiceType == interfaceType && s.ImplementationType == handlerType))
                    services.AddScoped(interfaceType, handlerType);
        }
    }
}