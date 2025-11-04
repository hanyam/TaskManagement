using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TaskManagement.Infrastructure.Data;
using System.Linq;

namespace TaskManagement.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration tests that uses in-memory database only.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Data Source=:memory:"},
                {"Jwt:SecretKey", "supersecretkeythatisatleast32characterslong"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:ExpiryInHours", "1"},
                {"AzureAd:Issuer", "https://login.microsoftonline.com/test-tenant-id/v2.0"},
                {"AzureAd:ClientId", "test-client-id"},
                {"AzureAd:ClientSecret", "test-client-secret"},
                {"AzureAd:TenantId", "test-tenant-id"}
            });
        });

        // Configure services BEFORE they are registered by Program.cs
        // This ensures InMemory is registered first, and AddInfrastructure will skip SQL Server registration
        builder.ConfigureServices((context, services) =>
        {
            // Pre-register InMemory DbContext so AddInfrastructure will skip SQL Server registration
            services.AddDbContext<TaskManagementDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}", b => b.EnableNullChecks());
            }, ServiceLifetime.Scoped);
        });

        // Also configure services AFTER Program.cs registration to clean up any remaining SQL Server services
        builder.ConfigureServices((context, services) =>
        {
            // Remove any SQL Server provider-specific services that might have been registered
            // EF Core registers provider services internally, we need to find and remove them
            var sqlServerProviderServices = services
                .Where(d => 
                    (d.ImplementationType != null && 
                     d.ImplementationType.FullName != null &&
                     d.ImplementationType.FullName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)) ||
                    (d.ImplementationFactory != null && 
                     d.ImplementationFactory.Method.DeclaringType != null &&
                     d.ImplementationFactory.Method.DeclaringType.FullName != null &&
                     d.ImplementationFactory.Method.DeclaringType.FullName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)))
                .ToList();
            
            foreach (var descriptor in sqlServerProviderServices)
            {
                services.Remove(descriptor);
            }
        });

        builder.UseEnvironment("Testing");
    }
}
