using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Errors.Users;

/// <summary>
/// Centralized error definitions for User-related operations.
/// </summary>
public static class UserErrors
{
    // User not found errors
    public static Error NotFound => Error.NotFound("User", "Id");
    public static Error NotFoundById(Guid id) => Error.NotFound($"User with ID '{id}'", "Id");
    public static Error NotFoundByEmail(string email) => Error.NotFound($"User with email '{email}'", "Email");

    // User validation errors
    public static Error EmailRequired => Error.Validation("Email is required", "Email");
    public static Error EmailInvalid => Error.Validation("Invalid email format", "Email");
    public static Error EmailTooLong => Error.Validation("Email cannot exceed 254 characters", "Email");
    public static Error FirstNameRequired => Error.Validation("First name is required", "FirstName");
    public static Error FirstNameTooLong => Error.Validation("First name cannot exceed 50 characters", "FirstName");
    public static Error LastNameRequired => Error.Validation("Last name is required", "LastName");
    public static Error LastNameTooLong => Error.Validation("Last name cannot exceed 50 characters", "LastName");
    public static Error DisplayNameTooLong => Error.Validation("Display name cannot exceed 100 characters", "DisplayName");

    // User business logic errors
    public static Error UserInactive => Error.Validation("User account is inactive", "IsActive");
    public static Error UserAlreadyExists => Error.Conflict("User with this email already exists", "Email");
    public static Error CannotDeactivateSelf => Error.Validation("Cannot deactivate your own account", "IsActive");
    public static Error CannotDeleteSelf => Error.Validation("Cannot delete your own account", "Id");

    // User authentication errors
    public static Error InvalidCredentials => Error.Unauthorized("Invalid email or password");
    public static Error AccountLocked => Error.Forbidden("Account is locked");
    public static Error AccountSuspended => Error.Forbidden("Account is suspended");
    public static Error PasswordExpired => Error.Unauthorized("Password has expired");
    public static Error TooManyFailedAttempts => Error.Forbidden("Too many failed login attempts");

    // User creation errors
    public static Error AzureAdObjectIdRequired => Error.Validation("Azure AD Object ID is required", "AzureAdObjectId");
    public static Error AzureAdObjectIdTooLong => Error.Validation("Azure AD Object ID cannot exceed 100 characters", "AzureAdObjectId");
    public static Error CreatedByRequired => Error.Validation("Created by user is required", "CreatedBy");

    // User update errors
    public static Error CannotUpdateOtherUser => Error.Forbidden("Cannot update another user's profile");
    public static Error UserUpdateConflict => Error.Conflict("User was modified by another user", "Id");

    // User query errors
    public static Error InvalidUserId => Error.Validation("User ID is required", "Id");
    public static Error InvalidEmail => Error.Validation("Email is required", "Email");
    public static Error InvalidSearchParameters => Error.Validation("Invalid search parameters provided", "Search");
}
