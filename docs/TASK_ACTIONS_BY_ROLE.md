# Task Actions by Role

**Version:** 1.0  
**Last Updated:** December 2025

## Overview

This document provides a comprehensive list of all actions that can be performed on tasks, organized by user role (Employee vs Manager/Admin) and task status.

---

## Employee Actions

### General Actions (Available to Assigned Employees)

#### 1. **Accept Task**
- **Endpoint:** `POST /api/tasks/{id}/accept`
- **Authorization:** `EmployeeOrManager`
- **Available When:**
  - Task status: `Created` (only for progress-tracked tasks with due date not passed)
  - Task status: `Assigned` (only if user is NOT an employee - managers/admins/creators)
  - **Note:** Regular employees do NOT see accept/reject links in `Assigned` status
- **Description:** Employee accepts an assigned task, changing status from `Assigned` to `Accepted`
- **Result:** Task status → `Accepted`

#### 2. **Reject Task**
- **Endpoint:** `POST /api/tasks/{id}/reject`
- **Authorization:** `EmployeeOrManager`
- **Available When:**
  - Task status: `Created` (only for progress-tracked tasks with due date not passed)
  - Task status: `Assigned` (only if user is NOT an employee - managers/admins/creators)
  - **Note:** Regular employees do NOT see accept/reject links in `Assigned` status
- **Description:** Employee rejects an assigned task with optional reason
- **Result:** Task status → `Rejected`

#### 3. **Update Progress**
- **Endpoint:** `POST /api/tasks/{id}/progress`
- **Authorization:** `EmployeeOrManager`
- **Available When:**
  - Task status: `Assigned` or `Accepted` (if assigned to user)
  - Task type: `WithProgress` or `WithAcceptedProgress`
  - Task NOT in "Accepted by Manager" state (no ManagerRating)
- **Description:** Updates task progress percentage (0-100%)
- **Business Rules:**
  - Progress must be ≥ last approved progress (cannot decrease)
  - For `WithProgress` type: Automatically accepted
  - For `WithAcceptedProgress` type: Requires manager approval (status → `UnderReview`)
- **Result:** 
  - `WithProgress`: Progress immediately accepted
  - `WithAcceptedProgress`: Progress pending approval (status → `UnderReview`)

#### 4. **Mark Task as Completed**
- **Endpoint:** `POST /api/tasks/{id}/complete`
- **Authorization:** `Authorize` (any authenticated user)
- **Available When:**
  - Task status: `Assigned` or `Accepted` (if assigned to user)
  - Task NOT in "Accepted by Manager" state (no ManagerRating)
- **Description:** Employee marks task as completed with optional comment
- **Result:** Task status → `PendingManagerReview`

#### 5. **Request More Information**
- **Endpoint:** `POST /api/tasks/{id}/request-info`
- **Authorization:** `EmployeeOrManager`
- **Available When:**
  - Task status: `Assigned` or `Accepted` (if assigned to user)
- **Description:** Requests additional information or clarification about the task
- **Result:** Creates information request (no status change)

#### 6. **Request Deadline Extension**
- **Endpoint:** `POST /api/tasks/{id}/extension-request`
- **Authorization:** `EmployeeOrManager`
- **Available When:**
  - Task status: `Assigned` or `Accepted` (if assigned to user)
  - Task has a due date
- **Description:** Requests extension to task's due date with reason
- **Business Rules:**
  - Requested due date must be in the future
  - Requested due date must be later than current due date
- **Result:** Creates extension request (requires manager approval)

#### 7. **Upload Attachments**
- **Endpoint:** `POST /api/tasks/{id}/attachments`
- **Authorization:** Based on task status and user role
- **Available When:**
  - Task status: `Assigned` or `Accepted` (employee accepted, no ManagerRating)
  - Task status: `UnderReview`
  - User is assigned to the task
- **Description:** Upload files related to the task
- **Business Rules:**
  - Employees can upload in `Assigned`, `Accepted` (employee accepted), or `UnderReview` status
  - Cannot upload in `Created`, `PendingManagerReview`, `Completed`, `Cancelled`, or "Accepted by Manager" state

