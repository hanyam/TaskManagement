using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using TaskManagement.Application.Authentication.Commands.AuthenticateUser;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Controller for handling authentication operations.
/// </summary>
[ApiController]
[Route("authentication")]
public class AuthenticationController(ICommandMediator commandMediator, IRequestMediator requestMediator, ICurrentUserService currentUserService)
    : BaseController(commandMediator, requestMediator, currentUserService)
{
    /// <summary>
    ///     Authenticates a user with Azure AD token and returns a JWT token.
    /// </summary>
    /// <param name="request">The authentication request containing Azure AD token.</param>
    /// <returns>Authentication response with JWT token and user information.</returns>
    [HttpPost("authenticate")]
    [SwaggerOperation(
        Summary = "Authenticate User",
        Description = "Authenticates a user using an Azure AD token and returns a JWT token for subsequent API requests. Validates the Azure AD token, creates or updates the user in the database, and generates a JWT token with user claims including role, email, and user ID. This endpoint is publicly accessible (no authentication required)."
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [AllowAnonymous]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticateUserRequest request)
    {
        var command = new AuthenticateUserCommand
        {
            AzureAdToken = request.AzureAdToken
        };

        var result = await _commandMediator.Send(command);
        return HandleResult(result);
    }
}

/// <summary>
///     Request model for authentication endpoint.
/// </summary>
public class AuthenticateUserRequest
{
    [JsonPropertyName("azureAdToken")]
    public string AzureAdToken { get; set; } = string.Empty;
}
