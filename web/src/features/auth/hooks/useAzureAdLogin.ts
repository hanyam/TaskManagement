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

    const msalInstance = new PublicClientApplication({
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

    // Initialize the MSAL instance
    msalInstance.initialize().catch(console.error);

    return msalInstance;
  }, [azureAdConfig]);

  const login = useCallback(async () => {
    if (!client) {
      console.error("Azure AD client not initialized");
      return undefined;
    }

    console.log("Azure AD config:", { 
      clientId: azureAdConfig?.clientId,
      tenantId: azureAdConfig?.tenantId,
      redirectUri: azureAdConfig?.redirectUri,
      scopes 
    });

    setIsLoading(true);

    try {
      // Ensure MSAL is initialized before proceeding
      await client.initialize();
      
      const accounts = client.getAllAccounts();
      console.log("Existing accounts:", accounts.length);

      if (accounts.length > 0) {
        try {
          const silentResult = await client.acquireTokenSilent({
            account: accounts[0],
            scopes
          });

          console.log("Silent token acquisition successful");
          return silentResult;
        } catch (error) {
          console.log("Silent token acquisition failed:", error);
          if ((error as { errorCode?: string }).errorCode !== "interaction_required") {
            throw error;
          }
        }
      }

      const request: PopupRequest = {
        scopes,
        prompt: "select_account"
      };

      console.log("Starting popup authentication with request:", request);
      const result = await client.acquireTokenPopup(request);
      console.log("Popup authentication successful:", result);
      return result;
    } catch (error) {
      console.error("Azure AD login error:", error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  }, [client, scopes, azureAdConfig]);

  return {
    isConfigured: Boolean(client),
    isLoading,
    login
  };
}


