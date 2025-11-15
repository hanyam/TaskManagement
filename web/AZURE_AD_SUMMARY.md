# Azure AD Authentication - Complete Summary

## Quick Reference Card

### What You Need from Azure Portal

| What | Where to Find It | Environment Variable |
|------|------------------|---------------------|
| **Client ID** | App Registration → Overview → Application (client) ID | `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` |
| **Tenant ID** | App Registration → Overview → Directory (tenant) ID | `NEXT_PUBLIC_AZURE_AD_TENANT_ID` |

### What You Need to Configure in Azure Portal

| What | Where | Value |
|------|-------|-------|
| **Redirect URI** | App Registration → Authentication → Platform configurations → Single-page application | `http://localhost:3000` (dev)<br/>`https://yourdomain.com` (prod) |
| **API Permissions** | App Registration → API permissions | `openid`, `profile`, `email`, `api://your-backend/.default` |

### What Goes in `.env.local`

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5010
NEXT_PUBLIC_APP_NAME=Task Management Console

NEXT_PUBLIC_AZURE_AD_CLIENT_ID=paste-from-azure-portal
NEXT_PUBLIC_AZURE_AD_TENANT_ID=paste-from-azure-portal
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000
NEXT_PUBLIC_AZURE_AD_SCOPES=api://your-backend-client-id/.default,openid,profile,email
```

---

## How It Works (The Big Picture)

```
┌─────────────┐
│   Browser   │
│             │
│ User clicks │
│ "Sign in    │
│  with       │
│  Azure AD"  │
└──────┬──────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  1. MSAL opens Azure AD popup                            │
│     (useAzureAdLogin hook → @azure/msal-browser)         │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  2. User authenticates with Azure AD                     │
│     (Microsoft login page)                               │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  3. Azure AD returns tokens                              │
│     - ID token (identifies user)                         │
│     - Access token (authorizes API calls)                │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  4. UI sends ID token to /api/auth/login                 │
│     (Next.js API route)                                  │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  5. API route forwards token to backend                  │
│     POST /authentication/authenticate                    │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  6. Backend validates token with Azure AD                │
│     Returns first-party JWT + user profile               │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  7. API route stores JWT in httpOnly cookies             │
│     (setServerAuthSession)                               │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  8. UI stores session in localStorage                    │
│     (AuthProvider → setClientAuthSession)                │
└──────┬───────────────────────────────────────────────────┘
       │
       ↓
┌──────────────────────────────────────────────────────────┐
│  9. User redirected to dashboard                         │
│     Future requests include JWT in Authorization header  │
└──────────────────────────────────────────────────────────┘
```

---

## File Locations (Where Everything Lives)

### Configuration Files

| File | Purpose | Committed? |
|------|---------|------------|
| `.env.local` | Your actual Azure AD values | ❌ No (git ignored) |
| `.env.local.example` | Template for `.env.local` | ✅ Yes |
| `src/core/config/env.ts` | Validates and exposes env vars | ✅ Yes |

### Authentication Files

| File | What It Does |
|------|--------------|
| `src/features/auth/hooks/useAzureAdLogin.ts` | MSAL integration, popup authentication |
| `src/features/auth/components/SignInForm.tsx` | Sign-in UI with Azure AD button |
| `src/app/api/auth/login/route.ts` | Server-side proxy for token exchange |
| `src/app/api/auth/logout/route.ts` | Clears session cookies |
| `src/core/auth/AuthProvider.tsx` | React context for auth state |
| `src/core/auth/session.server.ts` | Server-side session (cookies) |
| `src/core/auth/session.client.ts` | Client-side session (localStorage) |
| `middleware.ts` | Route guards (redirects unauthenticated users) |

### Documentation Files

| File | What It Covers |
|------|----------------|
| `AZURE_AD_QUICKSTART.md` | 5-minute setup guide |
| `docs/AZURE_AD_SETUP.md` | Detailed configuration and troubleshooting |
| `docs/AZURE_AD_FILES.md` | File structure and architecture |
| `docs/ENVIRONMENT_VARIABLES.md` | All environment variables explained |
| `AZURE_AD_SUMMARY.md` | This file (overview) |

---

## Common Scenarios

### Scenario 1: I want to enable Azure AD SSO

**Steps**:
1. Read [AZURE_AD_QUICKSTART.md](./AZURE_AD_QUICKSTART.md)
2. Get client ID and tenant ID from Azure Portal
3. Add redirect URI in Azure Portal
4. Create `.env.local` with your values
5. Restart dev server
6. Test at `http://localhost:3000/en/sign-in`

