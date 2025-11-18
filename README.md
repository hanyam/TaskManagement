# Task Management API

A comprehensive .NET Core API solution implementing Vertical Slice Architecture with SOLID principles, Azure AD authentication, and JWT token generation.

## 🏗️ Architecture Overview

This solution follows **Vertical Slice Architecture** combined with **Clean Architecture** principles, implementing:

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: CQRS handlers using MediatR pattern
- **Infrastructure Layer**: Data access and external service implementations
- **API Layer**: Controllers and middleware

## 🚀 Key Features

### Architecture & Design
- ✅ **Vertical Slice Architecture** - Features organized by business capability
- ✅ **SOLID Principles** - Clean, maintainable, and extensible code
- ✅ **CQRS Pattern** - Command/Query Responsibility Segregation
- ✅ **Clean Architecture** - Layered architecture with clear separation
- ✅ **Custom Mediator** - Lightweight mediator implementation
- ✅ **Repository Pattern** - EF Core for commands, Dapper for queries

### Authentication & Security
- ✅ **Azure AD Authentication** - Enterprise authentication
- ✅ **JWT Token Generation** - Customizable JWT tokens with claims
- ✅ **Role-Based Access Control** - Employee, Manager, Admin roles
- ✅ **Endpoint Authorization** - Attribute-based access control

### Task Management
- ✅ **Task Types** - Simple, WithDueDate, WithProgress, WithAcceptedProgress
- ✅ **Task Status Lifecycle** - Created → Assigned → Accepted → Completed
- ✅ **Multi-User Assignment** - Assign tasks to multiple employees
- ✅ **Progress Tracking** - Progress updates with acceptance workflow
- ✅ **Task Review** - Accept, reject, or request more information

### Advanced Features
- ✅ **Deadline Extensions** - Request and approve deadline extensions
- ✅ **Reminder System** - Automatic reminder level calculation
- ✅ **Dashboard Statistics** - Comprehensive task statistics
- ✅ **Delegation Management** - Task assignment and reassignment

### Code Quality
- ✅ **Result Pattern** - Standardized API responses
- ✅ **Global Exception Handling** - Centralized error management
- ✅ **FluentValidation** - Input validation framework
- ✅ **Structured Logging** - Serilog with multiple sinks
- ✅ **Comprehensive Testing** - Unit and integration tests

## 📁 Project Structure

```
TaskManagement/
├── src/
│   ├── TaskManagement.Api/              # API Layer
│   │   ├── Controllers/                  # API Controllers
│   │   ├── Middleware/                   # Global middleware
│   │   └── Program.cs                   # Application startup
│   ├── TaskManagement.Application/      # Application Layer
│   │   ├── Common/                      # Shared application concerns
│   │   ├── Tasks/                       # Task feature slices
│   │   └── Authentication/              # Authentication feature slices
│   ├── TaskManagement.Domain/           # Domain Layer
│   │   ├── Common/                      # Shared domain concepts
│   │   ├── Entities/                    # Domain entities
│   │   ├── DTOs/                        # Data transfer objects
│   │   └── Interfaces/                   # Domain interfaces
│   └── TaskManagement.Infrastructure/  # Infrastructure Layer
│       ├── Authentication/              # Authentication services
│       └── Data/                        # Data access layer
├── tests/
│   └── TaskManagement.Tests/           # Test project
├── Directory.Build.props               # Build configuration
├── Directory.Packages.props            # Package management
├── NuGet.Config                        # NuGet configuration
└── TaskManagement.sln                 # Solution file
```

## 🛠️ Technology Stack

- **.NET 8.0** - Latest .NET framework
- **Entity Framework Core** - Data access
- **MediatR** - Mediator pattern implementation
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **xUnit** - Testing framework
- **FluentAssertions** - Test assertions
- **Moq** - Mocking framework
- **Swagger/OpenAPI** - API documentation

## 🔧 Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Configuration

1. **Update Connection String** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

2. **Configure Azure AD** in `appsettings.json`:
```json
{
  "AzureAd": {
    "Issuer": "https://login.microsoftonline.com/your-tenant-id/v2.0",
    "ClientId": "your-azure-ad-client-id",
    "ClientSecret": "your-azure-ad-client-secret"
  }
}
```

3. **Configure JWT Settings**:
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

### Running the Application

1. **Restore packages**:
```bash
dotnet restore
```

2. **Run the application**:
```bash
dotnet run --project src/TaskManagement.Api
```

3. **Access Swagger UI**: Navigate to `https://localhost:7000` (or the configured port)

## 🔐 Authentication Flow

### Azure AD Integration

1. **User authenticates** with Azure AD
2. **Azure AD returns** access token
3. **API validates** Azure AD token
4. **API generates** custom JWT token with additional claims
5. **Client uses** JWT token for subsequent requests

