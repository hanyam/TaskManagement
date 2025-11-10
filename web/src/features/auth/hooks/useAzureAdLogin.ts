import { PublicClientApplication, type AuthenticationResult, type PopupRequest } from "@azure/msal-browser";
import { useCallback, useMemo, useState } from "react";

import { getEnvConfig } from "@/core/config/env";

interface UseAzureAdLoginResult {
  isConfigured: boolean;
  isLoading: boolean;
  login: () => Promise<AuthenticationResult | undefined>;
}

export function useAzureAdLogin(): UseAzureAdLoginResult {
  const [isLoading, setIsLoading] = useState(false);
  const env = getEnvConfig();
  const azureAdConfig = env.azureAd;

  const scopes = useMemo(() => {
    if (!azureAdConfig) {
      return [];
    }

    const defaults = ["openid", "profile", "email"];
    const combined = [...defaults, ...azureAdConfig.scopes];

    return Array.from(new Set(combined));
  }, [azureAdConfig]);

  const client = useMemo(() => {
    if (typeof window === "undefined" || !azureAdConfig) {
      return undefined;
    }

    return new PublicClientApplication({
      auth: {
        clientId: azureAdConfig.clientId,
        authority: `https://login.microsoftonline.com/${azureAdConfig.tenantId}`,
        redirectUri: azureAdConfig.redirectUri ?? window.location.origin
      },
      cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: typeof window !== "undefined" && window.navigator.userAgent.includes("MSIE")
      }
    });
  }, [azureAdConfig]);

  const login = useCallback(async () => {
    if (!client) {
      return undefined;
    }

    setIsLoading(true);

    try {
      const accounts = client.getAllAccounts();

      if (accounts.length > 0) {
        try {
          const silentResult = await client.acquireTokenSilent({
            account: accounts[0],
            scopes
          });

          return silentResult;
        } catch (error) {
          if ((error as { errorCode?: string }).errorCode !== "interaction_required") {
            throw error;
          }
        }
      }

      const request: PopupRequest = {
        scopes,
        prompt: "select_account"
      };

      return await client.acquireTokenPopup(request);
    } finally {
      setIsLoading(false);
    }
  }, [client, scopes]);

  return {
    isConfigured: Boolean(client),
    isLoading,
    login
  };
}


