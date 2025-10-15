using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Authentication.Commands.AuthenticateUser;

/// <summary>
///     Command for authenticating a user with Azure AD token.
/// </summary>
public record AuthenticateUserCommand : ICommand<AuthenticationResponse>
{
    public string AzureAdToken { get; init; } = string.Empty;
}