### Sample Authentication Request

```http
POST /api/authentication/authenticate
Content-Type: application/json

{
  "azureAdToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIs..."
}
```

### Sample Authentication Response

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

## 📋 API Endpoints

### Authentication

- `POST /api/authentication/authenticate` - Authenticate with Azure AD token

### Dashboard

- `GET /api/dashboard/stats` - Get dashboard statistics for current user

### Tasks

- `GET /api/tasks` - Get tasks with filtering and pagination
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task

### Task Delegation

- `POST /api/tasks/{id}/assign` - Assign task to user(s) (Manager)
- `PUT /api/tasks/{id}/reassign` - Reassign task to different user(s) (Manager)

### Task Progress

- `POST /api/tasks/{id}/progress` - Update task progress (Employee)
- `POST /api/tasks/{id}/progress/accept` - Accept task progress update (Manager)

### Task Status Management

- `POST /api/tasks/{id}/accept` - Accept assigned task (Employee)
- `POST /api/tasks/{id}/reject` - Reject assigned task (Employee)
- `POST /api/tasks/{id}/request-info` - Request more information (Employee)
- `POST /api/tasks/{id}/complete` - Mark task as completed (Manager)

### Extension Requests

- `POST /api/tasks/{id}/extension-request` - Request deadline extension (Employee)
- `POST /api/tasks/{id}/extension-request/{requestId}/approve` - Approve extension request (Manager)

### Sample Task Creation

```http
POST /api/tasks
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "title": "Complete project documentation",
  "description": "Write comprehensive documentation for the Task Management API",
  "priority": 2,
  "dueDate": "2024-02-01T00:00:00Z",
  "assignedUserId": "123e4567-e89b-12d3-a456-426614174000",
  "type": 2
}
```

### Sample Task Assignment

```http
POST /api/tasks/{id}/assign
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "userIds": [
    "123e4567-e89b-12d3-a456-426614174000",
    "456e7890-e89b-12d3-a456-426614174001"
  ]
}
```

### Sample Progress Update

```http
POST /api/tasks/{id}/progress
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "progressPercentage": 75,
  "notes": "Completed authentication setup"
}
```

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/TaskManagement.Tests/
```

### Test Categories

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test API endpoints with in-memory database
- **Handler Tests**: Test CQRS command/query handlers
- **Domain Tests**: Test domain logic and business rules

## 🔄 Vertical Slice Architecture

Each feature is organized as a vertical slice containing:

```
Features/
└── Tasks/
    ├── Commands/
    │   └── CreateTask/
    │       ├── CreateTaskCommand.cs
    │       ├── CreateTaskCommandHandler.cs
    │       └── CreateTaskCommandValidator.cs
    └── Queries/
        └── GetTaskById/
            ├── GetTaskByIdQuery.cs
            └── GetTaskByIdQueryHandler.cs
```

### Benefits

- **Feature Isolation**: Each feature is self-contained
- **Easy Navigation**: Related code is grouped together
- **Independent Development**: Teams can work on different features
- **Simplified Testing**: Test files are co-located with features

## 🏛️ SOLID Principles Implementation

### Single Responsibility Principle (SRP)
- Each class has one reason to change
- Controllers handle HTTP concerns only
- Handlers process single commands/queries

### Open/Closed Principle (OCP)
- New features added without modifying existing code
- Pipeline behaviors extend functionality
- Repository pattern allows different implementations

### Liskov Substitution Principle (LSP)
- Interfaces define contracts
- Implementations are interchangeable
- Dependency injection enables substitution

### Interface Segregation Principle (ISP)
- Small, focused interfaces
- Clients depend only on needed methods
- Repository interfaces are specific to entities

### Dependency Inversion Principle (DIP)
- High-level modules don't depend on low-level modules
- Both depend on abstractions
- Dependency injection provides implementations

## 🔧 Extending the Solution

### Adding New Features

1. **Create Domain Entity**:
```csharp
public class Project : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    // ... other properties
}
```

2. **Create Commands/Queries**:
```csharp
public record CreateProjectCommand : ICommand<ProjectDto>
{
    public string Name { get; init; } = string.Empty;
}
```

3. **Implement Handlers**:
```csharp
public class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, ProjectDto>
{
    // Implementation
}
```

4. **Add Controller**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : BaseController
{
    // Endpoints
}
```

### Adding New Authentication Providers

1. **Implement Interface**:
```csharp
public interface IExternalAuthProvider
{
    Task<Result<ClaimsPrincipal>> ValidateTokenAsync(string token);
}
```

2. **Register Service**:
```csharp
services.AddScoped<IExternalAuthProvider, GoogleAuthProvider>();
```

## 📊 Performance Considerations

