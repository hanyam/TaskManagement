import { useQuery, useMutation, useQueryClient, type UseQueryOptions } from "@tanstack/react-query";

import { apiClient } from "@/core/api/client.browser";
import { useAuth } from "@/core/auth/AuthProvider";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import type {
  GetTasksResponse,
  TaskDto,
  TaskListFilters,
  DashboardStatsDto,
  UpdateTaskRequest,
  UpdateTaskProgressRequest,
  AssignTaskRequest,
  RequestMoreInfoRequest,
  RequestDeadlineExtensionRequest,
  ApproveExtensionRequestRequest,
  CreateTaskRequest,
  ExtensionRequestDto,
  TaskProgressDto,
  ReviewCompletedTaskRequest,
  TaskAttachmentDto
} from "@/features/tasks/types";


export const taskKeys = {
  all: ["tasks"] as const,
  lists: () => [...taskKeys.all, "list"] as const,
  list: (filters: TaskListFilters) => [...taskKeys.lists(), filters] as const,
  detail: (taskId: string) => [...taskKeys.all, "detail", taskId] as const,
  dashboard: () => [...taskKeys.all, "dashboard"] as const,
  attachments: (taskId: string) => [...taskKeys.all, "attachments", taskId] as const
};

export function useTasksQuery(filters: TaskListFilters, options?: Partial<UseQueryOptions<GetTasksResponse>>) {
  const locale = useCurrentLocale();
  return useQuery({
    queryKey: taskKeys.list(filters),
    queryFn: async ({ signal }) => {
      const { data } = await apiClient.request<GetTasksResponse>({
        path: "/tasks",
        method: "GET",
        query: filters as Record<string, unknown>,
        signal,
        locale
      });
      return data;
    },
    ...options
  });
}

export function useTaskDetailsQuery(taskId: string, enabled = true) {
  const locale = useCurrentLocale();
  return useQuery({
    queryKey: taskKeys.detail(taskId),
    enabled,
    queryFn: async ({ signal }) => {
      const response = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}`,
        method: "GET",
        signal,
        locale
      });
      return response; // Return full response including links
    }
  });
}

export function useDashboardStatsQuery() {
  const locale = useCurrentLocale();
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: taskKeys.dashboard(),
    enabled: isAuthenticated,
    queryFn: async ({ signal }) => {
      const { data } = await apiClient.request<DashboardStatsDto>({
        path: "/dashboard/stats",
        method: "GET",
        signal,
        locale
      });
      return data;
    },
    staleTime: 60_000
  });
}

export function useCreateTaskMutation() {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: CreateTaskRequest) => {
      const { data } = await apiClient.request<TaskDto>({
        path: "/tasks",
        method: "POST",
        body: payload,
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      return data;
    }
  });
}

export function useUpdateTaskMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: UpdateTaskRequest) => {
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}`,
        method: "PUT",
        body: payload,
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      return data;
    }
  });
}

export function useAssignTaskMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: AssignTaskRequest) => {
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}/assign`,
        method: "POST",
        body: payload,
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      return data;
    }
  });
}

export function useReassignTaskMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: AssignTaskRequest) => {
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}/reassign`,
        method: "PUT",
        body: { newUserIds: payload.userIds },
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      return data;
    }
  });
}

export function useUpdateTaskProgressMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: UpdateTaskProgressRequest) => {
      const { data } = await apiClient.request<TaskProgressDto>({
        path: `/tasks/${taskId}/progress`,
        method: "POST",
        body: payload,
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      return data;
    }
  });
}

export function useAcceptTaskProgressMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (payload: { progressHistoryId: string }) => {
      await apiClient.request<void>({
        path: `/tasks/${taskId}/progress/accept`,
        method: "POST",
        body: payload,
        locale
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
    }
  });
}

export function useCancelTaskMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      await apiClient.request<void>({
        path: `/tasks/${taskId}/cancel`,
        method: "POST",
        locale
      });
    },
    onSuccess: async () => {
      queryClient.removeQueries({ queryKey: taskKeys.detail(taskId) });
      queryClient.removeQueries({ queryKey: taskKeys.attachments(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
    }
  });
}

