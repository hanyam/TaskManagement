"use client";

import { Dialog, DialogPanel, DialogTitle, Description } from "@headlessui/react";
import { XMarkIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import type { ApiErrorResponse } from "@/core/api/types";
import { debugError } from "@/core/debug/logger";
import { useMarkTaskCompletedMutation } from "@/features/tasks/api/queries";
import { Button } from "@/ui/components/Button";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Label } from "@/ui/components/Label";

type MarkTaskCompleteFormValues = {
  comment: string | null;
};

interface MarkTaskCompleteDialogProps {
  taskId: string;
  taskTitle: string;
  isOpen: boolean;
  onClose: () => void;
}

export function MarkTaskCompleteDialog({
  taskId,
  taskTitle,
  isOpen,
  onClose
}: MarkTaskCompleteDialogProps) {
  const { t } = useTranslation(["tasks", "common"]);
  const markCompleteMutation = useMarkTaskCompletedMutation(taskId);

  const markTaskCompleteSchema = z.object({
    comment: z.string().max(1000, t("tasks:forms.markComplete.fields.commentMaxLength")).optional().nullable()
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset
  } = useForm<MarkTaskCompleteFormValues>({
    resolver: zodResolver(markTaskCompleteSchema),
    defaultValues: {
      comment: ""
    }
  });

  const onSubmit = async (data: MarkTaskCompleteFormValues) => {
    try {
      await markCompleteMutation.mutateAsync({
        comment: data.comment || null
      });

      toast.success(t("tasks:forms.markComplete.success"));
      reset();
      onClose();
    } catch (error) {
      debugError("Failed to mark task as complete", error);
      const apiError = error as ApiErrorResponse;
      const errorMessage =
        apiError?.message || 
        (apiError?.details && apiError.details.length > 0 
          ? apiError.details[0].message 
          : null) ||
        t("common:states.error");
      toast.error(errorMessage);
    }
  };

  const handleClose = () => {
    if (!isSubmitting) {
      reset();
      onClose();
    }
  };

  return (
    <Dialog open={isOpen} onClose={handleClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/40" aria-hidden="true" />
      
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="mx-auto max-w-lg w-full rounded-xl border border-border bg-background shadow-lg">
          <div className="flex items-center justify-between border-b border-border px-6 py-4">
            <DialogTitle className="text-lg font-semibold text-foreground">
              {t("tasks:forms.markComplete.title")}
            </DialogTitle>
            <button
              type="button"
              onClick={handleClose}
              className="rounded-md text-muted-foreground hover:text-foreground transition-colors"
              disabled={isSubmitting}
            >
              <XMarkIcon className="h-5 w-5" />
            </button>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="px-6 py-4">
            <Description className="text-sm text-muted-foreground mb-4">
              {t("tasks:forms.markComplete.description")}
            </Description>

            <div className="mb-4 p-3 bg-muted rounded-md">
              <p className="text-sm font-medium text-foreground">
                {taskTitle}
              </p>
            </div>

            {/* Comment */}
            <div className="mb-6">
              <Label htmlFor="comment">{t("tasks:forms.markComplete.fields.comment")}</Label>
              <textarea
                {...register("comment")}
                id="comment"
                rows={4}
                className="mt-2 flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm transition placeholder:text-muted-foreground focus-visible:border-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:cursor-not-allowed disabled:bg-muted disabled:text-muted-foreground"
                placeholder={t("tasks:forms.markComplete.fields.commentPlaceholder")}
              />
              {errors.comment?.message && (
                <FormFieldError message={errors.comment.message} />
              )}
            </div>

            <div className="flex items-center justify-end gap-3 border-t border-border pt-4">
              <Button
                type="button"
                variant="outline"
                onClick={handleClose}
                disabled={isSubmitting}
              >
                {t("common:actions.cancel")}
              </Button>
              <Button
                type="submit"
                variant="primary"
                disabled={isSubmitting}
              >
                {isSubmitting
                  ? t("common:states.loading")
                  : t("tasks:forms.markComplete.submit")}
              </Button>
            </div>
          </form>
        </DialogPanel>
      </div>
    </Dialog>
  );
}