#### 8. **Delete Attachments**
- **Endpoint:** `DELETE /api/tasks/{id}/attachments/{attachmentId}`
- **Authorization:** Based on attachment ownership and task status
- **Available When:**
  - Task status: `Assigned` or `Accepted` (employee accepted, no ManagerRating)
  - Task status: `UnderReview`
  - User owns the attachment (uploaded it)
- **Description:** Delete files uploaded by the employee
- **Business Rules:**
  - Employees can only delete their own uploaded attachments
  - Can delete in `Assigned`, `Accepted` (employee accepted), or `UnderReview` status

---

## Manager/Admin Actions

### Task Management Actions

#### 1. **Create Task**
- **Endpoint:** `POST /api/tasks`
- **Authorization:** `ManagerOrAdmin`
- **Available When:** Always (if user is Manager or Admin)
- **Description:** Creates a new task with title, description, priority, due date, type, and optional assignment
- **Result:** Task created with status `Created`

#### 2. **Update Task**
- **Endpoint:** `PUT /api/tasks/{id}`
- **Authorization:** `ManagerOrAdmin`
- **Available When:**
  - Task status: `Created`, `Assigned`, `Accepted`, or `Rejected`
  - User is task creator or Manager/Admin
- **Description:** Updates task details (title, description, priority, due date, assigned user)
- **Business Rules:**
  - Task type cannot be changed after creation
- **Result:** Task updated (status unchanged)

#### 3. **Assign Task**
- **Endpoint:** `POST /api/tasks/{id}/assign`
- **Authorization:** `Manager`
- **Available When:**
  - Task status: `Created`
- **Description:** Assigns task to one or multiple users
- **Result:** Task status → `Assigned`

#### 4. **Reassign Task**
- **Endpoint:** `PUT /api/tasks/{id}/reassign`
- **Authorization:** `Manager`
- **Available When:**
  - Task status: `Rejected`
- **Description:** Reassigns task to different user(s) (send back for rework)
- **Result:** Task status → `Assigned`

#### 5. **Cancel Task**
- **Endpoint:** `POST /api/tasks/{id}/cancel`
- **Authorization:** `EmployeeOrManager` (but only creators/managers can actually cancel)
- **Available When:**
  - Task status: `Created`, `Assigned`, `UnderReview`, `Accepted`, or `Rejected`
  - User is task creator or Manager/Admin
  - Task NOT in terminal state (Completed, RejectedByManager, Cancelled)
  - Task NOT accepted by manager (no ManagerRating)
- **Description:** Cancels a task (soft delete or status change)
- **Result:** Task status → `Cancelled` (or deleted if in `Created` status)

### Progress Management Actions

#### 6. **Accept Progress Update**
- **Endpoint:** `POST /api/tasks/{id}/progress/accept`
- **Authorization:** `Manager` (but only task creator can actually accept)
- **Available When:**
  - Task status: `UnderReview`
  - Task type: `WithAcceptedProgress`
  - User is task creator (not just any manager)
  - There is a pending progress update
- **Description:** Accepts a pending progress update
- **Business Rules:**
  - Only task creator can accept/reject progress (not any manager)
  - Updates progress percentage to the pending value
  - Task status remains `Accepted` (does not change overall task status)
- **Result:** Progress history entry → `Accepted`, Task status → `Accepted`

#### 7. **Reject Progress Update**
- **Endpoint:** `POST /api/tasks/{id}/progress/reject`
- **Authorization:** `Manager` (but only task creator can actually reject)
- **Available When:**
  - Task status: `UnderReview`
  - Task type: `WithAcceptedProgress`
  - User is task creator (not just any manager)
  - There is a pending progress update
- **Description:** Rejects a pending progress update
- **Business Rules:**
  - Only task creator can accept/reject progress (not any manager)
  - Reverts progress percentage to last approved progress
  - Task status returns to `Accepted` (so employee can update again)
- **Result:** Progress history entry → `Rejected`, Task status → `Accepted`, Progress percentage reverted

### Task Review Actions

