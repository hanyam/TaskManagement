import {
  ClipboardDocumentListIcon,
  Squares2X2Icon,
  UserGroupIcon
} from "@heroicons/react/24/outline";
import type { ComponentType, SVGProps } from "react";

export interface NavigationItem {
  id: string;
  labelKey: string;
  icon: ComponentType<SVGProps<SVGSVGElement>>;
  href: string;
  roles?: Array<"Employee" | "Manager" | "Admin">;
  exact?: boolean;
}

export const mainNavigation: NavigationItem[] = [
  {
    id: "dashboard",
    labelKey: "navigation:appShell.dashboard",
    icon: Squares2X2Icon,
    href: "/dashboard"
  },
  {
    id: "tasks",
    labelKey: "navigation:appShell.tasks",
    icon: ClipboardDocumentListIcon,
    href: "/tasks"
  },
  {
    id: "my-assignments",
    labelKey: "navigation:appShell.myAssignments",
    icon: UserGroupIcon,
    href: "/tasks?filter=assigned",
    roles: ["Employee", "Manager", "Admin"]
  }
];

