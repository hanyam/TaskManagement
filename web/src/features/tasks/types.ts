import type { TaskStatus, TaskPriority, TaskType, ReminderLevel, ExtensionRequestStatus, ProgressStatus } from "@/features/tasks/value-objects";

export interface TaskDto {
  id: string;
  title: string;
  description?: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string | null;
  originalDueDate?: string | null;
  extendedDueDate?: string | null;
  assignedUserId: string;
  assignedUserEmail?: string | null;
  type: TaskType;
  reminderLevel: ReminderLevel;
  progressPercentage?: number | null;
  createdById: string;
  createdBy: string;
  createdAt: string;
  updatedAt?: string | null;
  assignments?: TaskAssignmentDto[];
  recentProgressHistory?: TaskProgressDto[];
}

export interface TaskAssignmentDto {
  id: string;
  taskId: string;
  userId: string;
  userEmail?: string | null;
  userDisplayName?: string | null;
  isPrimary: boolean;
  assignedAt: string;
}

export interface TaskProgressDto {
  id: string;
  taskId: string;
  updatedById: string;
  updatedByEmail?: string | null;
  progressPercentage: number;
  notes?: string | null;
  status: ProgressStatus;
  acceptedById?: string | null;
  acceptedByEmail?: string | null;
  acceptedAt?: string | null;
  updatedAt: string;
}

export interface GetTasksResponse {
  tasks: TaskDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface TaskListFilters {
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedUserId?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
  page?: number;
  pageSize?: number;
  reminderLevel?: ReminderLevel;
}

export interface ExtensionRequestDto {
  id: string;
  taskId: string;
  taskTitle: string;
  requestedById: string;
  requestedByEmail?: string | null;
  requestedDueDate: string;
  reason: string;
  status: ExtensionRequestStatus;
  reviewedById?: string | null;
  reviewedByEmail?: string | null;
  reviewedAt?: string | null;
  reviewNotes?: string | null;
  createdAt: string;
}

export interface DashboardStatsDto {
  tasksCreatedByUser: number;
  tasksCompleted: number;
  tasksNearDueDate: number;
  tasksDelayed: number;
  tasksInProgress: number;
  tasksUnderReview: number;
  tasksPendingAcceptance: number;
}

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  priority: TaskPriority;
  dueDate?: string | null;
  assignedUserId: string;
  type: TaskType;
}

export interface AssignTaskRequest {
  userIds: string[];
}

export interface UpdateTaskProgressRequest {
  progressPercentage: number;
  notes?: string | null;
}

export interface RequestMoreInfoRequest {
  requestMessage: string;
}

export interface RequestDeadlineExtensionRequest {
  requestedDueDate: string;
  reason: string;
}

export interface ApproveExtensionRequestRequest {
  reviewNotes?: string | null;
}

