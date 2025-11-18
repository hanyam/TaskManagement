#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagement.Infrastructure.Migrations.TaskManagement;

/// <inheritdoc />
public class AddManagerReviewFieldsActual : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add ManagerRating column if it doesn't exist
        migrationBuilder.AddColumn<int>(
            "ManagerRating",
            schema: "Tasks",
            table: "Tasks",
            type: "int",
            nullable: true);

        // Add ManagerFeedback column if it doesn't exist
        migrationBuilder.AddColumn<string>(
            "ManagerFeedback",
            schema: "Tasks",
            table: "Tasks",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            "ManagerRating",
            schema: "Tasks",
            table: "Tasks");

        migrationBuilder.DropColumn(
            "ManagerFeedback",
            schema: "Tasks",
            table: "Tasks");
    }
}