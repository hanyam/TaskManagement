import { useQuery, useMutation, useQueryClient, type UseQueryOptions } from "@tanstack/react-query";

import { apiClient } from "@/core/api/client.browser";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import type {
  GetTasksResponse,
  TaskDto,
  TaskListFilters,
  DashboardStatsDto,
  UpdateTaskProgressRequest,
  AssignTaskRequest,
  RequestMoreInfoRequest,
  RequestDeadlineExtensionRequest,
  ApproveExtensionRequestRequest,
  CreateTaskRequest
, ExtensionRequestDto, TaskProgressDto } from "@/features/tasks/types";


export const taskKeys = {
  all: ["tasks"] as const,
  lists: () => [...taskKeys.all, "list"] as const,
  list: (filters: TaskListFilters) => [...taskKeys.lists(), filters] as const,
  detail: (taskId: string) => [...taskKeys.all, "detail", taskId] as const,
  dashboard: () => [...taskKeys.all, "dashboard"] as const
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
      const { data } = await apiClient.request<TaskDto>({
        path: `/tasks/${taskId}`,
        method: "GET",
        signal,
        locale
      });
      return data;
    }
  });
}

export function useDashboardStatsQuery() {
  const locale = useCurrentLocale();
  return useQuery({
    queryKey: taskKeys.dashboard(),
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

