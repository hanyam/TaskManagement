# Fix ManagerEmployee Migration Issue

## Problem
The migration `20251116000000_AddManagerEmployeeRelationship` exists but the `ManagerEmployees` table is not created in the database. The migration is NOT in the `__EFMigrationsHistory` table, so it should be detected as pending.

## Root Cause
The migration file was created manually without a Designer file. EF Core needs both the migration file AND the Designer file to properly recognize a migration. This has now been fixed by creating the Designer file.

## Solution

### Option 1: Remove Migration Record and Reapply (Recommended)

1. **Connect to your SQL Server database** (using SQL Server Management Studio, Azure Data Studio, or sqlcmd)

2. **Check if the table exists:**
   ```sql
   SELECT * FROM sys.tables 
   WHERE name = 'ManagerEmployees' AND schema_id = SCHEMA_ID('Tasks')
   ```

3. **If the table doesn't exist, remove the migration record:**
   ```sql
   DELETE FROM [Tasks].[__EFMigrationsHistory]
   WHERE MigrationId = '20251116000000_AddManagerEmployeeRelationship'
   ```

4. **Restart your API** - The migration will be automatically applied on startup

### Option 2: Manually Create the Table

If you prefer to create the table manually:

1. **Run the SQL script:**
   ```bash
   # Connect to your database and run:
   sqlcmd -S your_server -d your_database -U your_user -i scripts/CreateManagerEmployeeTable.sql
   ```

2. **Add the migration record:**
   ```sql
   INSERT INTO [Tasks].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
   VALUES ('20251116000000_AddManagerEmployeeRelationship', '9.0.0')
   ```

### Option 3: Use Docker SQL Server

If you're using Docker Compose:

1. **Connect to the SQL Server container:**
   ```bash
   docker exec -it taskmanagement-sqlserver /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P YourStrong@Passw0rd
   ```

2. **Switch to your database:**
   ```sql
   USE TaskManagement
   GO
   ```

3. **Check and remove migration record if needed:**
   ```sql
   -- Check if table exists
   SELECT * FROM sys.tables WHERE name = 'ManagerEmployees' AND schema_id = SCHEMA_ID('Tasks')
   GO
   
   -- If table doesn't exist, remove migration record
   DELETE FROM [Tasks].[__EFMigrationsHistory]
   WHERE MigrationId = '20251116000000_AddManagerEmployeeRelationship'
   GO
   ```

4. **Restart the API container:**
   ```bash
   docker-compose restart taskmanagement.api
   ```

## Verification

After applying the fix, verify the table was created:

```sql
SELECT * FROM [Tasks].[ManagerEmployees]
```

You should see an empty table (or rows if you've already added data).

## Prevention

The migration file now uses raw SQL with existence checks, so it's safe to run multiple times. The migration will:
- Check if the table exists before creating it
- Only create the table if it doesn't exist
- This prevents errors if the migration runs multiple times

