#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagement.Infrastructure.Migrations.TaskManagement;

/// <inheritdoc />
public partial class AddManagerReviewColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add columns only if they don't exist (for existing databases)
        migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Tasks].[Tasks]') AND name = 'ManagerRating')
                BEGIN
                    ALTER TABLE [Tasks].[Tasks] ADD [ManagerRating] int NULL
                END
            ");

        migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Tasks].[Tasks]') AND name = 'ManagerFeedback')
                BEGIN
                    ALTER TABLE [Tasks].[Tasks] ADD [ManagerFeedback] nvarchar(1000) NULL
                END
            ");

        // Update max length if column already exists
        migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Tasks].[Tasks]') AND name = 'ManagerFeedback')
                BEGIN
                    ALTER TABLE [Tasks].[Tasks] ALTER COLUMN [ManagerFeedback] nvarchar(1000) NULL
                END
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            "ManagerFeedback",
            schema: "Tasks",
            table: "Tasks",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000,
            oldNullable: true);
    }
}