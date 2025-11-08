const SUPPORTED_LOCALES = ["en", "ar"] as const;

export type SupportedLocale = (typeof SUPPORTED_LOCALES)[number];

export const DEFAULT_LOCALE: SupportedLocale = "en";
export const RTL_LOCALES: SupportedLocale[] = ["ar"];
export const LOCALE_COOKIE_KEY = "tm.locale";

export function isSupportedLocale(locale?: string | null): locale is SupportedLocale {
  if (!locale) {
    return false;
  }

  return (SUPPORTED_LOCALES as readonly string[]).includes(locale.toLowerCase());
}

export function normalizeLocale(locale?: string | null): SupportedLocale {
  if (isSupportedLocale(locale)) {
    return locale.toLowerCase() as SupportedLocale;
  }

  return DEFAULT_LOCALE;
}

export function resolveLocaleFromAcceptLanguage(headerValue: string | null): SupportedLocale | undefined {
  if (!headerValue) {
    return undefined;
  }

  const locales = headerValue
    .split(",")
    .map((part) => part.split(";")[0]?.trim().toLowerCase())
    .filter(Boolean);

  return locales.find((candidate) => isSupportedLocale(candidate)) as SupportedLocale | undefined;
}

export function getDirection(locale: SupportedLocale): "rtl" | "ltr" {
  return RTL_LOCALES.includes(locale) ? "rtl" : "ltr";
}

export function toLocalePath(locale: SupportedLocale, path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  return `/${locale}${normalizedPath === "/" ? "" : normalizedPath}`;
}

