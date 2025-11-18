using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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
    private readonly AzureAdOptions _azureAdOptions;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly JwtOptions _jwtOptions;
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

        // Initialize OpenID Connect configuration manager for Azure AD metadata
        if (string.IsNullOrWhiteSpace(_azureAdOptions.TenantId))
        {
            _logger.LogWarning("Azure AD TenantId is not configured. Token validation may fail.");
            _configurationManager = null!;
        }
        else
        {
            var metadataAddress =
                $"https://login.microsoftonline.com/{_azureAdOptions.TenantId}/v2.0/.well-known/openid-configuration";
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true });
        }
    }

    public async Task<Result<ClaimsPrincipal>> ValidateAzureAdTokenAsync(string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_configurationManager == null || string.IsNullOrWhiteSpace(_azureAdOptions.TenantId))
            {
                _logger.LogError("Azure AD TenantId is not configured. Cannot validate token.");
                return Result<ClaimsPrincipal>.Failure(AuthenticationErrors.AzureAdTokenValidationFailed);
            }

            // Get OpenID Connect configuration from Azure AD metadata endpoint
            var openIdConfig = await _configurationManager.GetConfigurationAsync(cancellationToken);

            // Build list of valid audiences - Azure AD tokens can have different audiences
            var validAudiences = new List<string>();

            // Add API Client ID
            if (!string.IsNullOrEmpty(_azureAdOptions.ClientId)) validAudiences.Add(_azureAdOptions.ClientId);

            // Add API Application ID URI (for access tokens with API scope)
            // Format: api://[API-Client-ID] or api://[API-Client-ID]/.default
            if (!string.IsNullOrEmpty(_azureAdOptions.ClientId))
            {
                validAudiences.Add($"api://{_azureAdOptions.ClientId}");
                validAudiences.Add($"api://{_azureAdOptions.ClientId}/.default");
            }

            // Note: If you're receiving ID tokens (audience = frontend Client ID), 
            // you may need to add the frontend Client ID to valid audiences.
            // For now, we'll decode the token first to check its audience (without validation)
            var jsonToken = _tokenHandler.ReadJwtToken(token);
            var tokenAudience = jsonToken.Audiences.FirstOrDefault();

            // If token audience doesn't match API audiences, add it (for ID tokens)
            if (!string.IsNullOrEmpty(tokenAudience) &&
                !validAudiences.Contains(tokenAudience) &&
                Guid.TryParse(tokenAudience, out _)) // Only add if it's a GUID (Client ID format)
                validAudiences.Add(tokenAudience);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = openIdConfig.Issuer,
                ValidAudiences = validAudiences,
                IssuerSigningKeys = openIdConfig.SigningKeys, // Use asymmetric keys from Azure AD
                ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                return Result<ClaimsPrincipal>.Failure(AuthenticationErrors.AzureAdTokenInvalidFormat);

            return Result<ClaimsPrincipal>.Success(principal);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
            return Result<ClaimsPrincipal>.Failure(AuthenticationErrors.InvalidAzureAdToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation: {Message}", ex.Message);
            return Result<ClaimsPrincipal>.Failure(AuthenticationErrors.AzureAdTokenValidationFailed);
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
                {
                    // Use ClaimTypes.Role for role claim so [Authorize(Roles = "...")] works
                    var claimType = claim.Key == "role" ? ClaimTypes.Role : claim.Key;
                    claims.Add(new Claim(claimType, claim.Value));
                }

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