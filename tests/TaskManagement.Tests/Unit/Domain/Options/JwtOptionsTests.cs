using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.Options;
using Xunit;

namespace TaskManagement.Tests.Unit.Domain.Options;

/// <summary>
///     Unit tests for JwtOptions configuration.
/// </summary>
public class JwtOptionsTests
{
    [Fact]
    public void JwtOptions_ShouldHaveCorrectSectionName()
    {
        // Act & Assert
        JwtOptions.SectionName.Should().Be("Jwt");
    }

    [Fact]
    public void JwtOptions_ShouldBindFromConfiguration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:ExpiryInHours"] = "2"
            })
            .Build();

        // Act
        var jwtOptions = new JwtOptions();
        configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);

        // Assert
        jwtOptions.SecretKey.Should().Be("test-secret-key");
        jwtOptions.Issuer.Should().Be("test-issuer");
        jwtOptions.Audience.Should().Be("test-audience");
        jwtOptions.ExpiryInHours.Should().Be(2);
    }

    [Fact]
    public void JwtOptions_ShouldHaveDefaultValues()
    {
        // Act
        var jwtOptions = new JwtOptions();

        // Assert
        jwtOptions.SecretKey.Should().Be(string.Empty);
        jwtOptions.Issuer.Should().Be(string.Empty);
        jwtOptions.Audience.Should().Be(string.Empty);
        jwtOptions.ExpiryInHours.Should().Be(1);
    }
}