# Azure AD Quick Start

## TL;DR - Get Azure AD Working in 5 Minutes

### 1. Get Your Azure AD Values

Go to [Azure Portal](https://portal.azure.com) → **Azure Active Directory** → **App registrations** → Your App

Copy these values:
- **Application (client) ID** from the Overview page
- **Directory (tenant) ID** from the Overview page

### 2. Add Environment Variables

Create `web/.env.local` (or edit if exists):

```bash
# API Configuration
NEXT_PUBLIC_API_BASE_URL=http://localhost:5010
NEXT_PUBLIC_APP_NAME=Task Management Console

# Azure AD Configuration
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=paste-your-client-id-here
NEXT_PUBLIC_AZURE_AD_TENANT_ID=paste-your-tenant-id-here
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000
NEXT_PUBLIC_AZURE_AD_SCOPES=api://your-backend-api-client-id/.default,openid,profile,email
```

**Replace**:
- `paste-your-client-id-here` with your Application (client) ID
- `paste-your-tenant-id-here` with your Directory (tenant) ID  
- `your-backend-api-client-id` with your backend API's client ID (or same as above if using one app registration)

### 3. Configure Redirect URI in Azure

1. In Azure Portal, go to your App Registration
2. Click **Authentication** in the left menu
3. Under **Platform configurations** → **Single-page application**
4. Click **Add URI** and enter: `http://localhost:3000`
5. Click **Save**

### 4. Add API Permissions

1. Click **API permissions** in the left menu
2. Click **Add a permission**
3. Add these Microsoft Graph permissions:
   - `openid` (delegated)
   - `profile` (delegated)
   - `email` (delegated)
4. Add your backend API permission if different from Microsoft Graph
5. Click **Grant admin consent for [Your Organization]**

### 5. Test It

```bash
cd web
npm run dev
```

Navigate to `http://localhost:3000/en/sign-in` and click **Continue with Azure AD**.

---

## What If It Doesn't Work?

### Check 1: Environment Variables Loaded?

Open browser console at `http://localhost:3000/en/sign-in` and look for:

```
Azure AD config: {
  clientId: "12345678-...",
  tenantId: "87654321-...",
  ...
}
```

If this is missing or shows `undefined`, your environment variables aren't loaded. **Restart the dev server** after editing `.env.local`.

### Check 2: Popup Blocked?

If the Azure AD popup doesn't open, allow popups for `localhost:3000` in your browser.

### Check 3: Redirect URI Mismatch?

Error: `AADSTS50011: Redirect URI mismatch`

**Fix**: Go to Azure Portal → App Registration → Authentication → Add `http://localhost:3000` exactly as a redirect URI.

### Check 4: Backend Token Validation Fails?

If sign-in popup works but login fails after, check:
1. Your backend API is configured with the same Azure AD tenant
2. The backend can validate Azure AD tokens
3. Backend logs for token validation errors

---

## Production Deployment

Before deploying to production:

1. **Add Production Redirect URI** in Azure Portal:
   - Go to Authentication → Add URI → `https://yourdomain.com`

2. **Set Production Environment Variables**:
   ```bash
   NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
   NEXT_PUBLIC_AZURE_AD_CLIENT_ID=your-client-id
   NEXT_PUBLIC_AZURE_AD_TENANT_ID=your-tenant-id
   NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=https://yourdomain.com
   NEXT_PUBLIC_AZURE_AD_SCOPES=api://your-backend-api-client-id/.default,openid,profile,email
   ```

3. **Use HTTPS**: Azure AD requires HTTPS for production redirect URIs.

---

## Optional: Manual Token Entry

If you don't want to set up Azure AD SSO, users can still sign in by:

1. Getting an Azure AD token from another source (Postman, Azure CLI, etc.)
2. Pasting it directly into the sign-in form

The Azure AD SSO feature is completely optional.

---

## Need More Details?

See the full [Azure AD Setup Guide](./docs/AZURE_AD_SETUP.md) for:
- Detailed authentication flow diagrams
- Security best practices
- Advanced troubleshooting
- MSAL configuration details


