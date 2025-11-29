"use client";

import { ArrowRightIcon, CloudIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import type { Route } from "next";
import { useRouter } from "next/navigation";
import { useEffect, useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

import { apiClient } from "@/core/api";
import { useAuth } from "@/core/auth/AuthProvider";
import type { AuthSession } from "@/core/auth/types";
import type { SupportedLocale } from "@/core/routing/locales";
import { useAzureAdLogin } from "@/features/auth/hooks/useAzureAdLogin";
import type { AuthenticationResponse } from "@/features/auth/types";
import { Button } from "@/ui/components/Button";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Input } from "@/ui/components/Input";
import { Label } from "@/ui/components/Label";
import { Spinner } from "@/ui/components/Spinner";
import { cn } from "@/ui/utils/cn";

const signInSchema = z.object({
  azureAdToken: z.string().min(10, { message: "validation:required" })
});

type SignInFormValues = z.infer<typeof signInSchema>;

interface SignInFormProps {
  locale: SupportedLocale;
  redirectTo?: string | undefined;
}

export function SignInForm({ locale, redirectTo }: SignInFormProps) {
  const { t } = useTranslation(["auth", "common", "validation"]);
  const router = useRouter();
  const { setSession } = useAuth();
  const { isConfigured: isAzureConfigured, isLoading: isAzureLoading, login: loginWithAzureAd, handleRedirect } = useAzureAdLogin();

  const form = useForm<SignInFormValues>({
    resolver: zodResolver(signInSchema),
    defaultValues: {
      azureAdToken: ""
    },
    mode: "onSubmit"
  });

  // Handle Azure AD redirect callback when returning from Azure AD
  useEffect(() => {
    let isMounted = true;

    const processRedirect = async () => {
      try {
        const result = await handleRedirect();
        if (result && isMounted) {
          const token = result.idToken ?? result.accessToken;
          if (token) {
            form.setValue("azureAdToken", token, { shouldDirty: false });
            await handleSubmit({ azureAdToken: token });
          }
        }
      } catch (error) {
        if (isMounted) {
          const { debugError } = await import("@/core/debug/logger");
          debugError("Error processing Azure AD redirect", error);
          form.setError("azureAdToken", {
            type: "server",
            message: "auth:signIn.azureAdSignInFailed"
          });
        }
      }
    };

    // Process redirect callback if present in URL
    processRedirect();

    return () => {
      isMounted = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [handleRedirect]);

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
        token: data.accessToken,
        user: {
          id: data.user.id,
          email: data.user.email,
          displayName: data.user.displayName,
          ...(data.user.role ? { role: data.user.role } : {})
        },
        expiresAt: new Date(Date.now() + data.expiresIn * 1000).toISOString()
      };

      setSession(session);
      const targetPath = (redirectTo ?? `/${locale}/dashboard`) as Route;
      router.push(targetPath);
      router.refresh();
    } catch (error) {
      form.setError("azureAdToken", {
        type: "server",
        message: t("auth:signIn.invalidToken")
      });
    }
  }

  async function handleAzureAdSignIn() {
    form.clearErrors("azureAdToken");

    try {
      // This will redirect to Azure AD - the redirect callback will handle the response
      await loginWithAzureAd();
      // Don't do anything after this - the page will redirect to Azure AD
    } catch (error) {
      const messageKey =
        (error as Error)?.message && (error as Error)?.message?.startsWith("auth:")
          ? (error as Error).message
          : "auth:signIn.azureAdSignInFailed";

      form.setError("azureAdToken", {
        type: "server",
        message: messageKey
      });
    }
  }

  const description = useMemo(() => t("auth:signIn.description"), [t]);
  const azureAdTokenErrorMessage = form.formState.errors.azureAdToken
    ? t(form.formState.errors.azureAdToken.message ?? "validation:required", {
        field: t("auth:signIn.azureAdToken")
      })
    : undefined;

  return (
    <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-6" noValidate>
      <header className="space-y-2 text-center">
        <h1 className="font-heading text-2xl text-foreground">{t("auth:signIn.title")}</h1>
        <p className="text-sm text-muted-foreground">{description}</p>
      </header>

      <div className="space-y-6">
        <div className="space-y-2">
          <Label htmlFor="azureAdToken">{t("auth:signIn.azureAdToken")}</Label>
          <Input
            id="azureAdToken"
            placeholder={t("auth:signIn.azureAdTokenHint")}
            autoComplete="off"
            {...form.register("azureAdToken")}
            aria-invalid={form.formState.errors.azureAdToken ? "true" : "false"}
            aria-describedby={form.formState.errors.azureAdToken ? "azureAdToken-error" : undefined}
          />
          <FormFieldError
            id="azureAdToken-error"
            {...(azureAdTokenErrorMessage ? { message: azureAdTokenErrorMessage } : {})}
          />
        </div>
      </div>

      <Button
        type="submit"
        className={cn("w-full")}
        disabled={isPending}
        icon={!isPending ? <ArrowRightIcon /> : undefined}
        iconPosition="right"
      >
        {isPending ? (
          <span className="flex items-center justify-center gap-2">
            <Spinner size="sm" />
            {t("common:actions.signIn")}
          </span>
        ) : (
          t("common:actions.signIn")
        )}
      </Button>

      <div className="space-y-2">
        <div className="text-center text-xs uppercase tracking-wide text-muted-foreground">
          {t("auth:signIn.or")}
        </div>
        <Button
          type="button"
          variant="outline"
          className="w-full"
          disabled={!isAzureConfigured || isAzureLoading || isPending}
          onClick={handleAzureAdSignIn}
          icon={!isAzureLoading ? <CloudIcon /> : undefined}
        >
          {isAzureLoading ? (
            <span className="flex items-center justify-center gap-2">
              <Spinner size="sm" />
              {t("auth:signIn.continueWithAzureAd")}
            </span>
          ) : (
            t("auth:signIn.continueWithAzureAd")
          )}
        </Button>
        {!isAzureConfigured ? (
          <p className="text-center text-xs text-muted-foreground">
            {t("auth:signIn.azureAdNotAvailable")}
          </p>
        ) : null}
      </div>
    </form>
  );
}

