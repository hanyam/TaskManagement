# Task State Machine Documentation

## Overview

This document describes the complete state machine for task management in the system, including state transitions, business rules, and HATEOAS-driven action availability.

## Task States

### State Enum Values

| State | Value | Description |
|-------|-------|-------------|
| `Created` | 0 | Task created but not yet assigned |
| `Assigned` | 1 | Task assigned to an employee |
| `UnderReview` | 2 | Task submitted for manager review |
| `Accepted` | 3 | Task accepted by manager after review |
| `Rejected` | 4 | Task rejected by manager |
| `Completed` | 5 | Legacy status (not used in new workflow) |
| `Cancelled` | 6 | Task cancelled |
| `PendingManagerReview` | 7 | Employee marked task complete, awaiting manager review with rating |
| `RejectedByManager` | 8 | Manager rejected completed task with rating |

## State Transitions

### Created (0)

**Description**: Initial state when a task is created.

**Available Actions**:
- **Assign** → `Assigned` (Manager/Admin only)
- **Update** → `Created` (Creator/Manager/Admin)
- **Cancel** → `Cancelled` (Creator/Manager/Admin)

**Business Rules**:
- Task must have a title
- Can optionally have assigned user, due date, priority, type

---

### Assigned (1)

**Description**: Task is assigned to an employee and ready for work.

**Available Actions**:
- **Update Progress** → `Assigned` (Assigned user, if task type supports progress)
- **Mark Completed** → `PendingManagerReview` (Assigned user)
- **Update** → `Assigned` (Manager/Admin)
- **Cancel** → `Cancelled` (Manager/Admin)

**Business Rules**:
- Must have assigned user
- Employee can update progress if task type is `WithProgress` or `WithAcceptedProgress`
- When employee marks as completed, task moves to `PendingManagerReview` status

---

### UnderReview (2)

**Description**: Task submitted for manager review (for progress updates or other reviews).

**Available Actions**:
- **Accept** → `Accepted` (Manager/Admin)
- **Reject** → `Rejected` (Manager/Admin)
- **Cancel** → `Cancelled` (Manager/Admin)

**Business Rules**:
- Only managers/admins can accept or reject
- Used for progress review in tasks requiring acceptance

---

### Accepted (3)

**Description**: Task has been accepted by manager and employee can continue working.

**Available Actions**:
- **Update Progress** → `Accepted` (Assigned user)
- **Mark Completed** → `PendingManagerReview` (Assigned user)
- **Update** → `Accepted` (Manager/Admin)
- **Cancel** → `Cancelled` (Manager/Admin)

**Business Rules**:
- Employee can continue updating and eventually complete the task
- Completion triggers manager review workflow

---

### Rejected (4)

**Description**: Task has been rejected by manager.

**Available Actions**:
- **Reassign** → `Assigned` (Manager/Admin - send back for rework)
- **Update** → `Rejected` (Manager/Admin)
- **Cancel** → `Cancelled` (Manager/Admin)

**Business Rules**:
- Manager can reassign to same or different employee
- Task can be sent back to `Assigned` status for rework

---

### PendingManagerReview (7)

**Description**: Employee marked task as completed. Manager must review, rate (1-5 stars), and provide feedback.

**Available Actions**:
- **Review Completed** (via HATEOAS link `review-completed`) with options:
  - **Accept with rating** → `Accepted` (Terminal state with positive outcome)
  - **Reject with rating** → `RejectedByManager` (Terminal state with negative outcome)
  - **Send back for rework** → `Assigned` (Task needs more work)

**Business Rules**:
- Rating is required (1-5 stars)
- Feedback is optional (max 1000 characters)
- Cannot both accept and send back for rework simultaneously
- This is a critical decision point in the workflow
- Only managers/admins can review completed tasks
- The `review-completed` link is only available when task status is `PendingManagerReview` (7)

