using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeadlineExtensionRequest = TaskManagement.Domain.Entities.DeadlineExtensionRequest;

namespace TaskManagement.Infrastructure.Data.EntityConfigurations;

/// <summary>
///     Entity Framework configuration for DeadlineExtensionRequest entity.
/// </summary>
public class DeadlineExtensionRequestConfiguration : IEntityTypeConfiguration<DeadlineExtensionRequest>
{
    public void Configure(EntityTypeBuilder<DeadlineExtensionRequest> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ReviewNotes)
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
            .WithMany(t => t.ExtensionRequests)
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.RequestedBy)
            .WithMany()
            .HasForeignKey(e => e.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReviewedBy)
            .WithMany()
            .HasForeignKey(e => e.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

