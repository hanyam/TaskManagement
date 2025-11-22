using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Authentication;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;

namespace TaskManagement.Infrastructure;

/// <summary>
///     Dependency injection configuration for Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers all Infrastructure layer services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Entity Framework DbContext
        // Skip SQL Server registration if DbContext is already registered (e.g., in tests with InMemory)
        var dbContextRegistered = services.Any(s =>
            s.ServiceType == typeof(TaskManagementDbContext) ||
            s.ServiceType == typeof(DbContextOptions<TaskManagementDbContext>));

        if (!dbContextRegistered)
            services.AddDbContext<TaskManagementDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Dapper query repositories (CQRS pattern: queries use Dapper)
        services.AddScoped<TaskDapperRepository>();
        services.AddScoped<UserDapperRepository>();

        // Register EF Core command repositories (CQRS pattern: commands use EF Core)
        services.AddScoped<TaskEfCommandRepository>();
        services.AddScoped<UserEfCommandRepository>();

        // Register authentication service
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}