"use client";

import { Dialog, Transition } from "@headlessui/react";
import { CheckIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { Fragment, useMemo, useEffect } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import { useUpdateTaskProgressMutation, useTaskDetailsQuery } from "@/features/tasks/api/queries";
import { displayApiError } from "@/features/tasks/utils/errorHandling";
import { Button } from "@/ui/components/Button";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Input } from "@/ui/components/Input";
import { Label } from "@/ui/components/Label";

interface UpdateProgressDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  taskId: string;
}

export function UpdateProgressDialog({ open, onOpenChange, taskId }: UpdateProgressDialogProps) {
  const { t } = useTranslation(["tasks", "validation"]);
  const mutation = useUpdateTaskProgressMutation(taskId);
  
  // Fetch task data to get last approved progress
  const { data: taskResponse } = useTaskDetailsQuery(taskId, open); // Only fetch when dialog is open
  const task = taskResponse?.data;
  
  // Find the last approved progress (status === 1) or use current progressPercentage
  const lastApprovedProgress = useMemo(() => {
    if (!task?.recentProgressHistory) {
      // If no progress history, use current progressPercentage or 0
      return task?.progressPercentage ?? 0;
    }
    
    // Find the most recent accepted progress entry (status === 1)
    const lastAccepted = task.recentProgressHistory
      .filter(p => p.status === 1) // 1 = Accepted
      .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())[0];
    
    // If found, use that; otherwise use current progressPercentage or 0
    return lastAccepted?.progressPercentage ?? task.progressPercentage ?? 0;
  }, [task]);
  
  // Ensure lastApprovedProgress is a number (fallback to 0 if undefined)
  const minProgress = lastApprovedProgress ?? 0;
  
  const form = useForm<{ progressPercentage: number; notes?: string | null }>({
    resolver: zodResolver(
      z.object({
        progressPercentage: z
          .number({ invalid_type_error: "validation:required" })
          .min(minProgress, {
            message: t("tasks:forms.progress.minProgressError", { min: minProgress }) || `Progress must be at least ${minProgress}% (last approved progress)`
          })
          .max(100),
        notes: z.string().max(500).optional()
      })
    ),
    defaultValues: {
      progressPercentage: minProgress,
      notes: ""
    }
  });
  
  // Update form when lastApprovedProgress changes or dialog opens
  useEffect(() => {
    if (open) {
      form.reset({
        progressPercentage: minProgress,
        notes: ""
      });
    }
  }, [open, minProgress, form]);

  async function onSubmit(values: { progressPercentage: number; notes?: string | null }) {
    try {
      await mutation.mutateAsync(values);
      toast.success(t("tasks:forms.progress.success"));
      onOpenChange(false);
      form.reset();
    } catch (error) {
      displayApiError(error, t("validation:server.INTERNAL_ERROR"));
    }
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
                  min={minProgress}
                  max={100}
                  {...form.register("progressPercentage", { valueAsNumber: true })}
                />
                <p className="text-xs text-muted-foreground">
                  {t("tasks:forms.progress.minProgressHint", { min: minProgress }) || `Minimum: ${minProgress}% (last approved progress). You can only increase the progress.`}
                </p>
                {form.formState.errors.progressPercentage ? (
                  <FormFieldError
                    message={form.formState.errors.progressPercentage.message || t("validation:required", {
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
                <Button type="button" variant="outline" onClick={() => onOpenChange(false)} icon={<XMarkIcon />}>
                  {t("common:actions.cancel")}
                </Button>
                <Button type="submit" disabled={mutation.isPending} icon={<CheckIcon />}>
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

