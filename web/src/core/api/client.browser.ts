import { createApiClient } from "@/core/api/client.shared";

async function resolveAuthToken(): Promise<string | undefined> {
  const { getClientAuthToken } = await import("@/core/auth/session.client");
  return getClientAuthToken();
}

export const apiClient = createApiClient(resolveAuthToken);