**Transition Details**:
```
PendingManagerReview + Accept → Accepted (with rating & feedback)
  - Status becomes Accepted (3)
  - managerRating and managerFeedback are set
  - UI shows "Accepted by Manager" badge
  - Task becomes read-only (no edit/action buttons)

PendingManagerReview + Reject → RejectedByManager (with rating & feedback)
  - Status becomes RejectedByManager (8)
  - managerRating and managerFeedback are set
  - Task becomes read-only (no edit/action buttons)

PendingManagerReview + SendBack → Assigned (with feedback for improvement)
  - Status returns to Assigned (1) - same status as before employee marked complete
  - managerFeedback is set (for improvement guidance)
  - managerRating is NOT set (task is not accepted/rejected, just sent back)
  - Task remains editable - employee can continue working
```

**UI Implementation**:
- When task status is `PendingManagerReview` (7) and user has manager/admin role, the "Review" button appears
- Clicking the button opens `ReviewCompletedTaskModal` with:
  - Decision selection (Accept/Reject/Send Back)
  - 5-star rating system (required)
  - Feedback textarea (optional, max 1000 characters)
  - Submit button that changes color based on decision (green for accept, red for reject, orange for send back)

---

### Completed/Accepted (from PendingManagerReview)

**Description**: Terminal state - task successfully completed and accepted by manager with rating.

**Available Actions**:
- **View** only (self link)

**Business Rules**:
- Task has `managerRating` (1-5)
- Task may have `managerFeedback`
- No further state transitions allowed
- **UI Display**: Status badge shows "Accepted by Manager" (not just "Accepted") when `managerRating` is present
- **UI Behavior**: All action buttons (edit, cancel, assign, etc.) are hidden - task is read-only
- **Manager Review Display**: Rating (stars) and feedback are displayed in a dedicated section in task details

---

### RejectedByManager (8)

**Description**: Terminal state - manager rejected the completed work with rating and feedback.

**Available Actions**:
- **View** only (self link)

**Business Rules**:
- Task has `managerRating` (typically low score)
- Task should have `managerFeedback` explaining rejection
- No further state transitions allowed
- Task is considered closed/failed
- **UI Behavior**: All action buttons (edit, cancel, assign, etc.) are hidden - task is read-only
- **Manager Review Display**: Rating (stars) and feedback are displayed in a dedicated section in task details

---

### Cancelled (6)

**Description**: Terminal state - task was cancelled and won't be completed.

**Available Actions**:
- **View** only (self link)

**Business Rules**:
- No further state transitions allowed
- Task preserves all history up to cancellation point

## HATEOAS Implementation

### Overview

The API uses HATEOAS (Hypermedia as the Engine of Application State) to communicate available actions to clients. The `ApiResponse<T>` envelope includes a `links` array that tells the client which actions are currently available based on:
- Task state
- User role (Employee, Manager, Admin)
- User relationship to task (creator, assigned user, etc.)

### Link Structure

```typescript
interface ApiActionLink {
  rel: string;      // Relationship type (e.g., "accept", "reject", "mark-completed")
  href: string;     // URI for the action (e.g., "/tasks/{id}/accept")
  method: string;   // HTTP method (e.g., "POST", "GET", "PUT", "DELETE")
}
```

### Example Response

```json
{
  "success": true,
  "data": {
    "id": "...",
    "title": "...",
    "status": 7,
    ...
  },
  "links": [
    {
      "rel": "self",
      "href": "/tasks/abc-123",
      "method": "GET"
    },
    {
      "rel": "review-completed",
      "href": "/tasks/abc-123/review-completed",
      "method": "POST"
    }
  ]
}
```

### Frontend Usage

The frontend uses helper functions to check for action availability:

```typescript
// Check if action is available
if (hasActionLink(response.links, "mark-completed")) {
  // Show "Mark as Completed" button
}

// Get specific link
const reviewLink = getActionLink(response.links, "review-completed");
if (reviewLink) {
  // Enable review action with proper endpoint
}
```

### Benefits

1. **Decoupling**: Frontend doesn't hardcode business rules
2. **Flexibility**: Backend controls what actions are available
3. **Security**: Only shows actions user is authorized to perform
4. **Maintainability**: Business rule changes don't require frontend updates

