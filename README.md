# Task Management API

A comprehensive .NET Core API solution implementing Vertical Slice Architecture with SOLID principles, Azure AD authentication, and JWT token generation.

## 🏗️ Architecture Overview

This solution follows **Vertical Slice Architecture** combined with **Clean Architecture** principles, implementing:

- **Domain Layer**: Core business entities and interfaces
- **Application Layer**: CQRS handlers using MediatR pattern
- **Infrastructure Layer**: Data access and external service implementations
- **API Layer**: Controllers and middleware

## 🚀 Key Features

- ✅ **Vertical Slice Architecture** - Features organized by business capability
- ✅ **SOLID Principles** - Clean, maintainable, and extensible code
- ✅ **Azure AD Authentication** - Secure user authentication
- ✅ **JWT Token Generation** - Customizable JWT tokens with claims
- ✅ **Mediator Pattern** - Decoupled request/response handling
- ✅ **Result Pattern** - Standardized API responses
- ✅ **Global Exception Handling** - Centralized error management
- ✅ **Comprehensive Testing** - Unit and integration tests
- ✅ **Dependency Management** - Centralized package management

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

### Tasks

- `GET /api/tasks` - Get tasks with filtering and pagination
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task

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
  "assignedUserId": "123e4567-e89b-12d3-a456-426614174000"
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

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Implement changes following the architecture patterns
4. Add comprehensive tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For questions and support:
- Create an issue in the repository
- Review the documentation
- Check the test examples for implementation patterns
