"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import type { Route } from "next";
import { useRouter } from "next/navigation";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

import { useAuth } from "@/core/auth/AuthProvider";
import type { AuthSession } from "@/core/auth/types";
import type { SupportedLocale } from "@/core/routing/locales";
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

interface LoginResponse {
  success: boolean;
  data: {
    user: {
      id: string;
      email: string;
      firstName: string;
      lastName: string;
      displayName: string;
      isActive: boolean;
      createdAt: string;
      lastLoginAt?: string | null;
      role?: string | null;
    };
    token: string;
    expiresIn: number;
  };
  errors?: Array<{ code: string; message: string; field?: string | null }>;
  message?: string;
}

export function SignInForm({ locale, redirectTo }: SignInFormProps) {
  const { t } = useTranslation(["auth", "common", "validation"]);
  const router = useRouter();
  const { setSession } = useAuth();

  const form = useForm<SignInFormValues>({
    resolver: zodResolver(signInSchema),
    defaultValues: {
      azureAdToken: ""
    },
    mode: "onSubmit"
  });

  const { mutateAsync, isPending } = useMutation({
    mutationFn: async (values: SignInFormValues) => {
      const response = await fetch("/api/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(values),
        credentials: "include",
        cache: "no-store"
      });

      const payload = (await response.json()) as LoginResponse;

      if (!response.ok || !payload.success) {
        const error = new Error(payload.message ?? "auth:signIn.invalidToken");
        (error as Error & { details?: LoginResponse["errors"] }).details = payload.errors;
        throw error;
      }

      return payload.data;
    }
  });

  async function handleSubmit(values: SignInFormValues) {
    try {
      const data = await mutateAsync(values);

      const session: AuthSession = {
        token: data.token,
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
      const err = error as Error & { details?: LoginResponse["errors"] };
      if (err.details?.length) {
        err.details.forEach((detail) => {
          if (detail.field) {
            const normalizedField = `${detail.field.charAt(0).toLowerCase()}${detail.field.slice(1)}`;
            if (normalizedField in form.getValues()) {
              form.setError(normalizedField as keyof SignInFormValues, {
                type: "server",
                message: detail.message
              });
              return;
            }
          }
        });
      } else {
        form.setError("azureAdToken", {
          type: "server",
          message: t("auth:signIn.invalidToken")
        });
      }
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

      <div className="space-y-4">
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

  <Button type="submit" className={cn("w-full")} disabled={isPending}>
        {isPending ? (
          <span className="flex items-center justify-center gap-2">
            <Spinner size="sm" />
            {t("common:actions.signIn")}
          </span>
        ) : (
          t("common:actions.signIn")
        )}
      </Button>
    </form>
  );
}

