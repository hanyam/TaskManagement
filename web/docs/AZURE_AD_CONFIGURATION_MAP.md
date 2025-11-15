# Azure AD Configuration Map

Visual guide showing where each Azure AD value comes from and where it goes.

## Configuration Flow

```
┌─────────────────────────────────────────────────────────────┐
│                      AZURE PORTAL                            │
│  https://portal.azure.com                                    │
│                                                              │
│  Azure Active Directory > App registrations > Your App      │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │  OVERVIEW PAGE                                      │    │
│  │                                                     │    │
│  │  Application (client) ID                           │    │
│  │  ┌──────────────────────────────────────────────┐  │    │
│  │  │ 12345678-1234-1234-1234-123456789abc         │  │◄───┼──┐
│  │  └──────────────────────────────────────────────┘  │    │  │
│  │                                                     │    │  │
│  │  Directory (tenant) ID                             │    │  │
│  │  ┌──────────────────────────────────────────────┐  │    │  │
│  │  │ 87654321-4321-4321-4321-210987654321         │  │◄───┼──┤
│  │  └──────────────────────────────────────────────┘  │    │  │
│  └────────────────────────────────────────────────────┘    │  │
│                                                              │  │
│  ┌────────────────────────────────────────────────────┐    │  │
│  │  AUTHENTICATION PAGE                                │    │  │
│  │                                                     │    │  │
│  │  Platform configurations                           │    │  │
│  │  Single-page application                           │    │  │
│  │  ┌──────────────────────────────────────────────┐  │    │  │
│  │  │ Redirect URIs:                               │  │    │  │
│  │  │ • http://localhost:3000                      │  │◄───┼──┤
│  │  │ • https://yourdomain.com (production)        │  │    │  │
│  │  └──────────────────────────────────────────────┘  │    │  │
│  └────────────────────────────────────────────────────┘    │  │
│                                                              │  │
│  ┌────────────────────────────────────────────────────┐    │  │
│  │  API PERMISSIONS PAGE                               │    │  │
│  │                                                     │    │  │
│  │  Configured permissions:                           │    │  │
│  │  ✓ openid                                          │    │  │
│  │  ✓ profile                                         │    │  │
│  │  ✓ email                                           │    │  │
│  │  ✓ api://backend-client-id/.default               │◄───┼──┘
│  │                                                     │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ Copy values to
                             ↓
┌─────────────────────────────────────────────────────────────┐
│                     .env.local FILE                          │
│  (Location: web/.env.local)                                  │
│                                                              │
│  # API Configuration                                        │
│  NEXT_PUBLIC_API_BASE_URL=http://localhost:5010            │
│  NEXT_PUBLIC_APP_NAME=Task Management Console              │
│                                                              │
│  # Azure AD Configuration                                   │
│  NEXT_PUBLIC_AZURE_AD_CLIENT_ID=12345678-1234-... ◄────────┼─── From Overview page
│  NEXT_PUBLIC_AZURE_AD_TENANT_ID=87654321-4321-... ◄────────┼─── From Overview page
│  NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000 ◄─┼─── Must match Authentication page
│  NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend/.default,... ◄──┼─── From API Permissions page
│                                                              │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ Loaded by
                             ↓
┌─────────────────────────────────────────────────────────────┐
│              src/core/config/env.ts                          │
│                                                              │
│  export function getEnvConfig(): EnvConfig {                │
│    // Reads process.env.NEXT_PUBLIC_AZURE_AD_*             │
│    // Validates with Zod schema                            │
│    // Returns typed config object                          │
│  }                                                           │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ Used by
                             ↓
┌─────────────────────────────────────────────────────────────┐
│       src/features/auth/hooks/useAzureAdLogin.ts            │
│                                                              │
│  const env = getEnvConfig();                                │
│  const azureAdConfig = env.azureAd;                         │
│                                                              │
│  const client = new PublicClientApplication({              │
│    auth: {                                                  │
│      clientId: azureAdConfig.clientId,    ◄─────────────────┼─── CLIENT_ID
│      authority: `https://login.microsoftonline.com/         │
│                  ${azureAdConfig.tenantId}`, ◄──────────────┼─── TENANT_ID
│      redirectUri: azureAdConfig.redirectUri ◄───────────────┼─── REDIRECT_URI
│    }                                                         │
│  });                                                         │
│                                                              │
│  const scopes = azureAdConfig.scopes; ◄─────────────────────┼─── SCOPES
└─────────────────────────────────────────────────────────────┘
                             │
                             │ Opens popup
                             ↓
┌─────────────────────────────────────────────────────────────┐
│                  AZURE AD LOGIN POPUP                        │
│  https://login.microsoftonline.com/{tenantId}/oauth2/...    │
│                                                              │
│  User enters:                                               │
│  • Username (email)                                         │
│  • Password                                                 │
│  • MFA code (if enabled)                                    │
│                                                              │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Microsoft                                            │  │
│  │                                                       │  │
│  │  Sign in                                              │  │
│  │  user@domain.com                                      │  │
│  │  ••••••••                                             │  │
│  │                                                       │  │
│  │  [Sign in]                                            │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ Returns tokens
                             ↓
┌─────────────────────────────────────────────────────────────┐
│              AUTHENTICATION RESULT                           │
│                                                              │
│  {                                                           │
│    idToken: "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIs...",     │
│    accessToken: "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1N...",    │
│    account: {                                               │
│      username: "user@domain.com",                          │
│      name: "John Doe",                                     │
│      ...                                                    │
│    }                                                         │
│  }                                                           │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ Sent to backend
                             ↓
┌─────────────────────────────────────────────────────────────┐
│         POST /api/auth/login (Next.js API Route)            │
│                                                              │
│  Request:                                                   │
│  {                                                           │
│    azureAdToken: "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1N..." │
│  }                                                           │
│                                                              │
│  Forwards to backend API ────────────────────────────────────┼─┐
└─────────────────────────────────────────────────────────────┘ │
                                                                 │
┌────────────────────────────────────────────────────────────────┘
│
↓
┌─────────────────────────────────────────────────────────────┐
│      POST /authentication/authenticate (Backend API)         │
│                                                              │
│  1. Validates Azure AD token with Microsoft                 │
│  2. Creates or updates user in database                     │
│  3. Generates first-party JWT                               │
│  4. Returns JWT + user profile                             │
│                                                              │
│  Response:                                                  │
│  {                                                           │
│    accessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6...",        │
│    tokenType: "Bearer",                                     │
│    expiresIn: 3600,                                         │
│    user: { id, email, displayName, role, ... }             │
│  }                                                           │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ Returns to
                             ↓
┌─────────────────────────────────────────────────────────────┐
│         POST /api/auth/login (Next.js API Route)            │
│                                                              │
│  1. Receives JWT from backend                               │
│  2. Sets httpOnly cookies:                                  │
│     • AUTH_TOKEN=eyJhbGciOiJIUzI1NiIsInR5cCI6...          │
│     • AUTH_USER={"id":"...","email":"..."}                 │
│     • AUTH_EXPIRES_AT=1234567890                            │
│  3. Returns user + token to client                         │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ Client receives
                             ↓
┌─────────────────────────────────────────────────────────────┐
│       src/features/auth/components/SignInForm.tsx           │
│                                                              │
│  1. Receives response from /api/auth/login                  │
│  2. Calls setSession() from AuthProvider                   │
│  3. Stores session in localStorage                         │
│  4. Redirects to dashboard                                 │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ User is now signed in!
                             ↓
┌─────────────────────────────────────────────────────────────┐
│                      DASHBOARD PAGE                          │
│  http://localhost:3000/en/dashboard                          │
│                                                              │
│  Future API requests include:                               │
│  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...     │
└─────────────────────────────────────────────────────────────┘
```

