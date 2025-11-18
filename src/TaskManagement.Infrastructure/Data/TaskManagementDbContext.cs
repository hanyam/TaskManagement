using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Infrastructure.Data;

/// <summary>
///     Entity Framework DbContext for the Task Management module.
/// </summary>
public class TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<TaskAssignment> TaskAssignments { get; set; } = null!;
    public DbSet<TaskProgressHistory> TaskProgressHistory { get; set; } = null!;
    public DbSet<DeadlineExtensionRequest> DeadlineExtensionRequests { get; set; } = null!;
    public DbSet<ManagerEmployee> ManagerEmployees { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for all entities in this module
        modelBuilder.HasDefaultSchema("Tasks");

        // Apply all entity configurations from the current assembly
        // This automatically discovers and applies all IEntityTypeConfiguration<T> implementations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskManagementDbContext).Assembly);
    }
}