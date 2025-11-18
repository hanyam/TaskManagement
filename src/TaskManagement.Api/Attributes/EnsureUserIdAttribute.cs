using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using static TaskManagement.Domain.Constants.CustomClaimTypes;

namespace TaskManagement.Api.Attributes;

/// <summary>
///     Authorization filter that ensures the current user ID exists.
///     Returns BadRequest if user ID is not found.
///     Works with ICurrentUserService override mechanism for testing.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class EnsureUserIdAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    ///     Called early in the filter pipeline to confirm request is authorized.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Get ICurrentUserService from DI
        var currentUserService = context.HttpContext.RequestServices.GetService<ICurrentUserService>();

        Guid? userId;

        if (currentUserService != null)
        {
            // Use ICurrentUserService (supports override for testing)
            userId = currentUserService.GetUserId();
        }
        else
        {
            // Fallback to HttpContext.User for backward compatibility
            var userIdClaim = context.HttpContext.User.FindFirst(UserId)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = null;
            }
            else
            {
                userId = parsedUserId;
            }
        }

        // If user ID is not found, return BadRequest
        if (!userId.HasValue)
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.ErrorResponse(
                    "User ID not found in token",
                    context.HttpContext.TraceIdentifier));
        }
        else
        {
            // Store userId in HttpContext.Items for easy access in controllers
            context.HttpContext.Items["CurrentUserId"] = userId.Value;
        }
    }
}

