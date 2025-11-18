using Serilog;
using TaskManagement.Api;
using TaskManagement.Api.Extensions;
using TaskManagement.Api.Middleware;
using TaskManagement.Application;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/taskmanagement-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Register services by layer
builder.Services.AddPresentation(builder.Configuration); // API/Presentation layer
builder.Services.AddApplication(builder.Configuration); // Application layer
builder.Services.AddInfrastructure(builder.Configuration); // Infrastructure layer

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskManagementDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// CORS must be before authentication/authorization
app.UseCors("AllowFrontend");

// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Apply any pending migrations automatically
app.ApplyMigrations();

try
{
    Log.Information("Starting Task Management API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program
{
}