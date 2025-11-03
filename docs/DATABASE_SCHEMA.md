# Task Management API - Database Schema Documentation

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Entity Relationship Diagram](#entity-relationship-diagram)
2. [Table Descriptions](#table-descriptions)
3. [Column Descriptions](#column-descriptions)
4. [Relationships](#relationships)
5. [Indexes](#indexes)
6. [Enum Storage](#enum-storage)
7. [Migration Guide](#migration-guide)

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                           Users                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Id (PK, Guid)                                             │   │
│  │ Email (Unique, NVARCHAR(256))                             │   │
│  │ FirstName (NVARCHAR(100))                                 │   │
│  │ LastName (NVARCHAR(100))                                   │   │
│  │ DisplayName (NVARCHAR(200))                                │   │
│  │ AzureAdObjectId (Unique, NVARCHAR(100))                    │   │
│  │ IsActive (BIT)                                             │   │
│  │ LastLoginAt (DATETIME2, nullable)                          │   │
│  │ Role (INT)                                                 │   │
│  │ CreatedAt (DATETIME2)                                     │   │
│  │ UpdatedAt (DATETIME2, nullable)                           │   │
│  │ CreatedBy (NVARCHAR(256))                                  │   │
│  │ UpdatedBy (NVARCHAR(256), nullable)                        │   │
│  └──────────────────────────────────────────────────────────┘   │
└──────────┬──────────────────────────────────────────────────────┘
           │
           │ 1:N (CreatedByUser)
           │
┌──────────▼──────────────────────────────────────────────────────┐
│                           Tasks                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Id (PK, Guid)                                             │   │
│  │ Title (NVARCHAR(200))                                     │   │
│  │ Description (NVARCHAR(1000), nullable)                     │   │
│  │ Status (INT)                                              │   │
│  │ Priority (INT)                                            │   │
│  │ DueDate (DATETIME2, nullable)                            │   │
│  │ OriginalDueDate (DATETIME2, nullable)                    │   │
│  │ ExtendedDueDate (DATETIME2, nullable)                    │   │
│  │ AssignedUserId (FK → Users.Id, RESTRICT)                  │   │
│  │ Type (INT)                                                │   │
│  │ ReminderLevel (INT)                                       │   │
│  │ ProgressPercentage (INT, nullable)                        │   │
│  │ CreatedById (FK → Users.Id, RESTRICT)                    │   │
│  │ CreatedAt (DATETIME2)                                     │   │
│  │ UpdatedAt (DATETIME2, nullable)                          │   │
│  │ CreatedBy (NVARCHAR(256))                                 │   │
│  │ UpdatedBy (NVARCHAR(256), nullable)                       │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────┬─────┬───────────────────────┬──────────────────────────────┘
      │     │                       │
      │     │ 1:N (CASCADE)        │ 1:N (CASCADE)
      │     │                       │
      │     ▼                       ▼
┌─────┴──────────────┐    ┌──────────────────────────┐
│ TaskAssignments    │    │ TaskProgressHistory       │
│ ┌────────────────┐ │    │ ┌──────────────────────┐ │
│ │ Id (PK, Guid)   │ │    │ │ Id (PK, Guid)        │ │
│ │ TaskId (FK)     │ │    │ │ TaskId (FK)          │ │
│ │ UserId (FK)     │ │    │ │ UpdatedById (FK)     │ │
│ │ IsPrimary (BIT) │ │    │ │ ProgressPercentage   │ │
│ │ CreatedAt       │ │    │ │ Notes (NVARCHAR(1000)│ │
│ │ UpdatedAt       │ │    │ │ Status (INT)         │ │
│ │ CreatedBy       │ │    │ │ AcceptedById (FK)    │ │
│ │ UpdatedBy       │ │    │ │ AcceptedAt           │ │
│ │ Unique (TaskId, │ │    │ │ CreatedAt            │ │
│ │         UserId) │ │    │ │ UpdatedAt           │ │
│ └────────────────┘ │    │ │ CreatedBy           │ │
└────────────────────┘    │ │ UpdatedBy            │ │
                          │ └──────────────────────┘ │
                          └──────────────────────────┘
                               │
                               │ 1:N (RESTRICT)
                               │
┌──────────────────────────────▼──────────────────────────────────┐
│                    DeadlineExtensionRequests                     │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ Id (PK, Guid)                                             │ │
│  │ TaskId (FK → Tasks.Id, CASCADE)                           │ │
│  │ RequestedById (FK → Users.Id, RESTRICT)                   │ │
│  │ RequestedDueDate (DATETIME2)                              │ │
│  │ Reason (NVARCHAR(500))                                    │ │
│  │ Status (INT)                                              │ │
│  │ ReviewedById (FK → Users.Id, RESTRICT, nullable)        │ │
│  │ ReviewedAt (DATETIME2, nullable)                         │ │
│  │ ReviewNotes (NVARCHAR(1000), nullable)                    │ │
│  │ CreatedAt (DATETIME2)                                     │ │
│  │ UpdatedAt (DATETIME2, nullable)                           │ │
│  │ CreatedBy (NVARCHAR(256))                                 │ │
│  │ UpdatedBy (NVARCHAR(256), nullable)                      │ │
│  └──────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────┘
```

---

## Table Descriptions

### Users Table

Stores user information with Azure AD integration and role-based access.

**Primary Key:** `Id` (Guid)

**Unique Constraints:**
- `Email`: Must be unique
- `AzureAdObjectId`: Must be unique

**Indexes:**
- Index on `Email` (unique)
- Index on `AzureAdObjectId` (unique)

**Relationships:**
- One-to-many with `Tasks` (as creator)
- One-to-many with `Tasks` (as assignee)
- One-to-many with `TaskAssignments`
- One-to-many with `TaskProgressHistory` (as updater and accepter)
- One-to-many with `DeadlineExtensionRequests` (as requester and reviewer)

### Tasks Table

Stores task information with support for multiple types, statuses, and progress tracking.

**Primary Key:** `Id` (Guid)

**Foreign Keys:**
- `AssignedUserId` → `Users.Id` (RESTRICT delete)
- `CreatedById` → `Users.Id` (RESTRICT delete)

**Relationships:**
- Many-to-one with `Users` (creator)
- Many-to-one with `Users` (primary assignee)
- One-to-many with `TaskAssignments` (CASCADE delete)
- One-to-many with `TaskProgressHistory` (CASCADE delete)
- One-to-many with `DeadlineExtensionRequests` (CASCADE delete)

**Constraints:**
- `Title` is required (max 200 characters)
- `Description` is optional (max 1000 characters)
- `ProgressPercentage` must be 0-100 (if not null)
- `DueDate` cannot be in the past (validation)

### TaskAssignments Table

Junction table for many-to-many relationship between tasks and users.

**Primary Key:** `Id` (Guid)

**Unique Constraint:**
- `(TaskId, UserId)`: Prevents duplicate assignments

**Foreign Keys:**
- `TaskId` → `Tasks.Id` (CASCADE delete)
- `UserId` → `Users.Id` (RESTRICT delete)

**Relationships:**
- Many-to-one with `Tasks`
- Many-to-one with `Users`

**Business Logic:**
- `IsPrimary` flag indicates primary assignee
- Only one primary assignee per task (enforced in application)

### TaskProgressHistory Table

Tracks historical progress updates for tasks with acceptance workflow.

**Primary Key:** `Id` (Guid)

**Foreign Keys:**
- `TaskId` → `Tasks.Id` (CASCADE delete)
- `UpdatedById` → `Users.Id` (RESTRICT delete)
- `AcceptedById` → `Users.Id` (RESTRICT delete, nullable)

**Relationships:**
- Many-to-one with `Tasks`
- Many-to-one with `Users` (updater)
- Many-to-one with `Users` (accepter)

**Constraints:**
- `ProgressPercentage` must be 0-100
- `Notes` is optional (max 1000 characters)

### DeadlineExtensionRequests Table

Manages deadline extension requests with approval workflow.

**Primary Key:** `Id` (Guid)

**Foreign Keys:**
- `TaskId` → `Tasks.Id` (CASCADE delete)
- `RequestedById` → `Users.Id` (RESTRICT delete)
- `ReviewedById` → `Users.Id` (RESTRICT delete, nullable)

**Relationships:**
- Many-to-one with `Tasks`
- Many-to-one with `Users` (requester)
- Many-to-one with `Users` (reviewer)

**Constraints:**
- `Reason` is required (max 500 characters)
- `RequestedDueDate` must be in the future
- `ReviewNotes` is optional (max 1000 characters)

---

## Column Descriptions

### BaseEntity Columns (All Tables)

**Id** (Guid, Primary Key)
- Unique identifier for the entity
- Generated as `Guid.NewGuid()` on creation
- Non-nullable

**CreatedAt** (DateTime2, Non-nullable)
- Timestamp when entity was created
- Set to `DateTime.UtcNow` on creation
- Indexed for time-based queries

**UpdatedAt** (DateTime2, Nullable)
- Timestamp when entity was last updated
- Set to `DateTime.UtcNow` on update via `SetUpdatedBy()`
- Null if never updated

**CreatedBy** (NVARCHAR(256), Non-nullable)
- Email of user who created the entity
- Set via `SetCreatedBy()` method
- Audit trail

**UpdatedBy** (NVARCHAR(256), Nullable)
- Email of user who last updated the entity
- Set via `SetUpdatedBy()` method
- Null if never updated

### Users Table Columns

**Email** (NVARCHAR(256), Unique, Non-nullable)
- User's email address
- Used for authentication and display
- Unique constraint enforced

**FirstName** (NVARCHAR(100), Non-nullable)
- User's first name
- Required for profile

**LastName** (NVARCHAR(100), Non-nullable)
- User's last name
- Required for profile

**DisplayName** (NVARCHAR(200), Non-nullable)
- Auto-generated from FirstName + LastName
- Used for display purposes

**AzureAdObjectId** (NVARCHAR(100), Unique, Non-nullable)
- Azure AD user identifier
- Used for Azure AD token validation
- Unique constraint enforced

**IsActive** (BIT, Non-nullable, Default: 1)
- Whether user account is active
- Inactive users cannot authenticate
- Default: true

**LastLoginAt** (DATETIME2, Nullable)
- Timestamp of last login
- Set via `RecordLogin()` method
- Null if never logged in

**Role** (INT, Non-nullable, Default: 0)
- User role enum value
- 0 = Employee, 1 = Manager, 2 = Admin
- Stored as integer

### Tasks Table Columns

**Title** (NVARCHAR(200), Non-nullable)
- Task title
- Required, max 200 characters
- Indexed for search

**Description** (NVARCHAR(1000), Nullable)
- Task description
- Optional, max 1000 characters

**Status** (INT, Non-nullable, Default: 0)
- Task status enum value
- 0 = Created, 1 = Assigned, 2 = UnderReview, 3 = Accepted, 4 = Rejected, 5 = Completed, 6 = Cancelled
- Stored as integer
- Indexed for filtering

**Priority** (INT, Non-nullable, Default: 1)
- Task priority enum value
- 0 = Low, 1 = Medium, 2 = High, 3 = Critical
- Stored as integer
- Indexed for filtering

**DueDate** (DATETIME2, Nullable)
- Current task due date
- Updated when extension is approved
- Used for reminder calculation

**OriginalDueDate** (DATETIME2, Nullable)
- Original due date before extension
- Preserved for audit trail
- Set when task is created or extended

**ExtendedDueDate** (DATETIME2, Nullable)
- Extended due date if extension approved
- Set when extension is approved
- Null if no extension or not yet approved

**AssignedUserId** (Guid, Foreign Key, Non-nullable)
- Primary assignee user ID
- References `Users.Id`
- Delete behavior: RESTRICT

**Type** (INT, Non-nullable, Default: 0)
- Task type enum value
- 0 = Simple, 1 = WithDueDate, 2 = WithProgress, 3 = WithAcceptedProgress
- Stored as integer
- Determines supported features

**ReminderLevel** (INT, Non-nullable, Default: 0)
- Calculated reminder level enum value
- 0 = None, 1 = Low, 2 = Medium, 3 = High, 4 = Critical
- Stored as integer
- Calculated by `ReminderCalculationService`

**ProgressPercentage** (INT, Nullable)
- Task progress (0-100)
- Only for `WithProgress` and `WithAcceptedProgress` types
- Null for other types
- Validated: 0-100 range

**CreatedById** (Guid, Foreign Key, Non-nullable)
- Creator user ID
- References `Users.Id`
- Delete behavior: RESTRICT

### TaskAssignments Table Columns

**TaskId** (Guid, Foreign Key, Non-nullable)
- Task ID
- References `Tasks.Id`
- Delete behavior: CASCADE

**UserId** (Guid, Foreign Key, Non-nullable)
- Assigned user ID
- References `Users.Id`
- Delete behavior: RESTRICT

**IsPrimary** (BIT, Non-nullable, Default: 0)
- Whether this is the primary assignee
- First assigned user is primary
- Used for backward compatibility

### TaskProgressHistory Table Columns

**TaskId** (Guid, Foreign Key, Non-nullable)
- Task ID
- References `Tasks.Id`
- Delete behavior: CASCADE

**UpdatedById** (Guid, Foreign Key, Non-nullable)
- User who updated progress
- References `Users.Id`
- Delete behavior: RESTRICT

**ProgressPercentage** (INT, Non-nullable)
- Progress value (0-100)
- Validated in application

**Notes** (NVARCHAR(1000), Nullable)
- Optional notes describing progress
- Max 1000 characters

**Status** (INT, Non-nullable, Default: 0)
- Progress acceptance status
- 0 = Pending, 1 = Accepted, 2 = Rejected
- Stored as integer

**AcceptedById** (Guid, Foreign Key, Nullable)
- Manager who accepted/rejected progress
- References `Users.Id`
- Delete behavior: RESTRICT
- Set when progress is accepted/rejected

**AcceptedAt** (DATETIME2, Nullable)
- Timestamp of acceptance/rejection
- Set when progress is accepted/rejected

### DeadlineExtensionRequests Table Columns

**TaskId** (Guid, Foreign Key, Non-nullable)
- Task ID
- References `Tasks.Id`
- Delete behavior: CASCADE

**RequestedById** (Guid, Foreign Key, Non-nullable)
- User who requested extension
- References `Users.Id`
- Delete behavior: RESTRICT

**RequestedDueDate** (DATETIME2, Non-nullable)
- Requested new due date
- Must be after current due date
- Validated in application

**Reason** (NVARCHAR(500), Non-nullable)
- Reason for extension request
- Required, max 500 characters

**Status** (INT, Non-nullable, Default: 0)
- Extension request status
- 0 = Pending, 1 = Approved, 2 = Rejected
- Stored as integer

**ReviewedById** (Guid, Foreign Key, Nullable)
- Manager who reviewed request
- References `Users.Id`
- Delete behavior: RESTRICT
- Set when request is approved/rejected

**ReviewedAt** (DATETIME2, Nullable)
- Timestamp of review
- Set when request is approved/rejected

**ReviewNotes** (NVARCHAR(1000), Nullable)
- Optional review notes
- Max 1000 characters

---

## Relationships

### One-to-Many Relationships

#### Users → Tasks (CreatedByUser)
- **Foreign Key:** `Tasks.CreatedById` → `Users.Id`
- **Delete Behavior:** RESTRICT (cannot delete user who created tasks)
- **Purpose:** Track task creator

#### Users → Tasks (AssignedUser)
- **Foreign Key:** `Tasks.AssignedUserId` → `Users.Id`
- **Delete Behavior:** RESTRICT (cannot delete user with assigned tasks)
- **Purpose:** Primary assignee

#### Tasks → TaskAssignments
- **Foreign Key:** `TaskAssignments.TaskId` → `Tasks.Id`
- **Delete Behavior:** CASCADE (deleting task deletes assignments)
- **Purpose:** Multiple assignees per task

#### Tasks → TaskProgressHistory
- **Foreign Key:** `TaskProgressHistory.TaskId` → `Tasks.Id`
- **Delete Behavior:** CASCADE (deleting task deletes progress history)
- **Purpose:** Historical progress tracking

#### Tasks → DeadlineExtensionRequests
- **Foreign Key:** `DeadlineExtensionRequests.TaskId` → `Tasks.Id`
- **Delete Behavior:** CASCADE (deleting task deletes extension requests)
- **Purpose:** Extension request tracking

#### Users → TaskAssignments
- **Foreign Key:** `TaskAssignments.UserId` → `Users.Id`
- **Delete Behavior:** RESTRICT (cannot delete user with assignments)
- **Purpose:** Track user assignments

#### Users → TaskProgressHistory (UpdatedByUser)
- **Foreign Key:** `TaskProgressHistory.UpdatedById` → `Users.Id`
- **Delete Behavior:** RESTRICT
- **Purpose:** Track who updated progress

#### Users → TaskProgressHistory (AcceptedByUser)
- **Foreign Key:** `TaskProgressHistory.AcceptedById` → `Users.Id`
- **Delete Behavior:** RESTRICT
- **Purpose:** Track who accepted progress

#### Users → DeadlineExtensionRequests (RequestedBy)
- **Foreign Key:** `DeadlineExtensionRequests.RequestedById` → `Users.Id`
- **Delete Behavior:** RESTRICT
- **Purpose:** Track who requested extension

#### Users → DeadlineExtensionRequests (ReviewedBy)
- **Foreign Key:** `DeadlineExtensionRequests.ReviewedById` → `Users.Id`
- **Delete Behavior:** RESTRICT
- **Purpose:** Track who reviewed extension request

---

## Indexes

### Primary Keys
All tables use `Id` (Guid) as primary key with clustered index.

### Unique Indexes

**Users:**
- `Email` (unique, non-clustered)
- `AzureAdObjectId` (unique, non-clustered)

**TaskAssignments:**
- `(TaskId, UserId)` (unique, non-clustered)

### Non-Unique Indexes

**Tasks:**
- `Status` (non-clustered, for filtering)
- `Priority` (non-clustered, for filtering)
- `DueDate` (non-clustered, for date range queries)
- `AssignedUserId` (non-clustered, foreign key index)
- `CreatedById` (non-clustered, foreign key index)

**TaskProgressHistory:**
- `TaskId` (non-clustered, foreign key index)
- `UpdatedById` (non-clustered, foreign key index)
- `Status` (non-clustered, for filtering)

**DeadlineExtensionRequests:**
- `TaskId` (non-clustered, foreign key index)
- `Status` (non-clustered, for filtering)

---

## Enum Storage

All enums are stored as integers in the database using Entity Framework Core's `HasConversion<int>()` method.

### TaskStatus Enum
- `Created = 0`
- `Assigned = 1`
- `UnderReview = 2`
- `Accepted = 3`
- `Rejected = 4`
- `Completed = 5`
- `Cancelled = 6`

### TaskPriority Enum
- `Low = 0`
- `Medium = 1`
- `High = 2`
- `Critical = 3`

### TaskType Enum
- `Simple = 0`
- `WithDueDate = 1`
- `WithProgress = 2`
- `WithAcceptedProgress = 3`

### ReminderLevel Enum
- `None = 0`
- `Low = 1`
- `Medium = 2`
- `High = 3`
- `Critical = 4`

### UserRole Enum
- `Employee = 0`
- `Manager = 1`
- `Admin = 2`

### ProgressStatus Enum
- `Pending = 0`
- `Accepted = 1`
- `Rejected = 2`

### ExtensionRequestStatus Enum
- `Pending = 0`
- `Approved = 1`
- `Rejected = 2`

---

## Migration Guide

### Creating Migrations

**Using EF Core CLI:**
```bash
dotnet ef migrations add MigrationName --project src/TaskManagement.Infrastructure --startup-project src/TaskManagement.Api
```

**Using Package Manager Console:**
```powershell
Add-Migration MigrationName -Project TaskManagement.Infrastructure -StartupProject TaskManagement.Api
```

### Applying Migrations

**Using EF Core CLI:**
```bash
dotnet ef database update --project src/TaskManagement.Infrastructure --startup-project src/TaskManagement.Api
```

**Using Package Manager Console:**
```powershell
Update-Database -Project TaskManagement.Infrastructure -StartupProject TaskManagement.Api
```

### Migration Best Practices

1. **Always Review Migrations**: Check generated migration files before applying
2. **Test Locally First**: Apply migrations in development environment first
3. **Backup Production**: Always backup production database before applying migrations
4. **Data Migration**: Handle data transformations in migration scripts if needed
5. **Rollback Plan**: Keep previous migration for rollback if needed

### Initial Migration

The initial migration creates all tables, indexes, and relationships. It should be applied to a fresh database.

### Future Migrations

When adding new features:
1. Update domain entities
2. Update `ApplicationDbContext` configuration
3. Create migration
4. Review migration SQL
5. Apply to development database
6. Test thoroughly
7. Apply to production database

---

## See Also

- [Domain Model](DOMAIN_MODEL.md) - Entity and relationship documentation
- [Architecture Documentation](ARCHITECTURE.md) - System architecture
- [Configuration Guide](CONFIGURATION.md) - Database configuration

