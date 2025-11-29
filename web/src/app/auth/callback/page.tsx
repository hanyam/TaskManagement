"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

import { setClientAuthSession } from "@/core/auth/session.client";
import { debugError } from "@/core/debug/logger";
import { useAzureAdLogin } from "@/features/auth/hooks/useAzureAdLogin";
import type { AuthSession } from "@/core/auth/types";
import { Spinner } from "@/ui/components/Spinner";

// Force dynamic rendering - this page requires client-side context and cannot be statically generated
export const dynamic = "force-dynamic";

export default function AuthCallbackPage() {
  const router = useRouter();
  const { handleRedirect } = useAzureAdLogin();
  const { t } = useTranslation();
  const [isProcessing, setIsProcessing] = useState(true);

  useEffect(() => {
    const processCallback = async () => {
      try {
        // Handle the redirect response from Azure AD
        const result = await handleRedirect();

        if (result) {
          const token = result.idToken ?? result.accessToken;

          if (token) {
            // Send token to backend for authentication
            const response = await fetch("/api/auth/login", {
              method: "POST",
              headers: {
                "Content-Type": "application/json"
              },
              body: JSON.stringify({ azureAdToken: token }),
              credentials: "include"
            });

            if (response.ok) {
              const data = await response.json();
              
              if (data.success && data.data) {
                // Create session from backend response
                const session: AuthSession = {
                  token: data.data.token,
                  user: {
                    id: data.data.user.id,
                    email: data.data.user.email,
                    displayName: data.data.user.displayName,
                    ...(data.data.user.role ? { role: data.data.user.role } : {})
                  },
                  expiresAt: new Date(Date.now() + (data.data.expiresIn * 1000)).toISOString()
                };

                // Store session in localStorage and cookies (client-side)
                setClientAuthSession(
                  session.token,
                  session.user,
                  session.expiresAt ? { expiresAt: session.expiresAt } : {}
                );

                // Get locale from cookie or default to 'en'
                const locale = document.cookie
                  .split("; ")
                  .find((row) => row.startsWith("NEXT_LOCALE="))
                  ?.split("=")[1] || "en";

                // Redirect to dashboard
                router.push(`/${locale}/dashboard`);
                router.refresh();
                return;
              }
            }

            // If authentication failed, redirect to sign-in
            throw new Error("Authentication failed");
          }
        }

        // No result from redirect - redirect to sign-in
        throw new Error("No authentication result");
      } catch (error) {
        debugError("Error processing Azure AD callback", error);
        setIsProcessing(false);
        
        // Get locale from cookie or default to 'en'
        const locale = document.cookie
          .split("; ")
          .find((row) => row.startsWith("NEXT_LOCALE="))
          ?.split("=")[1] || "en";

        // Redirect to sign-in with error
        router.push(`/${locale}/sign-in?error=auth_failed`);
      }
    };

    processCallback();
  }, [handleRedirect, router]);

  if (!isProcessing) {
    return null; // Will redirect, so don't render anything
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="flex flex-col items-center gap-4">
        <Spinner size="lg" />
        <p className="text-sm text-muted-foreground">{t("auth:callback.completing")}</p>
      </div>
    </div>
  );
}

