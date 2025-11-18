namespace TaskManagement.Domain.Options;

/// <summary>
///     Configuration options for JWT token settings.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    ///     The secret key used to sign JWT tokens.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    ///     The issuer of the JWT token.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    ///     The audience of the JWT token.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    ///     The expiration time in hours for JWT tokens.
    /// </summary>
    public int ExpiryInHours { get; set; } = 1;
}