# Task Management API - Domain Model Documentation

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Entity Overview](#entity-overview)
2. [Entity Relationships](#entity-relationships)
3. [Enum Definitions](#enum-definitions)
4. [Business Rules](#business-rules)
5. [Entity Methods](#entity-methods)

---

## Entity Overview

### BaseEntity

All domain entities inherit from `BaseEntity`, providing common audit and tracking properties.

**Properties:**
- `Guid Id`: Unique identifier (primary key)
- `DateTime CreatedAt`: Entity creation timestamp
- `DateTime? UpdatedAt`: Last update timestamp
- `string CreatedBy`: Email of user who created the entity
- `string? UpdatedBy`: Email of user who last updated the entity

**Methods:**
- `void SetUpdatedBy(string email)`: Updates the `UpdatedBy` and `UpdatedAt` properties

### Task Entity

Represents a task in the system with support for multiple types, statuses, progress tracking, and reminder levels.

**Properties:**
```csharp
public string Title { get; private set; }           // Required, max 200 chars
public string? Description { get; private set; }     // Optional, max 1000 chars
public TaskStatus Status { get; private set; }        // Current status (enum)
public TaskPriority Priority { get; private set; }    // Priority level (enum)
public DateTime? DueDate { get; private set; }        // Current due date
public DateTime? OriginalDueDate { get; private set; } // Original due date before extension
public DateTime? ExtendedDueDate { get; private set; } // Extended due date if extended
public Guid AssignedUserId { get; private set; }     // Primary assignee ID
public TaskType Type { get; private set; }           // Task type (enum)
public ReminderLevel ReminderLevel { get; private set; } // Calculated reminder level
public int? ProgressPercentage { get; private set; }  // Progress (0-100, nullable)
public Guid CreatedById { get; private set; }         // Creator user ID

// Navigation Properties
public User? AssignedUser { get; private set; }
public User? CreatedByUser { get; private set; }
public ICollection<TaskAssignment> Assignments { get; private set; }
public ICollection<TaskProgressHistory> ProgressHistory { get; private set; }
public ICollection<DeadlineExtensionRequest> ExtensionRequests { get; private set; }
```

**Constructor:**
```csharp
public Task(string title, string? description, TaskPriority priority, 
    DateTime? dueDate, Guid assignedUserId, TaskType type, Guid createdById)
```

**Business Methods:**
- `UpdateTitle(string title)`: Updates the task title
- `UpdateDescription(string? description)`: Updates the task description
- `UpdatePriority(TaskPriority priority)`: Updates the task priority
- `UpdateDueDate(DateTime? dueDate)`: Updates the due date
- `AssignToUser(Guid userId)`: Assigns task to a user
- `Assign()`: Transitions status from `Created` to `Assigned`
- `SetUnderReview()`: Transitions to `UnderReview` status
- `Accept()`: Transitions to `Accepted` status
- `Reject()`: Transitions to `Rejected` status
- `UpdateProgress(int percentage, bool requiresAcceptance)`: Updates progress percentage
- `AcceptProgress()`: Accepts a progress update (for `WithAcceptedProgress` type)
- `UpdateReminderLevel(ReminderLevel level)`: Updates reminder level
- `ExtendDeadline(DateTime newDueDate, string? reason)`: Extends the deadline
- `Complete()`: Marks task as completed
- `Cancel()`: Cancels the task

**Business Rules:**
- Title cannot be null or empty
- Description max length: 1000 characters
- Progress percentage must be between 0 and 100
- Simple tasks cannot have progress tracking
- Only `WithProgress` and `WithAcceptedProgress` types support progress
- Cannot cancel a completed task
- Extended due date must be after current due date

### User Entity

Represents a user in the system with Azure AD integration and role-based access.

**Properties:**
```csharp
public string Email { get; private set; }              // Required, unique, max 256 chars
public string FirstName { get; private set; }          // Required, max 100 chars
public string LastName { get; private set; }            // Required, max 100 chars
public string DisplayName { get; private set; }         // Auto-generated, max 200 chars
public string? AzureAdObjectId { get; private set; }    // Optional, unique when provided, max 100 chars
public bool IsActive { get; private set; }              // Active status
public DateTime? LastLoginAt { get; private set; }      // Last login timestamp
public UserRole Role { get; private set; }              // User role (enum)
```

**Constructor:**
```csharp
public User(string email, string firstName, string lastName, string? azureAdObjectId)
```

**Business Methods:**
- `UpdateProfile(string firstName, string lastName)`: Updates user profile
- `RecordLogin()`: Records login timestamp
- `Deactivate()`: Deactivates the user
- `Activate()`: Activates the user
- `UpdateRole(UserRole role)`: Updates user role

**Business Rules:**
- Email must be unique
- AzureAdObjectId must be unique when provided (nullable, allows multiple nulls)
- DisplayName is auto-generated from FirstName and LastName
- Default role is `Employee`

### TaskAssignment Entity

Represents a many-to-many relationship between tasks and users, allowing multiple assignees per task.

**Properties:**
```csharp
public Guid TaskId { get; private set; }               // Foreign key to Task
public Guid UserId { get; private set; }               // Foreign key to User
public bool IsPrimary { get; private set; }             // Primary assignee flag

// Navigation Properties
public Task? Task { get; private set; }
public User? User { get; private set; }
```

**Constructor:**
```csharp
public TaskAssignment(Guid taskId, Guid userId, bool isPrimary)
```

**Business Methods:**
- `UpdatePrimaryStatus(bool isPrimary)`: Updates primary assignee status

**Business Rules:**
- Unique constraint on (TaskId, UserId) combination
- One task can have multiple assignments
- One assignment per task-user combination

### TaskProgressHistory Entity

Tracks historical progress updates for tasks, including acceptance status.

**Properties:**
```csharp
public Guid TaskId { get; private set; }               // Foreign key to Task
public Guid UpdatedById { get; private set; }           // User who updated progress
public int ProgressPercentage { get; private set; }     // Progress value (0-100)
public string? Notes { get; private set; }              // Optional notes, max 1000 chars
public ProgressStatus Status { get; private set; }      // Acceptance status (enum)
public Guid? AcceptedById { get; private set; }         // Manager who accepted
public DateTime? AcceptedAt { get; private set; }      // Acceptance timestamp

// Navigation Properties
public Task? Task { get; private set; }
public User? UpdatedByUser { get; private set; }
public User? AcceptedByUser { get; private set; }
```

**Constructor:**
```csharp
public TaskProgressHistory(Guid taskId, Guid updatedById, int progressPercentage, string? notes = null)
```

**Business Methods:**
- `Accept(Guid acceptedById)`: Accepts the progress update
- `Reject(Guid rejectedById)`: Rejects the progress update

**Business Rules:**
- ProgressPercentage must be between 0 and 100
- Status defaults to `Pending` on creation
- AcceptedAt is set when progress is accepted or rejected

### DeadlineExtensionRequest Entity

Manages requests for extending task deadlines with approval workflow.

**Properties:**
```csharp
public Guid TaskId { get; private set; }               // Foreign key to Task
public Guid RequestedById { get; private set; }          // User who requested extension
public DateTime RequestedDueDate { get; private set; }  // Requested new due date
public string Reason { get; private set; }              // Required reason, max 500 chars
public ExtensionRequestStatus Status { get; private set; } // Request status (enum)
public Guid? ReviewedById { get; private set; }         // Manager who reviewed
public DateTime? ReviewedAt { get; private set; }       // Review timestamp
public string? ReviewNotes { get; private set; }        // Optional review notes, max 1000 chars

// Navigation Properties
public Task? Task { get; private set; }
public User? RequestedBy { get; private set; }
public User? ReviewedBy { get; private set; }
```

**Constructor:**
```csharp
public DeadlineExtensionRequest(Guid taskId, Guid requestedById, 
    DateTime requestedDueDate, string reason)
```

**Business Methods:**
- `Approve(Guid reviewedById, string? reviewNotes)`: Approves the extension request
- `Reject(Guid reviewedById, string? reviewNotes)`: Rejects the extension request

**Business Rules:**
- Status defaults to `Pending` on creation
- Reason is required and must be provided
- ReviewedAt is set when request is approved or rejected

---

## Entity Relationships

### Relationship Diagram

```
User
├── 1:N → Task (CreatedByUser)
│   └── Task.CreatedById → User.Id
├── 1:N → Task (AssignedUser)
│   └── Task.AssignedUserId → User.Id
├── 1:N → TaskAssignment (User)
│   └── TaskAssignment.UserId → User.Id
├── 1:N → TaskProgressHistory (UpdatedByUser)
│   └── TaskProgressHistory.UpdatedById → User.Id
├── 1:N → TaskProgressHistory (AcceptedByUser)
│   └── TaskProgressHistory.AcceptedById → User.Id
├── 1:N → DeadlineExtensionRequest (RequestedBy)
│   └── DeadlineExtensionRequest.RequestedById → User.Id
└── 1:N → DeadlineExtensionRequest (ReviewedBy)
    └── DeadlineExtensionRequest.ReviewedById → User.Id

Task
├── 1:N → TaskAssignment
│   └── TaskAssignment.TaskId → Task.Id (CASCADE DELETE)
├── 1:N → TaskProgressHistory
│   └── TaskProgressHistory.TaskId → Task.Id (CASCADE DELETE)
└── 1:N → DeadlineExtensionRequest
    └── DeadlineExtensionRequest.TaskId → Task.Id (CASCADE DELETE)
```

### Relationship Details

#### Task ↔ User (Many-to-One)

**Task.AssignedUserId → User.Id**
- **Type**: Foreign key relationship
- **Delete Behavior**: `Restrict` (cannot delete user if tasks are assigned)
- **Purpose**: Primary assignee for the task

**Task.CreatedById → User.Id**
- **Type**: Foreign key relationship
- **Delete Behavior**: `Restrict` (cannot delete user who created tasks)
- **Purpose**: User who created the task

#### Task ↔ TaskAssignment (One-to-Many)

**Task.Id → TaskAssignment.TaskId**
- **Type**: Foreign key relationship
- **Delete Behavior**: `Cascade` (deleting task deletes assignments)
- **Purpose**: Multiple assignees per task
- **Constraint**: Unique (TaskId, UserId)

#### Task ↔ TaskProgressHistory (One-to-Many)

**Task.Id → TaskProgressHistory.TaskId**
- **Type**: Foreign key relationship
- **Delete Behavior**: `Cascade` (deleting task deletes progress history)
- **Purpose**: Historical progress tracking

#### Task ↔ DeadlineExtensionRequest (One-to-Many)

**Task.Id → DeadlineExtensionRequest.TaskId**
- **Type**: Foreign key relationship
- **Delete Behavior**: `Cascade` (deleting task deletes extension requests)
- **Purpose**: Track deadline extension requests

#### User ↔ TaskAssignment (One-to-Many)

**User.Id → TaskAssignment.UserId**
- **Type**: Foreign key relationship
- **Delete Behavior**: `Restrict` (cannot delete user with assignments)
- **Purpose**: Track user assignments

#### User ↔ TaskProgressHistory (One-to-Many)

**User.Id → TaskProgressHistory.UpdatedById**
- **Type**: Foreign key relationship
- **Delete Behavior**: `Restrict`
- **Purpose**: Track who updated progress

**User.Id → TaskProgressHistory.AcceptedById**
- **Type**: Foreign key relationship (nullable)
- **Delete Behavior**: `Restrict`
- **Purpose**: Track who accepted progress

#### User ↔ DeadlineExtensionRequest (One-to-Many)

**User.Id → DeadlineExtensionRequest.RequestedById**
- **Type**: Foreign key relationship
- **Delete Behavior**: `Restrict`
- **Purpose**: Track who requested extension

**User.Id → DeadlineExtensionRequest.ReviewedById**
- **Type**: Foreign key relationship (nullable)
- **Delete Behavior**: `Restrict`
- **Purpose**: Track who reviewed extension request

---

## Enum Definitions

### TaskStatus

Represents the lifecycle state of a task.

```csharp
public enum TaskStatus
{
    Created = 0,      // Task created, not yet assigned
    Assigned = 1,     // Task assigned to user(s)
    UnderReview = 2,  // Task under review (progress update or info request)
    Accepted = 3,     // Task accepted by assignee
    Rejected = 4,     // Task rejected by assignee
    Completed = 5,   // Task completed
    Cancelled = 6,    // Task cancelled
    
    // Legacy mappings for backward compatibility
    Pending = 0,      // Maps to Created
    InProgress = 1    // Maps to Assigned
}
```

**Status Transitions:**
- `Created` → `Assigned` (via `Assign()`)
- `Assigned` → `UnderReview` (via `SetUnderReview()` or progress update)
- `Assigned` → `Accepted` (via `Accept()`)
- `Assigned` → `Rejected` (via `Reject()`)
- `UnderReview` → `Accepted` (via `Accept()` or `AcceptProgress()`)
- `Accepted` → `UnderReview` (via `SetUnderReview()`)
- Any → `Completed` (via `Complete()`)
- Any (except `Completed`) → `Cancelled` (via `Cancel()`)

### TaskPriority

Represents the priority level of a task.

```csharp
public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
```

### TaskType

Represents the type of task, determining supported features.

```csharp
public enum TaskType
{
    Simple = 0,              // Basic task, no due date or progress
    WithDueDate = 1,         // Task with due date tracking
    WithProgress = 2,        // Task with progress percentage tracking
    WithAcceptedProgress = 3 // Task with progress tracking requiring acceptance
}
```

**Feature Support:**
- `Simple`: Title, Description, Priority, Status
- `WithDueDate`: + DueDate, ReminderLevel
- `WithProgress`: + ProgressPercentage (self-reported)
- `WithAcceptedProgress`: + ProgressPercentage (requires manager acceptance)

### ReminderLevel

Represents the urgency level based on due date proximity.

```csharp
public enum ReminderLevel
{
    None = 0,     // No reminder needed
    Low = 1,      // Low urgency
    Medium = 2,   // Medium urgency
    High = 3,     // High urgency
    Critical = 4  // Critical - overdue or very close to due date
}
```

**Calculation:**
- Based on percentage of time elapsed or days remaining
- Configurable thresholds via `ReminderOptions`
- Auto-calculated by `ReminderCalculationService`

### UserRole

Represents the role of a user in the system.

```csharp
public enum UserRole
{
    Employee = 0,  // Basic user, can manage assigned tasks
    Manager = 1,  // Can assign tasks, accept progress, approve extensions
    Admin = 2     // Full system access
}
```

**Permissions:**
- **Employee**: Create tasks, update assigned tasks, update progress, request extensions
- **Manager**: All Employee permissions + assign tasks, accept progress, approve extensions, mark completed
- **Admin**: All Manager permissions + system administration

### ProgressStatus

Represents the acceptance status of a progress update.

```csharp
public enum ProgressStatus
{
    Pending = 0,   // Awaiting manager approval
    Accepted = 1,  // Approved by manager
    Rejected = 2   // Rejected by manager
}
```

### ExtensionRequestStatus

Represents the status of a deadline extension request.

```csharp
public enum ExtensionRequestStatus
{
    Pending = 0,   // Awaiting manager review
    Approved = 1,  // Approved by manager
    Rejected = 2   // Rejected by manager
}
```

---

## Business Rules

### Task Lifecycle Rules

#### Status Transitions

**Valid Transitions:**
1. `Created` → `Assigned`: When manager assigns task to user(s)
2. `Assigned` → `Accepted`: When assignee accepts the task
3. `Assigned` → `Rejected`: When assignee rejects the task
4. `Assigned` → `UnderReview`: When assignee requests more info or updates progress
5. `UnderReview` → `Accepted`: When progress is accepted or assignee accepts task
6. `Accepted` → `UnderReview`: When assignee updates progress requiring acceptance
7. Any (except `Completed`) → `Completed`: When manager marks task as completed
8. Any (except `Completed`) → `Cancelled`: When task is cancelled

**Invalid Transitions:**
- Cannot transition from `Completed` to any other status
- Cannot transition from `Cancelled` to any other status (except via system operations)
- Cannot directly transition from `Created` to `Accepted` (must be `Assigned` first)

#### Task Creation Rules

- Title is required (cannot be null or empty)
- Title max length: 200 characters
- Description max length: 1000 characters
- AssignedUserId must reference an existing active user
- CreatedById must reference an existing active user
- DueDate cannot be in the past (if provided)
- Task type determines supported features

#### Task Assignment Rules

- Manager role required to assign tasks
- Can assign to multiple users (via `TaskAssignment`)
- First assigned user becomes primary assignee (`AssignedUserId`)
- Existing assignments are cleared when reassigning
- Assigned users must be active

#### Task Progress Rules

- Progress can only be updated for `WithProgress` or `WithAcceptedProgress` types
- `Simple` tasks cannot have progress tracking
- Progress percentage must be between 0 and 100
- For `WithAcceptedProgress` type, progress updates require manager acceptance
- Progress updates create entries in `TaskProgressHistory`
- Task status becomes `UnderReview` when progress requires acceptance

#### Task Completion Rules

- Only managers can mark tasks as completed
- Cannot complete a cancelled task
- Cannot complete an already completed task
- For progress-tracked tasks, completion sets progress to 100%

### Reminder Calculation Rules

**Percentage-Based Calculation:**
- Requires both `DueDate` and `CreatedAt`
- Calculates: `elapsedTime / totalDuration`
- Thresholds (configurable):
  - Critical: ≥ 90% elapsed
  - High: ≥ 75% elapsed
  - Medium: ≥ 50% elapsed
  - Low: ≥ 25% elapsed

**Day-Based Calculation:**
- Based on days remaining until due date
- Thresholds (configurable):
  - Critical: ≤ 1 day remaining
  - High: ≤ 3 days remaining
  - Medium: ≤ 7 days remaining
  - Low: ≤ 14 days remaining

**Special Cases:**
- If `DueDate` is null: `ReminderLevel.None`
- If `DueDate` has passed: `ReminderLevel.Critical`

### Extension Request Rules

- Only employees assigned to a task can request extensions
- Requested due date must be after current due date
- Requested due date cannot be in the past
- Reason is required (max 500 characters)
- Request status starts as `Pending`
- Only managers can approve or reject requests
- Approved requests update task's `DueDate`, `ExtendedDueDate`, and `OriginalDueDate`
- Extension policy limits can be configured (`ExtensionPolicyOptions`)

### Delegation Rules

- Managers can assign tasks to one or multiple employees
- Assigned employees can accept, reject, or request more information
- Primary assignee is stored in `Task.AssignedUserId`
- Additional assignees stored in `TaskAssignment` table
- Reassignment clears existing assignments and creates new ones
- Cannot assign task to yourself (unless admin override)

### Role Permissions

#### Employee Permissions
- Create tasks
- View assigned tasks
- Update assigned task details (title, description, priority)
- Update task progress
- Request deadline extensions
- Accept assigned tasks
- Reject assigned tasks
- Request more information on assigned tasks

#### Manager Permissions
- All Employee permissions
- Assign tasks to employees
- Reassign tasks
- Accept task progress updates
- Approve/reject deadline extension requests
- Mark tasks as completed
- View all tasks (not just assigned)

#### Admin Permissions
- All Manager permissions
- System administration
- User management
- Override business rules (if implemented)

---

## Entity Methods

### Task Methods

#### `Assign()`
- **Purpose**: Transition task from `Created` to `Assigned` status
- **Preconditions**: Task status must be `Created`
- **Postconditions**: Status becomes `Assigned`

#### `Accept()`
- **Purpose**: Accept an assigned task
- **Preconditions**: Task status must be `Assigned` or `UnderReview`
- **Postconditions**: Status becomes `Accepted`

#### `Reject()`
- **Purpose**: Reject an assigned task
- **Preconditions**: Task status must be `Assigned` or `UnderReview`
- **Postconditions**: Status becomes `Rejected`

#### `SetUnderReview()`
- **Purpose**: Set task status to under review
- **Preconditions**: Status must be `Assigned` or `Accepted`
- **Postconditions**: Status becomes `UnderReview`

#### `UpdateProgress(int percentage, bool requiresAcceptance)`
- **Purpose**: Update task progress percentage
- **Preconditions**: 
  - Task type must be `WithProgress` or `WithAcceptedProgress`
  - Percentage must be 0-100
- **Postconditions**: 
  - `ProgressPercentage` updated
  - If `requiresAcceptance` and type is `WithAcceptedProgress`, status becomes `UnderReview`

#### `AcceptProgress()`
- **Purpose**: Accept a progress update (manager action)
- **Preconditions**: 
  - Task type must be `WithAcceptedProgress`
  - Status must be `UnderReview`
- **Postconditions**: Status becomes `Accepted`

#### `ExtendDeadline(DateTime newDueDate, string? reason)`
- **Purpose**: Extend task deadline
- **Preconditions**: 
  - `newDueDate` must be in the future
  - `newDueDate` must be after current `DueDate`
- **Postconditions**: 
  - `ExtendedDueDate` set to `newDueDate`
  - `OriginalDueDate` stores previous `DueDate`
  - `DueDate` updated to `newDueDate`

#### `Complete()`
- **Purpose**: Mark task as completed
- **Preconditions**: 
  - Task status is not `Completed`
  - Task status is not `Cancelled`
- **Postconditions**: 
  - Status becomes `Completed`
  - For progress-tracked tasks, `ProgressPercentage` set to 100

#### `Cancel()`
- **Purpose**: Cancel a task
- **Preconditions**: Task status is not `Completed`
- **Postconditions**: Status becomes `Cancelled`

---

## See Also

- [Architecture Documentation](ARCHITECTURE.md) - System architecture overview
- [API Reference](API_REFERENCE.md) - API endpoint documentation
- [Business Rules](BUSINESS_RULES.md) - Detailed business rules documentation
- [Database Schema](DATABASE_SCHEMA.md) - Database structure and relationships

