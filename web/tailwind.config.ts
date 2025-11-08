import type { Config } from "tailwindcss";
import forms from "@tailwindcss/forms";
import animate from "tailwindcss-animate";

const config: Config = {
  content: ["./src/**/*.{ts,tsx,mdx}"],
  darkMode: ["class"],
  theme: {
    container: {
      center: true,
      padding: {
        DEFAULT: "1.5rem",
        xl: "2rem"
      }
    },
    extend: {
      colors: {
        background: "hsl(var(--color-background) / <alpha-value>)",
        foreground: "hsl(var(--color-foreground) / <alpha-value>)",
        muted: "hsl(var(--color-muted) / <alpha-value>)",
        "muted-foreground": "hsl(var(--color-muted-foreground) / <alpha-value>)",
        primary: "hsl(var(--color-primary) / <alpha-value>)",
        "primary-foreground": "hsl(var(--color-primary-foreground) / <alpha-value>)",
        secondary: "hsl(var(--color-secondary) / <alpha-value>)",
        "secondary-foreground": "hsl(var(--color-secondary-foreground) / <alpha-value>)",
        accent: "hsl(var(--color-accent) / <alpha-value>)",
        "accent-foreground": "hsl(var(--color-accent-foreground) / <alpha-value>)",
        destructive: "hsl(var(--color-destructive) / <alpha-value>)",
        "destructive-foreground": "hsl(var(--color-destructive-foreground) / <alpha-value>)",
        border: "hsl(var(--color-border) / <alpha-value>)",
        input: "hsl(var(--color-input) / <alpha-value>)",
        ring: "hsl(var(--color-ring) / <alpha-value>)",
        info: "hsl(var(--color-info) / <alpha-value>)",
        success: "hsl(var(--color-success) / <alpha-value>)",
        warning: "hsl(var(--color-warning) / <alpha-value>)"
      },
      borderRadius: {
        xl: "var(--radius-xl)",
        lg: "var(--radius-lg)",
        md: "var(--radius-md)",
        sm: "var(--radius-sm)"
      },
      fontFamily: {
        sans: ["var(--font-sans)", "system-ui", "sans-serif"],
        heading: ["var(--font-heading)", "system-ui", "sans-serif"]
      },
      boxShadow: {
        card: "var(--shadow-card)",
        focus: "0 0 0 3px hsl(var(--color-ring) / 0.45)"
      },
      keyframes: {
        "fade-in": {
          from: { opacity: "0", transform: "translateY(4px)" },
          to: { opacity: "1", transform: "translateY(0)" }
        },
        "fade-out": {
          from: { opacity: "1", transform: "translateY(0)" },
          to: { opacity: "0", transform: "translateY(4px)" }
        }
      },
      animation: {
        "fade-in": "fade-in 0.18s ease-out",
        "fade-out": "fade-out 0.18s ease-in forwards"
      }
    }
  },
  plugins: [forms, animate]
};

export default config;
