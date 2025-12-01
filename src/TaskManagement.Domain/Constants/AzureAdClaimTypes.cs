namespace TaskManagement.Domain.Constants;

/// <summary>
///     Centralized Azure AD claim type names used when extracting claims from Azure AD tokens.
///     These are the claim names that Azure AD provides in its tokens.
///     Placed in Domain layer so both Application and Infrastructure can use them.
/// </summary>
public static class AzureAdClaimTypes
{
    /// <summary>
    ///     Azure AD object ID claim (user's unique identifier in Azure AD).
    ///     Alternative names: "oid" or ClaimTypes.NameIdentifier.
    /// </summary>
    public const string ObjectId = "oid";

    /// <summary>
    ///     Azure AD given name claim (first name).
    ///     Alternative names: "given_name" or ClaimTypes.GivenName.
    /// </summary>
    public const string GivenName = "given_name";

    /// <summary>
    ///     Azure AD family name claim (last name).
    ///     Alternative names: "family_name" or ClaimTypes.Surname.
    /// </summary>
    public const string FamilyName = "family_name";
}