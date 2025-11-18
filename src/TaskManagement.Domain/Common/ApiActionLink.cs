namespace TaskManagement.Domain.Common;

/// <summary>
///     Represents a hypermedia link for HATEOAS support.
/// </summary>
public class ApiActionLink
{
    /// <summary>
    ///     The relationship type (e.g., "accept", "reject", "update-progress", "self").
    /// </summary>
    public string Rel { get; set; } = string.Empty;

    /// <summary>
    ///     The URI for the action (e.g., "/tasks/{id}/accept").
    /// </summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    ///     The HTTP method to use (e.g., "GET", "POST", "PUT", "DELETE").
    /// </summary>
    public string Method { get; set; } = string.Empty;
}