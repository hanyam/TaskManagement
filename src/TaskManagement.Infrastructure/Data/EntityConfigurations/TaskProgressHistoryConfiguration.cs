using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskProgressHistory = TaskManagement.Domain.Entities.TaskProgressHistory;

namespace TaskManagement.Infrastructure.Data.EntityConfigurations;

/// <summary>
///     Entity Framework configuration for TaskProgressHistory entity.
/// </summary>
public class TaskProgressHistoryConfiguration : IEntityTypeConfiguration<TaskProgressHistory>
{
    public void Configure(EntityTypeBuilder<TaskProgressHistory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<int>();

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasOne(e => e.Task)
            .WithMany(t => t.ProgressHistory)
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.UpdatedByUser)
            .WithMany()
            .HasForeignKey(e => e.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AcceptedByUser)
            .WithMany()
            .HasForeignKey(e => e.AcceptedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


