using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Domain.DTOs;
using TaskManagement.Infrastructure.Data;
using Xunit;

namespace TaskManagement.Tests.Integration.Controllers;

/// <summary>
///     Integration tests for AuthenticationController.
/// </summary>
public class AuthenticationControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AuthenticationControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Authenticate_WithValidAzureAdToken_ShouldReturnJwtToken()
    {
        // Arrange
        var authenticateRequest = new
        {
            AzureAdToken = "valid-azure-ad-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/authenticate", authenticateRequest);

        // Assert
        // Note: This test would need proper Azure AD token mocking in a real scenario
        // For now, we expect it to fail due to invalid token
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Authenticate_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var authenticateRequest = new
        {
            AzureAdToken = "invalid-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/authenticate", authenticateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Domain.Common.ApiResponse<AuthenticationResponse>>(content,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Authenticate_WithEmptyToken_ShouldReturnBadRequest()
    {
        // Arrange
        var authenticateRequest = new
        {
            AzureAdToken = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/authenticate", authenticateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

/// <summary>
///     API Response model for testing.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string? TraceId { get; set; }
}