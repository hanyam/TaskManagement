#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagement.Infrastructure.Migrations.TaskManagement;

/// <inheritdoc />
public partial class AddTaskAttachments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "TaskAttachments",
            schema: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                TaskId = table.Column<Guid>("uniqueidentifier", nullable: false),
                FileName = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: false),
                OriginalFileName = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: false),
                ContentType = table.Column<string>("nvarchar(255)", maxLength: 255, nullable: false),
                FileSize = table.Column<long>("bigint", nullable: false),
                StoragePath = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: false),
                Type = table.Column<int>("int", nullable: false),
                UploadedById = table.Column<Guid>("uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: true),
                CreatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaskAttachments", x => x.Id);
                table.ForeignKey(
                    "FK_TaskAttachments_Tasks_TaskId",
                    x => x.TaskId,
                    principalSchema: "Tasks",
                    principalTable: "Tasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_TaskAttachments_Users_UploadedById",
                    x => x.UploadedById,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            "IX_TaskAttachments_TaskId",
            schema: "Tasks",
            table: "TaskAttachments",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            "IX_TaskAttachments_TaskId_Type",
            schema: "Tasks",
            table: "TaskAttachments",
            columns: new[] { "TaskId", "Type" });

        migrationBuilder.CreateIndex(
            "IX_TaskAttachments_UploadedById",
            schema: "Tasks",
            table: "TaskAttachments",
            column: "UploadedById");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "TaskAttachments",
            "Tasks");
    }
}