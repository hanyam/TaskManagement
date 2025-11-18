using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Application.Users.Queries.SearchManagedUsers;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using static TaskManagement.Domain.Constants.CustomClaimTypes;

namespace TaskManagement.Api.Controllers;

/// <summary>
///     Controller for user operations (Database-based user search with manager-employee relationship).
/// </summary>
[ApiController]
[Route("users")]
[Authorize]
public class UsersController(
    GraphServiceClient? graphClient,
    ILogger<UsersController> logger,
    IRequestMediator requestMediator,
    UserDapperRepository userRepository) : ControllerBase
{
    private readonly GraphServiceClient? _graphClient = graphClient;
    private readonly ILogger<UsersController> _logger = logger;
    private readonly IRequestMediator _requestMediator = requestMediator;
    private readonly UserDapperRepository _userRepository = userRepository;

    /// <summary>
    ///     Searches for users managed by the current user (manager-employee relationship).
    ///     Searches only employees from Tasks.Users table filtered by ManagerEmployee table.
    /// </summary>
    /// <param name="query">Search query (DisplayName).</param>
    /// <returns>List of matching users.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Ok(ApiResponse<List<UserSearchResult>>.SuccessResponse(new List<UserSearchResult>()));

        try
        {
            // Get current user's email from JWT claims
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ??
                            User.FindFirst(Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("User email not found in token claims.");
                return Ok(ApiResponse<List<UserSearchResult>>.SuccessResponse(new List<UserSearchResult>()));
            }

            // Find current user by email to get their ID
            var currentUser = await _userRepository.GetByEmailAsync(userEmail, CancellationToken.None);
            if (currentUser == null)
            {
                _logger.LogWarning("Current user not found in database: {Email}", userEmail);
                return Ok(ApiResponse<List<UserSearchResult>>.SuccessResponse(new List<UserSearchResult>()));
            }

            // Search for managed users using the new query
            var searchQuery = new SearchManagedUsersQuery
            {
                ManagerId = currentUser.Id,
                SearchQuery = query
            };

            var result = await _requestMediator.Send(searchQuery);

            if (result.IsFailure)
            {
                _logger.LogError("Error searching managed users: {Error}", result.Error?.Message);
                return StatusCode(500, ApiResponse<List<UserSearchResult>>.ErrorResponse(
                    "An error occurred while searching users",
                    HttpContext.TraceIdentifier));
            }

            // Map DTOs to UserSearchResult (for frontend compatibility)
            var results = result.Value!.Select(u => new UserSearchResult
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Mail = u.Mail,
                UserPrincipalName = u.UserPrincipalName,
                JobTitle = u.JobTitle
            }).ToList();

            return Ok(ApiResponse<List<UserSearchResult>>.SuccessResponse(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query: {Query}", query);
            return StatusCode(500, ApiResponse<List<UserSearchResult>>.ErrorResponse(
                "An error occurred while searching users",
                HttpContext.TraceIdentifier));
        }
    }

    // ============================================================================
    // OLD AZURE AD IMPLEMENTATION (KEPT FOR FUTURE USE)
    // Uncomment and replace the SearchUsers method above if you need to use Azure AD Graph API
    // ============================================================================
    /*
    /// <summary>
    ///     Searches for users in Azure AD (OLD IMPLEMENTATION - KEPT FOR FUTURE USE).
    /// </summary>
    private async Task<IActionResult> SearchUsersAzureAd([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Ok(ApiResponse<List<UserSearchResult>>.SuccessResponse(new List<UserSearchResult>()));
        }

        try
        {
            if (_graphClient == null)
            {
                _logger.LogWarning("GraphServiceClient is not configured. Returning empty results.");
                return Ok(ApiResponse<List<UserSearchResult>>.SuccessResponse(new List<UserSearchResult>()));
            }

            // Search users by displayName, mail, or userPrincipalName
            var users = await _graphClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter =
                        $"startswith(displayName,'{query}') or startswith(mail,'{query}') or startswith(userPrincipalName,'{query}')";
                    requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "mail", "userPrincipalName", "jobTitle" };
                    requestConfiguration.QueryParameters.Top = 10;
                    requestConfiguration.QueryParameters.Orderby = new[] { "displayName" };
                });

            var results = users?.Value?
                .Select(u => new UserSearchResult
                {
                    Id = u.Id ?? string.Empty,
                    DisplayName = u.DisplayName ?? string.Empty,
                    Mail = u.Mail,
                    UserPrincipalName = u.UserPrincipalName ?? string.Empty,
                    JobTitle = u.JobTitle
                })
                .ToList() ?? new List<UserSearchResult>();

            return Ok(ApiResponse<List<UserSearchResult>>.SuccessResponse(results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query: {Query}", query);
            return StatusCode(500, ApiResponse<List<UserSearchResult>>.ErrorResponse(
                "An error occurred while searching users",
                HttpContext.TraceIdentifier));
        }
    }
    */

    /// <summary>
    ///     Gets a specific user by ID.
    /// </summary>
    /// <param name="id">User ID (GUID).</param>
    /// <returns>User details.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            if (_graphClient == null)
            {
                _logger.LogWarning("GraphServiceClient is not configured.");
                return NotFound(
                    ApiResponse<UserSearchResult>.ErrorResponse("User not found", HttpContext.TraceIdentifier));
            }

            var user = await _graphClient.Users[id]
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[]
                        { "id", "displayName", "mail", "userPrincipalName", "jobTitle" };
                });

            if (user == null)
                return NotFound(
                    ApiResponse<UserSearchResult>.ErrorResponse("User not found", HttpContext.TraceIdentifier));

            var result = new UserSearchResult
            {
                Id = user.Id ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                Mail = user.Mail,
                UserPrincipalName = user.UserPrincipalName ?? string.Empty,
                JobTitle = user.JobTitle
            };

            return Ok(ApiResponse<UserSearchResult>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID: {UserId}", id);
            return StatusCode(500, ApiResponse<UserSearchResult>.ErrorResponse(
                "An error occurred while fetching user",
                HttpContext.TraceIdentifier));
        }
    }
}

/// <summary>
///     Represents a user search result from Azure AD.
/// </summary>
public class UserSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Mail { get; set; }
    public string UserPrincipalName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
}