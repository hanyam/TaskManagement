import type { ReactNode } from "react";

import { AppShell } from "@/ui/layout/AppShell";

export default function AppLayout({ children }: { children: ReactNode }) {
  return <AppShell>{children}</AppShell>;
}

