# Task Management API - Features Documentation

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Core Task Management](#core-task-management)
2. [Task Delegation System](#task-delegation-system)
3. [Progress Tracking System](#progress-tracking-system)
4. [Task Review Workflow](#task-review-workflow)
5. [Deadline Extension System](#deadline-extension-system)
6. [Reminder System](#reminder-system)
7. [Dashboard Statistics](#dashboard-statistics)
8. [Role-Based Access Control](#role-based-access-control)

---

## Core Task Management

### Task Creation with Types

The system supports four task types, each with different capabilities:

#### Simple Task (`TaskType.Simple`)
- Basic task with title, description, priority, and status
- No due date or progress tracking
- Suitable for simple to-do items

#### Task with Due Date (`TaskType.WithDueDate`)
- Includes all Simple task features
- Due date tracking
- Automatic reminder level calculation
- Deadline extension support

#### Task with Progress Tracking (`TaskType.WithProgress`)
- Includes all WithDueDate features
- Progress percentage tracking (0-100%)
- Self-reported progress updates
- Progress history tracking

#### Task with Accepted Progress (`TaskType.WithAcceptedProgress`)
- Includes all WithProgress features
- Progress updates require manager acceptance
- Task status transitions to `UnderReview` when progress is updated
- Manager approval workflow for progress updates

### Task Assignment Workflow

**Initial Assignment:**
1. Task is created with `Created` status
2. Manager assigns task to one or multiple employees
3. Task status transitions to `Assigned`
4. First assigned user becomes primary assignee
5. All assigned users are notified (via `TaskAssignment` entities)

**Assignment Features:**
- Multiple assignees per task
- Primary assignee tracking
- Assignment history via `TaskAssignment` entity
- Reassignment capability

### Task Status Lifecycle

The task status lifecycle supports the following states:

```
Created → Assigned → Accepted
                    ↓
              UnderReview → Accepted
                    ↓
              Rejected
                    ↓
              Completed / Cancelled
```

**Status Descriptions:**
- **Created**: Task created, not yet assigned
- **Assigned**: Task assigned to employee(s), awaiting acceptance
- **UnderReview**: Task or progress update under review
- **Accepted**: Task accepted by assignee or progress accepted by manager
- **Rejected**: Task rejected by assignee
- **Completed**: Task marked as completed
- **Cancelled**: Task cancelled

**Status Transition Rules:**
- Only managers can assign tasks (Created → Assigned)
- Only assigned employees can accept/reject tasks (Assigned → Accepted/Rejected)
- Progress updates can trigger UnderReview status
- Only managers can mark tasks as completed
- Completed or Cancelled tasks cannot transition to other states

---

## Task Delegation System

### Multi-User Assignment

Tasks can be assigned to multiple employees simultaneously:

**Features:**
- One task can have multiple assignees
- Each assignment tracked via `TaskAssignment` entity
- Primary assignee designated (stored in `Task.AssignedUserId`)
- All assignees can work on the task
- All assignees can accept/reject the task

**Use Cases:**
- Team collaboration on complex tasks
- Parallel work streams
- Backup assignees
- Cross-functional teams

### Assignment Management

**Creating Assignments:**
- Manager selects task and list of user IDs
- System creates `TaskAssignment` entries for each user
- First user becomes primary assignee
- Existing assignments are cleared when reassigning

**Assignment Properties:**
- `IsPrimary`: Flags the primary assignee
- `CreatedAt`: Assignment timestamp (from `BaseEntity`)
- Unique constraint: (TaskId, UserId) combination

### Reassignment Process

**When to Reassign:**
- Original assignee unavailable
- Task scope changes require different expertise
- Workload rebalancing
- Priority shifts

**Reassignment Workflow:**
1. Manager initiates reassignment
2. System clears existing `TaskAssignment` entries
3. Creates new assignments for new user(s)
4. Updates primary assignee on `Task` entity
5. Task remains in current status (Assigned or Accepted)

---

## Progress Tracking System

### Progress Update Workflow

**Employee Actions:**
1. Employee updates progress percentage (0-100%)
2. Optionally adds notes describing progress
3. System creates `TaskProgressHistory` entry
4. For `WithAcceptedProgress` type, task status becomes `UnderReview`
5. Progress update awaits manager acceptance

**Manager Actions:**
1. Manager reviews progress update
2. Manager accepts or rejects progress
3. If accepted, task status becomes `Accepted`
4. Progress percentage updated on task
5. Progress history entry marked as accepted

**Progress Rules:**
- Progress must be between 0 and 100
- Simple tasks cannot have progress
- Progress history tracks all updates
- Most recent progress visible in task details
- Progress acceptance required for `WithAcceptedProgress` type

### Progress History Tracking

**History Features:**
- Complete audit trail of progress updates
- Each update includes timestamp, user, percentage, notes
- Acceptance/rejection tracking
- Manager review tracking

**History Properties:**
- `UpdatedById`: Employee who updated progress
- `AcceptedById`: Manager who accepted progress
- `Status`: Pending, Accepted, or Rejected
- `AcceptedAt`: Timestamp of acceptance/rejection

---

## Task Review Workflow

### Accept/Reject Assigned Tasks

**Employee Actions:**
- **Accept Task**: Employee accepts assigned task, status becomes `Accepted`
- **Reject Task**: Employee rejects assigned task with optional reason, status becomes `Rejected`

**Business Rules:**
- Only assigned employees can accept/reject
- Task must be in `Assigned` or `UnderReview` status
- Rejection requires optional reason for tracking
- Accepted tasks can have progress updated
- Rejected tasks remain rejected (can be reassigned)

### Request More Information

**Workflow:**
1. Employee requests more information on assigned task
2. System sets task status to `UnderReview`
3. Manager is notified
4. Manager provides additional information
5. Task can be accepted or rejected by employee

**Use Cases:**
- Unclear requirements
- Missing context
- Need for clarification
- Technical questions

### Review States

**UnderReview Status:**
- Triggered by progress updates (for `WithAcceptedProgress` type)
- Triggered by information requests
- Manager can accept progress or provide information
- Employee can accept or reject after review

---

## Deadline Extension System

### Extension Request Process

**Employee Workflow:**
1. Employee identifies need for deadline extension
2. Submits extension request with:
   - Requested new due date (must be after current due date)
   - Reason (required, max 500 characters)
3. System creates `DeadlineExtensionRequest` entity
4. Request status set to `Pending`
5. Manager is notified

**Request Validation:**
- Requested due date must be in the future
- Requested due date must be after current due date
- Reason is required
- Only assigned employees can request extensions
- Extension policy limits can be configured

### Approval Workflow

**Manager Actions:**
1. Manager reviews extension request
2. Manager approves or rejects request
3. Optionally adds review notes
4. If approved:
   - Task `DueDate` updated to requested date
   - `ExtendedDueDate` set to requested date
   - `OriginalDueDate` stores previous due date
   - Extension request status set to `Approved`

**Rejection:**
- Extension request status set to `Rejected`
- Review notes explain rejection reason
- Task due date remains unchanged

### Extension Policies

**Configurable Options** (`ExtensionPolicyOptions`):
- `MaxExtensionRequestsPerTask`: Maximum number of extension requests per task
- `MaxExtensionDaysPerRequest`: Maximum days that can be requested per extension
- `ManagerApprovalRequired`: Whether manager approval is required (always true currently)

**Business Rules:**
- Each task can have multiple extension requests
- Only pending requests can be approved/rejected
- Approved extensions update task due date
- Extension history tracked in `DeadlineExtensionRequest` entities

---

## Reminder System

### Reminder Level Calculation

The system automatically calculates reminder levels based on due date proximity.

#### Percentage-Based Calculation

**Method:**
- Calculates percentage of time elapsed: `(now - createdAt) / (dueDate - createdAt)`
- Applies configurable thresholds

**Thresholds** (default, configurable):
- **Critical**: ≥ 90% of time elapsed
- **High**: ≥ 75% of time elapsed
- **Medium**: ≥ 50% of time elapsed
- **Low**: ≥ 25% of time elapsed
- **None**: < 25% of time elapsed

**Requirements:**
- Requires both `DueDate` and `CreatedAt`
- Falls back to day-based calculation if `CreatedAt` unavailable

#### Day-Based Calculation

**Method:**
- Calculates days remaining until due date
- Applies configurable day thresholds

**Thresholds** (default, configurable):
- **Critical**: ≤ 1 day remaining
- **High**: ≤ 3 days remaining
- **Medium**: ≤ 7 days remaining
- **Low**: ≤ 14 days remaining
- **None**: > 14 days remaining

**Special Cases:**
- If `DueDate` is null: `ReminderLevel.None`
- If `DueDate` has passed: `ReminderLevel.Critical`

### Configuration

**ReminderOptions:**
```json
{
  "Reminder": {
    "UseDayThresholds": false,
    "Thresholds": {
      "Critical": 0.90,
      "High": 0.75,
      "Medium": 0.50,
      "Low": 0.25
    },
    "DayThresholds": {
      "Critical": 1,
      "High": 3,
      "Medium": 7,
      "Low": 14
    }
  }
}
```

### Reminder Level Usage

**Dashboard Integration:**
- Tasks filtered by reminder level
- Near due date detection
- Delayed task identification

**Task Filtering:**
- Query tasks by reminder level
- Filter by user and reminder level combination
- Support for pagination

---

## Dashboard Statistics

### Task Counts by Category

The dashboard provides comprehensive statistics for the current user:

#### Tasks Created by User
- Count of all tasks created by the current user
- Includes all statuses
- Useful for tracking workload

#### Tasks Completed
- Count of completed tasks
- Includes tasks created or assigned to user
- Measures productivity

#### Tasks Near Due Date
- Count of tasks approaching due date
- Based on reminder level calculation
- Helps prioritize urgent work

#### Tasks Delayed
- Count of tasks past due date
- `DueDate < Now`
- Critical attention needed

#### Tasks In Progress
- Count of tasks with `Assigned` status
- Active work items
- Workload visibility

#### Tasks Under Review
- Count of tasks with `UnderReview` status
- Awaiting manager action
- Blocked work items

#### Tasks Pending Acceptance
- Count of tasks awaiting progress acceptance
- For `WithAcceptedProgress` type tasks
- Manager action required

### Statistics Calculation

**Implementation:**
- Uses Entity Framework Core queries
- Filters by user ID and status
- Efficient counting with `Count()` method
- Real-time calculation (no caching)

**Performance:**
- Single database query per statistic
- Optimized with indexes on status and dates
- Pagination support for large datasets

---

## Role-Based Access Control

### Employee Permissions

**Task Management:**
- Create tasks
- View assigned tasks
- Update assigned task details (title, description, priority)
- Update task progress
- Request deadline extensions

**Task Actions:**
- Accept assigned tasks
- Reject assigned tasks
- Request more information on tasks

**Limitations:**
- Cannot assign tasks to others
- Cannot accept progress updates
- Cannot approve extension requests
- Cannot mark tasks as completed

### Manager Permissions

**All Employee Permissions Plus:**
- Assign tasks to employees
- Reassign tasks to different employees
- Accept task progress updates
- Approve/reject deadline extension requests
- Mark tasks as completed
- View all tasks (not just assigned)

**Manager Actions:**
- Delegation management
- Progress review and acceptance
- Extension request review
- Task completion

### Admin Permissions

**All Manager Permissions Plus:**
- System administration (if implemented)
- User management (if implemented)
- Override business rules (if implemented)

**Future Enhancements:**
- User role management
- System configuration
- Audit log access

### Authorization Implementation

**Attribute-Based:**
```csharp
[Authorize(Roles = "Manager")]
[Authorize(Roles = "Employee,Manager")]
```

**Handler-Level Checks:**
- Additional validation in command/query handlers
- User ID verification
- Assignment verification
- Business rule enforcement

**JWT Claims:**
- `role` claim extracted from token
- Role validation via ASP.NET Core authorization
- User ID from `user_id` claim

---

## Feature Integration

### Combined Workflows

**Task Creation → Assignment → Progress → Completion:**
1. Manager creates task with `WithAcceptedProgress` type
2. Manager assigns to employees
3. Employees accept task and update progress
4. Progress updates require manager acceptance
5. Manager accepts progress updates
6. Manager marks task as completed

**Extension Request → Approval → Progress:**
1. Employee requests deadline extension
2. Manager approves extension
3. Task due date updated
4. Employee continues progress updates
5. Task completed within extended deadline

**Multiple Assignees → Collaboration:**
1. Manager assigns task to multiple employees
2. All employees can accept/reject task
3. All employees can update progress
4. Progress history shows all contributions
5. Manager reviews and accepts progress
6. Task completed collaboratively

---

## See Also

- [API Reference](API_REFERENCE.md) - Complete API endpoint documentation
- [Domain Model](DOMAIN_MODEL.md) - Entity and relationship documentation
- [Business Rules](BUSINESS_RULES.md) - Detailed business rules
- [Architecture Documentation](ARCHITECTURE.md) - System architecture

