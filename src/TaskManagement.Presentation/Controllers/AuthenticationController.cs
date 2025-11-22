using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using TaskManagement.Application.Authentication.Commands.AuthenticateUser;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Controller for handling authentication operations.
/// </summary>
[ApiController]
[Route("authentication")]
public class AuthenticationController(ICommandMediator commandMediator, IRequestMediator requestMediator)
    : BaseController(commandMediator, requestMediator)
{
    /// <summary>
    ///     Authenticates a user with Azure AD token and returns a JWT token.
    /// </summary>
    /// <param name="request">The authentication request containing Azure AD token.</param>
    /// <returns>Authentication response with JWT token and user information.</returns>
    [HttpPost("authenticate")]
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
    public string AzureAdToken { get; set; } = string.Empty;
}