#### 8. **Accept Task** (Non-Progress Tasks)
- **Endpoint:** `POST /api/tasks/{id}/accept`
- **Authorization:** `EmployeeOrManager`
- **Available When:**
  - Task status: `UnderReview`
  - Task type: NOT `WithAcceptedProgress` (for progress tasks, use accept-progress instead)
- **Description:** Accepts a task that is under review (non-progress tasks)
- **Result:** Task status → `Accepted`

#### 9. **Reject Task** (Non-Progress Tasks)
- **Endpoint:** `POST /api/tasks/{id}/reject`
- **Authorization:** `EmployeeOrManager`
- **Available When:**
  - Task status: `UnderReview`
  - Task type: NOT `WithAcceptedProgress` (for progress tasks, use reject-progress instead)
- **Description:** Rejects a task that is under review
- **Result:** Task status → `Rejected`

#### 10. **Review Completed Task**
- **Endpoint:** `POST /api/tasks/{id}/review-completed`
- **Authorization:** `ManagerOrAdmin`
- **Available When:**
  - Task status: `PendingManagerReview`
- **Description:** Reviews a completed task with rating (1-5 stars) and optional feedback
- **Options:**
  - **Accept:** Task status → `Accepted` (with ManagerRating set - terminal state)
  - **Reject:** Task status → `RejectedByManager` (terminal state)
  - **Send Back for Rework:** Task status → `Assigned` (employee can continue working)
- **Business Rules:**
  - Rating (1-5) is required if accepting
  - Feedback is optional (max 1000 characters)
- **Result:** 
  - Accept: Task status → `Accepted` (with ManagerRating - terminal)
  - Reject: Task status → `RejectedByManager` (terminal)
  - Send Back: Task status → `Assigned`

### Extension Management Actions

#### 11. **Approve Extension Request**
- **Endpoint:** `POST /api/tasks/{id}/extension-request/{requestId}/approve`
- **Authorization:** `Manager`
- **Available When:**
  - Extension request exists and is pending
- **Description:** Approves a deadline extension request with optional review notes
- **Result:** Extension request → `Approved`, Task extended due date updated

### Attachment Management Actions

#### 12. **Upload Attachments**
- **Endpoint:** `POST /api/tasks/{id}/attachments`
- **Authorization:** Based on task status and user role
- **Available When:**
  - Task status: `Created` or `Assigned`
  - User is task creator or Manager/Admin
- **Description:** Upload files related to the task (managers can upload during creation/assignment)
- **Business Rules:**
  - Managers can upload in `Created` or `Assigned` status
  - Cannot upload in other statuses

#### 13. **Delete Attachments**
- **Endpoint:** `DELETE /api/tasks/{id}/attachments/{attachmentId}`
- **Authorization:** Based on attachment ownership and task status
- **Available When:**
  - Task status: `Created` or `Assigned`
  - User is task creator or Manager/Admin
  - User owns the attachment OR is manager/admin
- **Description:** Delete files uploaded by manager
- **Business Rules:**
  - Managers can delete their own uploaded attachments
  - Can delete in `Created` or `Assigned` status

---

## Actions by Task Status

### Created (0)
**Employee Actions:**
- None (task not yet assigned)

**Manager Actions:**
- ✅ Assign task
- ✅ Update task
- ✅ Cancel task
- ✅ Upload attachments

### Assigned (1)
**Employee Actions (if assigned to user):**
- ✅ Update progress (if task type supports it)
- ✅ Mark as completed
- ✅ Request more information
- ✅ Request deadline extension
- ✅ Upload attachments
- ✅ Delete own attachments

**Manager Actions:**
- ✅ Update task
- ✅ Cancel task
- ✅ Upload attachments
- ✅ Delete own attachments

### UnderReview (2)
**Employee Actions:**
- ✅ Upload attachments (if assigned to user)
- ✅ Delete own attachments (if assigned to user)

**Manager Actions:**
- ✅ Accept progress (if task creator and `WithAcceptedProgress` type)
- ✅ Reject progress (if task creator and `WithAcceptedProgress` type)
- ✅ Accept task (if NOT `WithAcceptedProgress` type)
- ✅ Reject task (if NOT `WithAcceptedProgress` type)
- ✅ Cancel task

