import type { Metadata, Viewport } from "next";
import { Inter, Cairo } from "next/font/google";
import "./globals.css";

const inter = Inter({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-sans"
});

const cairo = Cairo({
  subsets: ["arabic"],
  weight: ["400", "600", "700"],
  display: "swap",
  variable: "--font-arabic"
});

export const metadata: Metadata = {
  title: {
    default: "Task Management Console",
    template: "%s | Task Management Console"
  },
  description:
    "Task Management portal powered by the Task Management API with role-based productivity workflows.",
  metadataBase: new URL("https://localhost"),
  alternates: {
    canonical: "/"
  }
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#fafcff" },
    { media: "(prefers-color-scheme: dark)", color: "#0f172a" }
  ]
};

export default function RootLayout({
  children
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${inter.variable} ${cairo.variable} bg-background font-sans text-foreground`}>
        {children}
      </body>
    </html>
  );
}
