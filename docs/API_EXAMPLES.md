# API Examples and Usage Guide

This document provides comprehensive examples of how to use the Task Management API.

## 🔐 Authentication Examples

### 1. Authenticate with Azure AD Token

**Request:**
```http
POST /api/authentication/authenticate
Content-Type: application/json

{
  "azureAdToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ik1yNS1BVWliZkJpaTdOZDFqQmViYXhib1hXMCJ9..."
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InVzZXJAZXhhbXBsZS5jb20iLCJ1c2VyX2lkIjoiMTIzZTQ1NjctZTg5Yi0xMmQzLWE0NTYtNDI2NjE0MTc0MDAwIiwibmFtZSI6InVzZXJAZXhhbXBsZS5jb20iLCJzdWIiOiJ1c2VyQGV4YW1wbGUuY29tIiwiZXhwIjoxNzA0MTI0MDAwLCJpYXQiOjE3MDQxMjA0MDAsImlzcyI6IlRhc2tNYW5hZ2VtZW50LkFwaSIsImF1ZCI6IlRhc2tNYW5hZ2VtZW50LkNsaWVudCJ9...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "displayName": "John Doe",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z",
      "lastLoginAt": "2024-01-01T12:00:00Z"
    }
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

**Response (Error):**
```json
{
  "success": false,
  "data": null,
  "message": "Invalid Azure AD token",
  "errors": [],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

## 📋 Task Management Examples

### 1. Create a New Task

**Request:**
```http
POST /api/tasks
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "title": "Implement user authentication",
  "description": "Add Azure AD authentication to the API",
  "priority": 2,
  "dueDate": "2024-02-15T17:00:00Z",
  "assignedUserId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response (Success):**
```json
{
  "success": true,
  "data": {
    "id": "456e7890-e89b-12d3-a456-426614174001",
    "title": "Implement user authentication",
    "description": "Add Azure AD authentication to the API",
    "status": 0,
    "priority": 2,
    "dueDate": "2024-02-15T17:00:00Z",
    "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
    "assignedUserEmail": "developer@example.com",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": null,
    "createdBy": "user@example.com"
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### 2. Get Task by ID

**Request:**
```http
GET /api/tasks/456e7890-e89b-12d3-a456-426614174001
Authorization: Bearer {jwt-token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "456e7890-e89b-12d3-a456-426614174001",
    "title": "Implement user authentication",
    "description": "Add Azure AD authentication to the API",
    "status": 0,
    "priority": 2,
    "dueDate": "2024-02-15T17:00:00Z",
    "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
    "assignedUserEmail": "developer@example.com",
    "createdAt": "2024-01-01T12:00:00Z",
    "updatedAt": null,
    "createdBy": "user@example.com"
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### 3. Get Tasks with Filtering and Pagination

**Request:**
```http
GET /api/tasks?status=0&priority=2&page=1&pageSize=10
Authorization: Bearer {jwt-token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "tasks": [
      {
        "id": "456e7890-e89b-12d3-a456-426614174001",
        "title": "Implement user authentication",
        "description": "Add Azure AD authentication to the API",
        "status": 0,
        "priority": 2,
        "dueDate": "2024-02-15T17:00:00Z",
        "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
        "assignedUserEmail": "developer@example.com",
        "createdAt": "2024-01-01T12:00:00Z",
        "updatedAt": null,
        "createdBy": "user@example.com"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "message": null,
  "errors": [],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### 4. Filter Tasks by Date Range

**Request:**
```http
GET /api/tasks?dueDateFrom=2024-01-01T00:00:00Z&dueDateTo=2024-12-31T23:59:59Z
Authorization: Bearer {jwt-token}
```

### 5. Filter Tasks by Assigned User

**Request:**
```http
GET /api/tasks?assignedUserId=123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer {jwt-token}
```

## 🔍 Query Parameters

### Task Status Values
- `0` - Pending
- `1` - InProgress
- `2` - Completed
- `3` - Cancelled

### Task Priority Values
- `0` - Low
- `1` - Medium
- `2` - High
- `3` - Critical

### Pagination Parameters
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 10)

### Filtering Parameters
- `status` - Filter by task status
- `priority` - Filter by task priority
- `assignedUserId` - Filter by assigned user
- `dueDateFrom` - Filter by due date from
- `dueDateTo` - Filter by due date to

## ❌ Error Response Examples

### 1. Validation Error

**Request:**
```http
POST /api/tasks
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "title": "",
  "description": "Invalid task",
  "priority": 2,
  "assignedUserId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response:**
```json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Title is required"
  ],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### 2. Not Found Error

**Request:**
```http
GET /api/tasks/00000000-0000-0000-0000-000000000000
Authorization: Bearer {jwt-token}
```

**Response:**
```json
{
  "success": false,
  "data": null,
  "message": "Task not found",
  "errors": [],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

### 3. Unauthorized Error

**Request:**
```http
GET /api/tasks
```

**Response:**
```json
{
  "success": false,
  "data": null,
  "message": "Unauthorized access",
  "errors": [],
  "timestamp": "2024-01-01T12:00:00Z",
  "traceId": "0HMQ8VQJQJQJQ"
}
```

## 🧪 Testing Examples

### 1. Unit Test Example

```csharp
[Fact]
public async Task CreateTask_WithValidData_ShouldReturnCreatedTask()
{
    // Arrange
    var command = new CreateTaskCommand
    {
        Title = "Test Task",
        Description = "Test Description",
        Priority = TaskPriority.High,
        AssignedUserId = Guid.NewGuid(),
        CreatedBy = "test@example.com"
    };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Title.Should().Be("Test Task");
}
```

### 2. Integration Test Example

```csharp
[Fact]
public async Task CreateTask_WithValidData_ShouldReturnCreatedTask()
{
    // Arrange
    var createTaskRequest = new
    {
        Title = "Test Task",
        Description = "Test Description",
        Priority = TaskPriority.High,
        AssignedUserId = Guid.NewGuid()
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/tasks", createTaskRequest);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

## 🔧 Configuration Examples

### 1. Azure AD Configuration

```json
{
  "AzureAd": {
    "Issuer": "https://login.microsoftonline.com/your-tenant-id/v2.0",
    "ClientId": "your-azure-ad-client-id",
    "ClientSecret": "your-azure-ad-client-secret"
  }
}
```

### 2. JWT Configuration

```json
{
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "TaskManagement.Api",
    "Audience": "TaskManagement.Client",
    "ExpiryInHours": 1
  }
}
```

### 3. Database Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## 📊 Health Check Examples

### 1. Application Health

**Request:**
```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "db": {
      "status": "Healthy",
      "description": "Entity Framework health check",
      "data": {},
      "duration": "00:00:00.0123456"
    }
  }
}
```

## 🚀 Deployment Examples

### 1. Docker Configuration

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TaskManagement.Api/TaskManagement.Api.csproj", "src/TaskManagement.Api/"]
RUN dotnet restore "src/TaskManagement.Api/TaskManagement.Api.csproj"
COPY . .
WORKDIR "/src/src/TaskManagement.Api"
RUN dotnet build "TaskManagement.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagement.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
```

### 2. Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection="Server=localhost;Database=TaskManagementDb;Trusted_Connection=true"

# JWT
Jwt__SecretKey="YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
Jwt__Issuer="TaskManagement.Api"
Jwt__Audience="TaskManagement.Client"

# Azure AD
AzureAd__Issuer="https://login.microsoftonline.com/your-tenant-id/v2.0"
AzureAd__ClientId="your-azure-ad-client-id"
AzureAd__ClientSecret="your-azure-ad-client-secret"
```

This comprehensive guide provides all the examples needed to understand and use the Task Management API effectively.
