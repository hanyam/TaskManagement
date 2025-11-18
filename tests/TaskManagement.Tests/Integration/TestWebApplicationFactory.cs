using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Tests.Integration.Auth;

namespace TaskManagement.Tests.Integration;

/// <summary>
///     Custom WebApplicationFactory for integration tests that target the real SQL Server database.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                                   ??
                                   "Server=localhost,1433;Database=TaskManagementIntegrationTests;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true;";

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString },
                { "Jwt:SecretKey", "supersecretkeythatisatleast32characterslong" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryInHours", "1" },
                { "AzureAd:Issuer", "https://login.microsoftonline.com/test-tenant-id/v2.0" },
                { "AzureAd:ClientId", "test-client-id" },
                { "AzureAd:ClientSecret", "test-client-secret" },
                { "AzureAd:TenantId", "test-tenant-id" }
            });
        });

        builder.ConfigureServices((context, services) =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });

        builder.UseEnvironment("Testing");
    }
}