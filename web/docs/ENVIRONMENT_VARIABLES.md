# Environment Variables Reference

Complete guide to all environment variables used in the Task Management Console.

## File Locations

- **Development**: `web/.env.local` (not committed to git)
- **Production**: Set in your hosting platform (Vercel, Azure, AWS, etc.)
- **Template**: See `web/.env.local.example` for a copy-paste template

---

## Required Variables

### API Configuration

#### `NEXT_PUBLIC_API_BASE_URL`

**Type**: String (URL)  
**Required**: Yes  
**Default**: `http://localhost:5000`  
**Description**: Base URL of the Task Management API backend.

**Examples**:

```bash
# Development
NEXT_PUBLIC_API_BASE_URL=http://localhost:5010

# Production
NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
```

**Notes**:

- Must be a valid URL (validated by Zod schema)
- Do NOT include a trailing slash
- Must be prefixed with `NEXT_PUBLIC_` to be accessible in the browser

---

#### `NEXT_PUBLIC_APP_NAME`

**Type**: String  
**Required**: No  
**Default**: `Task Management Console`  
**Description**: Application display name shown in the browser title and UI.

**Examples**:

```bash
NEXT_PUBLIC_APP_NAME=Task Management Console
NEXT_PUBLIC_APP_NAME=Acme Corp Task Manager
```

---

## Optional Variables (Azure AD SSO)

All Azure AD variables are optional. If not configured, users can still sign in by manually entering an Azure AD token.

### `NEXT_PUBLIC_AZURE_AD_CLIENT_ID`

**Type**: String (GUID)  
**Required**: No (Yes for Azure AD SSO)  
**Default**: None  
**Description**: Application (client) ID from Azure Portal.

**Where to find it**:

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Select your app
4. Copy the **Application (client) ID** from the Overview page

**Example**:

```bash
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=12345678-1234-1234-1234-123456789abc
```

**Validation**: Must be a valid GUID format.

---

### `NEXT_PUBLIC_AZURE_AD_TENANT_ID`

**Type**: String (GUID)  
**Required**: No (Yes for Azure AD SSO)  
**Default**: None  
**Description**: Directory (tenant) ID from Azure Portal.

**Where to find it**:

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Select your app
4. Copy the **Directory (tenant) ID** from the Overview page

**Example**:

```bash
NEXT_PUBLIC_AZURE_AD_TENANT_ID=87654321-4321-4321-4321-210987654321
```

**Validation**: Must be a valid GUID format.

---

### `NEXT_PUBLIC_AZURE_AD_REDIRECT_URI`

**Type**: String (URL)  
**Required**: No  
**Default**: `window.location.origin` (e.g., `http://localhost:3000`)  
**Description**: URI where Azure AD redirects users after authentication.

**Examples**:

```bash
# Development
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000

# Production
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=https://yourdomain.com
```

**Important**:

- Must match **EXACTLY** what's configured in Azure Portal → Authentication → Redirect URIs
- Include or exclude trailing slash consistently
- Must be registered as a **Single-page application (SPA)** redirect URI in Azure Portal

---

### `NEXT_PUBLIC_AZURE_AD_SCOPES`

**Type**: String (comma-separated list)  
**Required**: No  
**Default**: `api://{clientId}/.default`  
**Description**: OAuth 2.0 scopes to request during authentication.

**Common scopes**:

- `openid` - Required for authentication
- `profile` - User's profile information
- `email` - User's email address
- `api://your-backend-api-client-id/.default` - Access to your backend API

**Examples**:

```bash
# Minimal (default)
NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend-client-id/.default

# Full (recommended)
NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend-client-id/.default,openid,profile,email

# With Microsoft Graph
NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend-client-id/.default,openid,profile,email,User.Read
```

**Notes**:

- Comma-separated, no spaces
- Each scope must be granted in Azure Portal → API permissions
- Backend API scope format: `api://{backend-client-id}/.default`

---

## Complete Examples

### Development (.env.local)

```bash
# ================================================================
# Task Management Console - Development Environment
# ================================================================

# API Configuration
NEXT_PUBLIC_API_BASE_URL=http://localhost:5010
NEXT_PUBLIC_APP_NAME=Task Management Console

# Azure AD Configuration (Optional - remove if not using SSO)
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=12345678-1234-1234-1234-123456789abc
NEXT_PUBLIC_AZURE_AD_TENANT_ID=87654321-4321-4321-4321-210987654321
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000
NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend-client-id/.default,openid,profile,email
```

### Production

```bash
# ================================================================
# Task Management Console - Production Environment
# ================================================================

# API Configuration
NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
NEXT_PUBLIC_APP_NAME=Task Management Console

# Azure AD Configuration
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=12345678-1234-1234-1234-123456789abc
NEXT_PUBLIC_AZURE_AD_TENANT_ID=87654321-4321-4321-4321-210987654321
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=https://yourdomain.com
NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend-client-id/.default,openid,profile,email
```

### Docker Compose

In `docker-compose.yml`, set environment variables for the `taskmanagement.web` service:

