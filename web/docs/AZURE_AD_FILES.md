# Azure AD Integration - File Structure

This document maps all files involved in Azure AD authentication.

## Architecture Overview

```
┌─────────────────┐
│   User Browser  │
└────────┬────────┘
         │
         │ 1. Click "Sign in with Azure AD"
         ↓
┌─────────────────────────────────────────────────────────┐
│  SignInForm.tsx                                          │
│  - Renders both manual token input and Azure AD button │
│  - Calls useAzureAdLogin() hook                         │
└────────┬────────────────────────────────────────────────┘
         │
         │ 2. Trigger Azure AD popup
         ↓
┌─────────────────────────────────────────────────────────┐
│  useAzureAdLogin.ts                                      │
│  - Reads Azure AD config from getEnvConfig()            │
│  - Initializes MSAL PublicClientApplication             │
│  - Opens popup for authentication                       │
└────────┬────────────────────────────────────────────────┘
         │
         │ 3. User authenticates
         ↓
┌─────────────────┐
│  Azure AD       │
│  (Microsoft)    │
└────────┬────────┘
         │
         │ 4. Returns ID token + access token
         ↓
┌─────────────────────────────────────────────────────────┐
│  SignInForm.tsx                                          │
│  - Receives tokens from useAzureAdLogin()               │
│  - Sends ID token to /api/auth/login                    │
└────────┬────────────────────────────────────────────────┘
         │
         │ 5. POST /api/auth/login { azureAdToken }
         ↓
┌─────────────────────────────────────────────────────────┐
│  /api/auth/login/route.ts (Next.js API Route)           │
│  - Validates request body with Zod                      │
│  - Calls backend API /authentication/authenticate       │
│  - Sets server-side auth cookies via setServerAuthSession()
└────────┬────────────────────────────────────────────────┘
         │
         │ 6. POST /authentication/authenticate
         ↓
┌─────────────────┐
│  Backend API    │
│  - Validates    │
│  - Returns JWT  │
└────────┬────────┘
         │
         │ 7. JWT + user profile
         ↓
┌─────────────────────────────────────────────────────────┐
│  /api/auth/login/route.ts                               │
│  - Receives first-party JWT from backend               │
│  - Calls setServerAuthSession() to store in cookies    │
│  - Returns user + token to client                      │
└────────┬────────────────────────────────────────────────┘
         │
         │ 8. Session stored
         ↓
┌─────────────────────────────────────────────────────────┐
│  SignInForm.tsx                                          │
│  - Calls setSession() from AuthProvider                │
│  - Redirects to dashboard                              │
└─────────────────────────────────────────────────────────┘
```

---

## File Breakdown

### 1. Configuration Files

#### `.env.local` (not committed)
```bash
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=...
NEXT_PUBLIC_AZURE_AD_TENANT_ID=...
NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=...
NEXT_PUBLIC_AZURE_AD_SCOPES=...
```

**Purpose**: Stores Azure AD configuration values.  
**Location**: `web/.env.local`  
**Git**: Ignored (not committed)

---

#### `src/core/config/env.ts`

```typescript
export function getEnvConfig(): EnvConfig {
  // Reads NEXT_PUBLIC_AZURE_AD_* from process.env
  // Validates with Zod schema
  // Returns typed config object
}
```

**Purpose**: Validates and exposes environment variables.  
**Used by**: `useAzureAdLogin.ts`, other config consumers  
**Key exports**:
- `getEnvConfig()` - Returns typed config including `azureAd` object

---

### 2. Authentication Hook

#### `src/features/auth/hooks/useAzureAdLogin.ts`

```typescript
export function useAzureAdLogin(): UseAzureAdLoginResult {
  // Creates MSAL PublicClientApplication
  // Manages popup authentication flow
  // Returns { isConfigured, isLoading, login }
}
```

**Purpose**: Manages MSAL integration and popup authentication.  
**Dependencies**:
- `@azure/msal-browser` - Microsoft Authentication Library
- `src/core/config/env.ts` - Azure AD configuration

