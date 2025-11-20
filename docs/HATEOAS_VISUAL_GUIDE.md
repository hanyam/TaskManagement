# HATEOAS Visual Guide

## How HATEOAS Works in Task Management API

### Architecture Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CLIENT REQUEST                               │
│  GET /tasks/853cb87b-ac29-4709-8b22-25f0deea9249                    │
│  Authorization: Bearer {JWT with role claims}                        │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      TasksController                                 │
│  1. Validate JWT and extract user ID + role                         │
│  2. Call GetTaskByIdQuery handler                                   │
│  3. Call GenerateTaskLinks(taskId, userId)                          │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    TaskActionService                                 │
│  GetAvailableActions(task, userId, userRole)                        │
│                                                                      │
│  Business Logic:                                                     │
│  • Check task.Status (Created, Assigned, PendingManagerReview, etc) │
│  • Check userRole (Employee, Manager, Admin)                        │
│  • Check ownership (CreatedById, AssignedUserId)                    │
│  • Check task.Type (Simple, WithProgress, etc)                      │
│  • Generate appropriate links based on rules                        │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      API RESPONSE                                    │
│  {                                                                   │
│    "success": true,                                                  │
│    "data": {                                                         │
│      "id": "853cb87b-...",                                          │
│      "status": 0,  // Created                                       │
│      ...                                                             │
│    },                                                                │
│    "links": [                                                        │
│      { "rel": "self", "href": "/tasks/853...", "method": "GET" },  │
│      { "rel": "assign", "href": "/tasks/853.../assign", ... },     │
│      { "rel": "edit", "href": "/tasks/853...", "method": "PUT" }   │
│    ]                                                                 │
│  }                                                                   │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       FRONTEND                                       │
│  const canAssign = hasActionLink(task.links, "assign");            │
│  const canEdit = hasActionLink(task.links, "edit");                │
│                                                                      │
│  return (                                                            │
│    <>                                                                │
│      {canAssign && <Button>Assign</Button>}                         │
│      {canEdit && <Button>Edit</Button>}                             │
│    </>                                                               │
│  );                                                                  │
└─────────────────────────────────────────────────────────────────────┘
```

## Task State Transitions with HATEOAS

### Example: Manager Review Workflow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    TASK STATUS: Created (0)                          │
│  Creator: Manager with ID=8a4fb9e8-06ec-484f-8361-0171d69d29d1     │
│                                                                      │
│  HATEOAS Links:                                                      │
│  ✅ self      - GET /tasks/{id}                                     │
│  ✅ assign    - POST /tasks/{id}/assign                             │
│  ✅ edit      - PUT /tasks/{id}                                     │
│                                                                      │
│  UI Shows: [Assign] [Edit] buttons only                             │
└─────────────────────────────────────────────────────────────────────┘
                      │
                      │ Manager clicks [Assign] → POST /tasks/{id}/assign
                      │ Assigns to Employee (user_id: abc-123)
                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    TASK STATUS: Assigned (1)                         │
│  Assigned to: Employee (abc-123)                                    │
│                                                                      │
│  HATEOAS Links (for assigned employee):                             │
│  ✅ self              - GET /tasks/{id}                             │
│  ✅ accept            - POST /tasks/{id}/accept                     │
│  ✅ reject            - POST /tasks/{id}/reject                     │
│  ✅ request-more-info - POST /tasks/{id}/request-more-info          │
│                                                                      │
│  HATEOAS Links (for manager):                                       │
│  ✅ self              - GET /tasks/{id}                             │
│  ✅ reassign          - POST /tasks/{id}/reassign                   │
│  ✅ request-extension - POST /tasks/{id}/request-extension          │
│                                                                      │
│  UI Shows (Employee): [Accept] [Reject] [Request Info] buttons      │
│  UI Shows (Manager):  [Reassign] [Request Extension] buttons        │
└─────────────────────────────────────────────────────────────────────┘
                      │
                      │ Employee clicks [Accept] → POST /tasks/{id}/accept
                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    TASK STATUS: Accepted (2)                         │
│  Task Type: WithProgress (1)                                        │
│                                                                      │
│  HATEOAS Links (for assigned employee):                             │
│  ✅ self             - GET /tasks/{id}                              │
│  ✅ update-progress  - POST /tasks/{id}/update-progress             │
│  ✅ mark-completed   - POST /tasks/{id}/mark-completed              │
│                                                                      │
│  UI Shows: [Update Progress] [Mark Completed] buttons               │
└─────────────────────────────────────────────────────────────────────┘
                      │
                      │ Employee updates progress to 100%
                      │ Employee clicks [Mark Completed]
                      │ → POST /tasks/{id}/mark-completed
                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│              TASK STATUS: PendingManagerReview (7)                   │
│  ProgressPercentage: 100%                                           │
│  Waiting for manager review...                                      │
│                                                                      │
│  HATEOAS Links (for employee):                                      │
│  ✅ self - GET /tasks/{id}                                          │
│  (No action buttons - employee can only view)                       │
│                                                                      │
│  HATEOAS Links (for manager):                                       │
│  ✅ self              - GET /tasks/{id}                             │
│  ✅ review-completed  - POST /tasks/{id}/review-completed           │
│                                                                      │
│  UI Shows (Manager): [Review & Rate] button                         │
└─────────────────────────────────────────────────────────────────────┘
                      │
                      │ Manager clicks [Review & Rate]
                      │ Opens ReviewCompletedTaskModal
                      │ Manager selects: Accept, Rating=5, Feedback="Great!"
                      │ → POST /tasks/{id}/review-completed
                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    TASK STATUS: Accepted (2)                         │
│  ManagerRating: 5                                                   │
│  ManagerFeedback: "Great work! Excellent results."                  │
│                                                                      │
│  HATEOAS Links:                                                      │
│  ✅ self - GET /tasks/{id}                                          │
│  (Task is complete - no further actions)                            │
│                                                                      │
│  UI Shows: Task completed with 5⭐ rating and feedback              │
└─────────────────────────────────────────────────────────────────────┘
```

