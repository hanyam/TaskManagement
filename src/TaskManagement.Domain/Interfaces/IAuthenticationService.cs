using System.Security.Claims;
using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Service for handling authentication operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    ///     Validates an Azure AD token and extracts user information.
    /// </summary>
    /// <param name="token">The Azure AD token to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing user claims if validation is successful.</returns>
    Task<Result<ClaimsPrincipal>>
        ValidateAzureAdTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Generates a JWT token with custom claims for the authenticated user.
    /// </summary>
    /// <param name="userEmail">The user's email address.</param>
    /// <param name="additionalClaims">Additional claims to include in the token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the generated JWT token.</returns>
    Task<Result<string>> GenerateJwtTokenAsync(string userEmail, Dictionary<string, string>? additionalClaims = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Validates a JWT token and extracts claims.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the claims principal if validation is successful.</returns>
    Task<Result<ClaimsPrincipal>> ValidateJwtTokenAsync(string token, CancellationToken cancellationToken = default);
}