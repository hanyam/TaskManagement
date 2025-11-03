using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registrations
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                     d.ServiceType == typeof(ApplicationDbContext)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        });

        builder.UseEnvironment("Testing");
    }
}
