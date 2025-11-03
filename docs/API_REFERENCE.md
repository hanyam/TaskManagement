# Task Management API - API Reference

**Version:** 1.0  
**Last Updated:** 2024-01-15

## Table of Contents

1. [Base URL](#base-url)
2. [Authentication](#authentication)
3. [Response Format](#response-format)
4. [Error Handling](#error-handling)
5. [Authentication Endpoints](#authentication-endpoints)
6. [Dashboard Endpoints](#dashboard-endpoints)
7. [Task Management Endpoints](#task-management-endpoints)
8. [Task Delegation Endpoints](#task-delegation-endpoints)
9. [Task Progress Endpoints](#task-progress-endpoints)
10. [Task Status Management Endpoints](#task-status-management-endpoints)
11. [Extension Request Endpoints](#extension-request-endpoints)
12. [Query Endpoints](#query-endpoints)
13. [Request/Response Schemas](#requestresponse-schemas)
14. [Status Codes](#status-codes)
15. [Authorization Requirements](#authorization-requirements)

---

## Base URL

```
Development: https://localhost:7000
Production: https://api.taskmanagement.com
```

All endpoints are prefixed with `/api`.

---

## Authentication

The API uses **JWT Bearer Token** authentication. Most endpoints require a valid JWT token in the `Authorization` header.

**Header Format:**
```
Authorization: Bearer {jwt-token}
```

**Token Claims:**
- `user_id`: Unique identifier for the user (Guid)
- `email`: User's email address
- `name`: User's display name
- `role`: User role (Employee, Manager, Admin)

**Token Expiry:**
- Default: 1 hour (configurable via `Jwt:ExpiryInHours`)

---

## Response Format

### Success Response

All successful responses follow this format:

```json
{
  "success": true,
  "data": { /* response data */ },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### Error Response

All error responses follow this format:

```json
{
  "success": false,
  "data": null,
  "message": "Error message",
  "errors": [
    {
      "code": "ERROR_CODE",
      "message": "Error message",
      "field": "FieldName"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

---

## Error Handling

### Error Codes

- `NOT_FOUND`: Resource not found
- `VALIDATION_ERROR`: Input validation failed
- `UNAUTHORIZED`: Authentication required
- `FORBIDDEN`: Insufficient permissions
- `CONFLICT`: Business rule violation
- `INTERNAL_ERROR`: Server error

### Status Codes

- `200 OK`: Success
- `201 Created`: Resource created successfully
- `400 Bad Request`: Validation error or bad request
- `401 Unauthorized`: Authentication required or invalid token
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

---

## Authentication Endpoints

### POST /api/authentication/authenticate

Authenticates a user with Azure AD token and returns a JWT token.

**Authorization:** None (public endpoint)

**Request:**
```json
{
  "azureAdToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIs..."
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "displayName": "John Doe",
      "isActive": true,
      "role": 0,
      "createdAt": "2024-01-01T00:00:00Z",
      "lastLoginAt": "2024-01-15T10:30:00Z"
    }
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**Response (400 Bad Request):**
```json
{
  "success": false,
  "data": null,
  "message": "Invalid Azure AD token",
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

---

## Dashboard Endpoints

### GET /api/dashboard/stats

Retrieves dashboard statistics for the current user.

**Authorization:** Required (Employee, Manager, Admin)

**Query Parameters:** None

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "tasksCreatedByUser": 15,
    "tasksCompleted": 8,
    "tasksNearDueDate": 3,
    "tasksDelayed": 2,
    "tasksInProgress": 5,
    "tasksUnderReview": 2,
    "tasksPendingAcceptance": 1
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**Statistics Description:**
- `tasksCreatedByUser`: Count of tasks created by the current user
- `tasksCompleted`: Count of completed tasks
- `tasksNearDueDate`: Count of tasks approaching due date (within threshold)
- `tasksDelayed`: Count of tasks past due date
- `tasksInProgress`: Count of tasks in progress (status: Assigned)
- `tasksUnderReview`: Count of tasks under review
- `tasksPendingAcceptance`: Count of tasks pending acceptance

---

## Task Management Endpoints

### GET /api/tasks/{id}

Retrieves a task by its ID.

**Authorization:** Required (Employee, Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "456e7890-e89b-12d3-a456-426614174001",
    "title": "Implement user authentication",
    "description": "Add Azure AD authentication to the API",
    "status": 1,
    "priority": 2,
    "dueDate": "2024-02-15T17:00:00Z",
    "originalDueDate": "2024-02-15T17:00:00Z",
    "extendedDueDate": null,
    "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
    "assignedUserEmail": "developer@example.com",
    "type": 2,
    "reminderLevel": 2,
    "progressPercentage": 75,
    "createdById": "789e0123-e89b-12d3-a456-426614174002",
    "createdBy": "manager@example.com",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": "2024-01-15T09:00:00Z",
    "assignments": [
      {
        "id": "111e2222-e89b-12d3-a456-426614174003",
        "taskId": "456e7890-e89b-12d3-a456-426614174001",
        "userId": "123e4567-e89b-12d3-a456-426614174000",
        "userEmail": "developer@example.com",
        "userDisplayName": "Developer User",
        "isPrimary": true,
        "assignedAt": "2024-01-01T12:00:00Z"
      }
    ],
    "recentProgressHistory": [
      {
        "id": "222e3333-e89b-12d3-a456-426614174004",
        "taskId": "456e7890-e89b-12d3-a456-426614174001",
        "updatedById": "123e4567-e89b-12d3-a456-426614174000",
        "updatedByEmail": "developer@example.com",
        "progressPercentage": 75,
        "notes": "Completed authentication setup",
        "status": 1,
        "acceptedById": "789e0123-e89b-12d3-a456-426614174002",
        "acceptedByEmail": "manager@example.com",
        "acceptedAt": "2024-01-15T09:00:00Z",
        "updatedAt": "2024-01-15T08:00:00Z"
      }
    ]
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "data": null,
  "message": "Task not found",
  "errors": [
    {
      "code": "NOT_FOUND",
      "message": "Task not found",
      "field": "Id"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### GET /api/tasks

Retrieves a list of tasks with optional filtering and pagination.

**Authorization:** Required (Employee, Manager, Admin)

**Query Parameters:**
- `status` (int?, optional): Filter by task status (0-6)
- `priority` (int?, optional): Filter by task priority (0-3)
- `assignedUserId` (Guid?, optional): Filter by assigned user ID
- `dueDateFrom` (DateTime?, optional): Filter by due date from
- `dueDateTo` (DateTime?, optional): Filter by due date to
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 10): Items per page (max: 100)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "tasks": [
      {
        "id": "456e7890-e89b-12d3-a456-426614174001",
        "title": "Implement user authentication",
        "description": "Add Azure AD authentication to the API",
        "status": 1,
        "priority": 2,
        "dueDate": "2024-02-15T17:00:00Z",
        "originalDueDate": "2024-02-15T17:00:00Z",
        "extendedDueDate": null,
        "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
        "assignedUserEmail": "developer@example.com",
        "type": 2,
        "reminderLevel": 2,
        "progressPercentage": 75,
        "createdById": "789e0123-e89b-12d3-a456-426614174002",
        "createdBy": "manager@example.com",
        "createdAt": "2024-01-01T12:00:00Z",
        "updatedAt": "2024-01-15T09:00:00Z",
        "assignments": [],
        "recentProgressHistory": []
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### POST /api/tasks

Creates a new task.

**Authorization:** Required (Employee, Manager, Admin)

**Request:**
```json
{
  "title": "Complete project documentation",
  "description": "Write comprehensive documentation for the Task Management API",
  "priority": 2,
  "dueDate": "2024-02-01T00:00:00Z",
  "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
  "type": 2
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174005",
    "title": "Complete project documentation",
    "description": "Write comprehensive documentation for the Task Management API",
    "status": 0,
    "priority": 2,
    "dueDate": "2024-02-01T00:00:00Z",
    "originalDueDate": "2024-02-01T00:00:00Z",
    "extendedDueDate": null,
    "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
    "assignedUserEmail": "developer@example.com",
    "type": 2,
    "reminderLevel": 0,
    "progressPercentage": null,
    "createdById": "999e8888-e89b-12d3-a456-426614174006",
    "createdBy": "user@example.com",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": null,
    "assignments": [],
    "recentProgressHistory": []
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**Response (400 Bad Request):**
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Title is required",
      "field": "Title"
    },
    {
      "code": "VALIDATION_ERROR",
      "message": "Due date cannot be in the past",
      "field": "DueDate"
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

---

## Task Delegation Endpoints

### POST /api/tasks/{id}/assign

Assigns a task to one or multiple users (manager only).

**Authorization:** Required (Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Request:**
```json
{
  "userIds": [
    "123e4567-e89b-12d3-a456-426614174000",
    "456e7890-e89b-12d3-a456-426614174001"
  ]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174005",
    "title": "Complete project documentation",
    "status": 1,
    "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
    "assignments": [
      {
        "id": "111e2222-e89b-12d3-a456-426614174003",
        "taskId": "789e0123-e89b-12d3-a456-426614174005",
        "userId": "123e4567-e89b-12d3-a456-426614174000",
        "userEmail": "developer1@example.com",
        "isPrimary": true,
        "assignedAt": "2024-01-15T10:30:00Z"
      },
      {
        "id": "222e3333-e89b-12d3-a456-426614174004",
        "taskId": "789e0123-e89b-12d3-a456-426614174005",
        "userId": "456e7890-e89b-12d3-a456-426614174001",
        "userEmail": "developer2@example.com",
        "isPrimary": false,
        "assignedAt": "2024-01-15T10:30:00Z"
      }
    ]
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### PUT /api/tasks/{id}/reassign

Reassigns a task to different user(s) (manager only).

**Authorization:** Required (Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Request:**
```json
{
  "newUserIds": [
    "789e0123-e89b-12d3-a456-426614174002"
  ]
}
```

**Response (200 OK):** Same as Assign Task endpoint

---

## Task Progress Endpoints

### POST /api/tasks/{id}/progress

Updates task progress (employee).

**Authorization:** Required (Employee, Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Request:**
```json
{
  "progressPercentage": 75,
  "notes": "Completed authentication setup"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "333e4444-e89b-12d3-a456-426614174007",
    "taskId": "789e0123-e89b-12d3-a456-426614174005",
    "updatedById": "123e4567-e89b-12d3-a456-426614174000",
    "updatedByEmail": "developer@example.com",
    "progressPercentage": 75,
    "notes": "Completed authentication setup",
    "status": 0,
    "acceptedById": null,
    "acceptedByEmail": null,
    "acceptedAt": null,
    "updatedAt": "2024-01-15T10:30:00Z"
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### POST /api/tasks/{id}/progress/accept

Accepts a task progress update (manager).

**Authorization:** Required (Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Request:**
```json
{
  "progressHistoryId": "333e4444-e89b-12d3-a456-426614174007"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": null,
  "message": "Progress update accepted",
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

---

## Task Status Management Endpoints

### POST /api/tasks/{id}/accept

Accepts an assigned task (employee).

**Authorization:** Required (Employee, Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174005",
    "title": "Complete project documentation",
    "status": 3,
    // ... other task properties
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### POST /api/tasks/{id}/reject

Rejects an assigned task (employee).

**Authorization:** Required (Employee, Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Request:**
```json
{
  "reason": "Insufficient information provided"
}
```

**Response (200 OK):** Same format as Accept Task

### POST /api/tasks/{id}/request-info

Requests more information on a task (employee).

**Authorization:** Required (Employee, Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Request:**
```json
{
  "requestMessage": "Need clarification on requirements"
}
```

**Response (200 OK):** Same format as Accept Task

### POST /api/tasks/{id}/complete

Marks a task as completed (manager).

**Authorization:** Required (Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174005",
    "title": "Complete project documentation",
    "status": 5,
    "progressPercentage": 100,
    // ... other task properties
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

---

## Extension Request Endpoints

### POST /api/tasks/{id}/extension-request

Requests a deadline extension (employee).

**Authorization:** Required (Employee, Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID

**Request:**
```json
{
  "requestedDueDate": "2024-02-15T00:00:00Z",
  "reason": "Additional requirements discovered, need more time"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "444e5555-e89b-12d3-a456-426614174008",
    "taskId": "789e0123-e89b-12d3-a456-426614174005",
    "taskTitle": "Complete project documentation",
    "requestedById": "123e4567-e89b-12d3-a456-426614174000",
    "requestedByEmail": "developer@example.com",
    "requestedDueDate": "2024-02-15T00:00:00Z",
    "reason": "Additional requirements discovered, need more time",
    "status": 0,
    "reviewedById": null,
    "reviewedByEmail": null,
    "reviewedAt": null,
    "reviewNotes": null,
    "createdAt": "2024-01-15T10:30:00Z"
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### POST /api/tasks/{id}/extension-request/{requestId}/approve

Approves a deadline extension request (manager).

**Authorization:** Required (Manager, Admin)

**Path Parameters:**
- `id` (Guid, required): Task ID
- `requestId` (Guid, required): Extension request ID

**Request:**
```json
{
  "reviewNotes": "Extension approved due to scope change"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": null,
  "message": "Extension request approved",
  "errors": [],
  "timestamp": "2024-01-15T10:30:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

---

## Query Endpoints

### GET /api/tasks?assignedUserId={userId}

Retrieves tasks assigned to a specific user (alternative to filtering in GetTasks).

**Authorization:** Required (Employee, Manager, Admin)

**Query Parameters:**
- `assignedUserId` (Guid, required): User ID
- `status` (int?, optional): Filter by status
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 10): Items per page

**Response (200 OK):** Same format as GetTasks

### GET /api/tasks?reminderLevel={level}

Retrieves tasks filtered by reminder level.

**Authorization:** Required (Employee, Manager, Admin)

**Query Parameters:**
- `reminderLevel` (int, required): Reminder level (0-4)
- `userId` (Guid, optional): Filter by user (tasks assigned to user)
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 10): Items per page

**Response (200 OK):** Same format as GetTasks

---

## Request/Response Schemas

### TaskDto

```json
{
  "id": "Guid",
  "title": "string",
  "description": "string?",
  "status": "int (0-6)",
  "priority": "int (0-3)",
  "dueDate": "DateTime?",
  "originalDueDate": "DateTime?",
  "extendedDueDate": "DateTime?",
  "assignedUserId": "Guid",
  "assignedUserEmail": "string?",
  "type": "int (0-3)",
  "reminderLevel": "int (0-4)",
  "progressPercentage": "int? (0-100)",
  "createdById": "Guid",
  "createdBy": "string",
  "createdAt": "DateTime",
  "updatedAt": "DateTime?",
  "assignments": "TaskAssignmentDto[]",
  "recentProgressHistory": "TaskProgressDto[]"
}
```

### TaskAssignmentDto

```json
{
  "id": "Guid",
  "taskId": "Guid",
  "userId": "Guid",
  "userEmail": "string?",
  "userDisplayName": "string?",
  "isPrimary": "bool",
  "assignedAt": "DateTime"
}
```

### TaskProgressDto

```json
{
  "id": "Guid",
  "taskId": "Guid",
  "updatedById": "Guid",
  "updatedByEmail": "string?",
  "progressPercentage": "int (0-100)",
  "notes": "string?",
  "status": "int (0-2)",
  "acceptedById": "Guid?",
  "acceptedByEmail": "string?",
  "acceptedAt": "DateTime?",
  "updatedAt": "DateTime"
}
```

### ExtensionRequestDto

```json
{
  "id": "Guid",
  "taskId": "Guid",
  "taskTitle": "string",
  "requestedById": "Guid",
  "requestedByEmail": "string?",
  "requestedDueDate": "DateTime",
  "reason": "string",
  "status": "int (0-2)",
  "reviewedById": "Guid?",
  "reviewedByEmail": "string?",
  "reviewedAt": "DateTime?",
  "reviewNotes": "string?",
  "createdAt": "DateTime"
}
```

### DashboardStatsDto

```json
{
  "tasksCreatedByUser": "int",
  "tasksCompleted": "int",
  "tasksNearDueDate": "int",
  "tasksDelayed": "int",
  "tasksInProgress": "int",
  "tasksUnderReview": "int",
  "tasksPendingAcceptance": "int"
}
```

---

## Status Codes

### Success Codes

- **200 OK**: Request successful
- **201 Created**: Resource created successfully

### Error Codes

- **400 Bad Request**: Validation error or invalid request
- **401 Unauthorized**: Authentication required or invalid token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

---

## Authorization Requirements

### Role-Based Access Control

#### Employee Role
- Create tasks
- View assigned tasks
- Update assigned task progress
- Accept/reject assigned tasks
- Request deadline extensions
- View dashboard stats

#### Manager Role
- All Employee permissions
- Assign tasks to users
- Reassign tasks
- Accept task progress updates
- Approve/reject deadline extensions
- Mark tasks as completed
- View all tasks

#### Admin Role
- All Manager permissions
- System administration (if implemented)

### Endpoint Authorization Matrix

| Endpoint | Employee | Manager | Admin |
|----------|----------|---------|-------|
| POST /api/tasks | ✅ | ✅ | ✅ |
| GET /api/tasks | ✅ | ✅ | ✅ |
| GET /api/tasks/{id} | ✅ | ✅ | ✅ |
| POST /api/tasks/{id}/assign | ❌ | ✅ | ✅ |
| PUT /api/tasks/{id}/reassign | ❌ | ✅ | ✅ |
| POST /api/tasks/{id}/progress | ✅ | ✅ | ✅ |
| POST /api/tasks/{id}/progress/accept | ❌ | ✅ | ✅ |
| POST /api/tasks/{id}/accept | ✅ | ✅ | ✅ |
| POST /api/tasks/{id}/reject | ✅ | ✅ | ✅ |
| POST /api/tasks/{id}/request-info | ✅ | ✅ | ✅ |
| POST /api/tasks/{id}/complete | ❌ | ✅ | ✅ |
| POST /api/tasks/{id}/extension-request | ✅ | ✅ | ✅ |
| POST /api/tasks/{id}/extension-request/{requestId}/approve | ❌ | ✅ | ✅ |
| GET /api/dashboard/stats | ✅ | ✅ | ✅ |

---

## See Also

- [API Examples](API_EXAMPLES.md) - Detailed request/response examples
- [Features Documentation](FEATURES.md) - Feature descriptions
- [Architecture Documentation](ARCHITECTURE.md) - System architecture
- [Domain Model](DOMAIN_MODEL.md) - Entity and relationship documentation