**Time**: 5-10 minutes

---

### Scenario 2: I don't want Azure AD SSO (manual token only)

**Steps**:
1. Do nothing! Azure AD SSO is optional
2. Users can paste Azure AD tokens manually

**What happens**:
- The "Continue with Azure AD" button will be disabled
- Users see: "Azure AD is not currently available"
- Manual token entry still works

---

### Scenario 3: Azure AD popup isn't working

**Common Causes**:
- Popup blocked by browser
- Redirect URI mismatch
- Invalid client ID / tenant ID
- API permissions not granted

**Fix**:
See [AZURE_AD_SETUP.md - Troubleshooting](./docs/AZURE_AD_SETUP.md#troubleshooting)

---

### Scenario 4: I'm deploying to production

**Steps**:
1. Add production redirect URI in Azure Portal:
   - Go to Authentication → Add URI → `https://yourdomain.com`
2. Set production environment variables in your hosting platform:
   ```bash
   NEXT_PUBLIC_API_BASE_URL=https://api.yourdomain.com
   NEXT_PUBLIC_AZURE_AD_CLIENT_ID=your-client-id
   NEXT_PUBLIC_AZURE_AD_TENANT_ID=your-tenant-id
   NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=https://yourdomain.com
   NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend-client-id/.default,openid,profile,email
   ```
3. Redeploy

---

### Scenario 5: I want to change the OAuth scopes

**Steps**:
1. Edit `.env.local`:
   ```bash
   NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend/.default,openid,profile,email,User.Read
   ```
2. Add new scope in Azure Portal:
   - Go to API permissions → Add a permission
   - Select Microsoft Graph or your API
   - Add the scope (e.g., `User.Read`)
   - Click "Grant admin consent"
3. Restart dev server

---

### Scenario 6: Backend token validation is failing

**Common Causes**:
- Backend tenant ID doesn't match frontend
- Backend not configured to accept your app's tokens
- Token audience mismatch

**Fix**:
1. Verify backend Azure AD config matches frontend
2. Check backend logs for token validation errors
3. Ensure backend API scope in `NEXT_PUBLIC_AZURE_AD_SCOPES` matches backend configuration

---

## Decision Tree: Do I Need Azure AD SSO?

```
Do you want users to sign in with a popup?
├─ Yes: Enable Azure AD SSO (follow AZURE_AD_QUICKSTART.md)
└─ No: Skip Azure AD SSO
    └─ Users can still sign in by pasting Azure AD tokens manually
```

**Azure AD SSO is optional.** The manual token entry method always works, regardless of SSO configuration.

---

## Dependencies

### NPM Package

- **Package**: `@azure/msal-browser`
- **Version**: `^4.26.1`
- **Purpose**: Microsoft Authentication Library for browser-based Azure AD authentication
- **Installed**: Already in `package.json`

### Backend Requirements

- Backend API must be configured to accept and validate Azure AD tokens
- Backend must have endpoint: `POST /authentication/authenticate`
- Backend must return a first-party JWT + user profile

---

## Environment Variables (At a Glance)

| Variable | Required | Default | Example |
|----------|----------|---------|---------|
| `NEXT_PUBLIC_API_BASE_URL` | ✅ Yes | - | `http://localhost:5010` |
| `NEXT_PUBLIC_APP_NAME` | No | `Task Management Console` | `Acme Task Manager` |
| `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` | For SSO | - | `12345678-1234-...` |
| `NEXT_PUBLIC_AZURE_AD_TENANT_ID` | For SSO | - | `87654321-4321-...` |
| `NEXT_PUBLIC_AZURE_AD_REDIRECT_URI` | No | `window.location.origin` | `http://localhost:3000` |
| `NEXT_PUBLIC_AZURE_AD_SCOPES` | No | `api://{clientId}/.default` | `api://backend/.default,openid,profile,email` |

See [docs/ENVIRONMENT_VARIABLES.md](./docs/ENVIRONMENT_VARIABLES.md) for full details.

---

## Testing Your Setup

### 1. Check Configuration is Loaded

Open browser console at `http://localhost:3000/en/sign-in`:

```javascript
// You should see:
Azure AD config: {
  clientId: "12345678-...",
  tenantId: "87654321-...",
  redirectUri: "http://localhost:3000",
  scopes: ["api://...", "openid", "profile", "email"]
}
```

**If you see `undefined`**: Environment variables not loaded. Restart dev server.

### 2. Check Button is Enabled

On the sign-in page:

✅ **Azure AD button enabled**: Configuration successful  
❌ **Azure AD button disabled**: Check environment variables

### 3. Test Authentication Flow

Click "Continue with Azure AD":

1. ✅ Popup opens with Azure AD login
2. ✅ User enters credentials
3. ✅ Popup closes, user redirected to dashboard

**If popup doesn't open**: Allow popups for `localhost:3000` in browser settings.

### 4. Check Authentication Result

After signing in, check browser console:

```javascript
Popup authentication successful: {
  idToken: "eyJ0eXAi...",
  accessToken: "eyJ0eXAi...",
  account: { username: "user@domain.com", ... }
}
```

---

## Security Checklist

- [x] `.env.local` is in `.gitignore` (not committed)
- [x] Cookies are `httpOnly`, `secure`, `sameSite: 'lax'`
- [x] Client ID and tenant ID are public identifiers (safe to expose)
- [x] No client secrets in environment variables (not needed for SPA)
- [x] HTTPS required for production redirect URIs
- [x] Tokens have expiration (session expires after backend's `expiresIn`)
- [x] Middleware guards protected routes
- [x] Only requested scopes are granted in Azure Portal

---

## What's Next?

### For Development

1. ✅ Follow [AZURE_AD_QUICKSTART.md](./AZURE_AD_QUICKSTART.md) to set up Azure AD
2. ✅ Test authentication at `http://localhost:3000/en/sign-in`
3. ✅ Verify user can access protected routes

### For Production

1. ✅ Add production redirect URI in Azure Portal
2. ✅ Set production environment variables
3. ✅ Test authentication in production environment
4. ✅ Enable MFA for users in Azure AD (recommended)

### For Advanced Scenarios

- **Silent token refresh**: Already implemented in `useAzureAdLogin.ts`
- **Multi-tenant support**: Change authority to `https://login.microsoftonline.com/common`
- **B2C support**: Change authority to your B2C tenant URL
- **Redirect flow (instead of popup)**: Requires code changes in `useAzureAdLogin.ts`

---

## Support and Resources

### Documentation

- 📖 [AZURE_AD_QUICKSTART.md](./AZURE_AD_QUICKSTART.md) - Quick setup guide
- 📖 [docs/AZURE_AD_SETUP.md](./docs/AZURE_AD_SETUP.md) - Detailed configuration
- 📖 [docs/AZURE_AD_FILES.md](./docs/AZURE_AD_FILES.md) - File structure
- 📖 [docs/ENVIRONMENT_VARIABLES.md](./docs/ENVIRONMENT_VARIABLES.md) - Environment variables

### External Resources

- [Microsoft Authentication Library (MSAL) Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [Azure AD App Registration](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
- [Single-page application: Sign-in and Sign-out](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-spa-sign-in)

### Troubleshooting

If you encounter issues:

1. Check browser console for errors
2. Review [AZURE_AD_SETUP.md - Troubleshooting](./docs/AZURE_AD_SETUP.md#troubleshooting)
3. Verify Azure Portal configuration matches environment variables
4. Check backend API logs for token validation errors

---

## Summary

✅ **Azure AD SSO is optional** - manual token entry always works  
✅ **5-minute setup** - follow AZURE_AD_QUICKSTART.md  
✅ **Secure by default** - httpOnly cookies, HTTPS in production  
✅ **Well documented** - 4+ documentation files covering all scenarios  
✅ **Production ready** - used by enterprise applications  

**Start here**: [AZURE_AD_QUICKSTART.md](./AZURE_AD_QUICKSTART.md)


