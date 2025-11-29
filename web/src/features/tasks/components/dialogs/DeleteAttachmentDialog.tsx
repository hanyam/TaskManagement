"use client";

import { Dialog, Transition } from "@headlessui/react";
import { TrashIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { Fragment } from "react";
import { useTranslation } from "react-i18next";

import { Button } from "@/ui/components/Button";

interface DeleteAttachmentDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void | Promise<void>;
  isPending?: boolean;
}

export function DeleteAttachmentDialog({ open, onOpenChange, onConfirm, isPending = false }: DeleteAttachmentDialogProps) {
  const { t } = useTranslation(["tasks", "common"]);

  return (
    <Dialog open={open} onClose={onOpenChange} as="div" className="relative z-50">
      <Transition appear show={open} as={Fragment}>
        <div className="fixed inset-0 bg-black/40 backdrop-blur-sm" aria-hidden="true" />
      </Transition>
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <Transition
          appear
          show={open}
          as={Fragment}
          enter="transition ease-out duration-200"
          enterFrom="opacity-0 scale-95 translate-y-2"
          enterTo="opacity-100 scale-100 translate-y-0"
          leave="transition ease-in duration-150"
          leaveFrom="opacity-100 scale-100 translate-y-0"
          leaveTo="opacity-0 scale-95 translate-y-2"
        >
          <Dialog.Panel className="w-full max-w-md transform overflow-hidden rounded-xl border border-border bg-background p-6 shadow-2xl transition-all">
            <div className="flex items-start gap-4">
              <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-destructive/10">
                <TrashIcon className="h-6 w-6 text-destructive" />
              </div>
              <div className="flex-1 space-y-2">
                <Dialog.Title as="h3" className="text-lg font-semibold text-foreground">
                  {t("tasks:attachments.actions.delete")}
                </Dialog.Title>
                <p className="text-sm text-muted-foreground">{t("tasks:attachments.actions.deleteConfirm")}</p>
              </div>
            </div>
            <div className="mt-6 flex justify-end gap-3">
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                icon={<XMarkIcon />}
              >
                {t("common:actions.cancel")}
              </Button>
              <Button
                type="button"
                variant="destructive"
                onClick={onConfirm}
                disabled={isPending}
                icon={<TrashIcon />}
              >
                {t("common:actions.delete")}
              </Button>
            </div>
          </Dialog.Panel>
        </Transition>
      </div>
    </Dialog>
  );
}