**Key functions**:
- `login()` - Opens popup, authenticates user, returns tokens
- `isConfigured` - Boolean indicating if Azure AD is configured
- `isLoading` - Boolean indicating if authentication is in progress

**Flow**:
1. Reads config from `getEnvConfig()`
2. Creates `PublicClientApplication` with tenant ID, client ID, redirect URI
3. On `login()`, checks for existing accounts (silent token acquisition)
4. If no accounts, opens popup with `acquireTokenPopup()`
5. Returns `AuthenticationResult` with `idToken` and `accessToken`

---

### 3. Sign-In Form

#### `src/features/auth/components/SignInForm.tsx`

```typescript
export function SignInForm({ locale, redirectTo }: SignInFormProps) {
  const { login: loginWithAzureAd, isConfigured, isLoading } = useAzureAdLogin();
  
  async function handleAzureAdSignIn() {
    const result = await loginWithAzureAd();
    const token = result.idToken ?? result.accessToken;
    // Send token to /api/auth/login
  }
  
  // Renders manual token input + Azure AD button
}
```

**Purpose**: UI component for sign-in page.  
**Location**: `src/features/auth/components/SignInForm.tsx`  
**Used by**: `src/app/[locale]/(public)/sign-in/page.tsx`

**Key features**:
- Renders two authentication methods:
  1. Manual token entry (always available)
  2. Azure AD SSO button (only if `isConfigured === true`)
- Validates form with `react-hook-form` + `zod`
- Calls `useAzureAdLogin().login()` for SSO
- Sends token to `/api/auth/login` Next.js API route
- Handles errors and maps to form fields

---

### 4. Next.js API Routes

#### `src/app/api/auth/login/route.ts`

```typescript
export async function POST(request: Request) {
  // 1. Validate request body with Zod
  const parsed = schema.safeParse(body);
  
  // 2. Call backend API /authentication/authenticate
  const { data } = await apiClient.request<AuthenticationResponse>({
    path: "/authentication/authenticate",
    method: "POST",
    body: parsed.data,
    auth: false
  });
  
  // 3. Set server-side session cookies
  setServerAuthSession(data.accessToken, sessionUser, data.expiresIn);
  
  // 4. Return user + token to client
  return NextResponse.json({ success: true, data });
}
```

**Purpose**: Server-side proxy for authentication.  
**Location**: `src/app/api/auth/login/route.ts`  
**Called by**: `SignInForm.tsx` (client-side)

**Why a server-side route?**
- Allows secure cookie setting with `httpOnly`, `secure`, `sameSite`
- Hides backend API details from client
- Centralizes error handling

**Flow**:
1. Receives `{ azureAdToken }` from client
2. Validates with Zod schema
3. Forwards token to backend API `/authentication/authenticate`
4. Backend validates Azure AD token and returns first-party JWT
5. Sets JWT in `httpOnly` cookies via `setServerAuthSession()`
6. Returns user profile and token to client

---

#### `src/app/api/auth/logout/route.ts`

```typescript
export async function POST(request: Request) {
  clearServerAuthSession();
  return NextResponse.json({ success: true });
}
```

**Purpose**: Clears server-side session cookies.  
**Location**: `src/app/api/auth/logout/route.ts`

---

### 5. Session Management

#### `src/core/auth/session.server.ts`

```typescript
export function setServerAuthSession(token: string, user: PersistedUser, expiresIn: number) {
  // Sets cookies: AUTH_TOKEN, AUTH_USER, AUTH_EXPIRES_AT
  cookies().set("AUTH_TOKEN", token, { httpOnly: true, secure: true, ... });
}

export function getServerAuthToken(): string | undefined {
  return cookies().get("AUTH_TOKEN")?.value;
}

export function clearServerAuthSession() {
  cookies().delete("AUTH_TOKEN");
  cookies().delete("AUTH_USER");
  cookies().delete("AUTH_EXPIRES_AT");
}
```

