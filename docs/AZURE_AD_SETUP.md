# Azure AD Configuration Guide

This guide provides complete step-by-step instructions for configuring Azure Active Directory (Azure AD) authentication for the TaskManagement application.

## Overview

The TaskManagement application uses two Azure AD app registrations:
- **Frontend App (SPA)**: For user authentication in the Next.js application
- **Backend App (API)**: For protecting API endpoints and defining scopes

## Step 1: Create Azure AD App Registration (Frontend)

1. **Go to Azure Portal**: https://portal.azure.com
2. **Navigate to**: Azure Active Directory → App registrations
3. **Click**: "New registration"
4. **Fill in**:
   - **Name**: `TaskManagement-App`
   - **Supported account types**: "Accounts in this organizational directory only"
   - **Redirect URI**: Select "Single-page application (SPA)" and enter `http://localhost:3000/auth/callback`
5. **Click**: "Register"

## Step 2: Configure Authentication Settings (Frontend)

1. **Go to**: Your app → Authentication (left sidebar)
2. **Under "Platform configurations"**:
   - **Add platform** → "Single-page application"
   - **Redirect URIs**: Add these URLs:
     ```
     http://localhost:3000/auth/callback
     https://localhost:3000/auth/callback
     http://localhost:3000
     https://localhost:3000
     ```
3. **Under "Advanced settings"**:
   - ✅ **Access tokens** (used for implicit flows)
   - ✅ **ID tokens** (used for implicit and hybrid flows)
4. **Click**: "Save"

## Step 3: Create API App Registration (Backend)

1. **Create another app registration**:
   - **Name**: `TaskManagement-API`
   - **Supported account types**: "Accounts in this organizational directory only"
   - **Redirect URI**: Leave blank
2. **Go to**: TaskManagement-API → Expose an API
3. **Set Application ID URI**: Click "Set" and use default or `api://35f8e3e5-47ee-4de8-93b3-5f4bed4a4231`
4. **Add a scope**:
   - **Scope name**: `AccessApi`
   - **Who can consent**: Admins and users
   - **Admin consent display name**: `Access TaskManagement API`
   - **Admin consent description**: `Allow the application to access TaskManagement API on behalf of the signed-in user`
   - **User consent display name**: `Access TaskManagement API`
   - **User consent description**: `Allow the application to access TaskManagement API on your behalf`
   - **State**: Enabled
5. **Click**: "Add scope"

## Step 4: Configure API Permissions (Frontend App)

1. **Go to**: TaskManagement-App → API permissions
2. **Click**: "Add a permission"
3. **Select**: "My APIs" tab
4. **Choose**: TaskManagement-API
5. **Select**: `AccessApi` scope
6. **Click**: "Add permissions"
7. **Click**: "Grant admin consent for [Your Organization]"

## Step 5: Get Configuration Values

### From TaskManagement-App (Frontend):
1. **Go to**: Overview
2. **Copy**: 
   - **Application (client) ID**: This is your `NEXT_PUBLIC_AZURE_AD_CLIENT_ID`
   - **Directory (tenant) ID**: This is your `NEXT_PUBLIC_AZURE_AD_TENANT_ID`

### From TaskManagement-API (Backend):
1. **Go to**: Overview
2. **Copy**: **Application (client) ID** for the API scope
3. **Go to**: Certificates & secrets
4. **Create new client secret**:
   - **Description**: `TaskManagement API Secret`
   - **Expires**: 24 months
   - **Copy the secret value** (you won't see it again!)

## Step 6: Update Configuration Files

### Frontend Configuration (web/.env.local)

```env
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=[Frontend App Client ID]
NEXT_PUBLIC_AZURE_AD_TENANT_ID=[Your Tenant ID]
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000/auth/callback
NEXT_PUBLIC_AZURE_AD_SCOPES=api://[API App Client ID]/AccessApi
```

### Backend Configuration (src/TaskManagement.Api/appsettings.json)

```json
{
  "AzureAd": {
    "Issuer": "https://login.microsoftonline.com/[Your Tenant ID]/v2.0",
    "ClientId": "[API App Client ID]",
    "ClientSecret": "[API App Client Secret from Step 5]"
  }
}
```

## Step 7: Important Configuration Notes

### Two Separate App Registrations
- **Frontend (SPA)**: Handles user authentication and token acquisition
- **Backend (API)**: Defines protected scopes and validates tokens

### Redirect URI Requirements
- Must match exactly between Azure AD configuration and application code
- Include both HTTP and HTTPS variants for development
- Add production URLs when deploying

### Scope Format
- Format: `api://[API-App-Client-ID]/[Scope-Name]`
- Example: `api://35f8e3e5-47ee-4de8-93b3-5f4bed4a4231/AccessApi`

### Development vs Production
- Use separate app registrations for different environments
- Update redirect URIs for production domains
- Rotate client secrets regularly in production

## Step 8: Testing the Configuration

After completing the configuration:

1. **Restart both servers** (API and Next.js)
2. **Navigate to**: http://localhost:3000
3. **Click**: "Continue with Azure AD"
4. **Expected flow**:
   - Azure login popup appears
   - User authenticates with Azure AD
   - Popup closes and user is redirected back
   - Application receives and validates the token
   - User is successfully authenticated

## Common Issues and Troubleshooting

### AADSTS50011: Redirect URI Mismatch
- **Cause**: Redirect URI in request doesn't match Azure AD configuration
- **Solution**: Ensure exact match between app registration and environment variables

### AADSTS65001: Invalid Client
- **Cause**: Client ID not found or incorrect
- **Solution**: Verify client ID matches the app registration

### AADSTS7000215: Invalid Client Secret
- **Cause**: Client secret expired or incorrect
- **Solution**: Generate new client secret and update configuration

### Token Validation Failures
- **Cause**: API configuration doesn't match token issuer
- **Solution**: Verify tenant ID and issuer URL in API settings

## Security Best Practices

1. **Client Secrets**:
   - Store securely (Azure Key Vault in production)
   - Rotate regularly
   - Never commit to source control

2. **Scopes**:
   - Use principle of least privilege
   - Define granular scopes for different operations
   - Regularly audit permissions

3. **Redirect URIs**:
   - Only register necessary URIs
   - Use HTTPS in production
   - Validate URIs in application code

4. **Token Handling**:
   - Validate tokens server-side
   - Check token expiration
   - Implement proper token refresh logic

## Production Deployment Checklist

- [ ] Create production app registrations
- [ ] Configure production redirect URIs
- [ ] Update environment variables for production
- [ ] Store client secrets in secure vault
- [ ] Enable logging and monitoring
- [ ] Test authentication flow in production environment
- [ ] Document emergency procedures for authentication issues

---

**Last Updated**: November 2025  
**Maintainer**: Update when Azure AD configuration changes or new features are added



