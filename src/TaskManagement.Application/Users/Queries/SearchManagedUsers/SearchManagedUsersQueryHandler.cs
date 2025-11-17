using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;

namespace TaskManagement.Application.Users.Queries.SearchManagedUsers;

/// <summary>
///     Handler for searching users managed by the current user using Dapper for optimized querying.
/// </summary>
public class SearchManagedUsersQueryHandler : IRequestHandler<SearchManagedUsersQuery, List<UserSearchResultDto>>
{
    private readonly UserDapperRepository _userRepository;

    public SearchManagedUsersQueryHandler(UserDapperRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<List<UserSearchResultDto>>> Handle(SearchManagedUsersQuery request, CancellationToken cancellationToken)
    {
        // Validate search query
        if (string.IsNullOrWhiteSpace(request.SearchQuery) || request.SearchQuery.Length < 2)
        {
            return Result<List<UserSearchResultDto>>.Success(new List<UserSearchResultDto>());
        }

        // Search for users where:
        // 1. Current user is the manager (ManagerEmployee.ManagerId == request.ManagerId)
        // 2. User's DisplayName or Email contains the search query (case-insensitive)
        // 3. User is active
        var managedUsers = await _userRepository.SearchManagedUsersAsync(
            request.ManagerId,
            request.SearchQuery,
            cancellationToken);

        // Map to DTO
        var results = managedUsers.Select(u => new UserSearchResultDto
        {
            Id = u.Id.ToString(),
            DisplayName = u.DisplayName,
            Mail = u.Email,
            UserPrincipalName = u.Email,
            JobTitle = null // Not stored in User entity
        }).ToList();

        return Result<List<UserSearchResultDto>>.Success(results);
    }
}

