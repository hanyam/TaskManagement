"use client";

import { CheckIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useMemo } from "react";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { useCreateTaskMutation } from "@/features/tasks/api/queries";
import { UserSearchInput } from "@/features/tasks/components/UserSearchInput";
import type { TaskPriority, TaskType } from "@/features/tasks/value-objects";
import { TaskPriorityEnum, TaskTypeEnum } from "@/features/tasks/value-objects";
import { Breadcrumb } from "@/ui/components/Breadcrumb";
import { Button } from "@/ui/components/Button";
import { DatePicker } from "@/ui/components/DatePicker";
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
      await mutateAsync({
        title: values.title,
        description: values.description ? values.description : null,
        priority: TaskPriorityEnum[values.priority], // Convert string to numeric enum
        assignedUserId: values.assignedUserId && values.assignedUserId.trim() !== "" 
          ? values.assignedUserId 
          : null, // Null for draft tasks
        type: TaskTypeEnum[values.type], // Convert string to numeric enum
        dueDate: values.dueDate ? new Date(values.dueDate).toISOString() : null
      });
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
      <Breadcrumb
        items={[
          { label: t("navigation:breadcrumbs.tasks"), href: `/${locale}/tasks` },
          { label: t("navigation:breadcrumbs.createTask") }
        ]}
      />
      <div className="rounded-xl border border-border bg-background p-6 shadow-sm">
        <div>
          <h1 className="font-heading text-2xl text-foreground">{t("tasks:forms.create.title")}</h1>
          <p className="text-sm text-muted-foreground">{instructions}</p>
        </div>

      <form onSubmit={form.handleSubmit(handleSubmit)} className="grid gap-6" noValidate>
        <div className="grid gap-2">
          <Label htmlFor="title">{t("tasks:forms.create.fields.title")}</Label>
          <Input id="title" {...form.register("title")} />
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
                  placeholder={t("tasks:forms.create.fields.dueDate")}
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
