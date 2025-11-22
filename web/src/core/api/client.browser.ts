import { createApiClient } from "@/core/api/client.shared";

async function resolveAuthToken(): Promise<string | undefined> {
  const { getClientSession, isTokenExpired } = await import("@/core/auth/session.client");
  
  // Get full session (validates expiration)
  const session = getClientSession();
  if (!session) {
    return undefined;
  }

  // Double-check expiration (defensive)
  if (isTokenExpired(session.token, session.expiresAt)) {
    return undefined;
  }

  return session.token;
}

export const apiClient = createApiClient(resolveAuthToken);


