using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Errors.Users;

/// <summary>
///     Centralized error definitions for User-related operations.
/// </summary>
public static class UserErrors
{
    // User not found errors
    public static Error NotFound => Error.Create("NOT_FOUND", "User not found", "Id", "Errors.Users.NotFound");

    // User validation errors
    public static Error EmailRequired =>
        Error.Create("VALIDATION_ERROR", "Email is required", "Email", "Errors.Users.EmailRequired");

    public static Error EmailInvalid =>
        Error.Create("VALIDATION_ERROR", "Invalid email format", "Email", "Errors.Users.EmailInvalid");

    public static Error EmailTooLong => Error.Create("VALIDATION_ERROR", "Email cannot exceed 254 characters", "Email",
        "Errors.Users.EmailTooLong");

    public static Error FirstNameRequired => Error.Create("VALIDATION_ERROR", "First name is required", "FirstName",
        "Errors.Users.FirstNameRequired");

    public static Error FirstNameTooLong => Error.Create("VALIDATION_ERROR", "First name cannot exceed 50 characters",
        "FirstName", "Errors.Users.FirstNameTooLong");

    public static Error LastNameRequired => Error.Create("VALIDATION_ERROR", "Last name is required", "LastName",
        "Errors.Users.LastNameRequired");

    public static Error LastNameTooLong => Error.Create("VALIDATION_ERROR", "Last name cannot exceed 50 characters",
        "LastName", "Errors.Users.LastNameTooLong");

    public static Error DisplayNameTooLong =>
        Error.Create("VALIDATION_ERROR", "Display name cannot exceed 100 characters", "DisplayName",
            "Errors.Users.DisplayNameTooLong");

    // User business logic errors
    public static Error UserInactive => Error.Create("VALIDATION_ERROR", "User account is inactive", "IsActive",
        "Errors.Users.UserInactive");

    public static Error UserAlreadyExists => Error.Create("CONFLICT", "User with this email already exists", "Email",
        "Errors.Users.UserAlreadyExists");

    public static Error CannotDeactivateSelf => Error.Create("VALIDATION_ERROR", "Cannot deactivate your own account",
        "IsActive", "Errors.Users.CannotDeactivateSelf");

    public static Error CannotDeleteSelf => Error.Create("VALIDATION_ERROR", "Cannot delete your own account", "Id",
        "Errors.Users.CannotDeleteSelf");

    // User authentication errors
    public static Error InvalidCredentials => Error.Create("UNAUTHORIZED", "Invalid email or password", null,
        "Errors.Users.InvalidCredentials");

    public static Error AccountLocked =>
        Error.Create("FORBIDDEN", "Account is locked", null, "Errors.Users.AccountLocked");

    public static Error AccountSuspended =>
        Error.Create("FORBIDDEN", "Account is suspended", null, "Errors.Users.AccountSuspended");

    public static Error PasswordExpired =>
        Error.Create("UNAUTHORIZED", "Password has expired", null, "Errors.Users.PasswordExpired");

    public static Error TooManyFailedAttempts => Error.Create("FORBIDDEN", "Too many failed login attempts", null,
        "Errors.Users.TooManyFailedAttempts");

    // User creation errors
    public static Error AzureAdObjectIdRequired =>
        Error.Create("VALIDATION_ERROR", "Azure AD Object ID is required", "AzureAdObjectId",
            "Errors.Users.AzureAdObjectIdRequired");

    public static Error AzureAdObjectIdTooLong =>
        Error.Create("VALIDATION_ERROR", "Azure AD Object ID cannot exceed 100 characters", "AzureAdObjectId",
            "Errors.Users.AzureAdObjectIdTooLong");

    public static Error CreatedByRequired => Error.Create("VALIDATION_ERROR", "Created by user is required",
        "CreatedBy", "Errors.Users.CreatedByRequired");

    // User update errors
    public static Error CannotUpdateOtherUser => Error.Create("FORBIDDEN", "Cannot update another user's profile", null,
        "Errors.Users.CannotUpdateOtherUser");

    public static Error UserUpdateConflict => Error.Create("CONFLICT", "User was modified by another user", "Id",
        "Errors.Users.UserUpdateConflict");

    // User query errors
    public static Error InvalidUserId =>
        Error.Create("VALIDATION_ERROR", "User ID is required", "Id", "Errors.Users.InvalidUserId");

    public static Error InvalidEmail =>
        Error.Create("VALIDATION_ERROR", "Email is required", "Email", "Errors.Users.InvalidEmail");

    public static Error InvalidSearchParameters => Error.Create("VALIDATION_ERROR",
        "Invalid search parameters provided", "Search", "Errors.Users.InvalidSearchParameters");

    public static Error NotFoundById(Guid id)
    {
        return Error.Create("NOT_FOUND", $"User with ID '{id}' not found", "Id", "Errors.Users.NotFoundById");
    }

    public static Error NotFoundByEmail(string email)
    {
        return Error.Create("NOT_FOUND", $"User with email '{email}' not found", "Email",
            "Errors.Users.NotFoundByEmail");
    }
}