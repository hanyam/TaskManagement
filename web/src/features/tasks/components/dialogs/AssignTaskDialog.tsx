"use client";

import { Dialog, Transition } from "@headlessui/react";
import { CheckIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { Fragment } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import { displayApiError } from "@/features/tasks/utils/errorHandling";
import type { AssignTaskRequest } from "@/features/tasks/types";
import { Button } from "@/ui/components/Button";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Input } from "@/ui/components/Input";
import { Label } from "@/ui/components/Label";

interface AssignTaskDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  mutation: {
    mutateAsync: (payload: AssignTaskRequest) => Promise<unknown>;
    isPending: boolean;
  };
  mode?: "assign" | "reassign";
}

export function AssignTaskDialog({ open, onOpenChange, mutation, mode = "assign" }: AssignTaskDialogProps) {
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
    try {
      const userIds = values.userIds
        .split(",")
        .map((id) => id.trim())
        .filter(Boolean);

      await mutation.mutateAsync({ userIds });
      toast.success(
        mode === "assign" ? t("tasks:forms.assign.success") : t("tasks:forms.assign.success")
      );
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
              {t("tasks:forms.assign.title")}
            </Dialog.Title>
            <p className="text-sm text-muted-foreground">{t("tasks:forms.assign.description")}</p>
            <form className="space-y-4" onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid gap-2">
                <Label htmlFor="assignUserIds">{t("tasks:forms.assign.fields.userIds")}</Label>
                <Input
                  id="assignUserIds"
                  placeholder={t("tasks:forms.assign.fields.userIdsPlaceholder")}
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