### Alternative Path: Send Back for Rework

```
┌─────────────────────────────────────────────────────────────────────┐
│              TASK STATUS: PendingManagerReview (7)                   │
└─────────────────────────────────────────────────────────────────────┘
                      │
                      │ Manager reviews and decides to send back
                      │ → POST /tasks/{id}/review-completed
                      │ { accepted: false, rating: 2, 
                      │   feedback: "Needs revision", 
                      │   sendBackForRework: true }
                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    TASK STATUS: Assigned (1)                         │
│  ManagerRating: 2                                                   │
│  ManagerFeedback: "Please revise the documentation section"         │
│                                                                      │
│  HATEOAS Links (for employee):                                      │
│  ✅ self             - GET /tasks/{id}                              │
│  ✅ update-progress  - POST /tasks/{id}/update-progress             │
│  ✅ mark-completed   - POST /tasks/{id}/mark-completed              │
│                                                                      │
│  Employee can now rework the task and resubmit                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Alternative Path: Reject and Close

```
┌─────────────────────────────────────────────────────────────────────┐
│              TASK STATUS: PendingManagerReview (7)                   │
└─────────────────────────────────────────────────────────────────────┘
                      │
                      │ Manager rejects and closes
                      │ → POST /tasks/{id}/review-completed
                      │ { accepted: false, rating: 1, 
                      │   feedback: "Requirements changed", 
                      │   sendBackForRework: false }
                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│                 TASK STATUS: RejectedByManager (8)                   │
