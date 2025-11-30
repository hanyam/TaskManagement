using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Errors.Authentication;

/// <summary>
///     Centralized error definitions for Authentication-related operations.
/// </summary>
public static class AuthenticationErrors
{
    // Azure AD token errors
    public static Error InvalidAzureAdToken => Error.Create("UNAUTHORIZED", "Invalid Azure AD token", null, "Errors.Authentication.InvalidAzureAdToken");
    public static Error AzureAdTokenExpired => Error.Create("UNAUTHORIZED", "Azure AD token has expired", null, "Errors.Authentication.AzureAdTokenExpired");
    public static Error AzureAdTokenInvalidFormat => Error.Create("VALIDATION_ERROR", "Invalid Azure AD token format", "AzureAdToken", "Errors.Authentication.AzureAdTokenInvalidFormat");
    public static Error AzureAdTokenMissing => Error.Create("VALIDATION_ERROR", "Azure AD token is required", "AzureAdToken", "Errors.Authentication.AzureAdTokenMissing");
    public static Error AzureAdTokenValidationFailed => Error.Create("INTERNAL_ERROR", "Azure AD token validation failed", null, "Errors.Authentication.AzureAdTokenValidationFailed");

    // JWT token errors
    public static Error JwtTokenGenerationFailed => Error.Create("INTERNAL_ERROR", "Failed to generate JWT token", null, "Errors.Authentication.JwtTokenGenerationFailed");
    public static Error JwtTokenInvalid => Error.Create("UNAUTHORIZED", "Invalid JWT token", null, "Errors.Authentication.JwtTokenInvalid");
    public static Error JwtTokenExpired => Error.Create("UNAUTHORIZED", "JWT token has expired", null, "Errors.Authentication.JwtTokenExpired");
    public static Error JwtTokenMissing => Error.Create("VALIDATION_ERROR", "JWT token is required", "Authorization", "Errors.Authentication.JwtTokenMissing");
    public static Error JwtTokenInvalidFormat => Error.Create("VALIDATION_ERROR", "Invalid JWT token format", "Authorization", "Errors.Authentication.JwtTokenInvalidFormat");

    // Claims errors
    public static Error EmailClaimMissing =>
        Error.Create("VALIDATION_ERROR", "Email claim not found in Azure AD token", "AzureAdToken", "Errors.Authentication.EmailClaimMissing");

    public static Error InvalidEmailClaim => Error.Create("VALIDATION_ERROR", "Invalid email claim in token", "Email", "Errors.Authentication.InvalidEmailClaim");
    public static Error MissingRequiredClaims => Error.Create("VALIDATION_ERROR", "Required claims are missing from token", "Claims", "Errors.Authentication.MissingRequiredClaims");

    // Authentication configuration errors
    public static Error JwtConfigurationMissing => Error.Create("INTERNAL_ERROR", "JWT configuration is missing", null, "Errors.Authentication.JwtConfigurationMissing");
    public static Error AzureAdConfigurationMissing => Error.Create("INTERNAL_ERROR", "Azure AD configuration is missing", null, "Errors.Authentication.AzureAdConfigurationMissing");
    public static Error InvalidJwtConfiguration => Error.Create("INTERNAL_ERROR", "Invalid JWT configuration", null, "Errors.Authentication.InvalidJwtConfiguration");
    public static Error InvalidAzureAdConfiguration => Error.Create("INTERNAL_ERROR", "Invalid Azure AD configuration", null, "Errors.Authentication.InvalidAzureAdConfiguration");

    // Authentication service errors
    public static Error AuthenticationServiceUnavailable => Error.Create("INTERNAL_ERROR", "Authentication service is unavailable", null, "Errors.Authentication.AuthenticationServiceUnavailable");
    public static Error TokenValidationServiceError => Error.Create("INTERNAL_ERROR", "Token validation service error", null, "Errors.Authentication.TokenValidationServiceError");
    public static Error UserCreationFailed => Error.Create("INTERNAL_ERROR", "Failed to create user from Azure AD claims", null, "Errors.Authentication.UserCreationFailed");
    public static Error UserUpdateFailed => Error.Create("INTERNAL_ERROR", "Failed to update user login information", null, "Errors.Authentication.UserUpdateFailed");

    // Authorization errors
    public static Error InsufficientPermissions => Error.Create("FORBIDDEN", "Insufficient permissions to perform this action", null, "Errors.Authentication.InsufficientPermissions");
    public static Error RoleRequired => Error.Create("FORBIDDEN", "Required role is missing", null, "Errors.Authentication.RoleRequired");
    public static Error ScopeRequired => Error.Create("FORBIDDEN", "Required scope is missing", null, "Errors.Authentication.ScopeRequired");

    // Session errors
    public static Error SessionExpired => Error.Create("UNAUTHORIZED", "Session has expired", null, "Errors.Authentication.SessionExpired");
    public static Error SessionInvalid => Error.Create("UNAUTHORIZED", "Invalid session", null, "Errors.Authentication.SessionInvalid");
    public static Error SessionNotFound => Error.Create("NOT_FOUND", "Session not found", "SessionId", "Errors.Authentication.SessionNotFound");

    // Rate limiting errors
    public static Error TooManyAuthenticationAttempts => Error.Create("FORBIDDEN", "Too many authentication attempts", null, "Errors.Authentication.TooManyAuthenticationAttempts");
    public static Error AuthenticationRateLimited => Error.Create("FORBIDDEN", "Authentication rate limit exceeded", null, "Errors.Authentication.AuthenticationRateLimited");

    // Security errors
    public static Error SecurityViolation => Error.Create("FORBIDDEN", "Security violation detected", null, "Errors.Authentication.SecurityViolation");
    public static Error SuspiciousActivity => Error.Create("FORBIDDEN", "Suspicious activity detected", null, "Errors.Authentication.SuspiciousActivity");
    public static Error AccountCompromised => Error.Create("FORBIDDEN", "Account may be compromised", null, "Errors.Authentication.AccountCompromised");
}
