using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskManagement.Api.Middleware;
using TaskManagement.Application;
using TaskManagement.Application.Authentication.Commands.AuthenticateUser;
using TaskManagement.Application.Common;
using TaskManagement.Application.Common.Behaviors;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Tasks.Commands.AcceptTask;
using TaskManagement.Application.Tasks.Commands.AcceptTaskProgress;
using TaskManagement.Application.Tasks.Commands.ApproveExtensionRequest;
using TaskManagement.Application.Tasks.Commands.AssignTask;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Application.Tasks.Commands.MarkTaskCompleted;
using TaskManagement.Application.Tasks.Commands.ReassignTask;
using TaskManagement.Application.Tasks.Commands.RejectTask;
using TaskManagement.Application.Tasks.Commands.RequestDeadlineExtension;
using TaskManagement.Application.Tasks.Commands.RequestMoreInfo;
using TaskManagement.Application.Tasks.Commands.UpdateTaskProgress;
using TaskManagement.Application.Tasks.Queries.GetAssignedTasks;
using TaskManagement.Application.Tasks.Queries.GetDashboardStats;
using TaskManagement.Application.Tasks.Queries.GetExtensionRequests;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Application.Tasks.Queries.GetTaskProgressHistory;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Application.Tasks.Queries.GetTasksByReminderLevel;
using TaskManagement.Application.Common.Services;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Options;
using TaskManagement.Infrastructure.Authentication;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/taskmanagement-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AzureAdOptions>(builder.Configuration.GetSection(AzureAdOptions.SectionName));

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and unit of work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Dapper query repositories
builder.Services.AddScoped<TaskDapperRepository>();
builder.Services.AddScoped<UserDapperRepository>();

// Register EF Core command repositories
builder.Services.AddScoped<TaskEfCommandRepository>();
builder.Services.AddScoped<UserEfCommandRepository>();

// Register authentication service
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Configure Custom Mediators
builder.Services.AddScoped<IServiceLocator, ServiceLocator>();
builder.Services.AddScoped<ICommandMediator, CommandMediator>();
builder.Services.AddScoped<IRequestMediator, RequestMediator>();

// Register command handlers
builder.Services.AddScoped<ICommandHandler<CreateTaskCommand, TaskDto>, CreateTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AuthenticateUserCommand, AuthenticationResponse>, AuthenticateUserCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AssignTaskCommand, TaskDto>, AssignTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateTaskProgressCommand, TaskProgressDto>, UpdateTaskProgressCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AcceptTaskProgressCommand>, AcceptTaskProgressCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AcceptTaskCommand, TaskDto>, AcceptTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RejectTaskCommand, TaskDto>, RejectTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RequestMoreInfoCommand, TaskDto>, RequestMoreInfoCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ReassignTaskCommand, TaskDto>, ReassignTaskCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RequestDeadlineExtensionCommand, ExtensionRequestDto>, RequestDeadlineExtensionCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ApproveExtensionRequestCommand>, ApproveExtensionRequestCommandHandler>();
builder.Services.AddScoped<ICommandHandler<MarkTaskCompletedCommand, TaskDto>, MarkTaskCompletedCommandHandler>();

// Register request handlers (queries)
builder.Services.AddScoped<IRequestHandler<GetTaskByIdQuery, TaskDto>, GetTaskByIdQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetTasksQuery, GetTasksResponse>, GetTasksQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>, GetDashboardStatsQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetTaskProgressHistoryQuery, List<TaskProgressDto>>, GetTaskProgressHistoryQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetExtensionRequestsQuery, List<ExtensionRequestDto>>, GetExtensionRequestsQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetAssignedTasksQuery, GetTasksResponse>, GetAssignedTasksQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetTasksByReminderLevelQuery, GetTasksResponse>, GetTasksByReminderLevelQueryHandler>();

// Add pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<AssemblyReference>();

// Configure options
builder.Services.Configure<ReminderOptions>(builder.Configuration.GetSection(ReminderOptions.SectionName));
builder.Services.Configure<ExtensionPolicyOptions>(builder.Configuration.GetSection(ExtensionPolicyOptions.SectionName));

// Register business services
builder.Services.AddScoped<IReminderCalculationService, ReminderCalculationService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions?.Issuer,
            ValidAudience = jwtOptions?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions?.SecretKey ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "A .NET Core API implementing Vertical Slice Architecture with Azure AD authentication"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Ensure database is created (skip in test environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }
}

try
{
    Log.Information("Starting Task Management API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program
{
}