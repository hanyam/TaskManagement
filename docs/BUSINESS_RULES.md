# Task Management API - Business Rules Documentation

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Task Lifecycle Rules](#task-lifecycle-rules)
2. [Progress Tracking Rules](#progress-tracking-rules)
3. [Reminder Calculation Rules](#reminder-calculation-rules)
4. [Extension Request Rules](#extension-request-rules)
5. [Delegation Rules](#delegation-rules)
6. [Role Permissions Matrix](#role-permissions-matrix)

---

## Task Lifecycle Rules

### Status Transition Rules

**Valid Transitions:**

```
Created → Assigned
  ↓
Assigned → Accepted
Assigned → Rejected
Assigned → UnderReview
  ↓
UnderReview → Accepted
UnderReview → Rejected
  ↓
Accepted → UnderReview
  ↓
Any (except Completed/Cancelled) → Completed
Any (except Completed) → Cancelled
```

**Status Transition Diagram:**

```
                    [Created]
                       │
                       ▼
                   [Assigned]
                  ╱    │    ╲
                 ╱     │     ╲
                ▼      ▼      ▼
          [Accepted] [Rejected] [UnderReview]
                │                │
                └────────────────┘
                       │
                       ▼
                 [Completed]
                       
                    [Cancelled]
```

**Invalid Transitions:**
- `Completed` → Any status (except system operations)
- `Cancelled` → Any status (except system operations)
- `Created` → `Accepted` (must be `Assigned` first)
- `Accepted` → `Assigned` (not allowed)

**Enforcement:**
- Status transitions validated in domain entity methods
- Invalid transitions throw `InvalidOperationException`
- Error returned to client with appropriate message

### Allowed Transitions Detail

#### Created → Assigned
- **Action:** Manager assigns task to employee(s)
- **Method:** `Task.Assign()`
- **Precondition:** Task status is `Created`
- **Postcondition:** Status becomes `Assigned`

#### Assigned → Accepted
- **Action:** Employee accepts assigned task
- **Method:** `Task.Accept()`
- **Precondition:** Task status is `Assigned` or `UnderReview`
- **Postcondition:** Status becomes `Accepted`

#### Assigned → Rejected
- **Action:** Employee rejects assigned task
- **Method:** `Task.Reject()`
- **Precondition:** Task status is `Assigned` or `UnderReview`
- **Postcondition:** Status becomes `Rejected`

#### Assigned → UnderReview
- **Action:** Employee requests more information or updates progress (with acceptance required)
- **Method:** `Task.SetUnderReview()` or `Task.UpdateProgress()`
- **Precondition:** Task status is `Assigned` or `Accepted`
- **Postcondition:** Status becomes `UnderReview`

#### UnderReview → Accepted
- **Action:** Manager accepts progress or employee accepts after review
- **Method:** `Task.AcceptProgress()` or `Task.Accept()`
- **Precondition:** Task status is `UnderReview`
- **Postcondition:** Status becomes `Accepted`

#### Any → Completed
- **Action:** Manager marks task as completed
- **Method:** `Task.Complete()`
- **Precondition:** Task status is not `Completed` and not `Cancelled`
- **Postcondition:** Status becomes `Completed`, progress set to 100% (if applicable)

#### Any → Cancelled
- **Action:** User cancels task
- **Method:** `Task.Cancel()`
- **Precondition:** Task status is not `Completed`
- **Postcondition:** Status becomes `Cancelled`

### Invalid Transition Handling

**Error Response:**
```json
{
  "success": false,
  "data": null,
  "message": "Invalid status transition",
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Task must be assigned or under review to be accepted",
      "field": "Status"
    }
  ]
}
```

---

## Progress Tracking Rules

### Progress Percentage Validation

**Rules:**
- Progress must be between 0 and 100 (inclusive)
- Progress can be null for task types that don't support it
- Validation occurs in `Task.UpdateProgress()` method

**Validation Logic:**
```csharp
if (percentage < 0 || percentage > 100)
    throw new ArgumentException("Progress percentage must be between 0 and 100");
```

### Progress Acceptance Requirements

**Task Type Support:**
- `Simple`: Progress not supported (cannot have progress)
- `WithDueDate`: Progress not supported
- `WithProgress`: Progress supported, no acceptance required
- `WithAcceptedProgress`: Progress supported, acceptance required

**Acceptance Workflow:**
1. Employee updates progress
2. Progress entry created in `TaskProgressHistory` with `Pending` status
3. Task status becomes `UnderReview` (if acceptance required)
4. Manager reviews progress update
5. Manager accepts or rejects progress
6. If accepted, task status becomes `Accepted`, progress updated on task
7. If rejected, task remains `UnderReview`, progress entry marked as rejected

### Task Type Constraints

**Simple Tasks:**
- Cannot have progress tracking
- Attempting to set progress throws `InvalidOperationException`
- Progress percentage always null

**WithProgress Tasks:**
- Progress can be updated freely
- No acceptance required
- Progress history tracked

**WithAcceptedProgress Tasks:**
- Progress updates require manager acceptance
- Task status transitions to `UnderReview` on progress update
- Manager must accept before progress is finalized

---

## Reminder Calculation Rules

### Percentage Calculation Formula

**Formula:**
```
percentageElapsed = (now - createdAt) / (dueDate - createdAt)
```

**Example:**
- Task created: 2024-01-01
- Due date: 2024-02-01 (31 days)
- Current date: 2024-01-20 (20 days elapsed)
- Percentage: 20 / 31 = 0.645 (64.5%)

**Thresholds (default):**
- Critical: ≥ 90% elapsed
- High: ≥ 75% elapsed
- Medium: ≥ 50% elapsed
- Low: ≥ 25% elapsed
- None: < 25% elapsed

**Requirements:**
- Requires both `DueDate` and `CreatedAt`
- Falls back to day-based calculation if `CreatedAt` unavailable

### Day Threshold Calculation

**Formula:**
```
daysRemaining = dueDate - now
```

**Example:**
- Due date: 2024-02-01
- Current date: 2024-01-29
- Days remaining: 3 days

**Thresholds (default):**
- Critical: ≤ 1 day remaining
- High: ≤ 3 days remaining
- Medium: ≤ 7 days remaining
- Low: ≤ 14 days remaining
- None: > 14 days remaining

**Special Cases:**
- If `DueDate` is null: `ReminderLevel.None`
- If `DueDate` has passed: `ReminderLevel.Critical`

### Critical Level Determination

**Automatic Critical:**
- Due date has passed (`dueDate < now`)
- 90% or more of time elapsed (percentage-based)
- 1 day or less remaining (day-based)

**Behavior:**
- Critical level indicates urgent attention needed
- Dashboard highlights critical tasks
- Filtering by critical level available

---

## Extension Request Rules

### Eligibility Criteria

**Who Can Request:**
- Employees assigned to the task
- Managers assigned to the task
- Must be assigned via `TaskAssignment` or be primary assignee

**When Can Request:**
- Before task is completed
- Before task is cancelled
- Configurable: `MinDaysBeforeDueDate` (default: 1 day before due date)

**What Can Request:**
- New due date must be after current due date
- New due date must be in the future
- Reason is required (max 500 characters)

### Approval Workflow

**Request Process:**
1. Employee submits extension request
2. Request status set to `Pending`
3. Manager notified (via system)
4. Manager reviews request

**Approval Process:**
1. Manager approves or rejects request
2. If approved:
   - Task `DueDate` updated to requested date
   - `ExtendedDueDate` set to requested date
   - `OriginalDueDate` preserves previous due date
   - Request status set to `Approved`
3. If rejected:
   - Request status set to `Rejected`
   - Review notes explain rejection
   - Task due date unchanged

### Extension Limits

**Per Task Limits:**
- Maximum requests per task: `MaxExtensionRequestsPerTask` (default: 3)
- Maximum extension days: `MaxExtensionDays` (default: 30 days)

**Business Rules:**
- Each task can have multiple extension requests
- Only pending requests can be approved/rejected
- Approved extensions update task due date
- Extension history tracked in `DeadlineExtensionRequest` entities

**Policy Enforcement:**
- Validated in handler before creating request
- Returns error if limit exceeded
- Configurable via `ExtensionPolicyOptions`

---

## Delegation Rules

### Assignment Rules

**Who Can Assign:**
- Managers can assign tasks
- Admins can assign tasks
- Employees cannot assign tasks

**Assignment Process:**
1. Manager selects task and list of user IDs
2. System validates all users exist and are active
3. System clears existing assignments (if reassigning)
4. System creates `TaskAssignment` entries for each user
5. First user becomes primary assignee (`AssignedUserId`)
6. Task status becomes `Assigned`

**Validation Rules:**
- All user IDs must exist
- All users must be active
- Task must exist
- At least one user must be specified
- Cannot assign to yourself (unless admin override)

### Reassignment Rules

**When to Reassign:**
- Original assignee unavailable
- Task scope changes require different expertise
- Workload rebalancing
- Priority shifts

**Reassignment Process:**
1. Manager initiates reassignment
2. System clears existing `TaskAssignment` entries
3. System creates new assignments for new user(s)
4. System updates primary assignee on `Task` entity
5. Task remains in current status (Assigned or Accepted)

**Validation:**
- Same validation as initial assignment
- Cannot reassign completed tasks
- Cannot reassign cancelled tasks

### Multiple Assignee Handling

**Features:**
- One task can have multiple assignees
- Each assignment tracked via `TaskAssignment`
- Primary assignee designated (stored in `Task.AssignedUserId`)
- All assignees can work on task
- All assignees can accept/reject task

**Business Rules:**
- Only one primary assignee per task
- Unique constraint: (TaskId, UserId) combination
- Assignment timestamp tracked via `CreatedAt`

---

## Role Permissions Matrix

### Employee Allowed Actions

**Task Management:**
- ✅ Create tasks
- ✅ View assigned tasks
- ✅ Update assigned task details (title, description, priority)
- ✅ Update task progress
- ✅ Request deadline extensions

**Task Actions:**
- ✅ Accept assigned tasks
- ✅ Reject assigned tasks
- ✅ Request more information on tasks

**Viewing:**
- ✅ View dashboard statistics (own tasks)
- ✅ View task progress history (assigned tasks)

**Restrictions:**
- ❌ Cannot assign tasks to others
- ❌ Cannot accept progress updates
- ❌ Cannot approve extension requests
- ❌ Cannot mark tasks as completed
- ❌ Cannot view all tasks (only assigned)

### Manager Allowed Actions

**All Employee Permissions Plus:**

**Delegation:**
- ✅ Assign tasks to employees
- ✅ Reassign tasks to different employees
- ✅ View all tasks (not just assigned)

**Progress Management:**
- ✅ Accept task progress updates
- ✅ View progress history for all tasks

**Extension Management:**
- ✅ Approve deadline extension requests
- ✅ Reject deadline extension requests

**Task Completion:**
- ✅ Mark tasks as completed

**Viewing:**
- ✅ View dashboard statistics (all tasks)
- ✅ View extension requests for all tasks

### Admin Allowed Actions

**All Manager Permissions Plus:**

**System Administration:**
- ✅ User management (if implemented)
- ✅ Role management (if implemented)
- ✅ System configuration (if implemented)
- ✅ Override business rules (if implemented)

**Future Enhancements:**
- Audit log access
- System-wide reporting
- Configuration management

---

## Rule Enforcement

### Domain Entity Enforcement

**Location:** Domain entity methods (`Task.cs`, `User.cs`, etc.)

**Examples:**
- `Task.Accept()`: Validates status before accepting
- `Task.UpdateProgress()`: Validates task type and percentage
- `Task.Complete()`: Prevents completing already completed tasks

**Errors:**
- Throws `InvalidOperationException` for business rule violations
- Throws `ArgumentException` for invalid parameter values

### Application Layer Enforcement

**Location:** Command/Query handlers

**Examples:**
- Assignment validation in `AssignTaskCommandHandler`
- Progress validation in `UpdateTaskProgressCommandHandler`
- Extension eligibility in `RequestDeadlineExtensionCommandHandler`

**Errors:**
- Returns `Result<T>.Failure()` with appropriate error
- Uses centralized error definitions (`TaskErrors`)

### Authorization Enforcement

**Location:** Controllers and handlers

**Examples:**
- `[Authorize(Roles = "Manager")]` on assignment endpoints
- Role checks in handlers
- User ID verification

**Errors:**
- Returns `401 Unauthorized` for authentication failures
- Returns `403 Forbidden` for authorization failures

---

## See Also

- [Domain Model](DOMAIN_MODEL.md) - Entity methods and business logic
- [Features Documentation](FEATURES.md) - Feature descriptions
- [API Reference](API_REFERENCE.md) - Endpoint documentation

