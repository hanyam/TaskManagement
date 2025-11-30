using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using TaskManagement.Presentation.Attributes;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetDashboardStats;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Controller for dashboard operations.
/// </summary>
[ApiController]
[Route("dashboard")]
[Authorize]
public class DashboardController(ICommandMediator commandMediator, IRequestMediator requestMediator, ICurrentUserService currentUserService)
    : BaseController(commandMediator, requestMediator, currentUserService)
{
    /// <summary>
    ///     Gets dashboard statistics for the current user.
    /// </summary>
    /// <returns>Dashboard statistics.</returns>
    [HttpGet("stats")]
    [SwaggerOperation(
        Summary = "Get Dashboard Statistics",
        Description = "Retrieves comprehensive dashboard statistics for the current authenticated user. Returns counts of tasks created by the user, tasks completed, tasks near due date, delayed tasks, tasks in progress, tasks under review, and tasks pending acceptance. Statistics are calculated based on the user's role and assigned/created tasks."
    )]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
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
