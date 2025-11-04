# Extending the Task Management API

This guide explains how to extend the Task Management API with new features while maintaining the Vertical Slice Architecture and SOLID principles.

## 🏗️ Adding New Features

### 1. Create Domain Entity

First, create your domain entity in the Domain layer:

```csharp
// src/TaskManagement.Domain/Entities/Project.cs
namespace TaskManagement.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Guid OwnerId { get; private set; }
    public User? Owner { get; private set; }

    private Project() { }

    public Project(string name, string? description, DateTime? startDate, DateTime? endDate, Guid ownerId)
    {
        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        OwnerId = ownerId;
        Status = ProjectStatus.Planning;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        
        Name = name;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void Start()
    {
        if (Status != ProjectStatus.Planning)
            throw new InvalidOperationException("Only projects in planning status can be started");
        
        Status = ProjectStatus.InProgress;
    }

    public void Complete()
    {
        if (Status == ProjectStatus.Completed)
            throw new InvalidOperationException("Project is already completed");
        
        Status = ProjectStatus.Completed;
        EndDate = DateTime.UtcNow;
    }
}

public enum ProjectStatus
{
    Planning = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
```

### 2. Create DTOs

Create data transfer objects for your feature:

```csharp
// src/TaskManagement.Domain/DTOs/ProjectDto.cs
namespace TaskManagement.Domain.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid OwnerId { get; set; }
    public string? OwnerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid OwnerId { get; set; }
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid OwnerId { get; set; }
}

public class ProjectFilterRequest
{
    public ProjectStatus? Status { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

### 3. Create Commands and Queries

Implement the CQRS pattern for your feature:

```csharp
// src/TaskManagement.Application/Projects/Commands/CreateProject/CreateProjectCommand.cs
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Projects.Commands.CreateProject;

public record CreateProjectCommand : ICommand<ProjectDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public Guid OwnerId { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}
```

```csharp
// src/TaskManagement.Application/Projects/Commands/CreateProject/CreateProjectCommandHandler.cs
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // Validate that the owner exists
        var owner = await _unitOfWork.Users.GetByIdAsync(request.OwnerId, cancellationToken);
        if (owner == null)
        {
            return Result<ProjectDto>.Failure("Project owner not found");
        }

        // Create the project
        var project = new Project(
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.OwnerId);

        project.SetCreatedBy(request.CreatedBy);

        // Add to repository
        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return the created project as DTO
        return Result<ProjectDto>.Success(new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            OwnerId = project.OwnerId,
            OwnerEmail = owner.Email,
            CreatedAt = project.CreatedAt,
            CreatedBy = project.CreatedBy
        });
    }
}
```

```csharp
// src/TaskManagement.Application/Projects/Commands/CreateProject/CreateProjectCommandValidator.cs
using FluentValidation;

namespace TaskManagement.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Created by is required");

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Start date must be in the future")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue && x.StartDate.HasValue);
    }
}
```

### 4. Create Queries

```csharp
// src/TaskManagement.Application/Projects/Queries/GetProjectById/GetProjectByIdQuery.cs
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery : IQuery<ProjectDto>
{
    public Guid Id { get; init; }
}
```

```csharp
// src/TaskManagement.Application/Projects/Queries/GetProjectById/GetProjectByIdQueryHandler.cs
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        
        if (project == null)
        {
            return Result<ProjectDto>.Failure("Project not found");
        }

        var owner = await _unitOfWork.Users.GetByIdAsync(project.OwnerId, cancellationToken);

        return Result<ProjectDto>.Success(new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            OwnerId = project.OwnerId,
            OwnerEmail = owner?.Email,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            CreatedBy = project.CreatedBy
        });
    }
}
```

### 5. Update Infrastructure

Add the new entity to your DbContext:

```csharp
// src/TaskManagement.Infrastructure/Data/TaskManagementDbContext.cs
public class TaskManagementDbContext : DbContext
{
    // ... existing code ...
    
