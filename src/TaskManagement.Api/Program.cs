using Azure.Identity;
using Serilog;
using TaskManagement.Application;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Presentation;
using TaskManagement.Presentation.Extensions;
using TaskManagement.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with enhanced configuration
// Enrichment is configured in appsettings.json (WithMachineName, WithThreadId, WithCorrelationId)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Application", "TaskManagement.Api")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .CreateLogger();

builder.Host.UseSerilog();

// Register services by layer
builder.AddObservability();
builder.Services.AddPresentation(builder.Configuration); // API/Presentation layer
builder.Services.AddApplication(builder.Configuration); // Application layer
builder.Services.AddInfrastructure(builder.Configuration); // Infrastructure layer

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskManagementDbContext>();

var keyVaultUrl = builder.Configuration["AzureKeyVault:VaultUrl"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());

    // Note: The secret name in Azure Key Vault must be "ConnectionStrings--DefaultConnection"
    // The "--" maps to ":" in configuration, so Configuration.GetConnectionString("DefaultConnection") works.
    // Local development uses developer identity via DefaultAzureCredential (VS/Azure CLI login).
    // For production, use a managed identity.
}
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

// Add correlation ID middleware (must be early in pipeline)
app.UseMiddleware<CorrelationIdMiddleware>();

// Add request logging middleware
app.UseMiddleware<RequestLoggingMiddleware>();

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