using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.src.TaskManagement.Infrastructure.Migrations.TaskManagement
{
    /// <inheritdoc />
    public partial class AddManagerReviewWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedUserId",
                schema: "Tasks",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ManagerFeedback",
                schema: "Tasks",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerRating",
                schema: "Tasks",
                table: "Tasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManagerFeedback",
                schema: "Tasks",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ManagerRating",
                schema: "Tasks",
                table: "Tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedUserId",
                schema: "Tasks",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
