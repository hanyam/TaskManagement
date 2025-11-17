-- Script to fix ManagerEmployee migration issue
-- This removes the migration record from __EFMigrationsHistory if the table doesn't exist
-- Then you can reapply the migration

-- Check if the table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ManagerEmployees' AND schema_id = SCHEMA_ID('Tasks'))
BEGIN
    -- Remove the migration record from history if it exists
    DELETE FROM [Tasks].[__EFMigrationsHistory]
    WHERE MigrationId = '20251116000000_AddManagerEmployeeRelationship';
    
    PRINT 'Migration record removed from history. You can now reapply the migration.';
END
ELSE
BEGIN
    PRINT 'Table already exists. No action needed.';
END


