# Azure AD User Search Setup Guide

## Quick Reference

This guide walks you through setting up Azure AD integration for the user search autocomplete feature in the Task Management system.

---

## Prerequisites

- Azure AD tenant
- Application registration for TaskManagement API
- Admin access to grant consent

---

## Step-by-Step Setup

### 1. Navigate to Azure Portal

1. Go to https://portal.azure.com
2. Sign in with admin account
3. Navigate to **Azure Active Directory**
4. Click **App registrations** in left menu
5. Find your **TaskManagement** application

### 2. Add API Permissions

1. In your app registration, click **API permissions** (left menu)
2. Click **+ Add a permission**
3. Select **Microsoft Graph**
4. Choose **Application permissions** (NOT Delegated permissions)
5. Search for: `User.Read.All`
6. Check the box next to `User.Read.All`
7. Click **Add permissions**

**Alternative Permission (Less Privileged):**
- If you cannot get `User.Read.All`, use `User.ReadBasic.All` instead
- This only reads basic profile info (name, email, UPN)
- Does not include extended fields like jobTitle

### 3. Grant Admin Consent ⚠️ CRITICAL

**This is the most commonly missed step!**

1. On the **API permissions** page, you'll see your new permission
2. Status will show "Not granted for..."
3. Click button: **"Grant admin consent for {Your Organization}"**
4. In the popup, click **Yes** to confirm
5. Wait for the page to refresh
6. Verify status shows: **"Granted for {Your Organization}"** with green checkmark ✅

**Without admin consent, the API will return "Insufficient privileges" error!**

### 4. Get Application Credentials

You need three values for configuration:

#### Tenant ID
1. App registrations → Your app → Overview
2. Copy **Directory (tenant) ID**

#### Client ID
1. Same page → Copy **Application (client) ID**

#### Client Secret
1. App registrations → Your app → **Certificates & secrets**
2. Click **+ New client secret**
3. Description: "TaskManagement Graph API Access"
4. Expires: Choose duration (12 months recommended)
5. Click **Add**
6. **IMMEDIATELY COPY THE VALUE** - you can't see it again!

### 5. Configure appsettings.json

Update your API configuration:

```json
{
  "AzureAd": {
    "TenantId": "paste-tenant-id-here",
    "ClientId": "paste-client-id-here",
    "ClientSecret": "paste-client-secret-value-here",
    "Issuer": "https://login.microsoftonline.com/{tenant-id}/v2.0"
  }
}
```

**Important:** Replace `{tenant-id}` in the Issuer URL with your actual tenant ID.

### 6. Wait for Propagation

After granting consent, wait **2-5 minutes** for changes to propagate through Azure AD.

### 7. Restart API

```bash
# Stop API
Stop-Process -Name "dotnet" -Force

# Start API
cd src/TaskManagement.Api
dotnet run
```

### 8. Test the Feature

1. Open browser → http://localhost:3000
2. Navigate to **Create Task** page
3. Click on **"Assigned user"** field
4. Type a name or email from your Azure AD
5. You should see autocomplete suggestions appear!

---

## Verification Checklist

Use this checklist to verify your setup:

- [ ] App registration exists for TaskManagement
- [ ] `User.Read.All` or `User.ReadBasic.All` permission added
- [ ] Permission type is **Application** (not Delegated)
- [ ] Admin consent **granted** (green checkmark visible)
- [ ] Tenant ID, Client ID, Client Secret copied correctly
- [ ] appsettings.json updated with correct values
- [ ] Waited 2-5 minutes after granting consent
- [ ] API restarted
- [ ] User search returns results

---

## Troubleshooting

### Error: "Insufficient privileges to complete the operation"

**Cause:** Admin consent not granted

**Solution:**
1. Go to API permissions page
2. Check if permission shows "Granted" status with green checkmark
3. If not, click "Grant admin consent for {org}" button
4. Wait 5 minutes
5. Restart API

### Error: "InvalidAuthenticationToken - Signing key is invalid"

**Cause:** Trying to call Graph API directly from frontend (old implementation)

**Solution:** This should be fixed - frontend now calls backend proxy at `/users/search`

### Empty results when searching