---

## Value Mapping Table

| Azure Portal Location | Value Example | Environment Variable | Code Usage |
|----------------------|---------------|---------------------|------------|
| **Overview → Application (client) ID** | `12345678-1234-1234-...` | `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` | `PublicClientApplication({ auth: { clientId } })` |
| **Overview → Directory (tenant) ID** | `87654321-4321-4321-...` | `NEXT_PUBLIC_AZURE_AD_TENANT_ID` | `authority: "https://login.microsoftonline.com/{tenantId}"` |
| **Authentication → Redirect URIs** | `http://localhost:3000` | `NEXT_PUBLIC_AZURE_AD_REDIRECT_URI` | `PublicClientApplication({ auth: { redirectUri } })` |
| **API Permissions → Configured permissions** | `openid, profile, email, api://...` | `NEXT_PUBLIC_AZURE_AD_SCOPES` | `acquireTokenPopup({ scopes })` |

---

## Configuration Checklist

### ✅ Azure Portal Setup

- [ ] Create or select App Registration
- [ ] Copy **Application (client) ID** from Overview page
- [ ] Copy **Directory (tenant) ID** from Overview page
- [ ] Add redirect URI in Authentication → Platform configurations → Single-page application:
  - [ ] `http://localhost:3000` (development)
  - [ ] `https://yourdomain.com` (production)
