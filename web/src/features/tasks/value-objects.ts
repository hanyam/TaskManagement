export type TaskStatus = "Created" | "Assigned" | "UnderReview" | "Accepted" | "Rejected" | "Completed" | "Cancelled" | "PendingManagerReview" | "RejectedByManager";

export type TaskPriority = "Low" | "Medium" | "High" | "Critical";

export type TaskType = "Simple" | "WithDueDate" | "WithProgress" | "WithAcceptedProgress";

export type ReminderLevel = "None" | "Low" | "Medium" | "High" | "Critical";

export type ExtensionRequestStatus = "Pending" | "Approved" | "Rejected";

export type ProgressStatus = "Pending" | "Accepted" | "Rejected";

// Enum mappings for API requests (backend expects numeric values)
export const TaskPriorityEnum = {
  Low: 0,
  Medium: 1,
  High: 2,
  Critical: 3
} as const;

export const TaskTypeEnum = {
  Simple: 0,
  WithDueDate: 1,
  WithProgress: 2,
  WithAcceptedProgress: 3
} as const;

export const TaskStatusEnum = {
  Created: 0,
  Assigned: 1,
  UnderReview: 2,
  Accepted: 3,
  Rejected: 4,
  Completed: 5,
  Cancelled: 6,
  PendingManagerReview: 7,
  RejectedByManager: 8
} as const;

export const ReminderLevelEnum = {
  None: 0,
  Low: 1,
  Medium: 2,
  High: 3,
  Critical: 4
} as const;

// Helper functions to convert numeric enums to strings
export function getTaskPriorityString(priority: number): TaskPriority {
  const map: Record<number, TaskPriority> = {
    0: "Low",
    1: "Medium",
    2: "High",
    3: "Critical"
  };
  return map[priority] ?? "Medium";
}

export function getTaskTypeString(type: number): TaskType {
  const map: Record<number, TaskType> = {
    0: "Simple",
    1: "WithDueDate",
    2: "WithProgress",
    3: "WithAcceptedProgress"
  };
  return map[type] ?? "Simple";
}

export function getTaskStatusString(status: number): TaskStatus {
  const map: Record<number, TaskStatus> = {
    0: "Created",
    1: "Assigned",
    2: "UnderReview",
    3: "Accepted",
    4: "Rejected",
    5: "Completed",
    6: "Cancelled",
    7: "PendingManagerReview",
    8: "RejectedByManager"
  };
  return map[status] ?? "Created";
}

export function getReminderLevelString(level: number): ReminderLevel {
  const map: Record<number, ReminderLevel> = {
    0: "None",
    1: "Low",
    2: "Medium",
    3: "High",
    4: "Critical"
  };
  return map[level] ?? "None";
}

// HATEOAS link helper functions
export function hasActionLink(links: Array<{ rel: string; href: string; method: string}> | undefined, rel: string): boolean {
  return links?.some(link => link.rel === rel) ?? false;
}

export function getActionLink(links: Array<{ rel: string; href: string; method: string}> | undefined, rel: string): { rel: string; href: string; method: string} | undefined {
  return links?.find(link => link.rel === rel);
}

