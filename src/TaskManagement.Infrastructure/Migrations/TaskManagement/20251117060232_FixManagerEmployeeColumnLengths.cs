#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagement.Infrastructure.Migrations.TaskManagement;

/// <inheritdoc />
public partial class FixManagerEmployeeColumnLengths : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Alter columns only if they are currently nvarchar(max)
        // This migration is safe to run even if columns are already nvarchar(256)
        migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ManagerEmployees' AND schema_id = SCHEMA_ID('Tasks'))
                BEGIN
                    -- Check if CreatedBy is nvarchar(max) and alter it
                    IF EXISTS (
                        SELECT * FROM sys.columns c
                        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                        WHERE c.object_id = OBJECT_ID('Tasks.ManagerEmployees')
                        AND c.name = 'CreatedBy'
                        AND t.name = 'nvarchar'
                        AND c.max_length = -1
                    )
                    BEGIN
                        ALTER TABLE [Tasks].[ManagerEmployees]
                        ALTER COLUMN [CreatedBy] nvarchar(256) NOT NULL;
                    END

                    -- Check if UpdatedBy is nvarchar(max) and alter it
                    IF EXISTS (
                        SELECT * FROM sys.columns c
                        INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                        WHERE c.object_id = OBJECT_ID('Tasks.ManagerEmployees')
                        AND c.name = 'UpdatedBy'
                        AND t.name = 'nvarchar'
                        AND c.max_length = -1
                    )
                    BEGIN
                        ALTER TABLE [Tasks].[ManagerEmployees]
                        ALTER COLUMN [UpdatedBy] nvarchar(256) NULL;
                    END
                END
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revert to nvarchar(max) if needed (usually not necessary)
        migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ManagerEmployees' AND schema_id = SCHEMA_ID('Tasks'))
                BEGIN
                    ALTER TABLE [Tasks].[ManagerEmployees]
                    ALTER COLUMN [CreatedBy] nvarchar(max) NOT NULL;

                    ALTER TABLE [Tasks].[ManagerEmployees]
                    ALTER COLUMN [UpdatedBy] nvarchar(max) NULL;
                END
            ");
    }
}

