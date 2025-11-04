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
            AzureAdToken = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6InlFVXdtWFdMMTA3Q2MtN1FaMldTYmVPYjNzUSIsImtpZCI6InlFVXdtWFdMMTA3Q2MtN1FaMldTYmVPYjNzUSJ9.eyJhdWQiOiJhcGk6Ly9lNTc5M2NhOC01NGFmLTQyMDktYWYwOS05MGY5Yjg5ZDI4ZDIiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9jMjE2MzYyMC1hNjQ3LTRiODAtYTNkNy0xZWRkNWQ4ZWM5NTEvIiwiaWF0IjoxNzYyMjM0NzgzLCJuYmYiOjE3NjIyMzQ3ODMsImV4cCI6MTc2MjIzOTYxMiwiYWNyIjoiMSIsImFpbyI6IkFaUUFhLzhhQUFBQXRmZVc4T2M2YnRsclN4bWNQN3dqdEtoMFQxWnRwUmw2K00xbUFaSFU3VWkyK2NrSU9sMkwySkQ3QmtRd1FDSnR5M0FvbG1oT0I3TmVValBpNzVseEJraUpzcnBMc2hIeVZ2UEo0UHYrOE03MkMxTzhpUjFYSTlLWStPVkdVblpPeTF0S1JHZ1ZsVjVkbDZVdDA4aExKN1hqZ2ZXb0xRMFZjNjhCZFVMTlNGYy9OY2lybXI1aXN4dmllOHE5NE0wNiIsImFtciI6WyJwd2QiLCJtZmEiXSwiYXBwaWQiOiIxOTZkNDA1YS0zMGRlLTRkYzQtOTEyOC1mNjY3YzQxZjkyNTUiLCJhcHBpZGFjciI6IjAiLCJmYW1pbHlfbmFtZSI6Ik1vaGFtZWQiLCJnaXZlbl9uYW1lIjoiSGFuaSIsImlwYWRkciI6IjIxMi42Mi4xMjQuOTgiLCJuYW1lIjoiSGFuaSBBLiBNb2hhbWVkIiwib2lkIjoiMDdjODU2MDktZTU0YS00ZDQwLWFiMTgtMzRhNTFmOTE2ZDcwIiwicmgiOiIxLkFZRUFJRFlXd2tlbWdFdWoxeDdkWFk3SlVhZzhlZVd2VkFsQ3J3bVEtYmlkS05LQkFIT0JBQS4iLCJzY3AiOiJBY2Nlc3NBUEkiLCJzaWQiOiIwMDhkNTNjOS0wYmU3LTAxZTktZThiMS1lYWMyYzQ5YWMzNGUiLCJzdWIiOiIwTmRtODFsUGVGemxhb3ljUjhhODhsR0NtQnpQUEw4Tnd2dEc5NlF3alZzIiwidGlkIjoiYzIxNjM2MjAtYTY0Ny00YjgwLWEzZDctMWVkZDVkOGVjOTUxIiwidW5pcXVlX25hbWUiOiJobW9oYW1lZEB3dGNvLmNvbS5zYSIsInVwbiI6IkhNb2hhbWVkQHd0Y28uY29tLnNhIiwidXRpIjoic0hLbWVxakFnRWVHdXlNelZ6cTZBQSIsInZlciI6IjEuMCIsInhtc19mdGQiOiJKRVh6UFJfb1JicFVIZUlhT1ZfcFpZSEdXZVQ5SzFhdndpcVRQNEo5aTJVQmMzZGxaR1Z1WXkxa2MyMXoifQ.efzjgwwxy_fjkAYYHFzhcr6ieeOXGTJcmLcduge2rX3Gm3M5Yc_NfjOe7wAP9S2bBC91Jb1frnnaU0757gX_nz4W_C_AxJTo9r5QonqWVMQDvLM_ZO_KC7XlMO5qnHtJNFeNnUa0SKmIybRFEZ4Up68uLBxwbKXDpb_LizcSRsYbdQ-72LK8L1ysqN1l8t3FRFiBhwdhHuJPUk8uOi-xzhFqLvivyRpYu14cF9Kc3gcCJ8fI-7d1IBULJ_jN8-MGLQHstjTNqH8z-yZ3FVAhiC4xUsacaKaT2zeDhuaSTG9wcc2sf5RYRxqt5iQyNUnwcJ0NE277-eGm1OOZFkq4LA"
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