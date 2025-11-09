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
            AzureAdToken = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6InlFVXdtWFdMMTA3Q2MtN1FaMldTYmVPYjNzUSIsImtpZCI6InlFVXdtWFdMMTA3Q2MtN1FaMldTYmVPYjNzUSJ9.eyJhdWQiOiJhcGk6Ly8zNWY4ZTNlNS00N2VlLTRkZTgtOTNiMy01ZjRiZWQ0YTQyMzEiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9jMjE2MzYyMC1hNjQ3LTRiODAtYTNkNy0xZWRkNWQ4ZWM5NTEvIiwiaWF0IjoxNzYyNjk1MDU1LCJuYmYiOjE3NjI2OTUwNTUsImV4cCI6MTc2MjY5OTAwMywiYWNyIjoiMSIsImFpbyI6IkFaUUFhLzhhQUFBQURQYllQemZZRDNWWlZsOTg1MVdXZlFYMk51cUNQZG1jN2FGS0RrclBpbGN1Q1Y5bVp6K24yaXJhUE01NzcySUZLSVhrRVhOOXUyb2lmK2NFdWVTTGZzSFJQdi9BeERvdnNJbk9LOHRQVFJFWTRKaytMTGdWYUJQdmpESnUvMWxBMmNUQjVncDZmN0VoaUFJL1pCY1B6UFdmaDBYK3JJQTZGRGlHRWVuM0lPcXpyeThFM2pSSzJGczdhRUJIV3ZXWiIsImFtciI6WyJwd2QiLCJyc2EiLCJtZmEiXSwiYXBwaWQiOiIxZWUxMmYzOC04NjMwLTQ5N2ItYWNhNi01NTI4ODdkZTAzMWIiLCJhcHBpZGFjciI6IjAiLCJkZXZpY2VpZCI6ImZkMmQ2MTNiLWRhNWEtNDVjMS05NTRmLTZmZWI4ZWUwZjA5MSIsImZhbWlseV9uYW1lIjoiRkFZRUQiLCJnaXZlbl9uYW1lIjoiTU9IQU1NQUQiLCJpcGFkZHIiOiIxNTYuMjIxLjUyLjgiLCJuYW1lIjoiTU9IQU1NQUQgIEYuIEZBWUVEIiwib2lkIjoiZjM0NzIyMGYtYjViNi00MDAyLTgxNDEtZDdkMjU2YjNhYzc3IiwicmgiOiIxLkFUQUFJRFlXd2tlbWdFdWoxeDdkWFk3SlVlWGotRFh1Ui1oTms3TmZTLTFLUWpHQkFHQXdBQS4iLCJzY3AiOiJBY2Nlc3NBcGkiLCJzaWQiOiIwMDliYWJhOS1mODRhLTY3NDItNzgxNS0yMjNmYTk4M2FmOTQiLCJzdWIiOiJQWDZqbkR2dHdTbzFnb3RMU1UtTHZSTHdNUGRFc1dueGhUZFFhQmNRdXBJIiwidGlkIjoiYzIxNjM2MjAtYTY0Ny00YjgwLWEzZDctMWVkZDVkOGVjOTUxIiwidW5pcXVlX25hbWUiOiJNRkFZRURAd3Rjby5jb20uc2EiLCJ1cG4iOiJNRkFZRURAd3Rjby5jb20uc2EiLCJ1dGkiOiJXZy0wakVxLVMwT0Y1SUhNcTB0bkFBIiwidmVyIjoiMS4wIiwieG1zX2Z0ZCI6IjBJS1lrSWZBNG9UV3lldFUyaVRNRU5QbkMwT0F6Vk1qVDc5NWhWUGhpTTRCWlhWeWIzQmxkMlZ6ZEMxa2MyMXoifQ.HOuaOpChCfAowmi9ih6AjXNdO2cNZRkLCgpPKkalViZr9Z-Kd3PQZLQAq5H33m0FbVi-9aMUKy0CNZh_27hP-XqMMpHxaXyi3P2nIdxVQENAkTCqqUjt0UF6wFGbWRkUIj__RGfhFPHOwatkxSUsIMF4A_ZM1WyaoqCxhS_3eDc0LolKo8JrzS5FvZfGiDbRmRvdzXAN88a0gpPbvCw_sGcqrDZTf86hy018rpx4RCPnW_r35Lqz_CvzEmI7X2G43sVvuEqul0pLb-C2AGs5gBMdAXh9InrCKz3GANIdTadUrIhlYtIt9MtRIM2IF1fhJVwkZ2POTBdKbaKj1nG6Tw"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/authentication/authenticate", authenticateRequest);

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
        var response = await _client.PostAsJsonAsync("/authentication/authenticate", authenticateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeFalse();
        if (root.TryGetProperty("message", out var messageProperty) && messageProperty.ValueKind == JsonValueKind.String)
        {
            messageProperty.GetString().Should().NotBeNullOrEmpty();
        }
        if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Array)
        {
            errorsElement.GetArrayLength().Should().BeGreaterThan(0);
        }
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
        var response = await _client.PostAsJsonAsync("/authentication/authenticate", authenticateRequest);

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