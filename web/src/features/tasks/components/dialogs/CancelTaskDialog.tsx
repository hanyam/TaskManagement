"use client";

import { Dialog, Transition } from "@headlessui/react";
import { StopIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { Fragment } from "react";
import { useTranslation } from "react-i18next";

import { Button } from "@/ui/components/Button";

interface CancelTaskDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void | Promise<void>;
  isPending?: boolean;
}

export function CancelTaskDialog({ open, onOpenChange, onConfirm, isPending = false }: CancelTaskDialogProps) {
  const { t } = useTranslation(["tasks", "common"]);

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
              {t("tasks:details.actions.cancelTask")}
            </Dialog.Title>
            <p className="text-sm text-muted-foreground">{t("tasks:details.actions.cancelTaskConfirm")}</p>
            <div className="flex justify-end gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                icon={<XMarkIcon />}
              >
                {t("common:actions.close")}
              </Button>
              <Button
                type="button"
                variant="destructive"
                onClick={onConfirm}
                icon={<StopIcon />}
                disabled={isPending}
              >
                {t("tasks:details.actions.cancelTask")}
              </Button>
            </div>
          </Dialog.Panel>
        </Transition>
      </div>
    </Dialog>
  );
}

