using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Authentication;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using static TaskManagement.Domain.Constants.CustomClaimTypes;
using static TaskManagement.Domain.Constants.AzureAdClaimTypes;

namespace TaskManagement.Application.Authentication.Commands.AuthenticateUser;

/// <summary>
///     Handler for authenticating a user with Azure AD token using EF Core for commands and Dapper for queries.
/// </summary>
public class AuthenticateUserCommandHandler(
    IAuthenticationService authenticationService,
    UserDapperRepository userQueryRepository,
    UserEfCommandRepository userCommandRepository,
    TaskManagementDbContext context,
    ILogger<AuthenticateUserCommandHandler> logger,
    IAuditLogService auditLogService) : ICommandHandler<AuthenticateUserCommand, AuthenticationResponse>
{
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly TaskManagementDbContext _context = context;
    private readonly UserEfCommandRepository _userCommandRepository = userCommandRepository;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;
    private readonly ILogger<AuthenticateUserCommandHandler> _logger = logger;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task<Result<AuthenticationResponse>> Handle(AuthenticateUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting authentication process");

        var errors = new List<Error>();

        // Validate Azure AD token
        var validationResult =
            await _authenticationService.ValidateAzureAdTokenAsync(request.AzureAdToken, cancellationToken);
        if (validationResult.IsFailure)
        {
            _logger.LogWarning("Azure AD token validation failed: {Error}", validationResult.Error?.Message);
            errors.Add(validationResult.Error ?? AuthenticationErrors.InvalidAzureAdToken);
        }

        // If there are any validation errors, return them all
        if (errors.Any())
        {
            _auditLogService.LogAuthenticationFailure("Unknown", "Token validation failed");
            return Result<AuthenticationResponse>.Failure(errors);
        }

        var claimsPrincipal = validationResult.Value!;
        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value ??
                    claimsPrincipal.FindFirst(Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Email claim missing from Azure AD token");
            _auditLogService.LogAuthenticationFailure("Unknown", "Email claim missing");
            errors.Add(AuthenticationErrors.EmailClaimMissing);
            return Result<AuthenticationResponse>.Failure(errors);
        }

        // Find user using Dapper (fast query)
        var user = await _userQueryRepository.GetByEmailAsync(email!, cancellationToken);

        if (user == null)
        {
            _logger.LogInformation("Creating new user from Azure AD: {Email}", email);
            // Create new user from Azure AD claims using EF Core (for change tracking)
            var firstName = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value ??
                            claimsPrincipal.FindFirst(GivenName)?.Value ?? string.Empty;
            var lastName = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value ??
                           claimsPrincipal.FindFirst(FamilyName)?.Value ?? string.Empty;
            var objectId = claimsPrincipal.FindFirst(ObjectId)?.Value ??
                           claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            user = new User(email!, firstName, lastName, objectId);
            user.SetCreatedBy(email!);

            await _userCommandRepository.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created new user {UserId} with email {Email}", user.Id, email);

            // Check if user is a manager and update role accordingly
            var isManager = await _userQueryRepository.IsManagerAsync(user.Id, cancellationToken);
            if (isManager && user.Role == UserRole.Employee)
            {
                user.UpdateRole(UserRole.Manager);
                await _userCommandRepository.UpdateAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            // Update last login using EF Core (for change tracking)
            user.RecordLogin();

            // Check if user is a manager and update role accordingly (role may have changed in database)
            var isManager = await _userQueryRepository.IsManagerAsync(user.Id, cancellationToken);
            if (isManager && user.Role == UserRole.Employee)
                // User is a manager but role is Employee - update to Manager
                user.UpdateRole(UserRole.Manager);
            else if (!isManager && user.Role == UserRole.Manager)
                // User is no longer a manager - revert to Employee (unless Admin)
                // Note: Admin role should be set manually and not changed automatically
                user.UpdateRole(UserRole.Employee);

            await _userCommandRepository.UpdateAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Generate JWT token with custom claims
        var additionalClaims = new Dictionary<string, string>
        {
            { UserId, user.Id.ToString() },
            { "display_name", user.DisplayName },
            { "first_name", user.FirstName },
            { "last_name", user.LastName },
            { Role, user.Role.ToString() }
        };

        var jwtResult = await _authenticationService.GenerateJwtTokenAsync(email!, additionalClaims, cancellationToken);
        if (jwtResult.IsFailure)
        {
            _logger.LogError("JWT token generation failed for user {Email}: {Error}", email, jwtResult.Error?.Message);
            errors.Add(jwtResult.Error ?? AuthenticationErrors.JwtTokenGenerationFailed);
            return Result<AuthenticationResponse>.Failure(errors);
        }

        _logger.LogInformation("Successfully authenticated user {UserId} with email {Email} and role {Role}", user.Id, email, user.Role);
        _auditLogService.LogAuthenticationSuccess(user.Id.ToString(), email);

        return new AuthenticationResponse
        {
            AccessToken = jwtResult.Value!,
            ExpiresIn = 3600, // 1 hour
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Role = user.Role.ToString()
            }
        };
    }
}