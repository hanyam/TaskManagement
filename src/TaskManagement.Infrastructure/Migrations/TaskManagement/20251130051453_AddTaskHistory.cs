#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagement.Infrastructure.Migrations.TaskManagement;

/// <inheritdoc />
public partial class AddTaskHistory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "TaskHistory",
            schema: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                TaskId = table.Column<Guid>("uniqueidentifier", nullable: false),
                FromStatus = table.Column<int>("int", nullable: false),
                ToStatus = table.Column<int>("int", nullable: false),
                Action = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                PerformedById = table.Column<Guid>("uniqueidentifier", nullable: false),
                Notes = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: true),
                CreatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaskHistory", x => x.Id);
                table.ForeignKey(
                    "FK_TaskHistory_Tasks_TaskId",
                    x => x.TaskId,
                    principalSchema: "Tasks",
                    principalTable: "Tasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_TaskHistory_Users_PerformedById",
                    x => x.PerformedById,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            "IX_TaskHistory_CreatedAt",
            schema: "Tasks",
            table: "TaskHistory",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            "IX_TaskHistory_PerformedById",
            schema: "Tasks",
            table: "TaskHistory",
            column: "PerformedById");

        migrationBuilder.CreateIndex(
            "IX_TaskHistory_TaskId",
            schema: "Tasks",
            table: "TaskHistory",
            column: "TaskId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "TaskHistory",
            "Tasks");
    }
}