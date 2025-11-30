"use client";

import { CheckIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useMemo, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import { useAuth } from "@/core/auth/AuthProvider";
import { getRoleFromToken } from "@/core/auth/session.client";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { debugWarn, debugError } from "@/core/debug/logger";
import { useCreateTaskMutation } from "@/features/tasks/api/queries";
import { UserSearchInput } from "@/features/tasks/components/UserSearchInput";
import type { TaskPriority, TaskType } from "@/features/tasks/value-objects";
import { TaskPriorityEnum, TaskTypeEnum, AttachmentTypeEnum } from "@/features/tasks/value-objects";
import { Button } from "@/ui/components/Button";
import { DatePicker } from "@/ui/components/DatePicker";
import { FileUpload, type FileUploadItem } from "@/ui/components/FileUpload";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Input } from "@/ui/components/Input";
import { Label } from "@/ui/components/Label";
import { Select } from "@/ui/components/Select";

const createTaskSchema = z.object({
  title: z.string().min(1, "validation:required"),
  description: z.string().max(1000).optional().or(z.literal("")),
  priority: z.enum(["Low", "Medium", "High", "Critical"]),
  dueDate: z
    .string()
    .optional()
    .refine((value) => !value || !Number.isNaN(Date.parse(value)), {
      message: "validation:futureDate"
    }),
  assignedUserId: z
    .string()
    .optional()
    .or(z.literal("")), // Optional - allows draft tasks without assignment
  type: z.enum(["Simple", "WithDueDate", "WithProgress", "WithAcceptedProgress"])
});

type CreateTaskFormValues = z.infer<typeof createTaskSchema>;

const PRIORITY_OPTIONS: TaskPriority[] = ["Low", "Medium", "High", "Critical"];
const TYPE_OPTIONS: TaskType[] = ["Simple", "WithDueDate", "WithProgress", "WithAcceptedProgress"];

