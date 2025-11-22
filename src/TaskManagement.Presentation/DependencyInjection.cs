using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Text;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Options;

namespace TaskManagement.Presentation;

/// <summary>
///     Dependency injection configuration for Presentation layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers all Presentation layer services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add controllers
        services.AddControllers();

        // Configure options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AzureAdOptions>(configuration.GetSection(AzureAdOptions.SectionName));

        // Configure JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions?.Issuer,
                    ValidAudience = jwtOptions?.Audience,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions?.SecretKey ?? string.Empty))
                };
            });

        services.AddAuthorization();
        // Configure Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
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
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
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

        // Configure CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // Required when frontend sends credentials: "include"
            });
        });

        // Configure Microsoft Graph API client (optional - only if Azure AD is configured)
        var azureAdOptions = configuration.GetSection(AzureAdOptions.SectionName).Get<AzureAdOptions>();
        if (azureAdOptions != null &&
            !string.IsNullOrEmpty(azureAdOptions.TenantId) &&
            !string.IsNullOrEmpty(azureAdOptions.ClientId) &&
            !string.IsNullOrEmpty(azureAdOptions.ClientSecret) &&
            azureAdOptions.TenantId != "FAKE-DATA")
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(
                azureAdOptions.TenantId,
                azureAdOptions.ClientId,
                azureAdOptions.ClientSecret);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
            services.AddSingleton(graphClient);
        }

        return services;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        // .dotnet add package--prerelease OpenTelemetry.Exporter.Prometheus.HttpListener
        builder.Services.AddMetrics();
        builder.Services.AddSingleton<TaskManagementMetrics>();
    //    using var meterProvider = Sdk.CreateMeterProviderBuilder()
    //.AddProcessInstrumentation()
    ////.AddPrometheusHttpListener()
    //.Build();
        var openTelemetryBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddSource("DevHabit.Tracing")
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                // .AddSqlClientInstrumentation()
                )
            .WithMetrics(metrics => metrics
                .AddMeter(TaskManagementMetrics.MeterName)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation());
        
        // Note: Logging is handled by Serilog (configured in Program.cs)
        // OpenTelemetry logging can be added via Serilog.Sinks.OpenTelemetry if needed
        
        if (builder.Environment.IsDevelopment())
        {
            openTelemetryBuilder.UseOtlpExporter();
        }
        else
        {
            openTelemetryBuilder.UseAzureMonitor();
        }

        //builder.Services.AddHealthChecks()
        //    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

        return builder;
    }
}