### Accepted (3)
**Employee Actions (if assigned to user and NOT accepted by manager):**
- ✅ Update progress (if task type supports it)
- ✅ Mark as completed
- ✅ Request more information
- ✅ Request deadline extension
- ✅ Upload attachments
- ✅ Delete own attachments

**Manager Actions:**
- ✅ Cancel task (if NOT accepted by manager)

**Note:** If task has `ManagerRating` set (accepted by manager), it's a terminal state - no actions available.

### Rejected (4)
**Employee Actions:**
- None

**Manager Actions:**
- ✅ Reassign task
- ✅ Update task
- ✅ Cancel task

### PendingManagerReview (7)
**Employee Actions:**
- None (waiting for manager review)

**Manager Actions:**
- ✅ Review completed task (with rating and feedback)
  - Accept with rating
  - Reject with rating
  - Send back for rework

### Terminal States
**Completed (5), RejectedByManager (8), Cancelled (6)**
- **Employee Actions:** None
- **Manager Actions:** None
- Only "self" link available (view task details)

---

## Special Notes

### Progress Acceptance/Rejection
- **Important:** Accepting/rejecting progress does NOT change the overall task status
- Progress acceptance only updates the progress percentage
- Progress rejection reverts to last approved progress percentage
- Task status remains `Accepted` after progress acceptance/rejection
- Only the **task creator** can accept/reject progress (not any manager)

### Task Acceptance vs Progress Acceptance
- **Accept Task:** Changes overall task status (e.g., `UnderReview` → `Accepted`)
- **Accept Progress:** Only updates progress percentage, task status remains `Accepted`
- These are different actions for different purposes

### Employee Accept/Reject in Assigned Status
- **Regular employees** do NOT see accept/reject links in `Assigned` status
- Only managers/admins/creators who are assigned can see accept/reject in `Assigned` status
- This prevents employees from accepting/rejecting tasks they're assigned to

### Accepted by Manager State
- When task status is `Accepted` AND `ManagerRating` is set, it's a terminal state
- No further actions available (task successfully completed and reviewed)
- This is different from regular `Accepted` status (employee accepted, no rating)

---

## Summary Table

| Action | Employee | Manager/Admin | Notes |
|--------|----------|---------------|-------|
| **Create Task** | ❌ | ✅ | Manager/Admin only |
| **Update Task** | ❌ | ✅ | Creator or Manager/Admin |
| **Assign Task** | ❌ | ✅ | Manager only, status `Created` |
| **Reassign Task** | ❌ | ✅ | Manager only, status `Rejected` |
| **Cancel Task** | ❌ | ✅ | Creator or Manager/Admin |
| **Accept Task** | ⚠️ | ✅ | Limited conditions for employees |
| **Reject Task** | ⚠️ | ✅ | Limited conditions for employees |
| **Update Progress** | ✅ | ✅ | If assigned, task type supports it |
| **Accept Progress** | ❌ | ✅ | Only task creator, `WithAcceptedProgress` type |
| **Reject Progress** | ❌ | ✅ | Only task creator, `WithAcceptedProgress` type |
| **Mark Completed** | ✅ | ❌ | If assigned, status `Assigned` or `Accepted` |
| **Review Completed** | ❌ | ✅ | Manager/Admin only, status `PendingManagerReview` |
| **Request More Info** | ✅ | ✅ | If assigned |
| **Request Extension** | ✅ | ✅ | If assigned, task has due date |
| **Approve Extension** | ❌ | ✅ | Manager only |
| **Upload Attachments** | ✅ | ✅ | Based on status and ownership |
| **Delete Attachments** | ✅ | ✅ | Own attachments only, based on status |

**Legend:**
- ✅ = Available
- ❌ = Not available
- ⚠️ = Available under limited conditions

---

## See Also

- [State Machine Documentation](STATE_MACHINE.md) - Complete task state transitions
- [HATEOAS Implementation](HATEOAS.md) - How actions are dynamically determined
- [Business Rules](BUSINESS_RULES.md) - Detailed business logic and constraints




