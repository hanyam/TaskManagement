"use client";

import { Dialog, Transition } from "@headlessui/react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { Fragment, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import {
  useAssignTaskMutation,
  useAcceptTaskMutation,
  useApproveExtensionRequestMutation,
  useMarkTaskCompletedMutation,
  useRejectTaskMutation,
  useReassignTaskMutation,
  useRequestExtensionMutation,
  useRequestMoreInfoMutation,
  useTaskDetailsQuery,
  useUpdateTaskProgressMutation
} from "@/features/tasks/api/queries";
import { TaskStatusBadge } from "@/features/tasks/components/TaskStatusBadge";
import type { AssignTaskRequest } from "@/features/tasks/types";
import { Button } from "@/ui/components/Button";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Input } from "@/ui/components/Input";
import { Label } from "@/ui/components/Label";
import { Spinner } from "@/ui/components/Spinner";

interface TaskDetailsViewProps {
  taskId: string;
}

export function TaskDetailsView({ taskId }: TaskDetailsViewProps) {
  const { t } = useTranslation(["tasks", "common"]);
  const router = useRouter();
  const { data: task, isLoading, error, refetch } = useTaskDetailsQuery(taskId, Boolean(taskId));

  const acceptMutation = useAcceptTaskMutation(taskId);
  const assignMutation = useAssignTaskMutation(taskId);
  const reassignMutation = useReassignTaskMutation(taskId);
  const rejectMutation = useRejectTaskMutation(taskId);
  const markCompleteMutation = useMarkTaskCompletedMutation(taskId);

  const [isAssignOpen, setAssignOpen] = useState(false);
  const [isReassignOpen, setReassignOpen] = useState(false);
  const [isProgressOpen, setProgressOpen] = useState(false);
  const [isExtensionOpen, setExtensionOpen] = useState(false);
  const [isMoreInfoOpen, setMoreInfoOpen] = useState(false);
  const [isApproveExtensionOpen, setApproveExtensionOpen] = useState(false);

  const metadata = useMemo(() => {
    if (!task) {
      return [];
    }
    return [
      {
        label: t("tasks:details.metadata.createdBy"),
        value: task.createdBy
      },
      {
        label: t("tasks:details.metadata.createdAt"),
        value: formatDate(task.createdAt)
      },
      {
        label: t("tasks:details.metadata.updatedAt"),
        value: task.updatedAt ? formatDate(task.updatedAt) : "—"
      },
      {
        label: t("tasks:details.metadata.originalDueDate"),
        value: task.originalDueDate ? formatDate(task.originalDueDate) : "—"
      },
      {
        label: t("tasks:details.metadata.extendedDueDate"),
        value: task.extendedDueDate ? formatDate(task.extendedDueDate) : "—"
      },
      {
        label: t("tasks:details.metadata.type"),
        value: t(`common:taskType.${task.type.charAt(0).toLowerCase()}${task.type.slice(1)}`)
      },
      {
        label: t("tasks:details.metadata.progress"),
        value: task.progressPercentage != null ? `${task.progressPercentage}%` : "—"
      }
    ];
  }, [t, task]);

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error || !task) {
    return (
      <div className="rounded-xl border border-border bg-background p-6 text-center">
        <p className="text-sm text-destructive">{t("common:states.error")}</p>
        <Button variant="outline" className="mt-4" onClick={() => refetch()}>
          {t("common:actions.retry")}
        </Button>
      </div>
    );
  }

  async function handleAcceptTask() {
    await acceptMutation.mutateAsync();
    toast.success(t("tasks:details.actions.accept"));
    refetch();
  }

  async function handleRejectTask() {
    await rejectMutation.mutateAsync({ reason: "" });
    toast.success(t("tasks:details.actions.reject"));
    refetch();
  }

  async function handleMarkCompleted() {
    await markCompleteMutation.mutateAsync();
    toast.success(t("tasks:details.actions.markCompleted"));
    router.refresh();
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-col gap-4 rounded-xl border border-border bg-background p-6 shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex flex-col gap-2">
            <div className="flex items-center gap-3">
              <h1 className="font-heading text-2xl text-foreground">{task.title}</h1>
              <TaskStatusBadge status={task.status} />
            </div>
            <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
              <span>
                {t("common:filters.priority")}: {t(`common:priority.${task.priority.toLowerCase()}`)}
              </span>
              <span>
                {t("common:filters.assignedTo")}: {task.assignedUserEmail ?? "—"}
              </span>
              <span>
                {t("common:filters.dueDate")}: {task.dueDate ? formatDate(task.dueDate) : t("common:date.noDueDate")}
              </span>
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <Button variant="secondary" onClick={() => setProgressOpen(true)}>
              {t("tasks:details.actions.updateProgress")}
            </Button>
            <Button variant="outline" onClick={() => setMoreInfoOpen(true)}>
              {t("tasks:details.actions.requestInfo")}
            </Button>
            <Button variant="outline" onClick={() => setExtensionOpen(true)}>
              {t("tasks:details.actions.requestExtension")}
            </Button>
            <Button variant="outline" onClick={() => setApproveExtensionOpen(true)}>
              {t("tasks:details.actions.approveExtension")}
            </Button>
            <Button variant="outline" onClick={() => setAssignOpen(true)}>
              {t("tasks:forms.assign.title")}
            </Button>
            <Button variant="outline" onClick={() => setReassignOpen(true)}>
              {t("tasks:forms.assign.title")}
            </Button>
            <Button variant="primary" onClick={handleAcceptTask} disabled={acceptMutation.isPending}>
              {t("tasks:details.actions.accept")}
            </Button>
            <Button variant="outline" onClick={handleRejectTask} disabled={rejectMutation.isPending}>
              {t("tasks:details.actions.reject")}
            </Button>
            <Button variant="destructive" onClick={handleMarkCompleted} disabled={markCompleteMutation.isPending}>
              {t("tasks:details.actions.markCompleted")}
            </Button>
          </div>
        </div>
      </header>

      <div className="grid gap-6 lg:grid-cols-3">
        <section className="space-y-4 rounded-xl border border-border bg-background p-6 shadow-sm lg:col-span-2">
          <h2 className="text-lg font-semibold text-foreground">{t("tasks:details.sections.description")}</h2>
          <p className="text-sm text-muted-foreground">{task.description ?? t("common:states.empty")}</p>

          <h3 className="text-base font-semibold text-foreground">{t("tasks:details.sections.assignments")}</h3>
          <div className="space-y-2 text-sm">
            <div className="flex flex-col rounded-lg border border-border bg-muted/30 p-3">
              <span className="font-medium text-foreground">
                {t("common:filters.assignedTo")}: {task.assignedUserEmail ?? "—"}
              </span>
              <span className="text-xs text-muted-foreground">{task.assignedUserId}</span>
            </div>
          </div>

          <h3 className="text-base font-semibold text-foreground">{t("tasks:details.sections.progressHistory")}</h3>
          <div className="space-y-3 rounded-lg border border-border bg-muted/20 p-3">
            {task.recentProgressHistory?.length ? (
              task.recentProgressHistory.map((progress) => (
                <div key={progress.id} className="flex flex-col gap-1 rounded-md border border-border/80 bg-background px-3 py-2">
                  <div className="flex justify-between text-xs text-muted-foreground">
                    <span>{formatDate(progress.updatedAt)}</span>
                    <span>{progress.updatedByEmail ?? progress.updatedById}</span>
                  </div>
                  <div className="text-sm text-foreground">
                    {progress.progressPercentage}% — {progress.notes ?? t("common:states.empty")}
                  </div>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">{t("common:states.empty")}</p>
            )}
          </div>
        </section>

        <aside className="space-y-3 rounded-xl border border-border bg-background p-6 shadow-sm">
          <h3 className="text-lg font-semibold text-foreground">{t("tasks:details.title")}</h3>
          <dl className="space-y-3 text-sm">
            {metadata.map((item) => (
              <div key={item.label} className="flex flex-col">
                <dt className="text-xs text-muted-foreground">{item.label}</dt>
                <dd className="font-medium text-foreground">{item.value}</dd>
              </div>
            ))}
          </dl>
        </aside>
      </div>

      <AssignTaskDialog
        open={isAssignOpen}
        onOpenChange={setAssignOpen}
        mutation={{ mutateAsync: assignMutation.mutateAsync, isPending: assignMutation.isPending }}
      />
      <AssignTaskDialog
        open={isReassignOpen}
        onOpenChange={setReassignOpen}
        mutation={{ mutateAsync: reassignMutation.mutateAsync, isPending: reassignMutation.isPending }}
        mode="reassign"
      />
      <UpdateProgressDialog open={isProgressOpen} onOpenChange={setProgressOpen} taskId={task.id} />
      <RequestExtensionDialog open={isExtensionOpen} onOpenChange={setExtensionOpen} taskId={task.id} />
      <RequestMoreInfoDialog open={isMoreInfoOpen} onOpenChange={setMoreInfoOpen} taskId={task.id} />
      <ApproveExtensionDialog open={isApproveExtensionOpen} onOpenChange={setApproveExtensionOpen} taskId={task.id} />
    </div>
  );
}

function formatDate(value: string): string {
  try {
    return new Intl.DateTimeFormat(undefined, { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
  } catch {
    return value;
  }
}

interface ModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  taskId: string;
}

interface AssignTaskDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mutation: {
    mutateAsync: (payload: AssignTaskRequest) => Promise<unknown>;
    isPending: boolean;
  };
  mode?: "assign" | "reassign";
}

function AssignTaskDialog({ open, onOpenChange, mutation, mode = "assign" }: AssignTaskDialogProps) {
  const { t } = useTranslation(["tasks", "validation"]);
  const form = useForm<{ userIds: string }>({
    resolver: zodResolver(
      z.object({
        userIds: z
          .string()
          .min(1, "validation:required")
      })
    ),
    defaultValues: {
      userIds: ""
    }
  });

  async function onSubmit(values: { userIds: string }) {
    const userIds = values.userIds
      .split(",")
      .map((id) => id.trim())
      .filter(Boolean);

    await mutation.mutateAsync({ userIds });
    toast.success(
      mode === "assign" ? t("tasks:forms.assign.success") : t("tasks:forms.assign.success")
    );
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onClose={onOpenChange} as="div" className="relative z-50">
      <Transition appear show={open} as={Fragment}>
        <div className="fixed inset-0 bg-black/40" aria-hidden />
      </Transition>
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Transition
          appear
          show={open}
          as={Fragment}
          enter="transition ease-out duration-150"
          enterFrom="opacity-0 scale-95"
          enterTo="opacity-100 scale-100"
        >
          <Dialog.Panel className="w-full max-w-md space-y-4 rounded-xl border border-border bg-background p-6 shadow-lg">
            <Dialog.Title className="text-lg font-semibold text-foreground">
              {t("tasks:forms.assign.title")}
            </Dialog.Title>
            <p className="text-sm text-muted-foreground">{t("tasks:forms.assign.description")}</p>
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid gap-2">
                <Label htmlFor="assignUserIds">{t("tasks:forms.assign.fields.userIds")}</Label>
                <Input
                  id="assignUserIds"
                  placeholder="guid-1, guid-2"
                  {...form.register("userIds")}
                />
                {form.formState.errors.userIds ? (
                  <FormFieldError
                    message={t("validation:required", {
                      field: t("tasks:forms.assign.fields.userIds")
                    })}
                  />
                ) : null}
              </div>
              <div className="flex justify-end gap-2">
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                  {t("common:actions.cancel")}
                </Button>
                <Button type="submit" disabled={mutation.isPending}>
                  {t("common:actions.save")}
                </Button>
              </div>
            </form>
          </Dialog.Panel>
        </Transition>
      </div>
    </Dialog>
  );
}

function UpdateProgressDialog({ open, onOpenChange, taskId }: ModalProps) {
  const { t } = useTranslation(["tasks", "validation"]);
  const mutation = useUpdateTaskProgressMutation(taskId);
  const form = useForm<{ progressPercentage: number; notes?: string | null }>({
    resolver: zodResolver(
      z.object({
        progressPercentage: z.number({ invalid_type_error: "validation:required" }).min(0).max(100),
        notes: z.string().max(500).optional()
      })
    ),
    defaultValues: {
      progressPercentage: 0,
      notes: ""
    }
  });

  async function onSubmit(values: { progressPercentage: number; notes?: string | null }) {
    await mutation.mutateAsync(values);
    toast.success(t("tasks:forms.progress.success"));
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onClose={onOpenChange} as="div" className="relative z-50">
      <Transition appear show={open} as={Fragment}>
        <div className="fixed inset-0 bg-black/40" aria-hidden />
      </Transition>
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Transition
          appear
          show={open}
          as={Fragment}
          enter="transition ease-out duration-150"
          enterFrom="opacity-0 scale-95"
          enterTo="opacity-100 scale-100"
        >
          <Dialog.Panel className="w-full max-w-md space-y-4 rounded-xl border border-border bg-background p-6 shadow-lg">
            <Dialog.Title className="text-lg font-semibold text-foreground">{t("tasks:forms.progress.title")}</Dialog.Title>
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid gap-2">
                <Label htmlFor="progressPercentage">{t("tasks:forms.progress.fields.progressPercentage")}</Label>
                <Input
                  id="progressPercentage"
                  type="number"
                  min={0}
                  max={100}
                  {...form.register("progressPercentage", { valueAsNumber: true })}
                />
                {form.formState.errors.progressPercentage ? (
                  <FormFieldError
                    message={t("validation:required", {
                      field: t("tasks:forms.progress.fields.progressPercentage")
                    })}
                  />
                ) : null}
              </div>
              <div className="grid gap-2">
                <Label htmlFor="progressNotes">{t("tasks:forms.progress.fields.notes")}</Label>
                <textarea
                  id="progressNotes"
                  rows={3}
                  className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground"
                  {...form.register("notes")}
                />
              </div>
              <div className="flex justify-end gap-2">
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                  {t("common:actions.cancel")}
                </Button>
                <Button type="submit" disabled={mutation.isPending}>
                  {t("common:actions.save")}
                </Button>
              </div>
            </form>
          </Dialog.Panel>
        </Transition>
      </div>
    </Dialog>
  );
}

function RequestExtensionDialog({ open, onOpenChange, taskId }: ModalProps) {
  const { t } = useTranslation(["tasks", "validation"]);
  const mutation = useRequestExtensionMutation(taskId);
  const form = useForm<{ requestedDueDate: string; reason: string }>({
    resolver: zodResolver(
      z.object({
        requestedDueDate: z
          .string()
          .min(1, "validation:required")
          .refine((value) => !Number.isNaN(Date.parse(value)), "validation:futureDate"),
        reason: z.string().min(1, "validation:required").max(500)
      })
    ),
    defaultValues: {
      requestedDueDate: "",
      reason: ""
    }
  });

  async function onSubmit(values: { requestedDueDate: string; reason: string }) {
    await mutation.mutateAsync({
      requestedDueDate: new Date(values.requestedDueDate).toISOString(),
      reason: values.reason
    });
    toast.success(t("tasks:forms.extension.success"));
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onClose={onOpenChange} as="div" className="relative z-50">
      <Transition appear show={open} as={Fragment}>
        <div className="fixed inset-0 bg-black/40" aria-hidden />
      </Transition>
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Transition
          appear
          show={open}
          as={Fragment}
          enter="transition ease-out duration-150"
          enterFrom="opacity-0 scale-95"
          enterTo="opacity-100 scale-100"
        >
          <Dialog.Panel className="w-full max-w-md space-y-4 rounded-xl border border-border bg-background p-6 shadow-lg">
            <Dialog.Title className="text-lg font-semibold text-foreground">
              {t("tasks:forms.extension.title")}
            </Dialog.Title>
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid gap-2">
                <Label htmlFor="extensionDueDate">{t("tasks:forms.extension.fields.requestedDueDate")}</Label>
                <Input id="extensionDueDate" type="date" {...form.register("requestedDueDate")} />
                {form.formState.errors.requestedDueDate ? (
                  <FormFieldError
                    message={t(form.formState.errors.requestedDueDate.message ?? "validation:required", {
                      field: t("tasks:forms.extension.fields.requestedDueDate")
                    })}
                  />
                ) : null}
              </div>
              <div className="grid gap-2">
                <Label htmlFor="extensionReason">{t("tasks:forms.extension.fields.reason")}</Label>
                <textarea
                  id="extensionReason"
                  rows={3}
                  className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground"
                  {...form.register("reason")}
                />
                {form.formState.errors.reason ? (
                  <FormFieldError
                    message={t(form.formState.errors.reason.message ?? "validation:required", {
                      field: t("tasks:forms.extension.fields.reason")
                    })}
                  />
                ) : null}
              </div>
              <div className="flex justify-end gap-2">
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                  {t("common:actions.cancel")}
                </Button>
                <Button type="submit" disabled={mutation.isPending}>
                  {t("common:actions.save")}
                </Button>
              </div>
            </form>
          </Dialog.Panel>
        </Transition>
      </div>
    </Dialog>
  );
}

function RequestMoreInfoDialog({ open, onOpenChange, taskId }: ModalProps) {
  const { t } = useTranslation(["tasks", "validation"]);
  const mutation = useRequestMoreInfoMutation(taskId);
  const form = useForm<{ requestMessage: string }>({
    resolver: zodResolver(
      z.object({
        requestMessage: z.string().min(1, "validation:required").max(500)
      })
    ),
    defaultValues: {
      requestMessage: ""
    }
  });

  async function onSubmit(values: { requestMessage: string }) {
    await mutation.mutateAsync(values);
    toast.success(t("tasks:details.actions.requestInfo"));
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onClose={onOpenChange} as="div" className="relative z-50">
      <Transition appear show={open} as={Fragment}>
        <div className="fixed inset-0 bg-black/40" aria-hidden />
      </Transition>
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Transition
          appear
          show={open}
          as={Fragment}
          enter="transition ease-out duration-150"
          enterFrom="opacity-0 scale-95"
          enterTo="opacity-100 scale-100"
        >
          <Dialog.Panel className="w-full max-w-md space-y-4 rounded-xl border border-border bg-background p-6 shadow-lg">
            <Dialog.Title className="text-lg font-semibold text-foreground">
              {t("tasks:details.actions.requestInfo")}
            </Dialog.Title>
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid gap-2">
                <Label htmlFor="requestMessage">{t("tasks:details.actions.requestInfo")}</Label>
                <textarea
                  id="requestMessage"
                  rows={4}
                  className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground"
                  {...form.register("requestMessage")}
                />
                {form.formState.errors.requestMessage ? (
                  <FormFieldError
                    message={t(form.formState.errors.requestMessage.message ?? "validation:required", {
                      field: t("tasks:details.actions.requestInfo")
                    })}
                  />
                ) : null}
              </div>
              <div className="flex justify-end gap-2">
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                  {t("common:actions.cancel")}
                </Button>
                <Button type="submit" disabled={mutation.isPending}>
                  {t("common:actions.save")}
                </Button>
              </div>
            </form>
          </Dialog.Panel>
        </Transition>
      </div>
    </Dialog>
  );
}

function ApproveExtensionDialog({ open, onOpenChange, taskId }: ModalProps) {
  const { t } = useTranslation(["tasks", "validation"]);
  const mutation = useApproveExtensionRequestMutation(taskId);
  const form = useForm<{ extensionRequestId: string; reviewNotes?: string | null }>({
    resolver: zodResolver(
      z.object({
        extensionRequestId: z.string().uuid("validation:required"),
        reviewNotes: z.string().max(500).optional()
      })
    ),
    defaultValues: {
      extensionRequestId: "",
      reviewNotes: ""
    }
  });

  async function onSubmit(values: { extensionRequestId: string; reviewNotes?: string | null }) {
    await mutation.mutateAsync({
      requestId: values.extensionRequestId,
      reviewNotes: values.reviewNotes ?? null
    });
    toast.success(t("tasks:forms.approveExtension.success"));
    onOpenChange(false);
  }

  return (
    <Dialog open={open} onClose={onOpenChange} as="div" className="relative z-50">
      <Transition appear show={open} as={Fragment}>
        <div className="fixed inset-0 bg-black/40" aria-hidden />
      </Transition>
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Transition
          appear
          show={open}
          as={Fragment}
          enter="transition ease-out duration-150"
          enterFrom="opacity-0 scale-95"
          enterTo="opacity-100 scale-100"
        >
          <Dialog.Panel className="w-full max-w-md space-y-4 rounded-xl border border-border bg-background p-6 shadow-lg">
            <Dialog.Title className="text-lg font-semibold text-foreground">
              {t("tasks:forms.approveExtension.title")}
            </Dialog.Title>
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid gap-2">
                <Label htmlFor="extensionRequestId">
                  {t("tasks:forms.approveExtension.fields.extensionRequestId")}
                </Label>
                <Input id="extensionRequestId" {...form.register("extensionRequestId")} />
                {form.formState.errors.extensionRequestId ? (
                  <FormFieldError
                    message={t("validation:required", {
                      field: t("tasks:forms.approveExtension.fields.extensionRequestId")
                    })}
                  />
                ) : null}
              </div>
              <div className="grid gap-2">
                <Label htmlFor="approveNotes">{t("tasks:forms.approveExtension.fields.reviewNotes")}</Label>
                <textarea
                  id="approveNotes"
                  rows={3}
                  className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground"
                  {...form.register("reviewNotes")}
                />
              </div>
              <div className="flex justify-end gap-2">
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
                  {t("common:actions.cancel")}
                </Button>
                <Button type="submit" disabled={mutation.isPending}>
                  {t("common:actions.save")}
                </Button>
              </div>
            </form>
          </Dialog.Panel>
        </Transition>
      </div>
    </Dialog>
  );
}

