using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Tests.TestHelpers;

/// <summary>
/// Helper class for creating DbContext options for testing.
/// </summary>
public static class DbContextTestHelper
{
    /// <summary>
    /// Creates DbContextOptions for ApplicationDbContext that can be used in unit tests.
    /// This properly sets up the ContextType to avoid the "ContextType is null" error.
    /// </summary>
    /// <returns>DbContextOptions configured for ApplicationDbContext</returns>
    public static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use in-memory database for testing
        optionsBuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
        
        // Set the context type explicitly to avoid the validation error
        optionsBuilder.EnableServiceProviderCaching(false);
        optionsBuilder.EnableSensitiveDataLogging();
        
        return optionsBuilder.Options;
    }

    /// <summary>
    /// Creates a mock ApplicationDbContext with properly configured options.
    /// </summary>
    /// <returns>Mock ApplicationDbContext</returns>
    public static Mock<ApplicationDbContext> CreateMockDbContext()
    {
        var options = CreateOptions();
        return new Mock<ApplicationDbContext>(options);
    }
}
