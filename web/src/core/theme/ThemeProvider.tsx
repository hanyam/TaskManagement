"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";

type ThemeMode = "light" | "dark" | "system";

interface ThemeContextValue {
  theme: ThemeMode;
  resolvedTheme: "light" | "dark";
  setTheme: (mode: ThemeMode) => void;
  toggleTheme: () => void;
}

const THEME_STORAGE_KEY = "tm.theme";
const isBrowser = typeof window !== "undefined";

export const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

function resolveSystemPreference(): "light" | "dark" {
  if (!isBrowser) {
    return "light";
  }

  return window.matchMedia?.("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

function applyThemeClass(theme: "light" | "dark") {
  if (!isBrowser) {
    return;
  }

  const root = document.documentElement;
  root.classList.remove("light", "dark");
  root.classList.add(theme);
}

export function ThemeProvider({ children }: PropsWithChildren) {
  const [theme, setThemeState] = useState<ThemeMode>(() => {
    if (!isBrowser) {
      return "light";
    }

    const stored = window.localStorage.getItem(THEME_STORAGE_KEY) as ThemeMode | null;
    return stored ?? "system";
  });
  const [resolvedTheme, setResolvedTheme] = useState<"light" | "dark">(() =>
    theme === "system" ? resolveSystemPreference() : theme
  );

  useEffect(() => {
    if (!isBrowser) {
      return;
    }

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handleChange = () => {
      if (theme === "system") {
        const nextTheme = mediaQuery.matches ? "dark" : "light";
        setResolvedTheme(nextTheme);
        applyThemeClass(nextTheme);
      }
    };

    mediaQuery.addEventListener("change", handleChange);
    return () => mediaQuery.removeEventListener("change", handleChange);
  }, [theme]);

  useEffect(() => {
    const nextResolved = theme === "system" ? resolveSystemPreference() : theme;
    setResolvedTheme(nextResolved);
    applyThemeClass(nextResolved);
  }, [theme]);

  const setTheme = useCallback((mode: ThemeMode | ((previous: ThemeMode) => ThemeMode)) => {
    setThemeState((previous) => {
      const next = typeof mode === "function" ? mode(previous) : mode;
      if (isBrowser) {
        window.localStorage.setItem(THEME_STORAGE_KEY, next);
      }

      return next;
    });
  }, []);

  const toggleTheme = useCallback(() => {
    setTheme((previousTheme) => (previousTheme === "dark" ? "light" : "dark"));
  }, [setTheme]);

  const value = useMemo<ThemeContextValue>(
    () => ({
      theme,
      resolvedTheme,
      setTheme,
      toggleTheme
    }),
    [theme, resolvedTheme, setTheme, toggleTheme]
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error("useTheme must be used within a ThemeProvider");
  }

  return context;
}

