# Task Management API - Architecture Documentation

**Version:** 1.1  
**Last Updated:** December 2025

**Recent Changes (December 2025):**
- Repository implementations moved to Infrastructure layer (Clean Architecture compliance)
- Repository interfaces moved to Domain layer (proper dependency direction)
- Eliminated circular dependencies between Application and Infrastructure layers

## Table of Contents

1. [System Overview](#system-overview)
2. [Architectural Patterns](#architectural-patterns)
3. [Layer Responsibilities](#layer-responsibilities)
4. [Design Principles](#design-principles)
5. [Technology Decisions](#technology-decisions)
6. [Data Flow](#data-flow)

---

## System Overview

### Purpose and Goals

The Task Management API is a comprehensive .NET Core backend solution designed to manage tasks, assignments, progress tracking, and delegation workflows. The system supports:

- **Task Management**: Create, assign, track, and complete tasks with multiple types and statuses
- **Delegation System**: Multi-user task assignments with review and approval workflows
- **Progress Tracking**: Detailed progress updates with acceptance workflows
- **Reminder System**: Automatic reminder level calculation based on due date proximity
- **Extension Management**: Deadline extension requests with approval workflow
- **Dashboard Analytics**: User-specific task statistics and metrics
- **Role-Based Access Control**: Employee, Manager, and Admin roles with appropriate permissions

### Technology Stack Summary

- **.NET 8.0**: Latest .NET framework providing modern language features and performance
- **Entity Framework Core**: ORM for data access and change tracking
- **Dapper**: High-performance micro-ORM for optimized read operations
- **Custom Mediator Pattern**: Lightweight mediator implementation for CQRS
- **FluentValidation**: Input validation framework
- **Serilog**: Structured logging framework
- **Azure AD**: Enterprise authentication provider
- **JWT Bearer Tokens**: Stateless authentication tokens
- **xUnit**: Testing framework
- **FluentAssertions**: Natural language assertions for tests

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                          │
│                    (Web, Mobile, Desktop)                   │
└────────────────────────────┬────────────────────────────────┘
                             │ HTTP/REST
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │ Controllers  │  │  Middleware  │  │  Base        │       │
│  │  - Tasks     │  │  - Exception │  │  Controller  │       │
│  │  - Dashboard │  │  - Auth      │  │              │       │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘       │
└─────────┼──────────────────┼──────────────────┼────────────┘
          │                  │                  │
          ▼                  ▼                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │  Commands    │  │   Queries    │  │  Handlers    │       │
│  │  - Create    │  │   - Get     │  │  - Command   │       │
│  │  - Assign    │  │   - List    │  │  - Query     │       │
│  │  - Update    │  │   - Stats   │  │              │       │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘       │
│         │                  │                  │              │
│  ┌──────▼──────────────────▼──────────────────▼──────┐   │
│  │            Custom Mediator Pipeline                  │   │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐   │   │
│  │  │ Validation │  │  Logging   │  │ Exception  │   │   │
│  │  │  Behavior  │  │  Behavior  │  │  Handling  │   │   │
│  │  └────────────┘  └────────────┘  └────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────┼────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │  Entities    │  │     DTOs     │  │  Interfaces  │       │
│  │  - Task      │  │  - TaskDto   │  │  - IRepo     │       │
│  │  - User      │  │  - UserDto   │  │  - IUnitOfWork│     │
│  │  - Assignment│  │  - ProgressDto│  │              │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘       │
│         │                  │                  │              │
│  ┌──────▼──────────────────▼──────────────────▼──────┐      │
│  │              Business Logic                         │      │
│  │  - Status transitions                              │      │
│  │  - Progress validation                             │      │
│  │  - Reminder calculation                            │      │
│  └────────────────────────────────────────────────────┘      │
└─────────┼────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Data       │  │  Auth        │  │  Repositories │       │
│  │   Access     │  │  Services    │  │  - EF Core   │       │
│  │   - EF Core  │  │  - Azure AD  │  │  - Dapper    │       │
│  │   - Dapper   │  │  - JWT       │  │  - UnitOfWork│       │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘       │
└─────────┼──────────────────┼──────────────────┼────────────┘
          │                  │                  │
          ▼                  ▼                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    External Systems                           │
│  ┌──────────────┐              ┌──────────────┐            │
│  │  SQL Server  │              │   Azure AD   │            │
│  │   Database   │              │   Identity   │            │
│  └──────────────┘              └──────────────┘            │
└─────────────────────────────────────────────────────────────┘
```

---

## Architectural Patterns

### Vertical Slice Architecture

The application follows **Vertical Slice Architecture**, organizing code by feature/use case rather than by technical layers.

**Structure:**
```
Application/
└── Tasks/
    ├── Commands/
    │   ├── CreateTask/
    │   │   ├── CreateTaskCommand.cs
    │   │   ├── CreateTaskCommandHandler.cs
    │   │   └── CreateTaskCommandValidator.cs
    │   ├── AssignTask/
    │   │   ├── AssignTaskCommand.cs
    │   │   ├── AssignTaskCommandHandler.cs
    │   │   └── AssignTaskCommandValidator.cs
    │   └── ...
    └── Queries/
        ├── GetTaskById/
        │   ├── GetTaskByIdQuery.cs
        │   └── GetTaskByIdQueryHandler.cs
        └── ...
```

**Benefits:**
- **Feature Isolation**: Each feature is self-contained and independent
- **Easy Navigation**: Related code (command, handler, validator) is co-located
- **Independent Development**: Multiple developers can work on different features simultaneously
- **Simplified Testing**: Test files can be co-located with features
- **Reduced Coupling**: Features don't depend on each other's implementation details

### Clean Architecture Layers

The system implements Clean Architecture principles with clear layer separation:

#### 1. Domain Layer (Core)
- **Entities**: Core business objects with behavior (`Task`, `User`, `TaskAssignment`, etc.)
- **DTOs**: Data transfer objects for API contracts
- **Interfaces**: Abstractions for infrastructure concerns (`IRepository`, `IUnitOfWork`, `IQueryRepository`, `ICommandRepository`, `IAuthenticationService`)
- **Errors**: Centralized error definitions
- **Options**: Configuration classes for business logic

**Dependencies:** None (innermost layer)

#### 2. Application Layer
- **Commands**: Write operations (Create, Update, Delete, Assign, etc.)
- **Queries**: Read operations (Get, List, Search, etc.)
- **Handlers**: Command/Query handlers implementing business logic
- **Validators**: FluentValidation validators for input validation
- **Behaviors**: Pipeline behaviors (Validation, Logging, Exception Handling)
- **Services**: Application-level services (e.g., `ReminderCalculationService`)

**Dependencies:** Domain layer only

#### 3. Infrastructure Layer
- **Data Access**: Entity Framework DbContext, Repository implementations, Unit of Work
- **Repository Implementations**: 
  - Dapper repositories (`TaskDapperRepository`, `UserDapperRepository`) for queries
  - EF Core repositories (`TaskEfCommandRepository`, `UserEfCommandRepository`) for commands
  - Base repository classes (`DapperQueryRepository<T>`, `EfCommandRepository<T>`)
- **Authentication**: Azure AD integration, JWT token generation
- **External Services**: Third-party integrations

**Dependencies:** Domain layer (for interfaces) and Application layer (for DbContext usage)

#### 4. API Layer
- **Controllers**: HTTP endpoints and routing
- **Middleware**: Global exception handling, CORS, authentication
- **Configuration**: Service registration, dependency injection

**Dependencies:** All other layers

### CQRS Pattern Implementation

The application implements **Command Query Responsibility Segregation (CQRS)**:

- **Commands** (`ICommand<TResponse>`, `ICommand`): Mutate state, return `Result<TResponse>` or `Result`
- **Queries** (`IQuery<TResponse>`): Read data, return `Result<TResponse>`
- **Separate Handlers**: `ICommandHandler<TCommand, TResponse>` and `IRequestHandler<TQuery, TResponse>`
- **Different Data Access**: Commands use EF Core (change tracking), Queries use Dapper (performance)

**Example:**

```csharp
// Command - Write operation
public record CreateTaskCommand : ICommand<TaskDto>
{
    public string Title { get; init; }
    // ...
}

public class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, TaskDto>
{
    private readonly TaskManagementDbContext _context;
    // Uses EF Core for change tracking
}

// Query - Read operation
public record GetTaskByIdQuery : IQuery<TaskDto>
{
    public Guid Id { get; init; }
}

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly TaskDapperRepository _repository;
    // Uses Dapper for optimized reads
}
```

### Mediator Pattern Usage

A **custom mediator implementation** is used instead of MediatR for:

- **Lightweight**: Minimal dependencies and overhead
- **Flexibility**: Full control over pipeline behaviors
- **Customization**: Ability to add custom behaviors easily
- **Performance**: Direct service locator resolution, no reflection overhead

**Mediator Types:**
- `ICommandMediator`: Handles commands (mutations)
- `IRequestMediator`: Handles queries (reads)
- `IPipelineBehavior<TRequest, TResponse>`: Cross-cutting concerns

**Pipeline Execution:**
```
Request → ValidationBehavior → LoggingBehavior → ExceptionHandlingBehavior → Handler → Response
```

### Repository Pattern

The application uses a **hybrid repository pattern** with Clean Architecture compliance:

**Interface Location**: `TaskManagement.Domain/Interfaces/`
- `IQueryRepository<T>`: Generic interface for Dapper-based query repositories
- `ICommandRepository<T>`: Generic interface for EF Core-based command repositories
- `ITaskEfCommandRepository`, `IUserEfCommandRepository`: Specialized command repository interfaces

**Implementation Location**: `TaskManagement.Infrastructure/Data/Repositories/`
- **Dapper Repositories** (for queries):
  - `TaskDapperRepository`: Dapper-based for task queries (performance, raw SQL)
  - `UserDapperRepository`: Dapper-based for user queries
  - `DapperQueryRepository<T>`: Base class for Dapper repositories
- **EF Core Repositories** (for commands):
  - `TaskEfCommandRepository`: EF Core-based for task commands (change tracking)
  - `UserEfCommandRepository`: EF Core-based for user commands
  - `EfCommandRepository<T>`: Base class for EF Core repositories

**Why Hybrid?**
- **EF Core**: Best for writes (change tracking, transactions, relationships)
- **Dapper**: Best for reads (raw SQL, performance, complex queries)

**Registration**: Repositories registered in `Infrastructure/DependencyInjection.cs`

**Unit of Work**: `IUnitOfWork` manages transactions across multiple repository operations

---

## Layer Responsibilities

### Domain Layer

**Responsibilities:**
- Define core business entities with behavior
- Enforce business rules through entity methods
- Define domain interfaces (abstractions)
- Centralize error definitions
- Provide DTOs for data transfer

**Key Components:**
- `Entities/`: Task, User, TaskAssignment, TaskProgressHistory, DeadlineExtensionRequest
- `DTOs/`: TaskDto, UserDto, TaskProgressDto, DashboardStatsDto, etc.
- `Interfaces/`: IRepository, IUnitOfWork, IQueryRepository, ICommandRepository, ITaskEfCommandRepository, IUserEfCommandRepository, IAuthenticationService
- `Errors/`: TaskErrors, UserErrors, AuthenticationErrors, SystemErrors
- `Options/`: ReminderOptions, ExtensionPolicyOptions, JwtOptions, AzureAdOptions

### Application Layer

**Responsibilities:**
- Implement use cases (commands and queries)
- Orchestrate domain entities and infrastructure services
- Validate input through FluentValidation
- Handle cross-cutting concerns via pipeline behaviors
- Provide application-level services

**Key Components:**
- `Commands/`: CreateTask, AssignTask, UpdateTaskProgress, etc.
- `Queries/`: GetTaskById, GetTasks, GetDashboardStats, etc.
- `Common/Behaviors/`: ValidationBehavior, LoggingBehavior, ExceptionHandlingBehavior
- `Common/Services/`: ReminderCalculationService
- `Common/Interfaces/`: ICommandHandler, IRequestHandler, IMediator, IPipelineBehavior

### Infrastructure Layer

**Responsibilities:**
- Implement data access (EF Core, Dapper)
- Implement external service integrations (Azure AD)
- Provide concrete implementations of domain interfaces
- Manage database context and transactions

**Key Components:**
- `Data/`: TaskManagementDbContext, Repository implementations, UnitOfWork
- `Data/Repositories/`: 
  - Dapper repositories: `TaskDapperRepository`, `UserDapperRepository`, `DapperQueryRepository<T>`
  - EF Core repositories: `TaskEfCommandRepository`, `UserEfCommandRepository`, `EfCommandRepository<T>`
- `Authentication/`: AuthenticationService (Azure AD + JWT)

### API Layer

**Responsibilities:**
- Expose HTTP endpoints
- Handle HTTP concerns (routing, status codes, serialization)
- Extract user context from JWT claims
- Apply authorization attributes
- Handle global exceptions

**Key Components:**
- `Controllers/`: TasksController, DashboardController, AuthenticationController
- `Middleware/`: ExceptionHandlingMiddleware
- `Program.cs`: Service registration and configuration

---

## Design Principles

### SOLID Principles Implementation

#### Single Responsibility Principle (SRP)
- **Controllers**: Handle HTTP concerns only (routing, request/response mapping)
- **Handlers**: Process a single command or query
- **Validators**: Validate a single request type
- **Entities**: Encapsulate business logic for a single domain concept

#### Open/Closed Principle (OCP)
- **Pipeline Behaviors**: Add cross-cutting concerns without modifying handlers
- **Repository Pattern**: Add new repository implementations without changing clients
- **Validation**: Add new validators without modifying handlers
- **Command/Query Pattern**: Add new features without modifying existing code

#### Liskov Substitution Principle (LSP)
- **Interfaces**: All implementations can be substituted via dependency injection
- **Repository**: EF Core and Dapper repositories are interchangeable
- **Mediator**: Custom mediators can be swapped with different implementations

#### Interface Segregation Principle (ISP)
- **Focused Interfaces**: `ICommandHandler<TCommand, TResponse>`, `IRequestHandler<TQuery, TResponse>`
- **Domain Interfaces**: `IRepository<T>`, `IUnitOfWork`, `IAuthenticationService`
- **Small Interfaces**: Clients only depend on methods they use

#### Dependency Inversion Principle (DIP)
- **Dependency Injection**: All dependencies injected via constructors
- **Abstractions**: Application layer depends on domain interfaces, not infrastructure implementations
- **Configuration**: Options pattern for configuration injection

### Dependency Injection Strategy

**Service Registration** (`Program.cs`):
- **Scoped**: Repositories, UnitOfWork, Handlers, Services (per-request lifecycle)
- **Transient**: Pipeline Behaviors (stateless, can be instantiated per use)
- **Singleton**: Options (configuration), ServiceLocator

**Constructor Injection**: All dependencies provided via constructors

**Service Locator Pattern**: Used by mediator for dynamic handler resolution

### Error Handling Patterns

#### Result Pattern
All operations return `Result<T>` or `Result`:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
    public List<Error> Errors { get; }
}
```

**Benefits:**
- No exceptions for business logic errors
- Explicit error handling
- Multiple errors can be returned (validation)
- Type-safe error handling

#### Centralized Error Definitions
Errors defined in static classes:
- `TaskErrors`: Task-related errors
- `UserErrors`: User-related errors
- `AuthenticationErrors`: Authentication errors
- `SystemErrors`: System-level errors

#### Global Exception Handling
- `ExceptionHandlingMiddleware`: Catches unhandled exceptions
- `ExceptionHandlingBehavior`: Catches exceptions in handlers
- Returns standardized `ApiResponse` format

### Result Pattern Usage

**Success Response:**
```csharp
Result<TaskDto>.Success(taskDto)
```

**Single Error:**
```csharp
Result<TaskDto>.Failure(TaskErrors.NotFound)
```

**Multiple Errors:**
```csharp
Result<TaskDto>.Failure(new List<Error> { 
    TaskErrors.TitleRequired, 
    TaskErrors.DueDateInPast 
})
```

**Controller Usage:**
```csharp
var result = await _commandMediator.Send(command);
return HandleResult(result); // Returns appropriate HTTP status code
```

---

## Technology Decisions

### Why Entity Framework Core?

**Chosen For:**
- **Change Tracking**: Automatic tracking of entity modifications
- **Relationship Management**: Navigation properties and eager loading
- **Migrations**: Database schema versioning and migration scripts
- **LINQ Support**: Type-safe querying
- **Transaction Support**: Built-in transaction management

**Used In:**
- Command handlers (writes)
- Complex entity relationships
- Database migrations

### Why Dapper for Queries?

**Chosen For:**
- **Performance**: Faster than EF Core for read operations
- **Raw SQL**: Direct SQL control for complex queries
- **Simplicity**: Minimal overhead, simple mapping
- **Flexibility**: Custom SQL for optimized queries

**Used In:**
- Query handlers (reads)
- Dashboard statistics
- Complex filtering and aggregation

### Why Custom Mediator vs MediatR?

**Reasons:**
- **Lightweight**: No external dependencies
- **Control**: Full control over pipeline execution
- **Simplicity**: Easier to understand and modify
- **Performance**: Direct service resolution, minimal reflection

**Trade-offs:**
- More code to maintain
- No built-in decorator pattern support (handled via behaviors)

### Why Vertical Slice Architecture?

**Reasons:**
- **Feature Cohesion**: Related code grouped together
- **Reduced Coupling**: Features independent of each other
- **Easier Navigation**: Find all code for a feature in one place
- **Team Scalability**: Multiple teams can work on different features
- **Testing**: Tests co-located with features

**Alternative Considered:**
- Traditional Layered Architecture (rejected due to high coupling and harder navigation)

---

## Data Flow

### Command Flow (Write Operation)

```
1. HTTP Request → TasksController.CreateTask()
2. Extract userId from JWT claims
3. Map request to CreateTaskCommand
4. Send command via ICommandMediator
5. Pipeline Behaviors:
   - ValidationBehavior: Validate command
   - LoggingBehavior: Log request
   - ExceptionHandlingBehavior: Catch exceptions
6. CreateTaskCommandHandler:
   - Validate user exists
   - Create Task entity
   - Save via EF Core repository
   - Return TaskDto
7. Controller maps Result<TaskDto> to ApiResponse
8. HTTP Response (201 Created or 400 Bad Request)
```

### Query Flow (Read Operation)

```
1. HTTP Request → TasksController.GetTaskById()
2. Map to GetTaskByIdQuery
3. Send query via IRequestMediator
4. Pipeline Behaviors:
   - ValidationBehavior: Validate query
   - LoggingBehavior: Log request
   - ExceptionHandlingBehavior: Catch exceptions
5. GetTaskByIdQueryHandler:
   - Query via Dapper repository (from Infrastructure layer)
   - Map to TaskDto
   - Return TaskDto
6. Controller maps Result<TaskDto> to ApiResponse
7. HTTP Response (200 OK or 404 Not Found)
```

### Authentication Flow

```
1. HTTP Request → AuthenticationController.Authenticate()
2. Validate Azure AD token
3. Extract user claims from Azure AD
4. Find or create user in database
5. Generate JWT token with custom claims
6. Return AuthenticationResponse with JWT token
7. Client stores JWT token
8. Subsequent requests include JWT token in Authorization header
9. JWT middleware validates token and extracts claims
```

---

## See Also

- [Domain Model Documentation](DOMAIN_MODEL.md) - Detailed entity and relationship documentation
- [API Reference](API_REFERENCE.md) - Complete API endpoint documentation
- [Developer Guide](DEVELOPER_GUIDE.md) - Guide for extending the system
- [Configuration Guide](CONFIGURATION.md) - Configuration options and setup

