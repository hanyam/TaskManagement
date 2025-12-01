using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskHistory = TaskManagement.Domain.Entities.TaskHistory;

namespace TaskManagement.Infrastructure.Data.EntityConfigurations;

/// <summary>
///     Entity Framework configuration for TaskHistory entity.
/// </summary>
public class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.FromStatus)
            .HasConversion<int>();

        builder.Property(e => e.ToStatus)
            .HasConversion<int>();

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasOne(e => e.Task)
            .WithMany(t => t.History)
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PerformedByUser)
            .WithMany()
            .HasForeignKey(e => e.PerformedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for efficient queries
        builder.HasIndex(e => e.TaskId);
        builder.HasIndex(e => e.CreatedAt);
    }
}