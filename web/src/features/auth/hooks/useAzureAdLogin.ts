import { PublicClientApplication, type AuthenticationResult, type RedirectRequest } from "@azure/msal-browser";
import { useCallback, useEffect, useMemo, useState } from "react";

import { getEnvConfig } from "@/core/config/env";
import { debugLog, debugError } from "@/core/debug/logger";

interface UseAzureAdLoginResult {
  isConfigured: boolean;
  isLoading: boolean;
  login: () => Promise<void>;
  handleRedirect: () => Promise<AuthenticationResult | null>;
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

    // Use /auth/callback as the redirect URI (must match Azure AD app registration)
    const redirectUri = azureAdConfig.redirectUri ?? `${window.location.origin}/auth/callback`;
    
    const msalInstance = new PublicClientApplication({
      auth: {
        clientId: azureAdConfig.clientId,
        authority: `https://login.microsoftonline.com/${azureAdConfig.tenantId}`,
        redirectUri: redirectUri,
        knownAuthorities: [`login.microsoftonline.com`]
      },
      cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: typeof window !== "undefined" && window.navigator.userAgent.includes("MSIE")
      }
    });

    // Initialize the MSAL instance
    msalInstance.initialize().catch((error) => {
      debugError("MSAL initialization failed", error);
    });

    return msalInstance;
  }, [azureAdConfig]);

  // Handle redirect callback on page load
  const handleRedirect = useCallback(async (): Promise<AuthenticationResult | null> => {
    if (!client) {
      return null;
    }

    try {
      await client.initialize();
      const response = await client.handleRedirectPromise();
      return response;
    } catch (error) {
      debugError("Error handling redirect", error);
      return null;
    }
  }, [client]);

  // Check for redirect response on mount
  useEffect(() => {
    if (!client) {
      return;
    }

    handleRedirect().catch((error) => {
      debugError("Redirect handler failed", error);
    });
  }, [client, handleRedirect]);

  const login = useCallback(async () => {
    if (!client) {
      debugError("Azure AD client not initialized");
      return;
    }

    debugLog("Azure AD config", { 
      clientId: azureAdConfig?.clientId,
      tenantId: azureAdConfig?.tenantId,
      redirectUri: azureAdConfig?.redirectUri ?? window.location.origin,
      scopes 
    });

    setIsLoading(true);

    try {
      // Ensure MSAL is initialized before proceeding
      await client.initialize();
      
      const accounts = client.getAllAccounts();
      debugLog("Existing accounts", { count: accounts.length });

      // Try silent token acquisition first
      if (accounts.length > 0) {
        try {
          const silentResult = await client.acquireTokenSilent({
            account: accounts[0],
            scopes
          });

          debugLog("Silent token acquisition successful");
          // Return the token via a custom event or callback
          window.dispatchEvent(new CustomEvent("azureAdTokenAcquired", { detail: silentResult }));
          return;
        } catch (error) {
          debugLog("Silent token acquisition failed", error);
          const msalError = error as { errorCode?: string };
          if (msalError.errorCode !== "interaction_required" && msalError.errorCode !== "consent_required") {
            throw error;
          }
          // Fall through to redirect
        }
      }

      // Use redirect flow instead of popup
      const request: RedirectRequest = {
        scopes,
        prompt: "select_account"
      };

      debugLog("Starting redirect authentication", { request });
      await client.acquireTokenRedirect(request);
      // The redirect will navigate away, so we don't return here
    } catch (error) {
      debugError("Azure AD login error", error);
      setIsLoading(false);
      throw error;
    }
  }, [client, scopes, azureAdConfig]);

  return {
    isConfigured: Boolean(client),
    isLoading,
    login,
    handleRedirect
  };
}


