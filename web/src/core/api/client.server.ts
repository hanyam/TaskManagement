import { createApiClient } from "@/core/api/client.shared";

async function resolveAuthToken(): Promise<string | undefined> {
  const { getServerAuthToken } = await import("@/core/auth/session.server");
  return getServerAuthToken();
}

export const apiClient = createApiClient(resolveAuthToken);


