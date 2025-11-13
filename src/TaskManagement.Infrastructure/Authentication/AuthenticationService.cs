using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Errors.Authentication;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Options;

namespace TaskManagement.Infrastructure.Authentication;

/// <summary>
///     Implementation of authentication service for Azure AD and JWT token handling.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly JwtOptions _jwtOptions;
    private readonly AzureAdOptions _azureAdOptions;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public AuthenticationService(
        IOptions<JwtOptions> jwtOptions,
        IOptions<AzureAdOptions> azureAdOptions,
        ILogger<AuthenticationService> logger)
    {
        _jwtOptions = jwtOptions.Value;
        _azureAdOptions = azureAdOptions.Value;
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public Task<Result<ClaimsPrincipal>> ValidateAzureAdTokenAsync(string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
               // ValidateIssuer = true,
                ValidateAudience = true,
                //ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _azureAdOptions.Issuer,
                ValidAudience = _azureAdOptions.ClientId,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_azureAdOptions.ClientSecret))
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                return Task.FromResult(Result<ClaimsPrincipal>.Failure(AuthenticationErrors.AzureAdTokenInvalidFormat));

            return Task.FromResult(Result<ClaimsPrincipal>.Success(principal));
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return Task.FromResult(Result<ClaimsPrincipal>.Failure(AuthenticationErrors.InvalidAzureAdToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return Task.FromResult(Result<ClaimsPrincipal>.Failure(AuthenticationErrors.AzureAdTokenValidationFailed));
        }
    }

    public Task<Result<string>> GenerateJwtTokenAsync(string userEmail,
        Dictionary<string, string>? additionalClaims = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, userEmail),
                new(ClaimTypes.Name, userEmail),
                new(JwtRegisteredClaimNames.Sub, userEmail),
                new(JwtRegisteredClaimNames.Email, userEmail),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            // Add additional claims if provided
            if (additionalClaims != null)
                foreach (var claim in additionalClaims)
                    claims.Add(new Claim(claim.Key, claim.Value));

            var token = new JwtSecurityToken(
                _jwtOptions.Issuer,
                _jwtOptions.Audience,
                claims,
                expires: DateTime.UtcNow.AddHours(_jwtOptions.ExpiryInHours),
                signingCredentials: credentials);

            var tokenString = _tokenHandler.WriteToken(token);
            return Task.FromResult(Result<string>.Success(tokenString));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate JWT token for user: {UserEmail}", userEmail);
            return Task.FromResult(Result<string>.Failure(AuthenticationErrors.JwtTokenGenerationFailed));
        }
    }

    public Task<Result<ClaimsPrincipal>> ValidateJwtTokenAsync(string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey))
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            return Task.FromResult(Result<ClaimsPrincipal>.Success(principal));
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "JWT token validation failed");
            return Task.FromResult(Result<ClaimsPrincipal>.Failure(AuthenticationErrors.JwtTokenInvalid));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JWT token validation");
            return Task.FromResult(Result<ClaimsPrincipal>.Failure(AuthenticationErrors.TokenValidationServiceError));
        }
    }
}