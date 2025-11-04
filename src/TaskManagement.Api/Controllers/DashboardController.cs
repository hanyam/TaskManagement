using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetDashboardStats;
using TaskManagement.Domain.Common;
using System.Security.Claims;

namespace TaskManagement.Api.Controllers;

/// <summary>
///     Controller for dashboard operations.
/// </summary>
[ApiController]
[Route("dashboard")]
[Authorize]
public class DashboardController(ICommandMediator commandMediator, IRequestMediator requestMediator)
    : BaseController(commandMediator, requestMediator)
{
    /// <summary>
    ///     Gets dashboard statistics for the current user.
    /// </summary>
    /// <returns>Dashboard statistics.</returns>
    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        // Get user ID from claims
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var query = new GetDashboardStatsQuery { UserId = userId };
        var result = await _requestMediator.Send(query);
        return HandleResult(result);
    }
}

