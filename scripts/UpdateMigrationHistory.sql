-- Script to update migration history after manually creating the table
-- Only run this AFTER you've created the table using CreateManagerEmployeeTable.sql
-- AND after you've created a proper migration using dotnet ef migrations add

-- Check if migration record exists
IF NOT EXISTS (
    SELECT * FROM [Tasks].[__EFMigrationsHistory] 
    WHERE MigrationId LIKE '%ManagerEmployee%'
)
BEGIN
    -- Insert migration record (update MigrationId with actual migration name)
    -- You'll need to replace 'AddManagerEmployeeRelationship' with the actual migration ID
    -- after running: dotnet ef migrations add AddManagerEmployeeRelationship
    
    PRINT 'Please create the migration first using: dotnet ef migrations add AddManagerEmployeeRelationship';
    PRINT 'Then update this script with the actual MigrationId from the generated migration file.';
    PRINT 'Finally, uncomment and run the INSERT statement below.';
    
    -- Uncomment and update after creating migration:
    -- INSERT INTO [Tasks].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    -- VALUES ('20251116000000_AddManagerEmployeeRelationship', '9.0.0');
END
ELSE
BEGIN
    PRINT 'Migration record already exists in history.';
END


