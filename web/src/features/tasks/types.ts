import type { TaskStatus, TaskPriority, ReminderLevel, ExtensionRequestStatus, ProgressStatus } from "@/features/tasks/value-objects";

// API response types (backend sends numeric enums)
export interface TaskDto {
  id: string;
  title: string;
  description?: string | null;
  status: number; // Numeric enum from backend
  priority: number; // Numeric enum from backend
  dueDate?: string | null;
  originalDueDate?: string | null;
  extendedDueDate?: string | null;
  assignedUserId: string;
  assignedUserEmail?: string | null;
  type: number; // Numeric enum from backend
  reminderLevel: number; // Numeric enum from backend
  progressPercentage?: number | null;
  createdById: string;
  createdBy: string;
  createdAt: string;
  updatedAt?: string | null;
  managerRating?: number | null;
  managerFeedback?: string | null;
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

export type TaskListViewFilter = "created" | "assigned";

export interface TaskListFilters {
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedUserId?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
  page?: number;
  pageSize?: number;
  reminderLevel?: ReminderLevel;
  filter?: TaskListViewFilter;
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
  priority: number; // Numeric enum value (0=Low, 1=Medium, 2=High, 3=Critical)
  dueDate?: string | null;
  assignedUserId?: string | null; // Optional - null for draft tasks
  type: number; // Numeric enum value (0=Simple, 1=WithDueDate, 2=WithProgress, 3=WithAcceptedProgress)
}

export interface AssignTaskRequest {
  userIds: string[];
}

export interface UpdateTaskRequest {
  title: string;
  description?: string | null;
  priority: number; // TaskPriority enum
  dueDate?: string | null; // ISO date string
  assignedUserId?: string | null;
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

export interface ReviewCompletedTaskRequest {
  accepted: boolean;
  rating: number; // 1-5
  feedback?: string | null;
  sendBackForRework: boolean;
}

export interface TaskAttachmentDto {
  id: string;
  taskId: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  type: number; // Numeric enum: 0=ManagerUploaded, 1=EmployeeUploaded
  uploadedById: string;
  uploadedByEmail?: string | null;
  uploadedByDisplayName?: string | null;
  createdAt: string;
}

