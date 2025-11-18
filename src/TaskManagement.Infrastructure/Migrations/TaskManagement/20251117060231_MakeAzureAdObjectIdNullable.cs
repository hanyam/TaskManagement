#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagement.Infrastructure.Migrations.TaskManagement;

/// <inheritdoc />
public partial class MakeAzureAdObjectIdNullable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            "IX_Users_AzureAdObjectId",
            schema: "Tasks",
            table: "Users");

        migrationBuilder.AlterColumn<string>(
            "AzureAdObjectId",
            schema: "Tasks",
            table: "Users",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.CreateIndex(
            "IX_Users_AzureAdObjectId",
            schema: "Tasks",
            table: "Users",
            column: "AzureAdObjectId",
            unique: true,
            filter: "[AzureAdObjectId] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            "IX_Users_AzureAdObjectId",
            schema: "Tasks",
            table: "Users");

        migrationBuilder.AlterColumn<string>(
            "AzureAdObjectId",
            schema: "Tasks",
            table: "Users",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            "IX_Users_AzureAdObjectId",
            schema: "Tasks",
            table: "Users",
            column: "AzureAdObjectId",
            unique: true);
    }
}