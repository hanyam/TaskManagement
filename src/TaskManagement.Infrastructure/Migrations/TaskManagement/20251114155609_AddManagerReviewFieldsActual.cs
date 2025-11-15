using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.Infrastructure.Migrations.TaskManagement
{
    /// <inheritdoc />
    public partial class AddManagerReviewFieldsActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ManagerRating column if it doesn't exist
            migrationBuilder.AddColumn<int>(
                name: "ManagerRating",
                schema: "Tasks",
                table: "Tasks",
                type: "int",
                nullable: true);

            // Add ManagerFeedback column if it doesn't exist
            migrationBuilder.AddColumn<string>(
                name: "ManagerFeedback",
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
                name: "ManagerRating",
                schema: "Tasks",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ManagerFeedback",
                schema: "Tasks",
                table: "Tasks");
        }
    }
}
