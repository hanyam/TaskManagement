"use client";

import { Dialog, Transition } from "@headlessui/react";
import { CheckIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { Fragment } from "react";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import { useRequestExtensionMutation } from "@/features/tasks/api/queries";
import { displayApiError } from "@/features/tasks/utils/errorHandling";
import { Button } from "@/ui/components/Button";
import { DatePicker } from "@/ui/components/DatePicker";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Label } from "@/ui/components/Label";

interface RequestExtensionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  taskId: string;
}

export function RequestExtensionDialog({ open, onOpenChange, taskId }: RequestExtensionDialogProps) {
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
    try {
      await mutation.mutateAsync({
        requestedDueDate: new Date(values.requestedDueDate).toISOString(),
        reason: values.reason
      });
      toast.success(t("tasks:forms.extension.success"));
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
            <Dialog.Title className="text-lg font-semibold text-foreground">
              {t("tasks:forms.extension.title")}
            </Dialog.Title>
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid gap-2">
                <Label htmlFor="extensionDueDate">{t("tasks:forms.extension.fields.requestedDueDate")}</Label>
                <Controller
                  name="requestedDueDate"
                  control={form.control}
                  render={({ field }) => (
                    <DatePicker
                      id="extensionDueDate"
                      value={field.value}
                      onChange={field.onChange}
                      placeholder={t("tasks:forms.extension.fields.requestedDueDate")}
                    />
                  )}
                />
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
                  placeholder={t("tasks:forms.extension.fields.reasonPlaceholder")}
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

