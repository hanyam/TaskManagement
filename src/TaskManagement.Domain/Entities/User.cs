using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents a user in the system with Azure AD integration.
/// </summary>
public class User : BaseEntity
{
    private User()
    {
    }

    public User(string email, string firstName, string lastName, string azureAdObjectId)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        DisplayName = $"{firstName} {lastName}".Trim();
        AzureAdObjectId = azureAdObjectId;
    }

    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string AzureAdObjectId { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = $"{firstName} {lastName}".Trim();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}