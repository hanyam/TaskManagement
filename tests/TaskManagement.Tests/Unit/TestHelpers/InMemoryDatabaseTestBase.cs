using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskAssignment = TaskManagement.Domain.Entities.TaskAssignment;
using TaskProgressHistory = TaskManagement.Domain.Entities.TaskProgressHistory;
using DeadlineExtensionRequest = TaskManagement.Domain.Entities.DeadlineExtensionRequest;
using ManagerEmployee = TaskManagement.Domain.Entities.ManagerEmployee;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
///     Base class for unit tests that use in-memory database with real test data.
/// </summary>
public abstract class InMemoryDatabaseTestBase : IDisposable
{
    protected readonly List<Guid> TestTaskIds = new();
    protected readonly List<Guid> TestUserIds = new();

    protected InMemoryDatabaseTestBase()
    {
        var options = new DbContextOptionsBuilder<TaskManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new TaskManagementDbContext(options);

        // Seed test data
        SeedTestData();
    }

    protected TaskManagementDbContext Context { get; }

    private void SeedTestData()
    {
        // Create test users
        var testUsers = new[]
        {
            new User("john.doe@example.com", "John", "Doe", "azure-oid-john"),
            new User("jane.smith@example.com", "Jane", "Smith", "azure-oid-jane"),
            new User("bob.wilson@example.com", "Bob", "Wilson", "azure-oid-bob"),
            new User("alice.brown@example.com", "Alice", "Brown", "azure-oid-alice"),
            new User("charlie.davis@example.com", "Charlie", "Davis", "azure-oid-charlie")
        };

        // Set specific IDs for test users using reflection
        var userIds = new[]
        {
            new Guid("11111111-1111-1111-1111-111111111111"), // john.doe@example.com
            new Guid("22222222-2222-2222-2222-222222222222"), // jane.smith@example.com
            new Guid("33333333-3333-3333-3333-333333333333"), // bob.wilson@example.com
            new Guid("44444444-4444-4444-4444-444444444444"), // alice.brown@example.com
            new Guid("55555555-5555-5555-5555-555555555555") // charlie.davis@example.com
        };

        for (int i = 0; i < testUsers.Length; i++)
        {
            typeof(User).BaseType!.GetProperty("Id")!.SetValue(testUsers[i], userIds[i]);
            testUsers[i].SetCreatedBy("test@example.com");
            TestUserIds.Add(userIds[i]);
        }

        // Add users to database
        Context.Users.AddRange(testUsers);

        // Create some test tasks
        var testTasks = new[]
        {
            new Task(
                "Complete project documentation",
                "Write comprehensive documentation for the new API endpoints",
                TaskPriority.High,
                DateTime.UtcNow.AddDays(7),
                (Guid?)userIds[0], // John Doe - cast to nullable
                TaskType.WithProgress,
                userIds[0] // Created by John Doe
            ),
            new Task(
                "Review code changes",
                "Review the latest pull requests and provide feedback",
                TaskPriority.Medium,
                DateTime.UtcNow.AddDays(3),
                (Guid?)userIds[1], // Jane Smith - cast to nullable
                TaskType.Simple,
                userIds[0] // Created by John Doe
            ),
            new Task(
                "Update dependencies",
                "Update all project dependencies to latest versions",
                TaskPriority.Low,
                DateTime.UtcNow.AddDays(14),
                (Guid?)userIds[2], // Bob Wilson - cast to nullable
                TaskType.WithDueDate,
                userIds[1] // Created by Jane Smith
            )
        };

        // Set specific IDs for test tasks using reflection
        var taskIds = new[]
        {
            new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc")
        };

        for (int i = 0; i < testTasks.Length; i++)
        {
            typeof(Task).BaseType!.GetProperty("Id")!.SetValue(testTasks[i], taskIds[i]);
            TestTaskIds.Add(taskIds[i]);
        }

        // Add tasks to database
        Context.Tasks.AddRange(testTasks);

        // Save all changes
        Context.SaveChanges();
    }

    /// <summary>
    ///     Gets a test user by email.
    /// </summary>
    protected User GetTestUser(string email)
    {
        return Context.Users.First(u => u.Email == email);
    }

    /// <summary>
    ///     Gets a test user by ID.
    /// </summary>
    protected User GetTestUser(Guid id)
    {
        return Context.Users.First(u => u.Id == id);
    }

