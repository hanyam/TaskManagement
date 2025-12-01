namespace TaskManagement.Domain.Constants;

/// <summary>
///     Centralized role names used across the application.
///     These match the UserRole enum values but as strings for use in [Authorize] attributes.
///     Placed in Domain layer so both Application and Infrastructure can use them.
/// </summary>
public static class RoleNames
{
    /// <summary>
    ///     Employee role name.
    /// </summary>
    public const string Employee = "Employee";

    /// <summary>
    ///     Manager role name.
    /// </summary>
    public const string Manager = "Manager";

    /// <summary>
    ///     Admin role name.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    ///     Default role name (Employee).
    /// </summary>
    public const string Default = Employee;

    /// <summary>
    ///     Comma-separated string for Employee and Manager roles.
    ///     Used in [Authorize(Roles = RoleNames.EmployeeOrManager)].
    /// </summary>
    public const string EmployeeOrManager = "Employee,Manager";

    /// <summary>
    ///     Comma-separated string for Manager and Admin roles.
    ///     Used in [Authorize(Roles = RoleNames.ManagerOrAdmin)].
    /// </summary>
    public const string ManagerOrAdmin = "Manager,Admin";
}