**Possible Causes:**
1. Azure AD not configured (still set to "FAKE-DATA")
2. No users match your search query
3. Permission not yet effective (wait longer)
4. Client secret expired

**Solution:**
1. Check API logs: `logs/taskmanagement-*.txt`
2. Verify appsettings.json has real values (not "FAKE-DATA")
3. Try searching for a known user email
4. Generate new client secret if old one expired

### Error: "Client secret has expired"

**Solution:**
1. Azure Portal → Your app → Certificates & secrets
2. Delete expired secret
3. Create new secret
4. Update appsettings.json with new value
5. Restart API

---

## Production Deployment

### Azure App Service Configuration

Set these as Application Settings (not in appsettings.json):

```
AzureAd__TenantId = your-tenant-id
AzureAd__ClientId = your-client-id
AzureAd__ClientSecret = your-client-secret
AzureAd__Issuer = https://login.microsoftonline.com/{tenant-id}/v2.0
```

**Security Best Practice:** Use Azure Key Vault references:
```
AzureAd__ClientSecret = @Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/graph-client-secret)
```

### Docker Deployment

Set environment variables in docker-compose.yml:

```yaml
services:
  taskmanagement.api:
    environment:
      - AzureAd__TenantId=${AZURE_AD_TENANT_ID}
      - AzureAd__ClientId=${AZURE_AD_CLIENT_ID}
      - AzureAd__ClientSecret=${AZURE_AD_CLIENT_SECRET}
```

---

## API Endpoints Reference

### Search Users
```http
GET /users/search?query={searchTerm}
Authorization: Bearer {your-api-token}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "user-guid",
      "displayName": "John Doe",
      "mail": "john.doe@company.com",
      "userPrincipalName": "john.doe@company.com",
      "jobTitle": "Software Engineer"
    }
  ]
}
```

### Get User by ID
```http
GET /users/{userId}
Authorization: Bearer {your-api-token}
```

---

## Permission Comparison

| Permission | Scope | Requires Admin Consent | Access Level |
|------------|-------|----------------------|--------------|
| `User.Read.All` | Application | Yes | Full profile including photo, manager, direct reports |
| `User.ReadBasic.All` | Application | Yes | Basic profile (name, email, UPN only) |
| `User.Read` | Delegated | No | Only signed-in user's profile |

**Recommendation:** Start with `User.ReadBasic.All` if you don't need extended profile data.

---

## Security Considerations

### Why Backend Proxy Instead of Frontend Direct Access?

1. **Token Audience:** JWT tokens are scoped to specific audiences
   - API token audience: `TaskManagement.Api`
   - Graph token audience: `graph.microsoft.com`
   - Cannot use API token for Graph API

2. **Security:**
   - Client secret stays on server (never exposed to browser)
   - Backend can implement rate limiting
   - Backend can cache results
   - Backend can add business logic (e.g., filter by department)

3. **Maintainability:**
   - Centralized Graph API error handling
   - Single point for permission changes
   - Easier to audit Graph API usage

---

## FAQ

**Q: Do I need a separate app registration for this?**
A: No, use your existing TaskManagement API app registration.

**Q: Can I use Delegated permissions instead of Application permissions?**
A: No, delegated permissions require user consent flow and won't work with Client Credentials flow used by the backend.

**Q: How often do I need to renew the client secret?**
A: Depends on expiration set when creating it. Best practice: Set reminder to renew 1 month before expiration.

**Q: What if I don't have admin rights to grant consent?**
A: Contact your Azure AD administrator. They need to grant consent from Azure Portal.

**Q: Does this work with Azure AD B2C?**
A: This guide is for Azure AD (organizational accounts). B2C requires different setup.

**Q: Can I search external/guest users?**
A: Yes, if they're in your Azure AD tenant and the permission scope includes them.

---

## Additional Resources

- [Microsoft Graph API - User Resource](https://learn.microsoft.com/en-us/graph/api/resources/user)
- [Application vs Delegated Permissions](https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent)
- [Client Credentials Flow](https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
- [Microsoft Graph Permissions Reference](https://learn.microsoft.com/en-us/graph/permissions-reference)

---

**Last Updated:** November 15, 2025
**Tested with:** Azure AD, .NET 9, Microsoft.Graph 5.56.0

