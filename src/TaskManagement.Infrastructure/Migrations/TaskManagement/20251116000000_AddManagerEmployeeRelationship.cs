#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManagement.Infrastructure.Migrations.TaskManagement;

/// <inheritdoc />
public partial class AddManagerEmployeeRelationship : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create table only if it doesn't exist (for existing databases)
        migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ManagerEmployees' AND schema_id = SCHEMA_ID('Tasks'))
                BEGIN
                    CREATE TABLE [Tasks].[ManagerEmployees] (
                        [Id] uniqueidentifier NOT NULL,
                        [ManagerId] uniqueidentifier NOT NULL,
                        [EmployeeId] uniqueidentifier NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        [CreatedBy] nvarchar(256) NOT NULL,
                        [UpdatedBy] nvarchar(256) NULL,
                        CONSTRAINT [PK_ManagerEmployees] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ManagerEmployees_Users_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Tasks].[Users] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_ManagerEmployees_Users_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Tasks].[Users] ([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [CK_ManagerEmployee_NotSelf] CHECK ([ManagerId] != [EmployeeId])
                    );

                    CREATE INDEX [IX_ManagerEmployees_EmployeeId] ON [Tasks].[ManagerEmployees] ([EmployeeId]);
                    CREATE INDEX [IX_ManagerEmployees_ManagerId] ON [Tasks].[ManagerEmployees] ([ManagerId]);
                    CREATE UNIQUE INDEX [IX_ManagerEmployees_ManagerId_EmployeeId] ON [Tasks].[ManagerEmployees] ([ManagerId], [EmployeeId]);
                END
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ManagerEmployees' AND schema_id = SCHEMA_ID('Tasks'))
                BEGIN
                    DROP TABLE [Tasks].[ManagerEmployees];
                END
            ");
    }
}

