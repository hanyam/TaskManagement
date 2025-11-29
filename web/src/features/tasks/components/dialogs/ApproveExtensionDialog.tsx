"use client";

import { Dialog, Transition } from "@headlessui/react";
import { CheckIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { Fragment } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import { useApproveExtensionRequestMutation } from "@/features/tasks/api/queries";
import { displayApiError } from "@/features/tasks/utils/errorHandling";
import { Button } from "@/ui/components/Button";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Input } from "@/ui/components/Input";
import { Label } from "@/ui/components/Label";

interface ApproveExtensionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  taskId: string;
}

export function ApproveExtensionDialog({ open, onOpenChange, taskId }: ApproveExtensionDialogProps) {
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
    try {
      await mutation.mutateAsync({
        requestId: values.extensionRequestId,
        reviewNotes: values.reviewNotes ?? null
      });
      toast.success(t("tasks:forms.approveExtension.success"));
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