- **Async/Await**: All I/O operations are asynchronous
- **Connection Pooling**: Entity Framework connection pooling enabled
- **Caching**: Repository pattern allows for caching implementations
- **Pagination**: Large datasets are paginated
- **Logging**: Structured logging for monitoring and debugging

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Azure AD Integration**: Enterprise-grade authentication
- **Input Validation**: FluentValidation for request validation
- **SQL Injection Protection**: Entity Framework parameterized queries
- **CORS Configuration**: Configurable cross-origin requests
- **Health Checks**: Application health monitoring

## 📈 Monitoring and Logging

- **Serilog**: Structured logging with multiple sinks
- **Health Checks**: Application and database health monitoring
- **Request/Response Logging**: Pipeline behavior for request tracking
- **Exception Handling**: Global exception handling with standardized responses

## 🌐 Web UI

A modern Next.js front-end is available in the `web/` directory with:

- **Next.js 14 App Router** - Server-side rendering and routing
- **TypeScript** - Type-safe development
- **TanStack Query** - Server state management
- **React Hook Form + Zod** - Form validation
- **Tailwind CSS** - Modern styling
- **Internationalization** - Arabic and English with RTL support
- **Azure AD SSO** - Single sign-on integration

### Quick Start (Web UI)

```bash
cd web
npm install
npm run dev
# Navigate to http://localhost:3000
```

### Azure AD Configuration (Web UI)

See the web UI documentation for Azure AD setup:
- **[Quick Start Guide](web/AZURE_AD_QUICKSTART.md)** - 5-minute setup
- **[Detailed Setup](web/docs/AZURE_AD_SETUP.md)** - Complete configuration guide
- **[Environment Variables](web/docs/ENVIRONMENT_VARIABLES.md)** - Configuration reference

### Docker Compose

Run the entire stack (SQL Server + API + Web UI) with Docker:

```bash
docker compose build
docker compose up
# API: http://localhost:5010
# Web UI: http://localhost:3000
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Implement changes following the architecture patterns
4. Add comprehensive tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📚 Documentation

Comprehensive documentation is available in the `docs/` directory:

- **[Architecture Documentation](docs/ARCHITECTURE.md)** - System architecture and design patterns
- **[Domain Model](docs/DOMAIN_MODEL.md)** - Entity relationships and business rules
- **[API Reference](docs/API_REFERENCE.md)** - Complete API endpoint documentation
- **[Features Documentation](docs/FEATURES.md)** - Detailed feature descriptions
- **[Developer Guide](docs/DEVELOPER_GUIDE.md)** - Guide for extending the system
- **[Database Seeding Guide](docs/DATABASE_SEEDING.md)** - SQL script seeding and data initialization 🆕
- **[Testing Overrides Guide](docs/TESTING_OVERRIDES.md)** - Current user and date/time override for QA/testing 🆕
- **[EnsureUserIdAttribute Guide](docs/ENSURE_USER_ID_ATTRIBUTE.md)** - Custom authorization attribute for user ID validation 🆕
- **[HATEOAS Implementation](docs/HATEOAS.md)** - HATEOAS pattern and dynamic UI 🆕
- **[Task State Machine](docs/STATE_MACHINE.md)** - Task lifecycle and state transitions 🆕
- **[Implementation Status](docs/IMPLEMENTATION_STATUS.md)** - Feature completion tracking 🆕
- **[Technical Decisions Log](docs/TECHNICAL_DECISIONS_LOG.md)** - Architecture decision records 🆕
- **[Azure AD User Search Setup](docs/AZURE_AD_USER_SEARCH_SETUP.md)** - Azure AD integration guide 🆕
- **[Session: November 15, 2025](docs/SESSION_NOVEMBER_15_2025.md)** - Latest session summary 🆕
- **[Quick Start Resume](QUICK_START_RESUME.md)** - Quick reference to resume development 🆕
- **[Database Schema](docs/DATABASE_SCHEMA.md)** - Database structure and relationships
- **[Configuration Guide](docs/CONFIGURATION.md)** - Configuration options and setup
- **[Security Documentation](docs/SECURITY.md)** - Security and authorization guide
- **[Testing Documentation](docs/TESTING.md)** - Testing strategy and guidelines
- **[Deployment Guide](docs/DEPLOYMENT.md)** - Deployment instructions
- **[Business Rules](docs/BUSINESS_RULES.md)** - Business logic documentation
- **[Error Handling](docs/ERROR_HANDLING.md)** - Error handling guide
- **[Enterprise Maturity Assessment](docs/ENTERPRISE_MATURITY_ASSESSMENT.md)** - Enterprise readiness and gap analysis

## 🆘 Support

For questions and support:
- Create an issue in the repository
- Review the comprehensive documentation
- Check the test examples for implementation patterns
- See [API Examples](docs/API_EXAMPLES.md) for detailed request/response examples
