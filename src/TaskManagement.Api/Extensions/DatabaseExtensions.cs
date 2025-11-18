using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Api.Extensions;

/// <summary>
///     Extension methods for database operations.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    ///     Applies any pending migrations to the database.
    ///     This method is idempotent and safe to call on every application startup.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ApplyMigrations(this WebApplication app)
    {
        // Skip in test environment
        if (app.Environment.IsEnvironment("Testing")) return app;

        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<TaskManagementDbContext>();
            var pendingMigrations = context.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                logger.LogInformation(
                    "Applying {Count} pending migration(s): {Migrations}",
                    pendingMigrations.Count,
                    string.Join(", ", pendingMigrations)
                );

                context.Database.Migrate();

                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                logger.LogInformation("No pending migrations to apply");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations");
            throw;
        }

        return app;
    }
}