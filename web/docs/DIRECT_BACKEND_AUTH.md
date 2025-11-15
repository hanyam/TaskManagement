# Direct Backend Authentication

This document explains the updated authentication flow where the browser sends Azure AD tokens **directly** to the backend API (port 5010) instead of going through the Next.js API route (port 3000).

## Authentication Flow (Updated)

```
┌─────────────────────────────────────────────────────────────┐
│  1. User clicks "Sign in with Azure AD"                     │
│     on http://localhost:3000 (Next.js UI)                   │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────────┐
│  2. Azure AD popup authenticates user                       │
│     Returns Azure AD token to browser                       │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────────┐
│  3. Browser sends Azure AD token DIRECTLY to backend:       │
│     POST http://localhost:5010/authentication/authenticate  │
│     (Skips Next.js API route!)                              │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────────┐
│  4. Backend validates token with Azure AD                   │
│     Returns first-party JWT + user profile                  │
└────────────────┬────────────────────────────────────────────┘
                 │
                 ↓
┌─────────────────────────────────────────────────────────────┐
│  5. Browser receives JWT and stores in:                     │
│     - localStorage (client-side session)                    │
│     - Future requests include: Authorization: Bearer JWT   │
└─────────────────────────────────────────────────────────────┘
```

## What Changed

### Before (Proxy Pattern via Next.js)

```typescript
// SignInForm.tsx - OLD
const response = await fetch("/api/auth/login", {  // ← Next.js API route
  method: "POST",
  body: JSON.stringify(values)
});
```

Flow: Browser → Port 3000 (`/api/auth/login`) → Port 5010 → Browser

### After (Direct Backend Call)

```typescript
// SignInForm.tsx - NEW
const { data } = await apiClient.request<AuthenticationResponse>({
  path: "/authentication/authenticate",  // ← Direct to backend
  method: "POST",
  body: values,
  auth: false
});
```

Flow: Browser → Port 5010 (`/authentication/authenticate`) → Browser

## Files Modified

### `web/src/features/auth/components/SignInForm.tsx`

**Changes:**
1. Replaced `fetch("/api/auth/login")` with `apiClient.request()`
2. Added imports for `apiClient` and `AuthenticationResponse`
3. Updated response handling to use `data.accessToken` instead of `data.token`
4. Removed `LoginResponse` interface (no longer needed)
5. Simplified error handling

**Code:**

```typescript
import { apiClient } from "@/core/api";
import type { AuthenticationResponse } from "@/features/auth/types";

const { mutateAsync, isPending } = useMutation({
  mutationFn: async (values: SignInFormValues) => {
    // Send directly to backend API
    const { data } = await apiClient.request<AuthenticationResponse>({
      path: "/authentication/authenticate",
      method: "POST",
      body: values,
      auth: false
    });

    return data;
  }
});

async function handleSubmit(values: SignInFormValues) {
  try {
    const data = await mutateAsync(values);

    const session: AuthSession = {
      token: data.accessToken,  // ← Backend returns 'accessToken'
      user: {
        id: data.user.id,
        email: data.user.email,
        displayName: data.user.displayName,
        ...(data.user.role ? { role: data.user.role } : {})
      },
      expiresAt: new Date(Date.now() + data.expiresIn * 1000).toISOString()
    };

    setSession(session);
    router.push(`/${locale}/dashboard`);
  } catch (error) {
    form.setError("azureAdToken", {
      type: "server",
      message: t("auth:signIn.invalidToken")
    });
  }
}
```

## CORS Configuration

The backend already has CORS configured to allow requests from `http://localhost:3000`:

**`src/TaskManagement.Api/DependencyInjection.cs`:**

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

**`src/TaskManagement.Api/Program.cs`:**

```csharp
app.UseCors("AllowFrontend");
```

This allows the browser at `localhost:3000` to make requests to `localhost:5010`.

## Session Management

### Before (HttpOnly Cookies + localStorage)

- **Server-side:** JWT stored in httpOnly cookies (set by Next.js API route)
- **Client-side:** Session stored in localStorage
- **Security:** HttpOnly cookies protected against XSS

### After (localStorage Only)

- **Client-side only:** Session stored in localStorage
- **Security:** Relies on proper XSS protection in the application
- **Authorization:** `Authorization: Bearer <token>` header on every request

**Note:** This is less secure than httpOnly cookies but simpler for SPAs that don't need server-side rendering.

## API Client Configuration

The `apiClient` automatically:
1. Reads the token from localStorage
2. Adds `Authorization: Bearer <token>` header to requests
3. Handles the unified API response envelope
4. Throws typed errors for error handling

**How it works:**

