# Database Seeding Guide

## Overview

The Task Management API includes a database seeding feature that allows administrators to execute SQL scripts from the `scripts/Seeding` folder. This is useful for:

- Initial data population in development/test environments
- Restoring sample data
- Bulk data imports
- Database initialization

---

## Features

### ✅ Automatic Script Discovery
- Reads all `.sql` files from `scripts/Seeding` folder
- Orders scripts alphabetically by filename (use numbered prefixes like `00-`, `01-`, etc.)
- Supports selective execution of specific scripts

### ✅ Batch Execution
- Splits SQL scripts by `GO` statements (SQL Server batch separator)
- Executes each batch sequentially
- Continues even if individual scripts fail

### ✅ Detailed Reporting
- Returns execution status for each script
- Reports rows affected per script
- Includes execution time metrics
- Provides error messages for failed scripts

### ✅ Security
- Admin-only access (`[Authorize(Roles = "Admin")]`)
- Comprehensive logging
- Transaction-safe execution

---

## API Endpoints

### 1. Seed Database

**Endpoint:** `POST /database/seed`

**Authorization:** Admin role required

**Description:** Executes SQL scripts from the seeding folder in alphabetical order.

#### Request Body

```json
{
  "scriptNames": ["00-SeedEmployees.sql", "01-Add Employees.sql"]
}
```

**Parameters:**
- `scriptNames` (optional): Array of specific script names to execute. If omitted or empty, all scripts are executed.

#### Response (Success)

```json
{
  "success": true,
  "data": {
    "totalScripts": 2,
    "successfulScripts": 2,
    "failedScripts": 0,
    "executionDetails": [
      {
        "scriptName": "00-SeedEmployees.sql",
        "success": true,
        "errorMessage": null,
        "executionTimeMs": 1245,
        "rowsAffected": 150
      },
      {
        "scriptName": "01-Add Employees.sql",
        "success": true,
        "errorMessage": null,
        "executionTimeMs": 2103,
        "rowsAffected": 287
      }
    ],
    "totalExecutionTimeMs": 3348
  },
  "message": null,
  "errors": [],
  "timestamp": "2025-11-18T10:30:00Z",
  "traceId": null
}
```

#### Response (Partial Failure)

```json
{
  "success": true,
  "data": {
    "totalScripts": 2,
    "successfulScripts": 1,
    "failedScripts": 1,
    "executionDetails": [
      {
        "scriptName": "00-SeedEmployees.sql",
        "success": true,
        "errorMessage": null,
        "executionTimeMs": 1245,
        "rowsAffected": 150
      },
      {
        "scriptName": "01-Add Employees.sql",
        "success": false,
        "errorMessage": "SQL Error: Violation of PRIMARY KEY constraint (Line: 42)",
        "executionTimeMs": 523,
        "rowsAffected": null
      }
    ],
    "totalExecutionTimeMs": 1768
  }
}
```

#### Response (Error)

```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    {
      "code": "NOT_FOUND",
      "message": "Seeding folder not found at path: C:\\path\\to\\scripts\\Seeding",
      "field": null
    }
  ],
  "timestamp": "2025-11-18T10:30:00Z",
  "traceId": "0HNH42M4FEMFU:0000000A"
}
```

---

### 2. Get Available Scripts

**Endpoint:** `GET /database/seed/scripts`

**Authorization:** Admin role required

**Description:** Returns a list of all available SQL scripts in the seeding folder.

#### Response

```json
{
  "success": true,
  "data": [
    "00-SeedEmployees.sql",
    "01-Add Employees.sql"
  ],
  "message": null,
  "errors": [],
  "timestamp": "2025-11-18T10:30:00Z",
  "traceId": null
}
```

---

## Script Organization

### File Naming Convention

Use numbered prefixes to control execution order:

```
scripts/Seeding/
├── 00-SeedEmployees.sql      ← Executed first
├── 01-Add Employees.sql       ← Executed second
├── 02-SeedTasks.sql           ← Executed third
└── 03-SeedAssignments.sql     ← Executed fourth
```

**Recommended naming pattern:**
- `00-` to `09-`: Core entities (users, roles, etc.)
- `10-` to `19-`: Reference data (categories, statuses, etc.)
- `20-` to `29-`: Transactional data (tasks, assignments, etc.)
- `30-` to `99-`: Additional/optional data

---

## SQL Script Guidelines

### Basic Structure

