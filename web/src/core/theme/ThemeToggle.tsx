"use client";

import { MoonIcon, SunIcon } from "@heroicons/react/24/outline";

import { useTheme } from "@/core/theme/ThemeProvider";
import { Button } from "@/ui/components/Button";

export function ThemeToggle() {
  const { resolvedTheme, toggleTheme } = useTheme();
  const isDark = resolvedTheme === "dark";

  return (
    <Button
      type="button"
      variant="ghost"
      size="icon"
      className="relative"
      onClick={toggleTheme}
      aria-label={isDark ? "Switch to light mode" : "Switch to dark mode"}
    >
      <SunIcon className={`size-5 transition-all ${isDark ? "scale-0 opacity-0" : "scale-100 opacity-100"}`} />
      <MoonIcon
        className={`absolute size-5 transition-all ${isDark ? "scale-100 opacity-100" : "scale-0 opacity-0"}`}
      />
    </Button>
  );
}