```yaml
taskmanagement.web:
  image: taskmanagement.web
  build:
    context: ./web
    dockerfile: Dockerfile
  ports:
    - "3000:3000"
  environment:
    - NEXT_PUBLIC_API_BASE_URL=http://taskmanagement-api:8080
    - NEXT_PUBLIC_APP_NAME=Task Management Console
    - NEXT_PUBLIC_AZURE_AD_CLIENT_ID=12345678-1234-1234-1234-123456789abc
    - NEXT_PUBLIC_AZURE_AD_TENANT_ID=87654321-4321-4321-4321-210987654321
    - NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000
    - NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend-client-id/.default,openid,profile,email
  depends_on:
    - taskmanagement.api
```

---

## Environment-Specific Notes

### Local Development

- Use `http://localhost:3000` for redirect URI
- Backend API typically runs on `http://localhost:5010`
- Create `.env.local` in the `web/` directory
- Restart dev server after changing environment variables

### Vercel Deployment

1. Go to your Vercel project settings
2. Navigate to **Environment Variables**
3. Add each `NEXT_PUBLIC_*` variable
4. Set for **Production**, **Preview**, and **Development** as needed
5. Redeploy to apply changes

### Azure Static Web Apps

1. Go to your Azure Static Web App in Azure Portal
2. Navigate to **Configuration** → **Application settings**
3. Add each `NEXT_PUBLIC_*` variable
4. Save and redeploy

### Docker/Container Deployment

- Pass environment variables via `docker run -e` or `docker-compose.yml`
- Or use a `.env` file with `docker-compose --env-file`
- Build-time variables (NEXT*PUBLIC*\*) must be set during `docker build` or `docker-compose build`

---

## Validation and Errors

Environment variables are validated at runtime by `src/core/config/env.ts` using Zod schemas.

### Common Errors

#### "Invalid environment configuration: NEXT_PUBLIC_API_BASE_URL: Invalid url"

**Cause**: `NEXT_PUBLIC_API_BASE_URL` is not a valid URL.

**Fix**: Ensure it starts with `http://` or `https://` and has no trailing slash.

```bash
# ❌ Bad
NEXT_PUBLIC_API_BASE_URL=localhost:5010
NEXT_PUBLIC_API_BASE_URL=http://localhost:5010/

# ✅ Good
NEXT_PUBLIC_API_BASE_URL=http://localhost:5010
```

---

#### Azure AD not working after setting variables

**Cause**: Environment variables not loaded; dev server not restarted.

**Fix**:

1. Stop the dev server (Ctrl+C)
2. Verify `.env.local` exists and has correct values
3. Restart: `npm run dev`
4. Open browser console and check for:
   ```
   Azure AD config: { clientId: "...", tenantId: "...", ... }
   ```

---

#### "NEXT*PUBLIC*\* variable undefined in browser"

**Cause**: Environment variable not prefixed with `NEXT_PUBLIC_`.

**Fix**: Next.js only exposes variables starting with `NEXT_PUBLIC_` to the browser. All other variables are server-only.

```bash
# ❌ Won't work in browser
AZURE_AD_CLIENT_ID=12345678-...

# ✅ Accessible in browser
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=12345678-...
```

---

## Security Considerations

### What's Safe to Expose?

**All `NEXT_PUBLIC_*` variables are visible in the browser.** Only include values that are safe to expose:

✅ **Safe to expose**:

- API base URL (public endpoint)
- Azure AD client ID (public identifier)
- Azure AD tenant ID (public identifier)
- Azure AD redirect URI (public URL)
- OAuth scopes (public list of permissions requested)

❌ **NEVER expose**:

- Client secrets (not needed for SPA anyway)
- API keys
- Database connection strings
- Private keys

### Server-Only Variables

If you need server-only secrets (e.g., for API routes), use variables **without** the `NEXT_PUBLIC_` prefix:

```bash
# Server-only (not accessible in browser)
API_SECRET_KEY=your-secret-key
DATABASE_URL=postgresql://...

# Browser-accessible
NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
```

---

## Troubleshooting Checklist

### Environment not loading?

- [ ] File is named `.env.local` (not `.env.local.txt`)
- [ ] File is in the `web/` directory (not root)
- [ ] Dev server was restarted after creating/editing the file
- [ ] Variables start with `NEXT_PUBLIC_` for browser access
- [ ] No syntax errors (no quotes, no spaces around `=`)

### Azure AD not working?

- [ ] `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` and `NEXT_PUBLIC_AZURE_AD_TENANT_ID` are set
- [ ] Redirect URI in `.env.local` matches Azure Portal exactly
- [ ] Scopes are granted in Azure Portal → API permissions
- [ ] Dev server restarted after setting variables
- [ ] Browser console shows `Azure AD config: { ... }`

---

## Related Documentation

- [Azure AD Quick Start](../AZURE_AD_QUICKSTART.md) - 5-minute Azure AD SSO setup
- [Azure AD Setup Guide](./AZURE_AD_SETUP.md) - Detailed authentication configuration
- [Azure AD Files Reference](./AZURE_AD_FILES.md) - File structure and architecture

---

## Support

If environment variables aren't loading or validation fails:

1. Check the browser console for error messages
2. Verify file location and syntax
3. Restart the development server
4. Check `src/core/config/env.ts` for validation rules