```sql
-- 00-SeedEmployees.sql

-- Clear existing data (optional, use with caution)
DELETE FROM [Tasks].[Users] WHERE Email LIKE '%@example.com';

-- Insert new data
INSERT INTO [Tasks].[Users]
(Id, Email, FirstName, LastName, DisplayName, AzureAdObjectId, IsActive, [Role], CreatedAt, CreatedBy)
VALUES
(NEWID(), 'john.doe@example.com', 'John', 'Doe', 'John Doe', NULL, 1, 0, GETUTCDATE(), 'system'),
(NEWID(), 'jane.smith@example.com', 'Jane', 'Smith', 'Jane Smith', NULL, 1, 1, GETUTCDATE(), 'system');
```

### Using GO Statements

The seeding handler automatically splits scripts by `GO` statements:

```sql
-- Script with multiple batches

INSERT INTO [Tasks].[Users] (...) VALUES (...);
GO

-- Check if insert succeeded
IF @@ROWCOUNT = 0
BEGIN
    RAISERROR('No users were inserted', 16, 1);
END
GO

-- Update related data
UPDATE [Tasks].[TaskAssignments]
SET AssignedUserId = (SELECT TOP 1 Id FROM [Tasks].[Users] WHERE Email = 'john.doe@example.com')
WHERE AssignedUserId IS NULL;
GO
```

### Best Practices

1. **Idempotency:** Scripts should be safe to run multiple times
   ```sql
   -- Good: Check if data exists before inserting
   IF NOT EXISTS (SELECT 1 FROM [Tasks].[Users] WHERE Email = 'admin@example.com')
   BEGIN
       INSERT INTO [Tasks].[Users] (...) VALUES (...);
   END
   ```

2. **Use Transactions (Optional):** Wrap in transactions if needed
   ```sql
   BEGIN TRANSACTION;
   
   -- Insert data
   INSERT INTO [Tasks].[Users] (...) VALUES (...);
   
   -- Verify
   IF @@ROWCOUNT = 0
   BEGIN
       ROLLBACK TRANSACTION;
       RAISERROR('Insert failed', 16, 1);
   END
   ELSE
   BEGIN
       COMMIT TRANSACTION;
   END
   ```

3. **Include Comments:** Document what the script does
   ```sql
   -- Purpose: Seeds initial employee data for development environment
   -- Author: Your Name
   -- Date: 2025-11-18
   -- Dependencies: None
   ```

4. **Handle Errors Gracefully:** Use `TRY...CATCH` for complex operations
   ```sql
   BEGIN TRY
       INSERT INTO [Tasks].[Users] (...) VALUES (...);
   END TRY
   BEGIN CATCH
       PRINT 'Error: ' + ERROR_MESSAGE();
       -- Don't re-throw if you want execution to continue
   END CATCH
   ```

---

## Usage Examples

### cURL Commands

#### Seed All Scripts
```bash
curl -X POST "http://localhost:5000/database/seed" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{}"
```

#### Seed Specific Scripts
```bash
curl -X POST "http://localhost:5000/database/seed" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "scriptNames": ["00-SeedEmployees.sql"]
  }'
```

#### List Available Scripts
```bash
curl -X GET "http://localhost:5000/database/seed/scripts" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### PowerShell Examples

#### Seed All Scripts
```powershell
$token = "YOUR_JWT_TOKEN"
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$body = @{} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/database/seed" `
    -Method Post `
    -Headers $headers `
    -Body $body
