using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Attributes;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetDashboardStats;
using TaskManagement.Domain.Common;

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
    [EnsureUserId]
    public async Task<IActionResult> GetDashboardStats()
    {
        // Get user ID (guaranteed to exist by EnsureUserIdAttribute)
        var userId = GetRequiredUserId();

        var query = new GetDashboardStatsQuery { UserId = userId };
        var result = await _requestMediator.Send(query);
        return HandleResult(result);
    }
}