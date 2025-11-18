using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Tests.TestHelpers;

/// <summary>
///     Helper class for creating DbContext options for testing.
/// </summary>
public static class DbContextTestHelper
{
    /// <summary>
    ///     Creates DbContextOptions for TaskManagementDbContext that can be used in unit tests.
    ///     This properly sets up the ContextType to avoid the "ContextType is null" error.
    /// </summary>
    /// <returns>DbContextOptions configured for TaskManagementDbContext</returns>
    public static DbContextOptions<TaskManagementDbContext> CreateOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<TaskManagementDbContext>();

        // Use in-memory database for testing
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        // Set the context type explicitly to avoid the validation error
        optionsBuilder.EnableServiceProviderCaching(false);
        optionsBuilder.EnableSensitiveDataLogging();

        return optionsBuilder.Options;
    }

    /// <summary>
    ///     Creates a mock TaskManagementDbContext with properly configured options.
    /// </summary>
    /// <returns>Mock TaskManagementDbContext</returns>
    public static Mock<TaskManagementDbContext> CreateMockDbContext()
    {
        var options = CreateOptions();
        return new Mock<TaskManagementDbContext>(options);
    }
}