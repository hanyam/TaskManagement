using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Infrastructure.Data.EntityConfigurations;

/// <summary>
///     Entity Framework configuration for Task entity.
/// </summary>
public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<int>();

        builder.Property(e => e.Priority)
            .HasConversion<int>();

        builder.Property(e => e.Type)
            .HasConversion<int>();

        builder.Property(e => e.ReminderLevel)
            .HasConversion<int>();

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasOne(e => e.AssignedUser)
            .WithMany()
            .HasForeignKey(e => e.AssignedUserId)
            .IsRequired(false) // Allow null for draft tasks
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Assignments)
            .WithOne(a => a.Task)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ProgressHistory)
            .WithOne(ph => ph.Task)
            .HasForeignKey(ph => ph.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ExtensionRequests)
            .WithOne(er => er.Task)
            .HasForeignKey(er => er.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