- [ ] Enable ID tokens and Access tokens in Authentication → Implicit grant
- [ ] Add API permissions:
  - [ ] `openid` (Microsoft Graph, delegated)
  - [ ] `profile` (Microsoft Graph, delegated)
  - [ ] `email` (Microsoft Graph, delegated)
  - [ ] `api://your-backend-client-id/.default` (your backend API)
- [ ] Grant admin consent for API permissions

### ✅ Environment Configuration

- [ ] Create `web/.env.local` file
- [ ] Set `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` from Azure Portal
- [ ] Set `NEXT_PUBLIC_AZURE_AD_TENANT_ID` from Azure Portal
- [ ] Set `NEXT_PUBLIC_AZURE_AD_REDIRECT_URI` (must match Azure Portal exactly)
- [ ] Set `NEXT_PUBLIC_AZURE_AD_SCOPES` with required scopes
- [ ] Restart development server

### ✅ Testing

- [ ] Navigate to `http://localhost:3000/en/sign-in`
- [ ] Check browser console for Azure AD config log
- [ ] Click "Continue with Azure AD" button
- [ ] Verify popup opens
- [ ] Enter credentials and authenticate
- [ ] Verify redirect to dashboard
- [ ] Check browser console for authentication success log

---

## Common Mistakes

### ❌ Redirect URI Mismatch

**Problem**: `AADSTS50011: Redirect URI mismatch`

**Cause**: The URI in `.env.local` doesn't match Azure Portal exactly.

**Example**:
- Azure Portal: `http://localhost:3000/`
- `.env.local`: `http://localhost:3000`
- **Result**: Error (trailing slash matters!)

**Fix**: Ensure both match exactly, including trailing slashes.

---

### ❌ Wrong Client ID

**Problem**: `AADSTS700016: Application not found`

**Cause**: Client ID in `.env.local` doesn't match the App Registration.

**Fix**: Copy-paste the client ID directly from Azure Portal → Overview page.

---

### ❌ Wrong Tenant ID

**Problem**: `AADSTS90002: Tenant not found`

**Cause**: Tenant ID in `.env.local` is incorrect.

**Fix**: Copy-paste the tenant ID directly from Azure Portal → Overview page.

---

### ❌ Missing API Permissions

**Problem**: User can authenticate but backend rejects the token.

**Cause**: Backend API scope not requested or granted.

**Fix**:
1. Add `api://your-backend-client-id/.default` to `NEXT_PUBLIC_AZURE_AD_SCOPES`
2. Add permission in Azure Portal → API Permissions
3. Grant admin consent

---

### ❌ Environment Variables Not Loaded

**Problem**: Azure AD button is disabled.

**Cause**: Environment variables not loaded or dev server not restarted.

**Fix**:
1. Verify `.env.local` exists in `web/` directory
2. Check variable names start with `NEXT_PUBLIC_`
3. Restart dev server (Ctrl+C, then `npm run dev`)
4. Check browser console for config log

---

## Quick Debug Commands

### Check if environment variables are loaded:

```bash
cd web
npm run dev
# Open browser console at http://localhost:3000/en/sign-in
# Look for: "Azure AD config: { ... }"
```

### Check Azure AD configuration in code:

```typescript
import { getEnvConfig } from "@/core/config/env";

const config = getEnvConfig();
console.log("Azure AD config:", config.azureAd);
```

### Test MSAL configuration:

```typescript
import { useAzureAdLogin } from "@/features/auth/hooks/useAzureAdLogin";

const { isConfigured, login } = useAzureAdLogin();
console.log("Azure AD configured:", isConfigured);
```

---

## Related Documentation

- [Azure AD Quick Start](../AZURE_AD_QUICKSTART.md) - 5-minute setup guide
- [Azure AD Setup Guide](./AZURE_AD_SETUP.md) - Detailed configuration
- [Environment Variables](./ENVIRONMENT_VARIABLES.md) - All variables explained
- [Azure AD Files](./AZURE_AD_FILES.md) - File structure and architecture


