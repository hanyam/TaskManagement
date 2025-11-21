"use client";

import { XMarkIcon } from "@heroicons/react/24/outline";
import { useTranslation } from "react-i18next";

import { Button } from "@/ui/components/Button";

export default function SignedOutPage() {
  const { t } = useTranslation(["navigation"]);

  function handleCloseWindow() {
    if (typeof window !== "undefined") {
      window.close();
    }
  }

  const canClose = typeof window !== "undefined" && window.opener;

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-background via-background to-primary/10 px-4 py-16">
      <div className="w-full max-w-lg rounded-xl bg-background/80 p-8 shadow-card backdrop-blur">
        <div className="flex flex-col items-center gap-6 text-center">
          <div className="flex size-16 items-center justify-center rounded-full bg-primary/10">
            <XMarkIcon className="size-8 text-primary" />
          </div>
          <div className="space-y-2">
            <h1 className="text-2xl font-semibold text-foreground">
              {t("navigation:signedOut.title")}
            </h1>
            <p className="text-sm text-muted-foreground">
              {t("navigation:signedOut.message")}
            </p>
          </div>
          {canClose && (
            <Button variant="outline" onClick={handleCloseWindow} icon={<XMarkIcon />}>
              {t("navigation:signedOut.closeWindow")}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