    public DbSet<Project> Projects { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing configurations ...

        // Project entity configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

Update the UnitOfWork interface and implementation:

```csharp
// src/TaskManagement.Domain/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Task> Tasks { get; }
    IRepository<Project> Projects { get; } // Add this line
    
    // ... rest of the interface
}
```

```csharp
// src/TaskManagement.Infrastructure/Data/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    // ... existing code ...
    
    private IRepository<Project>? _projects;
    public IRepository<Project> Projects => _projects ??= new Repository<Project>(_context);
    
    // ... rest of the implementation
}
```

### 6. Create Controller

```csharp
// src/TaskManagement.Api/Controllers/ProjectsController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Projects.Commands.CreateProject;
using TaskManagement.Application.Projects.Queries.GetProjectById;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : BaseController
{
    public ProjectsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        var query = new GetProjectByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var command = new CreateProjectCommand
        {
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            OwnerId = request.OwnerId,
            CreatedBy = User.Identity?.Name ?? "system"
        };

        var result = await _mediator.Send(command);
        return HandleResult(result, 201);
    }
}
```

## 🔧 Adding New Authentication Providers

### 1. Create Authentication Provider Interface

```csharp
// src/TaskManagement.Domain/Interfaces/IExternalAuthProvider.cs
using System.Security.Claims;
using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Interfaces;

public interface IExternalAuthProvider
{
    Task<Result<ClaimsPrincipal>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    string ProviderName { get; }
}
```

### 2. Implement Google Authentication Provider

```csharp
// src/TaskManagement.Infrastructure/Authentication/GoogleAuthProvider.cs
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Authentication;

public class GoogleAuthProvider : IExternalAuthProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthProvider> _logger;

    public string ProviderName => "Google";

    public GoogleAuthProvider(IConfiguration configuration, ILogger<GoogleAuthProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<ClaimsPrincipal>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // Implement Google token validation logic
            // This is a simplified example - in production, you'd use Google's token validation
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, "user@example.com"),
                new(ClaimTypes.GivenName, "John"),
                new(ClaimTypes.Surname, "Doe"),
                new(ClaimTypes.Name, "John Doe")
            };

            var identity = new ClaimsIdentity(claims, "Google");
            var principal = new ClaimsPrincipal(identity);

            return Result<ClaimsPrincipal>.Success(principal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google token validation failed");
            return Result<ClaimsPrincipal>.Failure("Invalid Google token");
        }
    }
}
```

### 3. Update Authentication Service

```csharp
// src/TaskManagement.Infrastructure/Authentication/AuthenticationService.cs
public class AuthenticationService : IAuthenticationService
{
    private readonly IEnumerable<IExternalAuthProvider> _authProviders;
    // ... existing code ...

    public AuthenticationService(
        IConfiguration configuration,
        ILogger<AuthenticationService> logger,
        IEnumerable<IExternalAuthProvider> authProviders)
    {
        _configuration = configuration;
        _logger = logger;
        _authProviders = authProviders;
    }

    public async Task<Result<ClaimsPrincipal>> ValidateExternalTokenAsync(string token, string provider, CancellationToken cancellationToken = default)
    {
        var authProvider = _authProviders.FirstOrDefault(p => p.ProviderName.Equals(provider, StringComparison.OrdinalIgnoreCase));
        
        if (authProvider == null)
        {
            return Result<ClaimsPrincipal>.Failure($"Authentication provider '{provider}' not supported");
        }

        return await authProvider.ValidateTokenAsync(token, cancellationToken);
    }
}
```

### 4. Register Services

```csharp
// src/TaskManagement.Api/Program.cs
// Add this to the service registration section
builder.Services.AddScoped<IExternalAuthProvider, GoogleAuthProvider>();
```

## 🧪 Adding Tests for New Features

### 1. Unit Tests

```csharp
// tests/TaskManagement.Tests/Unit/Application/Projects/Commands/CreateProject/CreateProjectCommandHandlerTests.cs
using FluentAssertions;
using Moq;
using TaskManagement.Application.Projects.Commands.CreateProject;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Tests.Unit.Application.Projects.Commands.CreateProject;

public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IRepository<Project>> _projectRepositoryMock;
    private readonly CreateProjectCommandHandler _handler;

    public CreateProjectCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _projectRepositoryMock = new Mock<IRepository<Project>>();
        
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Projects).Returns(_projectRepositoryMock.Object);
        
        _handler = new CreateProjectCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenOwnerExists_ShouldCreateProjectSuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = new User("owner@example.com", "John", "Doe", "azure-oid-123");
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(30),
            OwnerId = ownerId,
            CreatedBy = "test@example.com"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        _projectRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project project) => project);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(command.Name);
        result.Value.Description.Should().Be(command.Description);
        result.Value.OwnerId.Should().Be(command.OwnerId);
        result.Value.OwnerEmail.Should().Be(owner.Email);

        _projectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### 2. Integration Tests

```csharp
// tests/TaskManagement.Tests/Integration/Controllers/ProjectsControllerIntegrationTests.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TaskManagement.Domain.DTOs;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Tests.Integration.Controllers;

public class ProjectsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProjectsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TaskManagementDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<TaskManagementDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestProjectDb");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateProject_WithValidData_ShouldReturnCreatedProject()
    {
        // Arrange
        var createProjectRequest = new
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(30),
            OwnerId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", createProjectRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

## 📊 Adding New Business Rules

### 1. Domain Events

```csharp
// src/TaskManagement.Domain/Common/IDomainEvent.cs
namespace TaskManagement.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
```

```csharp
// src/TaskManagement.Domain/Events/TaskCompletedEvent.cs
namespace TaskManagement.Domain.Events;

public class TaskCompletedEvent : IDomainEvent
{
    public Guid TaskId { get; }
    public Guid AssignedUserId { get; }
    public DateTime CompletedAt { get; }
    public DateTime OccurredOn { get; }

    public TaskCompletedEvent(Guid taskId, Guid assignedUserId, DateTime completedAt)
    {
        TaskId = taskId;
        AssignedUserId = assignedUserId;
        CompletedAt = completedAt;
        OccurredOn = DateTime.UtcNow;
    }
}
```

### 2. Event Handlers

```csharp
// src/TaskManagement.Application/Events/TaskCompletedEventHandler.cs
using MediatR;
using TaskManagement.Domain.Events;

namespace TaskManagement.Application.Events;

public class TaskCompletedEventHandler : INotificationHandler<TaskCompletedEvent>
{
    private readonly ILogger<TaskCompletedEventHandler> _logger;

    public TaskCompletedEventHandler(ILogger<TaskCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TaskCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task {TaskId} completed by user {UserId} at {CompletedAt}", 
            notification.TaskId, notification.AssignedUserId, notification.CompletedAt);

        // Add business logic here, such as:
        // - Send notification email
        // - Update user statistics
        // - Trigger workflow processes
        // - Update related entities

        await Task.CompletedTask;
    }
}
```

## 🔄 Adding Caching

### 1. Create Caching Interface

```csharp
// src/TaskManagement.Domain/Interfaces/ICacheService.cs
namespace TaskManagement.Domain.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}
```

### 2. Implement Caching Service

```csharp
// src/TaskManagement.Infrastructure/Caching/MemoryCacheService.cs
using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Default expiration
        }

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Implementation would depend on the caching provider
        // For memory cache, this is more complex and might require a different approach
        return Task.CompletedTask;
    }
}
```

### 3. Use Caching in Query Handlers

```csharp
// src/TaskManagement.Application/Projects/Queries/GetProjectById/GetProjectByIdQueryHandler.cs
public class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"project_{request.Id}";
        
        // Try to get from cache first
        var cachedProject = await _cacheService.GetAsync<ProjectDto>(cacheKey, cancellationToken);
        if (cachedProject != null)
        {
            return Result<ProjectDto>.Success(cachedProject);
        }

        // Get from database
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        
        if (project == null)
        {
            return Result<ProjectDto>.Failure("Project not found");
        }

        var owner = await _unitOfWork.Users.GetByIdAsync(project.OwnerId, cancellationToken);

        var projectDto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            OwnerId = project.OwnerId,
            OwnerEmail = owner?.Email,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            CreatedBy = project.CreatedBy
        };

        // Cache the result
        await _cacheService.SetAsync(cacheKey, projectDto, TimeSpan.FromMinutes(15), cancellationToken);

        return Result<ProjectDto>.Success(projectDto);
    }
}
```

This comprehensive guide shows how to extend the Task Management API with new features while maintaining the established architecture patterns and principles.
