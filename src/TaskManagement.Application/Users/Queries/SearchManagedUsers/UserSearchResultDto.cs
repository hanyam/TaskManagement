namespace TaskManagement.Application.Users.Queries.SearchManagedUsers;

/// <summary>
///     DTO for user search results (matches frontend expectations).
/// </summary>
public class UserSearchResultDto
{
    public string Id { get; set; } = string.Empty; // User GUID as string
    public string DisplayName { get; set; } = string.Empty;
    public string? Mail { get; set; }
    public string UserPrincipalName { get; set; } = string.Empty; // Email
    public string? JobTitle { get; set; }
}