│  ManagerRating: 1                                                   │
│  ManagerFeedback: "Requirements have changed, task no longer needed"│
│                                                                      │
│  HATEOAS Links (for manager):                                       │
│  ✅ self              - GET /tasks/{id}                             │
│  ✅ reassign          - POST /tasks/{id}/reassign                   │
│  ✅ review-completed  - POST /tasks/{id}/review-completed (to reopen)│
│                                                                      │
│  Manager can reassign or reopen if needed                           │
└─────────────────────────────────────────────────────────────────────┘
```

## Complete State Machine Diagram

```
                                  ┌──────────────────┐
                                  │   Created (0)    │
                                  │  • assign        │
                                  │  • edit          │
                                  └────────┬─────────┘
                                           │
                                    (assign) │
                                           │
                                           ▼
                                  ┌──────────────────┐
                                  │  Assigned (1)    │
                                  │  • accept        │
                                  │  • reject        │
                                  │  • request-info  │
                                  │  • reassign (M)  │
                                  └─────┬───┬────────┘
                                        │   │
                            (accept) ◄──┘   └──► (reject)
                                  │             │
                                  ▼             ▼
                         ┌──────────────┐  ┌─────────────┐
                         │ Accepted (2) │  │ Rejected (6)│
                         │• update-prog │  │• accept     │
                         │• mark-compl  │  │• reassign(M)│
                         │• reassign(M) │  └─────────────┘
                         └──────┬───────┘
                                │
                       (mark-completed) │
                                │
                                ▼
                    ┌──────────────────────────┐
                    │ PendingManagerReview (7) │
                    │ • review-completed (M)   │
                    └──────────┬───────────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         │                     │                     │
   (accept)                (reject +            (reject)
         │               sendBackForRework)          │
         ▼                     │                     ▼
┌────────────────┐             │            ┌──────────────────┐
│ Accepted (2)   │             │            │RejectedByMgr (8) │
│ ⭐ Rating      │             │            │ ⭐ Rating        │
│ 💬 Feedback    │             │            │ 💬 Feedback      │
│ (Complete)     │             │            │ • reassign (M)   │
└────────────────┘             │            │ • review-compl(M)│
                               │            └──────────────────┘
                               │
                               ▼
                      ┌──────────────────┐
                      │  Assigned (1)    │
                      │  (Sent back for  │
                      │   rework)        │
                      └──────────────────┘
```

## HATEOAS Benefits

### 1. **Dynamic UI Adaptation**
```typescript
// Before HATEOAS (hardcoded logic)
const canAccept = task.status === 1 && 
                  task.assignedUserId === currentUserId &&
                  currentUserRole === 'Employee';

// After HATEOAS (dynamic)
const canAccept = hasActionLink(task.links, "accept");
```

### 2. **Backend Controls Business Rules**
- Backend changes business rules → API returns different links
- Frontend automatically adapts → No frontend deployment needed
- Reduced frontend-backend coupling

### 3. **Clear API Contracts**
- Each endpoint documents its possible transitions via links
- Self-documenting API
- Easier integration for third-party clients

### 4. **Security by Design**
- Users only see actions they're authorized to perform
- Reduced attack surface
- Centralized permission logic

## Quick Reference: Link Relations

| Rel | Description | HTTP Method | Endpoint |
|-----|-------------|-------------|----------|
| `self` | View task details | GET | `/tasks/{id}` |
| `assign` | Assign task to user(s) | POST | `/tasks/{id}/assign` |
| `edit` | Update task details | PUT | `/tasks/{id}` |
| `delete` | Delete task | DELETE | `/tasks/{id}` |
| `accept` | Accept assigned task | POST | `/tasks/{id}/accept` |
| `reject` | Reject assigned task | POST | `/tasks/{id}/reject` |
| `request-more-info` | Request more information | POST | `/tasks/{id}/request-more-info` |
| `reassign` | Reassign to different user(s) | POST | `/tasks/{id}/reassign` |
| `update-progress` | Update task progress | POST | `/tasks/{id}/update-progress` |
| `accept-progress` | Accept progress update | POST | `/tasks/{id}/accept-progress` |
| `mark-completed` | Mark task as completed | POST | `/tasks/{id}/mark-completed` |
| `review-completed` | Review and rate completed task | POST | `/tasks/{id}/review-completed` |
| `request-extension` | Request deadline extension | POST | `/tasks/{id}/request-extension` |

---

**Last Updated**: November 14, 2025  
**Version**: 1.0



