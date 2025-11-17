-- Script to manually create the ManagerEmployees table
-- Run this script directly against your database if migrations aren't working

-- Check if table already exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ManagerEmployees' AND schema_id = SCHEMA_ID('Tasks'))
BEGIN
    -- Create the table
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

    -- Create indexes
    CREATE INDEX [IX_ManagerEmployees_EmployeeId] ON [Tasks].[ManagerEmployees] ([EmployeeId]);
    CREATE INDEX [IX_ManagerEmployees_ManagerId] ON [Tasks].[ManagerEmployees] ([ManagerId]);
    CREATE UNIQUE INDEX [IX_ManagerEmployees_ManagerId_EmployeeId] ON [Tasks].[ManagerEmployees] ([ManagerId], [EmployeeId]);

    PRINT 'ManagerEmployees table created successfully.';
END
ELSE
BEGIN
    PRINT 'ManagerEmployees table already exists.';
END

-- Add migration record to history (use the actual migration name that will be created)
-- This will be updated when you create the proper migration
-- INSERT INTO [Tasks].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
-- VALUES ('AddManagerEmployeeRelationship', '9.0.0');


