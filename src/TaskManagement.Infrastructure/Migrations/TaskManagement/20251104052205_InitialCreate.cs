#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagement.Infrastructure.Migrations.TaskManagement;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            "Tasks");

        migrationBuilder.CreateTable(
            "Users",
            schema: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                Email = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: false),
                FirstName = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                DisplayName = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
                AzureAdObjectId = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                IsActive = table.Column<bool>("bit", nullable: false),
                LastLoginAt = table.Column<DateTime>("datetime2", nullable: true),
                Role = table.Column<int>("int", nullable: false),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: true),
                CreatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });

        migrationBuilder.CreateTable(
            "Tasks",
            schema: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                Title = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                Status = table.Column<int>("int", nullable: false),
                Priority = table.Column<int>("int", nullable: false),
                DueDate = table.Column<DateTime>("datetime2", nullable: true),
                OriginalDueDate = table.Column<DateTime>("datetime2", nullable: true),
                ExtendedDueDate = table.Column<DateTime>("datetime2", nullable: true),
                AssignedUserId = table.Column<Guid>("uniqueidentifier", nullable: false),
                Type = table.Column<int>("int", nullable: false),
                ReminderLevel = table.Column<int>("int", nullable: false),
                ProgressPercentage = table.Column<int>("int", nullable: true),
                CreatedById = table.Column<Guid>("uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: true),
                CreatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
                table.ForeignKey(
                    "FK_Tasks_Users_AssignedUserId",
                    x => x.AssignedUserId,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    "FK_Tasks_Users_CreatedById",
                    x => x.CreatedById,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "DeadlineExtensionRequests",
            schema: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                TaskId = table.Column<Guid>("uniqueidentifier", nullable: false),
                RequestedById = table.Column<Guid>("uniqueidentifier", nullable: false),
                RequestedDueDate = table.Column<DateTime>("datetime2", nullable: false),
                Reason = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: false),
                Status = table.Column<int>("int", nullable: false),
                ReviewedById = table.Column<Guid>("uniqueidentifier", nullable: true),
                ReviewedAt = table.Column<DateTime>("datetime2", nullable: true),
                ReviewNotes = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: true),
                CreatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DeadlineExtensionRequests", x => x.Id);
                table.ForeignKey(
                    "FK_DeadlineExtensionRequests_Tasks_TaskId",
                    x => x.TaskId,
                    principalSchema: "Tasks",
                    principalTable: "Tasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_DeadlineExtensionRequests_Users_RequestedById",
                    x => x.RequestedById,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    "FK_DeadlineExtensionRequests_Users_ReviewedById",
                    x => x.ReviewedById,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "TaskAssignments",
            schema: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                TaskId = table.Column<Guid>("uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>("uniqueidentifier", nullable: false),
                IsPrimary = table.Column<bool>("bit", nullable: false),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: true),
                CreatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaskAssignments", x => x.Id);
                table.ForeignKey(
                    "FK_TaskAssignments_Tasks_TaskId",
                    x => x.TaskId,
                    principalSchema: "Tasks",
                    principalTable: "Tasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_TaskAssignments_Users_UserId",
                    x => x.UserId,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            "TaskProgressHistory",
            schema: "Tasks",
            columns: table => new
            {
                Id = table.Column<Guid>("uniqueidentifier", nullable: false),
                TaskId = table.Column<Guid>("uniqueidentifier", nullable: false),
                UpdatedById = table.Column<Guid>("uniqueidentifier", nullable: false),
                ProgressPercentage = table.Column<int>("int", nullable: false),
                Notes = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                Status = table.Column<int>("int", nullable: false),
                AcceptedById = table.Column<Guid>("uniqueidentifier", nullable: true),
                AcceptedAt = table.Column<DateTime>("datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>("datetime2", nullable: true),
                CreatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: false),
                UpdatedBy = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaskProgressHistory", x => x.Id);
                table.ForeignKey(
                    "FK_TaskProgressHistory_Tasks_TaskId",
                    x => x.TaskId,
                    principalSchema: "Tasks",
                    principalTable: "Tasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_TaskProgressHistory_Users_AcceptedById",
                    x => x.AcceptedById,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    "FK_TaskProgressHistory_Users_UpdatedById",
                    x => x.UpdatedById,
                    principalSchema: "Tasks",
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            "IX_DeadlineExtensionRequests_RequestedById",
            schema: "Tasks",
            table: "DeadlineExtensionRequests",
            column: "RequestedById");

        migrationBuilder.CreateIndex(
            "IX_DeadlineExtensionRequests_ReviewedById",
            schema: "Tasks",
            table: "DeadlineExtensionRequests",
            column: "ReviewedById");

        migrationBuilder.CreateIndex(
            "IX_DeadlineExtensionRequests_TaskId",
            schema: "Tasks",
            table: "DeadlineExtensionRequests",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            "IX_TaskAssignments_TaskId_UserId",
            schema: "Tasks",
            table: "TaskAssignments",
            columns: new[] { "TaskId", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_TaskAssignments_UserId",
            schema: "Tasks",
            table: "TaskAssignments",
            column: "UserId");

        migrationBuilder.CreateIndex(
            "IX_TaskProgressHistory_AcceptedById",
            schema: "Tasks",
            table: "TaskProgressHistory",
            column: "AcceptedById");

        migrationBuilder.CreateIndex(
            "IX_TaskProgressHistory_TaskId",
            schema: "Tasks",
            table: "TaskProgressHistory",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            "IX_TaskProgressHistory_UpdatedById",
            schema: "Tasks",
            table: "TaskProgressHistory",
            column: "UpdatedById");

        migrationBuilder.CreateIndex(
            "IX_Tasks_AssignedUserId",
            schema: "Tasks",
            table: "Tasks",
            column: "AssignedUserId");

        migrationBuilder.CreateIndex(
            "IX_Tasks_CreatedById",
            schema: "Tasks",
            table: "Tasks",
            column: "CreatedById");

        migrationBuilder.CreateIndex(
            "IX_Users_AzureAdObjectId",
            schema: "Tasks",
            table: "Users",
            column: "AzureAdObjectId",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_Users_Email",
            schema: "Tasks",
            table: "Users",
            column: "Email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "DeadlineExtensionRequests",
            "Tasks");

        migrationBuilder.DropTable(
            "TaskAssignments",
            "Tasks");

        migrationBuilder.DropTable(
            "TaskProgressHistory",
            "Tasks");

        migrationBuilder.DropTable(
            "Tasks",
            "Tasks");

        migrationBuilder.DropTable(
            "Users",
            "Tasks");
    }
}