    /// <summary>
    ///     Gets all test users.
    /// </summary>
    protected List<User> GetAllTestUsers()
    {
        return Context.Users.ToList();
    }

    /// <summary>
    ///     Gets a test task by ID.
    /// </summary>
    protected Task GetTestTask(Guid id)
    {
        return Context.Tasks.First(t => t.Id == id);
    }

    /// <summary>
    ///     Gets all test tasks.
    /// </summary>
    protected List<Task> GetAllTestTasks()
    {
        return Context.Tasks.ToList();
    }

    /// <summary>
    ///     Creates a new user for testing.
    /// </summary>
    protected User CreateTestUser(string email, string firstName, string lastName, string? azureAdObjectId = null)
    {
        var user = new User(email, firstName, lastName, azureAdObjectId);
        user.SetCreatedBy("test@example.com");
        Context.Users.Add(user);
        Context.SaveChanges();
        TestUserIds.Add(user.Id);
        return user;
    }

    /// <summary>
    ///     Creates a new task for testing.
    /// </summary>
    protected Task CreateTestTask(
        string title,
        string? description,
        TaskPriority priority,
        DateTime? dueDate,
        Guid? assignedUserId, // Now nullable to support draft tasks
        TaskType type = TaskType.Simple,
        Guid? createdById = null)
    {
        var creatorId = createdById ?? assignedUserId ?? Guid.NewGuid(); // Use a new GUID if both are null
        var task = new Task(title, description, priority, dueDate, assignedUserId, type, creatorId);
        task.SetCreatedBy("test@example.com");
        Context.Tasks.Add(task);
        Context.SaveChanges();
        TestTaskIds.Add(task.Id);
        return task;
    }

    /// <summary>
    ///     Creates a task assignment for testing.
    /// </summary>
    protected TaskAssignment CreateTestAssignment(Guid taskId, Guid userId, bool isPrimary = false)
    {
        var assignment = new TaskAssignment(taskId, userId, isPrimary);
        assignment.SetCreatedBy("test@example.com");
        Context.Set<TaskAssignment>().Add(assignment);
        Context.SaveChanges();
        return assignment;
    }

    /// <summary>
    ///     Creates task progress history entry for testing.
    /// </summary>
    protected TaskProgressHistory CreateTestProgressHistory(Guid taskId, Guid updatedById, int progressPercentage,
        string? notes = null)
    {
        var progressHistory = new TaskProgressHistory(taskId, updatedById, progressPercentage, notes);
        progressHistory.SetCreatedBy("test@example.com");
        Context.Set<TaskProgressHistory>().Add(progressHistory);
        Context.SaveChanges();
        return progressHistory;
    }

    /// <summary>
    ///     Creates a deadline extension request for testing.
    /// </summary>
    protected DeadlineExtensionRequest CreateTestExtensionRequest(Guid taskId, Guid requestedById,
        DateTime requestedDueDate, string reason)
    {
        var extensionRequest = new DeadlineExtensionRequest(taskId, requestedById, requestedDueDate, reason);
        extensionRequest.SetCreatedBy("test@example.com");
        Context.Set<DeadlineExtensionRequest>().Add(extensionRequest);
        Context.SaveChanges();
        return extensionRequest;
    }

    /// <summary>
    ///     Sets the role of a test user.
    /// </summary>
    protected void SetUserRole(Guid userId, UserRole role)
    {
        var user = GetTestUser(userId);
        user.UpdateRole(role);
        Context.SaveChanges();
    }

    /// <summary>
    ///     Gets a test user and sets their role.
    /// </summary>
    protected User GetTestUserWithRole(string email, UserRole role)
    {
        var user = GetTestUser(email);
        user.UpdateRole(role);
        Context.SaveChanges();
        return user;
    }

    /// <summary>
    ///     Creates a manager-employee relationship for testing.
    /// </summary>
    protected ManagerEmployee CreateManagerEmployeeRelationship(Guid managerId, Guid employeeId)
    {
        var relationship = new ManagerEmployee(managerId, employeeId);
        relationship.SetCreatedBy("test@example.com");
        Context.Set<ManagerEmployee>().Add(relationship);
        Context.SaveChanges();
        return relationship;
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}