export function TaskCreateView() {
  const { t } = useTranslation(["tasks", "common", "validation", "navigation"]);
  const router = useRouter();
  const locale = useCurrentLocale();
  const { mutateAsync, isPending } = useCreateTaskMutation();
  const { session } = useAuth();
  const [files, setFiles] = useState<FileUploadItem[]>([]);

  // Check if user can upload files (Managers and Admins)
  const canUploadFiles = useMemo(() => {
    // First try to get role from session user object
    let role: string | undefined = session?.user?.role;

    // Fallback: extract role from JWT token if not in session
    if (!role && session?.token) {
      const tokenRole = getRoleFromToken(session.token);
      role = tokenRole ?? undefined; // Convert null to undefined
      // If we found role in token, update the session user object for future use
      if (role && session.user) {
        session.user.role = role;
      }
    }

    if (!role) {
      debugWarn("[TaskCreateView] No role found in session or token", {
        hasSession: !!session,
        hasToken: !!session?.token,
        user: session?.user
      });
      return false;
    }

    // Case-insensitive role check
    const roleLower = role.toLowerCase();
    const canUpload = roleLower === "manager" || roleLower === "admin";

    return canUpload;
  }, [session?.user?.role, session?.token, session]);

  const form = useForm<CreateTaskFormValues>({
    resolver: zodResolver(createTaskSchema),
    defaultValues: {
      title: "",
      description: "",
      priority: "Medium",
      dueDate: "",
      assignedUserId: "", // Optional - empty means draft task
      type: "Simple"
    }
  });

  const instructions = useMemo(() => t("tasks:forms.create.instructions"), [t]);

  async function handleSubmit(values: CreateTaskFormValues) {
    try {
      const task = await mutateAsync({
        title: values.title,
        description: values.description ? values.description : null,
        priority: TaskPriorityEnum[values.priority], // Convert string to numeric enum
        assignedUserId: values.assignedUserId && values.assignedUserId.trim() !== ""
          ? values.assignedUserId
          : null, // Null for draft tasks
        type: TaskTypeEnum[values.type], // Convert string to numeric enum
        dueDate: values.dueDate ? new Date(values.dueDate).toISOString() : null
      });

      // Upload files after task creation (managers only)
      const uploadResults: { success: string[]; failed: Array<{ fileName: string; error: string }> } = {
        success: [],
        failed: []
      };

      if (files.length > 0 && task?.id && canUploadFiles) {
        // Upload files sequentially
        for (const fileItem of files) {
          if (!fileItem.error) {
            try {
              const formData = new FormData();
              formData.append("file", fileItem.file);
              formData.append("type", AttachmentTypeEnum.ManagerUploaded.toString());

              const token = session?.token;
              const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || "";
              const url = `${baseUrl}/tasks/${task.id}/attachments`;

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
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                const errorMessage = errorData.message || errorData.error?.message || response.statusText;
                uploadResults.failed.push({
                  fileName: fileItem.file.name,
                  error: errorMessage
                });
                debugError(`Failed to upload file ${fileItem.file.name}`, errorMessage);
              } else {
                uploadResults.success.push(fileItem.file.name);
              }
            } catch (error) {
              const errorMessage = error instanceof Error ? error.message : String(error);
              uploadResults.failed.push({
                fileName: fileItem.file.name,
                error: errorMessage
              });
              debugError(`Failed to upload file ${fileItem.file.name}`, error);
            }
          } else {
            // File had validation error before upload
            uploadResults.failed.push({
              fileName: fileItem.file.name,
              error: fileItem.error
            });
          }
        }

        // Show user feedback about upload results
        if (uploadResults.success.length > 0) {
          toast.success(
            t("tasks:attachments.upload.batchSuccess", {
              count: uploadResults.success.length,
              fileName: uploadResults.success[0]
            })
          );
        }

        if (uploadResults.failed.length > 0) {
          const failedFiles = uploadResults.failed.map((f) => f.fileName).join(", ");
          toast.error(
            t("tasks:attachments.upload.batchFailed", {
              count: uploadResults.failed.length,
              files: failedFiles
            }),
            {
              duration: 10000 // Show for 10 seconds so user can read it
            }
          );

          // Redirect to task details page so user can retry uploading
          router.push(`/${locale}/tasks/${task.id}`);
          router.refresh();
          return; // Don't show success message for task creation since we're redirecting
        }
      }

      // Show success message and redirect
      toast.success(t("tasks:forms.create.success"));
      router.push(`/${locale}/tasks`);
      router.refresh();
    } catch (error) {
      form.setError("title", {
        type: "server",
        message: error instanceof Error ? error.message : t("validation:server.VALIDATION_ERROR")
      });
    }
  }

  return (
    <div className="mx-auto w-full max-w-3xl space-y-6">
      <div className="rounded-xl border border-border bg-background p-6 shadow-sm">
        <div>
          <h1 className="font-heading text-2xl text-foreground">{t("tasks:forms.create.title")}</h1>
          <p className="text-sm text-muted-foreground">{instructions}</p>
        </div>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="grid gap-6" noValidate>
          <div className="grid gap-2">
            <Label htmlFor="title">{t("tasks:forms.create.fields.title")}</Label>
            <Input id="title" placeholder={t("tasks:forms.create.fields.titlePlaceholder")} {...form.register("title")} />
            {form.formState.errors.title ? (
              <FormFieldError
                message={t(form.formState.errors.title.message ?? "validation:required", {
                  field: t("tasks:forms.create.fields.title")
                })}
              />
            ) : null}
          </div>

          <div className="grid gap-2">
            <Label htmlFor="description">{t("tasks:forms.create.fields.description")}</Label>
            <textarea
              id="description"
              rows={4}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm focus-visible:border-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
              placeholder={t("tasks:forms.create.fields.descriptionPlaceholder")}
              {...form.register("description")}
            />
            {form.formState.errors.description ? (
              <FormFieldError
                message={t(form.formState.errors.description.message ?? "validation:maxLength", {
                  field: t("tasks:forms.create.fields.description"),
                  count: 1000
                })}
              />
            ) : null}
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="priority">{t("tasks:forms.create.fields.priority")}</Label>
              <Controller
                name="priority"
                control={form.control}
                render={({ field }) => (
                  <Select
                    id="priority"
                    options={PRIORITY_OPTIONS.map((option) => ({
                      value: option,
                      label: t(`common:priority.${option.toLowerCase()}`)
                    }))}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="type">{t("tasks:forms.create.fields.type")}</Label>
              <Controller
                name="type"
                control={form.control}
                render={({ field }) => (
                  <Select
                    id="type"
                    options={TYPE_OPTIONS.map((option) => ({
                      value: option,
                      label: t(`common:taskType.${option.charAt(0).toLowerCase()}${option.slice(1)}`)
                    }))}
                    value={field.value}
                    onChange={field.onChange}
                  />
                )}
              />
            </div>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <div className="grid gap-2">
              <Label htmlFor="dueDate">{t("tasks:forms.create.fields.dueDate")}</Label>
              <Controller
                name="dueDate"
                control={form.control}
                render={({ field }) => (
                  <DatePicker
                    id="dueDate"
                    value={field.value}
                    onChange={field.onChange}
                    placeholder={t("tasks:forms.create.fields.dueDatePlaceholder")}
                  />
                )}
              />
              {form.formState.errors.dueDate ? (
                <FormFieldError
                  message={t(form.formState.errors.dueDate.message ?? "validation:futureDate", {
                    field: t("tasks:forms.create.fields.dueDate")
                  })}
                />
              ) : null}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="assignedUserId">
                {t("tasks:forms.create.fields.assignedUserId")}
                <span className="ml-1 text-xs text-muted-foreground">({t("common:optional")})</span>
              </Label>
              <Controller
                name="assignedUserId"
                control={form.control}
                render={({ field }) => (
                  <UserSearchInput
                    value={field.value}
                    onChange={field.onChange}
                    placeholder={t("tasks:forms.create.fields.searchUserPlaceholder")}
                    error={!!form.formState.errors.assignedUserId}
                  />
                )}
              />
              {form.formState.errors.assignedUserId ? (
                <FormFieldError message={t(form.formState.errors.assignedUserId.message ?? "validation:required")} />
              ) : null}
            </div>
          </div>

          {/* File Upload Section - Only for Managers/Admins */}
          {canUploadFiles && (
            <div className="grid gap-2">
              <Label>{t("tasks:attachments.sections.managerUploaded")}</Label>
              <FileUpload
                files={files}
                onFilesChange={setFiles}
                maxSize={50 * 1024 * 1024} // 50MB
              />
              {files.length > 0 && (
                <p className="text-xs text-muted-foreground">
                  {t("tasks:attachments.upload.selectedFiles", { count: files.length })}
                </p>
              )}
            </div>
          )}

          <div className="flex justify-end gap-3">
            <Button type="submit" disabled={isPending} icon={<CheckIcon />}>
              {t("common:actions.save")}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