**Purpose**: Server-side session management using Next.js cookies.  
**Location**: `src/core/auth/session.server.ts`  
**Used by**: API routes, server components, middleware

**Key functions**:
- `setServerAuthSession()` - Stores JWT and user in `httpOnly` cookies
- `getServerAuthToken()` - Retrieves JWT from cookies
- `getServerUser()` - Retrieves user from cookies
- `clearServerAuthSession()` - Deletes all auth cookies

---

#### `src/core/auth/session.client.ts`

```typescript
export function setClientAuthSession(session: AuthSession) {
  localStorage.setItem("auth_session", JSON.stringify(session));
}

export function getClientAuthToken(): string | undefined {
  const session = localStorage.getItem("auth_session");
  return session ? JSON.parse(session).token : undefined;
}

export function clearClientAuthSession() {
  localStorage.removeItem("auth_session");
}
```

**Purpose**: Client-side session management using `localStorage`.  
**Location**: `src/core/auth/session.client.ts`  
**Used by**: Client components, hooks

**Why both server and client?**
- Server cookies (`httpOnly`) are secure and can't be accessed by client JS
- Client `localStorage` allows client components to read session without server round-trip
- Both are kept in sync by `AuthProvider`

---

### 6. Auth Context Provider

#### `src/core/auth/AuthProvider.tsx`

```typescript
export function AuthProvider({ children, initialSession }: AuthProviderProps) {
  const [session, setSessionState] = useState<AuthSession | null>(initialSession);
  
  const setSession = useCallback((newSession: AuthSession) => {
    setSessionState(newSession);
    setClientAuthSession(newSession); // Sync to localStorage
  }, []);
  
  const signOut = useCallback(async () => {
    // Call /api/auth/logout to clear server cookies
    await fetch("/api/auth/logout", { method: "POST" });
    
    // Clear client session
    clearClientAuthSession();
    setSessionState(null);
  }, []);
  
  // Provides: { session, setSession, signOut, isAuthenticated }
}
```

**Purpose**: React context for authentication state.  
**Location**: `src/core/auth/AuthProvider.tsx`  
**Used by**: All components via `useAuth()` hook

**Key functions**:
- `setSession()` - Updates session in state and `localStorage`
- `signOut()` - Clears server cookies and client storage
- `isAuthenticated` - Boolean computed from session state

**Initialization**:
- `initialSession` is passed from `app/[locale]/layout.tsx`
- Server reads session from cookies and passes to client
- Enables SSR with authenticated state

---

### 7. API Client

#### `src/core/api/client.ts`

```typescript
export const apiClient = {
  async request<T>(config: ApiRequestConfig): Promise<{ data: T }> {
    // 1. Resolve auth token (server or client)
    const token = await resolveAuthToken();
    
    // 2. Build request with headers
    const headers = {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    };
    
    // 3. Make fetch request
    // 4. Parse unified API envelope
    // 5. Handle errors
  }
};
```

**Purpose**: Centralized API client for backend requests.  
**Location**: `src/core/api/client.ts`  
**Used by**: API routes, TanStack Query hooks

**Key features**:
- Automatically attaches `Authorization: Bearer <token>` header
- Resolves token from server cookies (SSR) or client `localStorage` (CSR)
- Parses unified API response envelope `{ success, data, errors, message, meta }`
- Throws typed errors (`ApiErrorResponse`)

---

### 8. Middleware

#### `middleware.ts`

```typescript
export async function middleware(request: NextRequest) {
  // 1. Check if route requires authentication
  // 2. Read token from cookies
  const token = request.cookies.get("AUTH_TOKEN")?.value;
  
  // 3. If no token and protected route, redirect to sign-in
  if (!token && isProtectedRoute) {
    return NextResponse.redirect(new URL(`/${locale}/sign-in`, request.url));
  }
  
  // 4. Allow request to proceed
}
```

**Purpose**: Next.js middleware for authentication guards.  
**Location**: `web/middleware.ts`  
**Runs on**: Every request to the application

