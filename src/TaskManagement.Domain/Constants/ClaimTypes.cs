namespace TaskManagement.Domain.Constants;

/// <summary>
///     Centralized claim type names used across the application.
///     These are custom claim types (not System.Security.Claims.ClaimTypes).
///     Placed in Domain layer so both Application and Infrastructure can use them.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    ///     Claim type for user ID (Guid as string).
    ///     Used in JWT tokens and HttpContext.User claims.
    /// </summary>
    public const string UserId = "user_id";

    /// <summary>
    ///     Claim type for user email.
    ///     Used as fallback when ClaimTypes.Email is not available.
    /// </summary>
    public const string Email = "email";

    /// <summary>
    ///     Claim type for user role.
    ///     Used as fallback when ClaimTypes.Role is not available.
    /// </summary>
    public const string Role = "role";
}