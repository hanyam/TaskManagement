import { describe, expect, it } from "vitest";

import {
  DEFAULT_LOCALE,
  normalizeLocale,
  resolveLocaleFromAcceptLanguage,
  toLocalePath
} from "@/core/routing/locales";

describe("locales utilities", () => {
  it("normalizes supported locales", () => {
    expect(normalizeLocale("en")).toBe("en");
    expect(normalizeLocale("AR")).toBe("ar");
  });

  it("falls back to default locale for unsupported values", () => {
    expect(normalizeLocale("fr")).toBe(DEFAULT_LOCALE);
  });

  it("parses Accept-Language headers", () => {
    expect(resolveLocaleFromAcceptLanguage("ar-SA,ar;q=0.9,en;q=0.8")).toBe("ar");
    expect(resolveLocaleFromAcceptLanguage("fr-CA,fr;q=0.9,en;q=0.8")).toBe("en");
  });

  it("prefixes locale to paths", () => {
    expect(toLocalePath("en", "/tasks")).toBe("/en/tasks");
    expect(toLocalePath("ar", "dashboard")).toBe("/ar/dashboard");
  });
});