export function useAcceptTaskMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}/accept`,
        method: "POST",
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.attachments(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      return data;
    }
  });
}

export function useRejectTaskMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: { reason?: string | null }) => {
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}/reject`,
        method: "POST",
        body: payload,
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      return data;
    }
  });
}

export function useRequestMoreInfoMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: RequestMoreInfoRequest) => {
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}/request-info`,
        method: "POST",
        body: payload,
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      return data;
    }
  });
}

export function useRequestExtensionMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: RequestDeadlineExtensionRequest) => {
      const { data } = await apiClient.request<ExtensionRequestDto>({
        path: `/tasks/${taskId}/extension-request`,
        method: "POST",
        body: payload,
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      return data;
    }
  });
}

export function useApproveExtensionRequestMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ requestId, reviewNotes }: { requestId: string } & ApproveExtensionRequestRequest) => {
      await apiClient.request<void>({
        path: `/tasks/${taskId}/extension-request/${requestId}/approve`,
        method: "POST",
        body: { reviewNotes },
        locale
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
    }
  });
}

export function useMarkTaskCompletedMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}/complete`,
        method: "POST",
        locale
      });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      return data;
    }
  });
}

export function useReviewCompletedTaskMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (payload: ReviewCompletedTaskRequest) => {
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}/review-completed`,
        method: "POST",
        body: payload,
        locale
      });
      return data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.lists() });
      await queryClient.invalidateQueries({ queryKey: taskKeys.dashboard() });
    }
  });
}

// Attachment hooks
export function useTaskAttachmentsQuery(taskId: string) {
  const locale = useCurrentLocale();
  return useQuery({
    queryKey: taskKeys.attachments(taskId),
    queryFn: async ({ signal }) => {
      const { data } = await apiClient.request<TaskAttachmentDto[]>({
        path: `/tasks/${taskId}/attachments`,
        method: "GET",
        signal,
        locale
      });
      return data;
    }
  });
}

export function useUploadAttachmentMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();
  const { session } = useAuth();

  return useMutation({
    mutationFn: async ({ file, type }: { file: File; type: number }) => {
      const formData = new FormData();
      formData.append("file", file);
      formData.append("type", type.toString());

      // Use fetch directly for file uploads since apiClient expects JSON
      const token = session?.token;
      const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || "";
      const url = `${baseUrl}/tasks/${taskId}/attachments`;

      const headers = new Headers();
      if (token) {
        headers.set("Authorization", `Bearer ${token}`);
      }
      if (locale) {
        headers.set("Accept-Language", locale);
        headers.set("X-Locale", locale);
      }

      const response = await fetch(url, {
        method: "POST",
        headers,
        body: formData,
        credentials: "include"
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.message || "Failed to upload file");
      }

      const result = await response.json();
      return result.data as TaskAttachmentDto;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: taskKeys.attachments(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
    }
  });
}

export function useDeleteAttachmentMutation(taskId: string) {
  const locale = useCurrentLocale();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (attachmentId: string) => {
      await apiClient.request<void>({
        path: `/tasks/${taskId}/attachments/${attachmentId}`,
        method: "DELETE",
        locale
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: taskKeys.attachments(taskId) });
      await queryClient.invalidateQueries({ queryKey: taskKeys.detail(taskId) });
    }
  });
}

/**
 * Downloads an attachment file and returns it as a Blob.
 * This is a standalone function that can be called directly.
 */
export async function downloadAttachment(taskId: string, attachmentId: string): Promise<Blob> {
  const { getClientSession } = await import("@/core/auth/session.client");
  const session = getClientSession();
  const locale = typeof window !== "undefined" ? window.location.pathname.split("/")[1] || "en" : "en";

  const token = session?.token;
  const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || "";
  const url = `${baseUrl}/tasks/${taskId}/attachments/${attachmentId}/download`;

  const headers = new Headers();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }
  if (locale) {
    headers.set("Accept-Language", locale);
    headers.set("X-Locale", locale);
  }

  const response = await fetch(url, {
    method: "GET",
    headers,
    credentials: "include"
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({ message: "Failed to download file" }));
    throw new Error(errorData.message || "Failed to download file");
  }

  return response.blob();
}

