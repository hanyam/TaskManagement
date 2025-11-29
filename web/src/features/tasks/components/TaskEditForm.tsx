"use client";

import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

import { DatePicker } from "@/ui/components/DatePicker";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Input } from "@/ui/components/Input";
import { Label } from "@/ui/components/Label";
import { Select } from "@/ui/components/Select";
import { UserSearchInput } from "@/features/tasks/components/UserSearchInput";

const editTaskSchema = z.object({
  title: z.string().min(1, "validation:required"),
  description: z.string().max(2000).optional().or(z.literal("")),
  priority: z.enum(["Low", "Medium", "High", "Critical"]),
  dueDate: z
    .string()
    .optional()
    .refine((value) => !value || !Number.isNaN(Date.parse(value)), {
      message: "validation:futureDate"
    }),
  assignedUserId: z.string().optional().or(z.literal(""))
});

export type EditTaskFormValues = z.infer<typeof editTaskSchema>;

interface TaskEditFormProps {
  form: ReturnType<typeof useForm<EditTaskFormValues>>;
  onSubmit: (values: EditTaskFormValues) => void | Promise<void>;
}

export function TaskEditForm({ form, onSubmit }: TaskEditFormProps) {
  const { t } = useTranslation(["tasks", "common", "validation"]);

  return (
    <form className="space-y-6" onSubmit={form.handleSubmit(onSubmit)}>
      {/* Title Field */}
      <div className="space-y-2">
        <Label htmlFor="edit-title">{t("tasks:forms.create.fields.title")}</Label>
        <Input
          id="edit-title"
          {...form.register("title")}
          placeholder={t("tasks:forms.create.fields.titlePlaceholder")}
        />
        {form.formState.errors.title && (
          <FormFieldError message={t(form.formState.errors.title.message || "validation:required")} />
        )}
      </div>

      {/* Description Field */}
      <div className="space-y-2">
        <Label htmlFor="edit-description">{t("tasks:forms.create.fields.description")}</Label>
        <textarea
          id="edit-description"
          rows={4}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground"
          {...form.register("description")}
          placeholder={t("tasks:forms.create.fields.descriptionPlaceholder")}
        />
        {form.formState.errors.description && (
          <FormFieldError message={t(form.formState.errors.description.message || "validation:required")} />
        )}
      </div>

      {/* Priority Field */}
      <div className="space-y-2">
        <Label htmlFor="edit-priority">{t("tasks:forms.create.fields.priority")}</Label>
        <Controller
          name="priority"
          control={form.control}
          render={({ field }) => (
            <Select
              id="edit-priority"
              value={field.value}
              onChange={field.onChange}
              options={[
                { value: "Low", label: t("common:priority.low") },
                { value: "Medium", label: t("common:priority.medium") },
                { value: "High", label: t("common:priority.high") },
                { value: "Critical", label: t("common:priority.critical") }
              ]}
            />
          )}
        />
        {form.formState.errors.priority && (
          <FormFieldError message={t(form.formState.errors.priority.message || "validation:required")} />
        )}
      </div>

      {/* Due Date Field */}
      <div className="space-y-2">
        <Label htmlFor="edit-dueDate">{t("tasks:forms.create.fields.dueDate")}</Label>
        <Controller
          name="dueDate"
          control={form.control}
          render={({ field }) => (
            <DatePicker
              id="edit-dueDate"
              value={field.value || ""}
              onChange={field.onChange}
              placeholder={t("tasks:forms.create.fields.dueDatePlaceholder")}
            />
          )}
        />
        {form.formState.errors.dueDate && (
          <FormFieldError message={t(form.formState.errors.dueDate.message || "validation:required")} />
        )}
      </div>

      {/* Assigned User Field */}
      <div className="space-y-2">
        <Label htmlFor="edit-assignedUserId">{t("tasks:forms.create.fields.assignedUserId")}</Label>
        <Controller
          name="assignedUserId"
          control={form.control}
          render={({ field }) => (
            <UserSearchInput
              value={field.value || ""}
              onChange={field.onChange}
              placeholder={t("tasks:forms.create.fields.assignedUserIdPlaceholder")}
              error={!!form.formState.errors.assignedUserId}
            />
          )}
        />
        {form.formState.errors.assignedUserId && (
          <FormFieldError message={t(form.formState.errors.assignedUserId.message || "validation:required")} />
        )}
      </div>
    </form>
  );
}

