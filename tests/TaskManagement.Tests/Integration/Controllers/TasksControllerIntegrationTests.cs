using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Tests.Integration.Auth;
using Xunit;
using Task = System.Threading.Tasks.Task;
using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Tests.Integration.Controllers;

/// <summary>
///     Integration tests for TasksController.
/// </summary>
public class TasksControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private readonly Guid _existingUserId;
    private readonly Guid _existingTaskId;

    public TasksControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

        context.Database.EnsureDeleted();
        context.Database.Migrate();
        (_existingUserId, _existingTaskId) = EnsureSeedData(context);

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldReturnCreatedTask()
    {
        // Arrange
        var createTaskRequest = new
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = _existingUserId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Domain.Common.ApiResponse<TaskDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be(createTaskRequest.Title);
        result.Data.Description.Should().Be(createTaskRequest.Description);
        result.Data.Priority.Should().Be(createTaskRequest.Priority);
    }

    [Fact]
    public async Task CreateTask_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var createTaskRequest = new
        {
            Title = "", // Invalid: empty title
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = _existingUserId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTasks_ShouldReturnTasksList()
    {
        // Act
        var response = await _client.GetAsync("/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Domain.Common.ApiResponse<GetTasksResponse>>(content,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Tasks.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTaskById_WithValidId_ShouldReturnTask()
    {
        // Arrange
        var taskId = _existingTaskId;

        // Act
        var response = await _client.GetAsync($"/tasks/{taskId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private (Guid userId, Guid taskId) EnsureSeedData(TaskManagementDbContext context)
    {
        var user = context.Users.FirstOrDefault(u => u.Id == TestAuthHandler.TestUserId);
        if (user == null)
        {
            user = new User(TestAuthHandler.TestUserEmail, "Integration", "User", "integration-azure-oid");
            var idProperty = typeof(User).GetProperty(nameof(User.Id), BindingFlags.Public | BindingFlags.Instance);
            idProperty!.SetValue(user, TestAuthHandler.TestUserId);
            user.SetCreatedBy("integration-tests");
            user.UpdateRole(UserRole.Manager);

            context.Users.Add(user);
            context.SaveChanges();
        }

        var task = context.Tasks.FirstOrDefault(t => t.AssignedUserId == TestAuthHandler.TestUserId);
        if (task == null)
        {
            task = new DomainTask(
                "Seeded Integration Task",
                "Seeded task used for integration tests",
                TaskPriority.Medium,
                DateTime.UtcNow.AddDays(3),
                TestAuthHandler.TestUserId,
                TaskType.Simple,
                TestAuthHandler.TestUserId);
            task.SetCreatedBy(TestAuthHandler.TestUserEmail);

            context.Tasks.Add(task);
            context.SaveChanges();
        }

        return (TestAuthHandler.TestUserId, task.Id);
    }
}

/// <summary>
///     GetTasks response model for testing.
/// </summary>
public class GetTasksResponse
{
    public List<TaskDto> Tasks { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}