"use client";

import { Dialog, Transition } from "@headlessui/react";
import { CheckIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { Fragment } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import { useRequestMoreInfoMutation } from "@/features/tasks/api/queries";
import { displayApiError } from "@/features/tasks/utils/errorHandling";
import { Button } from "@/ui/components/Button";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Label } from "@/ui/components/Label";

interface RequestMoreInfoDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  taskId: string;
}

export function RequestMoreInfoDialog({ open, onOpenChange, taskId }: RequestMoreInfoDialogProps) {
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
    try {
      await mutation.mutateAsync(values);
      toast.success(t("tasks:details.actions.requestInfo"));
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

