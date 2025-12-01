using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Users.Queries.SearchManagedUsers;

/// <summary>
///     Query for searching users managed by the current user (manager-employee relationship).
/// </summary>
public record SearchManagedUsersQuery : IQuery<List<UserSearchResultDto>>
{
    public Guid ManagerId { get; init; }
    public string SearchQuery { get; init; } = string.Empty;
}