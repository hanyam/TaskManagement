"use client";

import { debugLog } from "@/core/debug/logger";
import { useAuth } from "@/core/auth/AuthProvider";
import { TaskDetailsViewEmployee } from "@/features/tasks/components/views/TaskDetailsViewEmployee";
import { TaskDetailsViewManager } from "@/features/tasks/components/views/TaskDetailsViewManager";
import { TaskDetailsViewAdmin } from "@/features/tasks/components/views/TaskDetailsViewAdmin";
import { Spinner } from "@/ui/components/Spinner";

interface TaskDetailsViewProps {
  taskId: string;
}

export function TaskDetailsView({ taskId }: TaskDetailsViewProps) {
  const { session } = useAuth();
  const userRole = session?.user?.role?.toLowerCase() || "";

  // Debug: Log which view is being rendered (works for all roles)
  debugLog("TaskDetailsView routing", {
    taskId,
    userRole,
    hasSession: !!session,
    userId: session?.user?.id
  });

  // Show loading while determining role
  if (!session?.user?.role) {
    debugLog("TaskDetailsView: No role, showing spinner");
    return (
      <div className="flex h-64 items-center justify-center">
        <Spinner size="lg" />
      </div>
    );
  }

  // Route to role-specific view (debug logging works for all roles)
  if (userRole === "admin") {
    debugLog("TaskDetailsView: Rendering Admin view", { taskId, userRole });
    return <TaskDetailsViewAdmin taskId={taskId} />;
  }

  if (userRole === "manager") {
    debugLog("TaskDetailsView: Rendering Manager view", { taskId, userRole });
    return <TaskDetailsViewManager taskId={taskId} />;
  }

  // Default to Employee view
  debugLog("TaskDetailsView: Rendering Employee view", { taskId, userRole });
  return <TaskDetailsViewEmployee taskId={taskId} />;
}