```typescript
// src/core/api/client.ts
async function resolveAuthToken(): Promise<string | undefined> {
  if (isBrowser) {
    const { getClientAuthToken } = await import("@/core/auth/session.client");
    return getClientAuthToken();  // Reads from localStorage
  }
  // Server-side would read from cookies (for SSR)
}

export const apiClient = {
  async request<T>(config: ApiRequestConfig) {
    const token = await resolveAuthToken();
    
    const headers = {
      "Content-Type": "application/json",
      ...(config.auth !== false && token ? { Authorization: `Bearer ${token}` } : {})
    };
    
    // Make request to backend...
  }
};
```

## Environment Variables

No changes needed! The `NEXT_PUBLIC_API_BASE_URL` is already configured:

**`.env.local`:**

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:5010
```

The `apiClient` uses this to construct the full URL:

```typescript
const url = `${env.apiBaseUrl}${config.path}`;
// Result: http://localhost:5010/authentication/authenticate
```

## Testing the Changes

### 1. Start the Backend API

```bash
cd src/TaskManagement.Api
dotnet run
# API running on http://localhost:5010
```

### 2. Start the Web UI

```bash
cd web
npm run dev
# UI running on http://localhost:3000
```

### 3. Test Authentication

1. Navigate to `http://localhost:3000/en/sign-in`
2. Open browser DevTools → Network tab
3. Click "Continue with Azure AD"
4. Authenticate with Azure
5. **Check Network tab:**
   - You should see a POST request to `http://localhost:5010/authentication/authenticate`
   - No request to `/api/auth/login`
6. After successful authentication, you should be redirected to the dashboard

### 4. Verify Token Storage

Open browser DevTools → Application → Local Storage → `http://localhost:3000`:

```json
{
  "auth_session": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "...",
      "email": "...",
      "displayName": "..."
    },
    "expiresAt": "2024-01-01T13:00:00.000Z"
  }
}
```

### 5. Verify Subsequent Requests

Make a request to the dashboard:

1. Navigate to `/en/dashboard`
2. Check Network tab
3. Request to `http://localhost:5010/dashboard/stats` should include:
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

## Advantages

✅ **Simpler architecture** - No Next.js API route middleman
✅ **Fewer moving parts** - Direct communication reduces complexity
✅ **Standard SPA pattern** - Common approach for single-page applications
✅ **Easier debugging** - Fewer layers to troubleshoot

## Trade-offs

⚠️ **No httpOnly cookies** - Token accessible to JavaScript (XSS risk)
⚠️ **CORS required** - Backend must explicitly allow frontend origin
⚠️ **Client-side only** - No server-side session management

## Security Considerations

### XSS Protection

Since tokens are stored in localStorage, ensure:

1. **CSP (Content Security Policy)** is configured
2. **Sanitize all user input** to prevent XSS attacks
3. **Use HTTPS in production** to prevent man-in-the-middle attacks
4. **Regular security audits** of third-party dependencies

### CSRF Protection

Not needed because:
- No cookies are used (CSRF attacks target cookies)
- Authorization header is manually added by JavaScript
- CORS prevents unauthorized origins from making requests

## Migration Notes

### If You Want to Revert to the Proxy Pattern

1. Restore the `/api/auth/login` route
2. Update `SignInForm.tsx` to use `fetch("/api/auth/login")`
3. Re-enable httpOnly cookie management

### Optional: Keep Both Methods

You can support both direct backend calls AND the Next.js proxy:

- Keep `/api/auth/login` for SSR pages
- Use direct backend calls for client-side SPA mode

## Related Files

- **Modified:** `web/src/features/auth/components/SignInForm.tsx`
- **Unchanged:** `web/src/core/api/client.ts` (already supports direct backend calls)
- **Unchanged:** `web/src/core/auth/session.client.ts` (already manages localStorage)
- **Unchanged:** `src/TaskManagement.Api/DependencyInjection.cs` (CORS already configured)
- **Optional:** `web/src/app/api/auth/login/route.ts` (can be removed if not needed)

## Troubleshooting

### Issue: CORS Error

**Error:**
```
Access to fetch at 'http://localhost:5010/authentication/authenticate' from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Solution:**
Ensure backend CORS is configured to allow `http://localhost:3000`:

```csharp
policy.WithOrigins("http://localhost:3000")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
```

### Issue: Token Not Sent

**Error:** Backend returns 401 Unauthorized for protected routes.

**Solution:**
Check that `apiClient` is including the Authorization header:

```typescript
// Verify token is in localStorage
const session = localStorage.getItem("auth_session");
console.log("Session:", session);

// Verify Authorization header is sent
// In Network tab, check request headers for:
Authorization: Bearer <token>
```

### Issue: Token Expired

**Error:** Backend returns 401 after some time.

**Solution:**
Implement token refresh or prompt user to sign in again when `expiresAt` is reached.

## Next Steps

- [ ] Consider implementing token refresh logic
- [ ] Add automatic logout when token expires
- [ ] Consider re-enabling httpOnly cookies for enhanced security (revert to proxy pattern)
- [ ] Implement CSP headers in production
- [ ] Add error boundary for authentication errors


