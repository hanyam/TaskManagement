using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Errors.Authentication;

/// <summary>
/// Centralized error definitions for Authentication-related operations.
/// </summary>
public static class AuthenticationErrors
{
    // Azure AD token errors
    public static Error InvalidAzureAdToken => Error.Unauthorized("Invalid Azure AD token");
    public static Error AzureAdTokenExpired => Error.Unauthorized("Azure AD token has expired");
    public static Error AzureAdTokenInvalidFormat => Error.Validation("Invalid Azure AD token format", "AzureAdToken");
    public static Error AzureAdTokenMissing => Error.Validation("Azure AD token is required", "AzureAdToken");
    public static Error AzureAdTokenValidationFailed => Error.Internal("Azure AD token validation failed");

    // JWT token errors
    public static Error JwtTokenGenerationFailed => Error.Internal("Failed to generate JWT token");
    public static Error JwtTokenInvalid => Error.Unauthorized("Invalid JWT token");
    public static Error JwtTokenExpired => Error.Unauthorized("JWT token has expired");
    public static Error JwtTokenMissing => Error.Validation("JWT token is required", "Authorization");
    public static Error JwtTokenInvalidFormat => Error.Validation("Invalid JWT token format", "Authorization");

    // Claims errors
    public static Error EmailClaimMissing => Error.Validation("Email claim not found in Azure AD token", "AzureAdToken");
    public static Error InvalidEmailClaim => Error.Validation("Invalid email claim in token", "Email");
    public static Error MissingRequiredClaims => Error.Validation("Required claims are missing from token", "Claims");

    // Authentication configuration errors
    public static Error JwtConfigurationMissing => Error.Internal("JWT configuration is missing");
    public static Error AzureAdConfigurationMissing => Error.Internal("Azure AD configuration is missing");
    public static Error InvalidJwtConfiguration => Error.Internal("Invalid JWT configuration");
    public static Error InvalidAzureAdConfiguration => Error.Internal("Invalid Azure AD configuration");

    // Authentication service errors
    public static Error AuthenticationServiceUnavailable => Error.Internal("Authentication service is unavailable");
    public static Error TokenValidationServiceError => Error.Internal("Token validation service error");
    public static Error UserCreationFailed => Error.Internal("Failed to create user from Azure AD claims");
    public static Error UserUpdateFailed => Error.Internal("Failed to update user login information");

    // Authorization errors
    public static Error InsufficientPermissions => Error.Forbidden("Insufficient permissions to perform this action");
    public static Error RoleRequired => Error.Forbidden("Required role is missing");
    public static Error ScopeRequired => Error.Forbidden("Required scope is missing");

    // Session errors
    public static Error SessionExpired => Error.Unauthorized("Session has expired");
    public static Error SessionInvalid => Error.Unauthorized("Invalid session");
    public static Error SessionNotFound => Error.NotFound("Session", "SessionId");

    // Rate limiting errors
    public static Error TooManyAuthenticationAttempts => Error.Forbidden("Too many authentication attempts");
    public static Error AuthenticationRateLimited => Error.Forbidden("Authentication rate limit exceeded");

    // Security errors
    public static Error SecurityViolation => Error.Forbidden("Security violation detected");
    public static Error SuspiciousActivity => Error.Forbidden("Suspicious activity detected");
    public static Error AccountCompromised => Error.Forbidden("Account may be compromised");
}