## Role-Based Permissions

### Employee
- Can update progress on assigned tasks
- Can mark assigned tasks as completed
- Can view own tasks

### Manager/Admin
- Can assign tasks
- Can accept/reject tasks in review
- Can review completed tasks with rating
- Can send tasks back for rework
- Can cancel tasks
- Can update task details
- All employee permissions

## Workflow Example

### Typical Happy Path

1. **Manager creates task** → `Created` (0)
2. **Manager assigns to employee** → `Assigned` (1)
3. **Employee updates progress** → `Assigned` (1)
4. **Employee marks completed** → `PendingManagerReview` (7)
5. **Manager reviews and accepts** (rating: 5, feedback: "Excellent work!") → `Accepted` (3) ✓

### Workflow with Rework

1. **Manager creates task** → `Created` (0)
2. **Manager assigns to employee** → `Assigned` (1)
3. **Employee marks completed** → `PendingManagerReview` (7)
4. **Manager sends back for rework** (feedback: "Please add documentation") → `Assigned` (1)
5. **Employee completes with documentation** → `PendingManagerReview` (7)
6. **Manager reviews and accepts** (rating: 4, feedback: "Good!") → `Accepted` (3) ✓

### Workflow with Rejection

1. **Manager creates task** → `Created` (0)
2. **Manager assigns to employee** → `Assigned` (1)
3. **Employee marks completed** → `PendingManagerReview` (7)
4. **Manager rejects** (rating: 2, feedback: "Did not meet requirements") → `RejectedByManager` (8) ✗

## Database Schema

### Task Table Columns

```sql
-- State management
Status INT NOT NULL  -- Enum value

-- Manager review fields (added for new workflow)
ManagerRating INT NULL  -- 1-5 stars, set when manager reviews
ManagerFeedback NVARCHAR(1000) NULL  -- Optional feedback from manager

-- Existing fields...
```

## API Endpoints

### New Endpoint for Manager Review

```
POST /tasks/{id}/review-completed
Authorization: Manager, Admin
Body: {
  "accepted": boolean,
  "rating": number,  // 1-5
  "feedback": string?,  // Optional, max 1000 chars
  "sendBackForRework": boolean
}
```

**Business Rules**:
- Task must be in `PendingManagerReview` status
- Rating is required
- Cannot set both `accepted` and `sendBackForRework` to true

## Testing Scenarios

### Unit Tests
- State transition validation in Task entity
- `MarkCompletedByEmployee()` transitions to `PendingManagerReview`
- `ReviewByManager()` with different scenarios
- `TaskActionService.GetAvailableActions()` for each state and role

### Integration Tests
- Full workflow: Created → Assigned → PendingManagerReview → Accepted
- Manager sends task back for rework
- Manager rejects completed task
- Verify HATEOAS links in all endpoints
- Permission checks for each action

## Migration Guide

### For Existing Tasks

Existing tasks in `Completed` (5) status will remain in that state. The new workflow applies to tasks marked complete after the feature deployment.

### Database Migration

Run migration `AddManagerReviewWorkflow` to add:
- `ManagerRating` column (nullable int)
- `ManagerFeedback` column (nullable nvarchar(1000))
- Support for new enum values (7, 8)

### Frontend Updates

1. Update task status type to include new statuses
2. Add status badge styling for `PendingManagerReview` and `RejectedByManager`
3. Create `ReviewCompletedTaskModal` component
4. Update `TaskDetailsView` to use HATEOAS links for button visibility
5. Add i18n translations for new statuses and actions

## Future Enhancements

1. **Notifications**: Notify employee when task is sent back for rework
2. **Analytics**: Track average ratings per employee/manager
3. **Comments Thread**: Allow back-and-forth discussion during review
4. **Partial Acceptance**: Accept parts of a task while requesting rework on others
5. **Rating Categories**: Break down rating into multiple dimensions (quality, timeliness, etc.)

