/**
 * Utility functions for handling token refresh and re-authentication
 */

// Flag to prevent multiple simultaneous re-authentication attempts
let isReAuthenticating = false;

/**
 * Attempts to silently refresh the Azure AD token and authenticate with backend
 * @returns The new backend JWT token if successful, null otherwise
 */
export async function attemptSilentTokenRefresh(): Promise<string | null> {
  if (isReAuthenticating) {
    return null; // Already attempting re-auth
  }

  if (typeof window === "undefined") {
    return null; // Server-side
  }

  try {
    isReAuthenticating = true;

    const { getEnvConfig } = await import("@/core/config/env");
    const { PublicClientApplication } = await import("@azure/msal-browser");

    const env = getEnvConfig();
    const azureAdConfig = env.azureAd;

    if (!azureAdConfig) {
      return null;
    }

    const redirectUri = azureAdConfig.redirectUri ?? `${window.location.origin}/auth/callback`;
    const scopes = ["openid", "profile", "email", ...(azureAdConfig.scopes || [])];

    const msalInstance = new PublicClientApplication({
      auth: {
        clientId: azureAdConfig.clientId,
        authority: `https://login.microsoftonline.com/${azureAdConfig.tenantId}`,
        redirectUri: redirectUri,
        knownAuthorities: [`login.microsoftonline.com`]
      },
      cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: false
      }
    });

    await msalInstance.initialize();
    const accounts = msalInstance.getAllAccounts();

    if (accounts.length === 0) {
      return null; // No accounts available for silent refresh
    }

    // Try to acquire token silently
    const result = await msalInstance.acquireTokenSilent({
      account: accounts[0],
      scopes: Array.from(new Set(scopes))
    });

    if (!result?.idToken && !result?.accessToken) {
      return null;
    }

    const azureToken = result.idToken ?? result.accessToken;

    // Authenticate with backend using the new Azure token
    const { getEnvConfig: getConfig } = await import("@/core/config/env");
    const config = getConfig();
    const response = await fetch(`${config.apiBaseUrl}/authentication/authenticate`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify({ azureAdToken: azureToken }),
      credentials: "include"
    });

    if (!response.ok) {
      return null;
    }

    const json = await response.json();
    const { parseEnvelope } = await import("@/core/api/response");
    const envelope = parseEnvelope<{
      accessToken: string;
      expiresIn: number;
      user: { id: string; email: string; displayName: string; role?: string };
    }>(json);

    if (!envelope.success || !envelope.data) {
      return null;
    }

    const authData = envelope.data;

    // Store the new session
    const { setClientAuthSession } = await import("@/core/auth/session.client");
    setClientAuthSession(
      authData.accessToken,
      {
        id: authData.user.id,
        email: authData.user.email,
        displayName: authData.user.displayName,
        ...(authData.user.role ? { role: authData.user.role } : {})
      },
      {
        expiresAt: new Date(Date.now() + authData.expiresIn * 1000).toISOString()
      }
    );

    return authData.accessToken;
  } catch (error) {
    // Silent refresh failed - will need interactive login
    console.warn("Silent token refresh failed:", error);
    return null;
  } finally {
    isReAuthenticating = false;
  }
}

