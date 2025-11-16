using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data.EntityConfigurations;

/// <summary>
///     Entity Framework configuration for ManagerEmployee entity.
/// </summary>
public class ManagerEmployeeConfiguration : IEntityTypeConfiguration<ManagerEmployee>
{
    public void Configure(EntityTypeBuilder<ManagerEmployee> builder)
    {
        builder.HasKey(e => e.Id);

        // Relationships
        builder.HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Employee)
            .WithMany()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: A manager-employee relationship should be unique
        builder.HasIndex(e => new { e.ManagerId, e.EmployeeId })
            .IsUnique();

        // Prevent self-referencing (manager cannot be their own employee)
        builder.ToTable("ManagerEmployees", "Tasks", t => t.HasCheckConstraint(
            "CK_ManagerEmployee_NotSelf",
            "[ManagerId] != [EmployeeId]"));
    }
}

