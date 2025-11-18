using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.Options;
using Xunit;

namespace TaskManagement.Tests.Unit.Domain.Options;

/// <summary>
///     Unit tests for AzureAdOptions configuration.
/// </summary>
public class AzureAdOptionsTests
{
    [Fact]
    public void AzureAdOptions_ShouldHaveCorrectSectionName()
    {
        // Act & Assert
        AzureAdOptions.SectionName.Should().Be("AzureAd");
    }

    [Fact]
    public void AzureAdOptions_ShouldBindFromConfiguration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAd:Issuer"] = "https://login.microsoftonline.com/test-tenant/v2.0",
                ["AzureAd:ClientId"] = "test-client-id",
                ["AzureAd:ClientSecret"] = "test-client-secret",
                ["AzureAd:TenantId"] = "test-tenant-id"
            })
            .Build();

        // Act
        var azureAdOptions = new AzureAdOptions();
        configuration.GetSection(AzureAdOptions.SectionName).Bind(azureAdOptions);

        // Assert
        azureAdOptions.Issuer.Should().Be("https://login.microsoftonline.com/test-tenant/v2.0");
        azureAdOptions.ClientId.Should().Be("test-client-id");
        azureAdOptions.ClientSecret.Should().Be("test-client-secret");
        azureAdOptions.TenantId.Should().Be("test-tenant-id");
    }

    [Fact]
    public void AzureAdOptions_ShouldHaveDefaultValues()
    {
        // Act
        var azureAdOptions = new AzureAdOptions();

        // Assert
        azureAdOptions.Issuer.Should().Be(string.Empty);
        azureAdOptions.ClientId.Should().Be(string.Empty);
        azureAdOptions.ClientSecret.Should().Be(string.Empty);
        azureAdOptions.TenantId.Should().Be(string.Empty);
    }
}