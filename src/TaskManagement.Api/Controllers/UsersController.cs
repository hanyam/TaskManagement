using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using TaskManagement.Domain.Common;

namespace TaskManagement.Api.Controllers;

/// <summary>
///     Controller for user operations (Azure AD integration).
/// </summary>
[ApiController]
[Route("users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly GraphServiceClient? _graphClient;
    private readonly ILogger<UsersController> _logger;

    public UsersController(GraphServiceClient? graphClient, ILogger<UsersController> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    /// <summary>
    ///     Searches for users in Azure AD.
    /// </summary>
    /// <param name="query">Search query (name, email, or username).</param>
    /// <returns>List of matching users.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
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
                return NotFound(ApiResponse<UserSearchResult>.ErrorResponse("User not found", HttpContext.TraceIdentifier));
            }

            var user = await _graphClient.Users[id]
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "mail", "userPrincipalName", "jobTitle" };
                });

            if (user == null)
            {
                return NotFound(ApiResponse<UserSearchResult>.ErrorResponse("User not found", HttpContext.TraceIdentifier));
            }

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

