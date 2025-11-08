"use client";

import { useTranslation } from "react-i18next";

import { normalizeLocale, type SupportedLocale } from "@/core/routing/locales";

export function useCurrentLocale(): SupportedLocale {
  const { i18n } = useTranslation();
  return normalizeLocale(i18n.language) as SupportedLocale;
}

