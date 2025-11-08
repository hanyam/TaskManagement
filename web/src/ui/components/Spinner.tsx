"use client";

import { cn } from "@/ui/utils/cn";

interface SpinnerProps {
  size?: "sm" | "md" | "lg";
  className?: string;
}

const SIZE_MAP: Record<NonNullable<SpinnerProps["size"]>, string> = {
  sm: "h-4 w-4 border-2",
  md: "h-5 w-5 border-2",
  lg: "h-10 w-10 border-4"
};

export function Spinner({ size = "md", className }: SpinnerProps) {
  return (
    <span
      className={cn(
        "inline-flex animate-spin rounded-full border-border border-t-primary",
        SIZE_MAP[size] ?? SIZE_MAP.md,
        className
      )}
      role="status"
      aria-live="polite"
      aria-label="Loading"
    />
  );
}

