"use client";

import { useEffect, useMemo } from "react";
import type { PropsWithChildren } from "react";
import { I18nextProvider } from "react-i18next";

import { getDirection, normalizeLocale, type SupportedLocale } from "@/core/routing/locales";
import { getI18nInstance } from "@/i18n/instance";

interface I18nProviderProps extends PropsWithChildren {
  locale: SupportedLocale;
}

export function I18nProvider({ locale, children }: I18nProviderProps) {
  const normalizedLocale = normalizeLocale(locale);
  const i18n = useMemo(() => getI18nInstance(normalizedLocale), [normalizedLocale]);

  useEffect(() => {
    void i18n.changeLanguage(normalizedLocale);
  }, [i18n, normalizedLocale]);

  useEffect(() => {
    if (typeof document === "undefined") {
      return;
    }

    document.documentElement.lang = normalizedLocale;
    document.documentElement.dir = getDirection(normalizedLocale);
  }, [normalizedLocale]);

  return <I18nextProvider i18n={i18n}>{children}</I18nextProvider>;
}