```

#### Seed Specific Scripts
```powershell
$body = @{
    scriptNames = @("00-SeedEmployees.sql", "01-Add Employees.sql")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/database/seed" `
    -Method Post `
    -Headers $headers `
    -Body $body
```

### Swagger UI

1. Navigate to `http://localhost:5000/swagger`
2. Expand **Database** section
3. Click **POST /database/seed**
4. Click **"Try it out"**
5. Modify request body (or leave empty for all scripts)
6. Click **"Execute"**
7. View response below

---

## Troubleshooting

### Issue: "Seeding folder not found"

**Cause:** The API cannot locate the `scripts/Seeding` folder.

**Solution:**
1. Ensure the folder exists in the project root
2. Check folder path: `{solution-root}/scripts/Seeding`
3. Verify folder contains `.sql` files
4. Restart API after creating folder

### Issue: "Unauthorized" (401)

**Cause:** Missing or invalid authentication token.

**Solution:**
1. Call `/authentication/login` endpoint first to get JWT token
2. Include token in `Authorization: Bearer {token}` header

### Issue: "Forbidden" (403)

**Cause:** User does not have Admin role.

**Solution:**
1. Ensure your user has `Role = Admin` in the database
2. Check JWT token claims include `role: Admin`
3. Update user role in database if needed:
   ```sql
   UPDATE [Tasks].[Users]
   SET [Role] = 2  -- Admin role
   WHERE Email = 'your-email@example.com';
   ```

### Issue: Script execution fails with SQL error

**Cause:** SQL syntax error or constraint violation.

**Solution:**
1. Check the `errorMessage` in the response
2. Review SQL script for syntax errors
3. Ensure referenced tables/columns exist
4. Check for constraint violations (primary key, foreign key, unique)
5. Test script manually in SSMS or Azure Data Studio

### Issue: Script hangs or times out

**Cause:** Script contains very large data inserts or slow operations.

**Solution:**
1. Break large scripts into smaller batches
2. Add `GO` statements between batches
3. Use bulk insert operations instead of row-by-row inserts
4. Increase command timeout if needed (requires code change)

---

## Performance Considerations

### For Small Scripts (< 1000 rows)
- ✅ Direct execution via API is fine
- ✅ Typical execution time: < 5 seconds

### For Medium Scripts (1000-10000 rows)
- ⚠️ Consider breaking into multiple scripts
- ⚠️ Use `GO` statements to split batches
- ⚠️ Expected execution time: 10-60 seconds

### For Large Scripts (> 10000 rows)
- ❌ Avoid using API for very large imports
- ✅ Use SQL Server Import/Export tools instead
- ✅ Or use `bcp` utility for bulk imports
- ✅ Or execute scripts directly in SSMS

---

## Security Considerations

### ⚠️ Important Security Notes

1. **Admin-Only Access:** Only users with Admin role can seed data
2. **No Input Validation on SQL:** Scripts are executed as-is (trusted input only)
3. **Production Warning:** Be extremely careful when seeding production databases
4. **Backup First:** Always backup database before running seed scripts
5. **Review Scripts:** Review all SQL scripts before execution
6. **Audit Logging:** All seeding operations are logged with timestamps

### Best Practices

1. **Development/Test Only:** Use seeding primarily in non-production environments
2. **Version Control:** Keep seed scripts in source control
3. **Code Review:** Have another developer review seed scripts before committing
4. **Separate Folders:** Consider separate folders for dev/test/prod seed data
5. **Disable in Production:** Consider disabling the endpoint in production environments

---

## Logging

All seeding operations are logged with the following information:

```
[Information] Starting database seeding operation
[Information] Looking for seeding scripts in: C:\path\to\scripts\Seeding
[Information] Found 2 SQL files to execute
[Information] Executing script: 00-SeedEmployees.sql
[Information] Script 00-SeedEmployees.sql executed successfully in 1245ms, rows affected: 150
[Information] Executing script: 01-Add Employees.sql
[Error] SQL error executing script 01-Add Employees.sql: Violation of PRIMARY KEY constraint
[Information] Database seeding completed. Total: 2, Success: 1, Failed: 1, Time: 1768ms
```

Logs are written to:
- Console (development)
- File: `logs/taskmanagement-{date}.txt` (production)
- Serilog sinks configured in `Program.cs`

---

## Architecture Details

### Implementation

**Vertical Slice Architecture:**
```
Application/
└── DatabaseSeeding/
    └── Commands/
        └── SeedDatabase/
            ├── SeedDatabaseCommand.cs
            ├── SeedDatabaseCommandHandler.cs
            └── (No validator - admin-only operation)

Domain/
└── DTOs/
    └── SeedDatabaseResultDto.cs

API/
└── Controllers/
    └── DatabaseController.cs
```

**Design Patterns:**
- ✅ Command pattern (CQRS)
- ✅ Result pattern (standardized responses)
- ✅ Repository pattern (EF Core DbContext)
- ✅ Mediator pattern (command routing)

**Dependencies:**
- `TaskManagementDbContext` - Database access
- `ILogger` - Structured logging
- `ICommandMediator` - Command dispatching

---

## Future Enhancements

### Potential Improvements

1. **Progress Reporting:** Real-time progress updates via SignalR
2. **Rollback Support:** Transaction-based rollback on failure
3. **Script Validation:** Pre-execution validation of SQL syntax
4. **Scheduling:** Schedule seeding operations for off-peak hours
5. **Backup Integration:** Automatic database backup before seeding
6. **Script History:** Track which scripts have been executed and when
7. **Dry Run Mode:** Preview what would be executed without actually running
8. **Parameterized Scripts:** Support for script parameters/variables

---

## Related Documentation

- **[Developer Guide](DEVELOPER_GUIDE.md)** - Adding new features
- **[API Reference](API_REFERENCE.md)** - Complete API documentation
- **[Deployment Guide](DEPLOYMENT.md)** - Production deployment

---

**Last Updated:** November 18, 2025  
**Feature Status:** ✅ Production Ready  
**Breaking Changes:** None

