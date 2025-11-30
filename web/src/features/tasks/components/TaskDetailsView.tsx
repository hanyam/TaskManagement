"use client";

import { useEffect, useRef } from "react";
import { debugLog } from "@/core/debug/logger";
import { useAuth } from "@/core/auth/AuthProvider";
import { useTaskDetailsQuery } from "@/features/tasks/api/queries";
import { TaskDetailsViewEmployee } from "@/features/tasks/components/views/TaskDetailsViewEmployee";
import { TaskDetailsViewManager } from "@/features/tasks/components/views/TaskDetailsViewManager";
import { TaskDetailsViewAdmin } from "@/features/tasks/components/views/TaskDetailsViewAdmin";
import { Spinner } from "@/ui/components/Spinner";

interface TaskDetailsViewProps {
  taskId: string;
}

export function TaskDetailsView({ taskId }: TaskDetailsViewProps) {
  const { session } = useAuth();
  const currentUserId = session?.user?.id || "";
  const userRole = session?.user?.role?.toLowerCase() || "";

  // Fetch task data to determine relationship
  const { data: response, isLoading, error } = useTaskDetailsQuery(taskId, Boolean(taskId));
  const task = response?.data;

  // Track last logged state to avoid duplicate logs
  const lastLoggedStateRef = useRef<string>("");
  const lastViewRef = useRef<string>("");

  // Determine which view to render based on backend-provided isManager property
  let viewToRender: "loading" | "error" | "admin" | "manager" | "employee" = "loading";
  
  if (isLoading || !session?.user?.id) {
    viewToRender = "loading";
  } else if (error || !task) {
    viewToRender = "error";
  } else if (userRole === "admin") {
    // Admin users always see Admin view (regardless of relationship)
    viewToRender = "admin";
  } else if (task.isManager) {
    // Backend indicates current user is the creator (manager) of this task
    viewToRender = "manager";
  } else {
    // Backend indicates current user is NOT the creator, so they must be assigned (employee view)
    // Note: Backend access control ensures user has access (either creator or assigned)
    viewToRender = "employee";
  }

  // Debug: Log routing decision only when meaningful state changes (skip isLoading transitions)
  useEffect(() => {
    // Only log when we have task data (not during loading)
    if (!task && isLoading) {
      return; // Skip logging during initial load
    }
    
    const stateKey = `${taskId}-${!!task}-${task?.isManager}-${userRole}`;
    if (lastLoggedStateRef.current !== stateKey) {
      lastLoggedStateRef.current = stateKey;
      debugLog("TaskDetailsView routing", {
        taskId,
        userRole,
        currentUserId,
        hasSession: !!session,
        isLoading,
        hasTask: !!task,
        isManager: task?.isManager, // Backend-provided property determines view
        viewToRender: viewToRender
      });
    }
  }, [taskId, userRole, currentUserId, session, isLoading, task, task?.isManager, viewToRender]);

  // Log view decision only when it changes
  useEffect(() => {
    if (lastViewRef.current !== viewToRender) {
      lastViewRef.current = viewToRender;
      
      switch (viewToRender) {
        case "loading":
          debugLog("TaskDetailsView: Loading task data or waiting for session");
          break;
        case "error":
          debugLog("TaskDetailsView: Error loading task or task not found", { error, hasTask: !!task });
          break;
        case "admin":
          debugLog("TaskDetailsView: Rendering Admin view (admin role)", { taskId, userRole });
          break;
        case "manager":
          debugLog("TaskDetailsView: Rendering Manager view (isManager=true from backend)", {
            taskId,
            isManager: task?.isManager
          });
          break;
        case "employee":
          debugLog("TaskDetailsView: Rendering Employee view (isManager=false from backend)", {
            taskId,
            isManager: task?.isManager
          });
          break;
      }
    }
  }, [viewToRender, taskId, userRole, currentUserId, error, task]);

  // Show loading while fetching task data or if no session
  if (viewToRender === "loading") {
    return (
      <div className="flex h-64 items-center justify-center">
        <Spinner size="lg" />
      </div>
    );
  }

  // Handle error state
  if (viewToRender === "error") {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-center">
          <p className="text-lg font-semibold text-gray-900 dark:text-gray-100">
            {error ? "Failed to load task" : "Task not found"}
          </p>
          <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
            Please try refreshing the page or contact support if the problem persists.
          </p>
        </div>
      </div>
    );
  }

  // Render appropriate view
  if (viewToRender === "admin") {
    return <TaskDetailsViewAdmin taskId={taskId} />;
  }

  if (viewToRender === "manager") {
    return <TaskDetailsViewManager taskId={taskId} />;
  }

  if (viewToRender === "employee") {
    return <TaskDetailsViewEmployee taskId={taskId} />;
  }

  // Fallback: User has no relationship to task
  return (
    <div className="flex h-64 items-center justify-center">
      <div className="text-center">
        <p className="text-lg font-semibold text-gray-900 dark:text-gray-100">Access Denied</p>
        <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
          You don't have permission to view this task.
        </p>
      </div>
    </div>
  );
}
