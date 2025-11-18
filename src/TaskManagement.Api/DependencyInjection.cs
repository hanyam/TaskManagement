using System.Text;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Graph;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManagement.Domain.Options;

namespace TaskManagement.Api;

/// <summary>
///     Dependency injection configuration for API/Presentation layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers all API/Presentation layer services.
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
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
}