**Key features**:
- Checks for `AUTH_TOKEN` cookie
- Redirects unauthenticated users from protected routes to sign-in
- Handles locale redirection
- Runs on Edge Runtime (fast, no server round-trip)

---

## Package Dependencies

### `@azure/msal-browser`

**Version**: `^4.26.1`  
**Purpose**: Microsoft Authentication Library for browser-based Azure AD authentication  
**Used in**: `src/features/auth/hooks/useAzureAdLogin.ts`

**Key classes**:
- `PublicClientApplication` - Main MSAL client
- `AuthenticationResult` - Token response object
- `PopupRequest` - Configuration for popup authentication

---

## Environment Variables Summary

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` | Yes* | - | Application (client) ID from Azure Portal |
| `NEXT_PUBLIC_AZURE_AD_TENANT_ID` | Yes* | - | Directory (tenant) ID from Azure Portal |
| `NEXT_PUBLIC_AZURE_AD_REDIRECT_URI` | No | `window.location.origin` | Redirect URI after authentication |
| `NEXT_PUBLIC_AZURE_AD_SCOPES` | No | `api://{clientId}/.default` | Comma-separated list of OAuth scopes |

\* Required only if Azure AD SSO is desired. Manual token entry works without these.

---

## Common Tasks

### Add a new OAuth scope

1. Edit `.env.local`:
   ```bash
   NEXT_PUBLIC_AZURE_AD_SCOPES=api://backend/.default,openid,profile,email,User.Read
   ```

2. Add permission in Azure Portal:
   - Go to **API permissions** → **Add a permission**
   - Select **Microsoft Graph** or your API
   - Add `User.Read` (or other scope)
   - Click **Grant admin consent**

3. Restart dev server

---

### Change redirect URI

1. Edit `.env.local`:
   ```bash
   NEXT_PUBLIC_AZURE_AD_REDIRECT_URI=http://localhost:3000/auth/callback
   ```

2. Update Azure Portal:
   - Go to **Authentication** → **Platform configurations** → **Single-page application**
   - Add `http://localhost:3000/auth/callback`
   - Click **Save**

3. Restart dev server

---

### Debug authentication flow

1. Open browser console at `/en/sign-in`
2. Look for logs:
   ```
   Azure AD config: { clientId: "...", tenantId: "...", ... }
   Starting popup authentication with request: { scopes: [...], prompt: "select_account" }
   Popup authentication successful: { idToken: "...", accessToken: "..." }
   ```

3. If popup fails, check:
   - Popup blocker settings
   - Azure Portal redirect URI matches exactly
   - API permissions granted in Azure Portal

---

## Security Notes

1. **Cookies are `httpOnly`**: Client JavaScript cannot access `AUTH_TOKEN` cookie
2. **Cookies are `secure`**: Sent only over HTTPS in production
3. **Cookies are `sameSite: 'lax'`**: Protects against CSRF attacks
4. **Tokens have expiration**: Session expires after `expiresIn` seconds
5. **Middleware guards routes**: Unauthorized users redirected to sign-in
6. **Client localStorage is synced**: For client-side token resolution in components/hooks

---

## Troubleshooting Reference

| Issue | Check | Fix |
|-------|-------|-----|
| Button disabled | `isConfigured === false` | Set `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` and `NEXT_PUBLIC_AZURE_AD_TENANT_ID` in `.env.local` |
| Popup doesn't open | Browser popup blocker | Allow popups for `localhost:3000` |
| AADSTS50011 error | Redirect URI mismatch | Add exact URI in Azure Portal → Authentication |
| Invalid scope error | Scope not granted | Add permission in Azure Portal → API permissions |
| Token validation fails | Backend config mismatch | Ensure backend has same tenant ID and accepts your app's tokens |

---

## Next Steps

- **Quick Setup**: See [AZURE_AD_QUICKSTART.md](../AZURE_AD_QUICKSTART.md)
- **Detailed Guide**: See [AZURE_AD_SETUP.md](./AZURE_AD_SETUP.md)
- **Production Deployment**: Update redirect URIs in Azure Portal and environment variables


