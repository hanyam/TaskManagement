using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.EntityConfigurations;

/// <summary>
///     Entity Framework configuration for TaskAttachment entity.
/// </summary>
public class TaskAttachmentConfiguration : IEntityTypeConfiguration<TaskAttachment>
{
    public void Configure(EntityTypeBuilder<TaskAttachment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FileSize)
            .IsRequired();

        builder.Property(e => e.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .HasConversion<int>();

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasOne(e => e.Task)
            .WithMany()
            .HasForeignKey(e => e.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.UploadedBy)
            .WithMany()
            .HasForeignKey(e => e.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.TaskId);
        builder.HasIndex(e => new { e.TaskId, e.Type });
    }
}