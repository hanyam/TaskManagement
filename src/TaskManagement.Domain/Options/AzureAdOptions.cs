namespace TaskManagement.Domain.Options;

/// <summary>
///     Configuration options for Azure AD settings.
/// </summary>
public class AzureAdOptions
{
    public const string SectionName = "AzureAd";

    /// <summary>
    ///     The Azure AD issuer URL.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    ///     The Azure AD client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    ///     The Azure AD client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    ///     The Azure AD tenant ID.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
}