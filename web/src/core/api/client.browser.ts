import { createApiClient } from "@/core/api/client.shared";
import { attemptSilentTokenRefresh } from "@/core/auth/tokenRefresh";

async function resolveAuthToken(): Promise<string | undefined> {
  const { getClientSession, isTokenExpired, clearClientAuthSession } = await import("@/core/auth/session.client");

  // Get full session (validates expiration)
  let session = getClientSession();

  if (!session) {
    return undefined;
  }

  // Check expiration right before sending request (most up-to-date check)
  if (isTokenExpired(session.token, session.expiresAt)) {
    // Token is expired - try to refresh silently
    const refreshedToken = await attemptSilentTokenRefresh();

    if (refreshedToken) {
      // Successfully refreshed - get the new session
      const { getClientSession: getNewSession } = await import("@/core/auth/session.client");
      const newSession = getNewSession();
      return newSession?.token;
    }

    // Silent refresh failed - clear expired session
    clearClientAuthSession();

    // Don't redirect here - let the 401 response handler do it
    // This prevents interrupting the user's workflow unnecessarily
    return undefined;
  }

  return session.token;
}

export const apiClient = createApiClient(resolveAuthToken);
export { attemptSilentTokenRefresh };


