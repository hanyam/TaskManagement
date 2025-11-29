"use client";

import {
  CheckCircleIcon,
  CheckIcon,
  ClockIcon,
  ClipboardDocumentCheckIcon,
  InformationCircleIcon,
  PencilIcon,
  StopIcon,
  UserPlusIcon,
  XMarkIcon,
  ChartBarIcon
} from "@heroicons/react/24/outline";
import { useTranslation } from "react-i18next";

import type { TaskDto } from "@/features/tasks/types";
import type { ApiActionLink } from "@/core/api/types";
import { Button } from "@/ui/components/Button";

interface TaskActionButtonsProps {
  task: TaskDto;
  links: ApiActionLink[];
  isEditMode: boolean;
  onEdit: () => void;
  onCancelEdit: () => void;
  onSave: () => void;
  onAccept: () => void;
  onReject: () => void;
  onMarkCompleted: () => void;
  onAssign: () => void;
  onReassign: () => void;
  onUpdateProgress: () => void;
  onRequestExtension: () => void;
  onApproveExtension: () => void;
  onRequestMoreInfo: () => void;
  onReview: () => void;
  onCancel: () => void;
  isSaving?: boolean;
  isAccepting?: boolean;
  isRejecting?: boolean;
  isMarkingCompleted?: boolean;
  isCancelling?: boolean;
}

export function TaskActionButtons({
  task,
  links,
  isEditMode,
  onEdit,
  onCancelEdit,
  onSave,
  onAccept,
  onReject,
  onMarkCompleted,
  onAssign,
  onReassign,
  onUpdateProgress,
  onRequestExtension,
  onApproveExtension,
  onRequestMoreInfo,
  onReview,
  onCancel,
  isSaving = false,
  isAccepting = false,
  isRejecting = false,
  isMarkingCompleted = false,
  isCancelling = false
}: TaskActionButtonsProps) {
  const { t } = useTranslation(["tasks", "common"]);

  const hasLink = (rel: string) => links.some((link) => link.rel === rel);
  const isFinalReviewedState = task.status === 3 && task.managerRating != null;
  const isCancelled = task.status === 8;

  return (
    <div className="flex flex-wrap items-center gap-2">
      {/* Edit/Save/Cancel buttons */}
      {hasLink("update") && !isFinalReviewedState && !isCancelled && (
        <>
          {!isEditMode ? (
            <Button variant="secondary" onClick={onEdit} icon={<PencilIcon />}>
              {t("common:actions.edit")}
            </Button>
          ) : (
            <>
              <Button variant="outline" onClick={onCancelEdit} icon={<XMarkIcon />}>
                {t("common:actions.cancel")}
              </Button>
              <Button variant="primary" onClick={onSave} disabled={isSaving} icon={<CheckIcon />}>
                {t("common:actions.save")}
              </Button>
            </>
          )}
        </>
      )}

      {/* Hide action buttons if task is in final reviewed state or in edit mode */}
      {!isEditMode && hasLink("cancel") && !isFinalReviewedState && !isCancelled && (
        <Button variant="destructive" onClick={onCancel} icon={<StopIcon />} disabled={isCancelling}>
          {t("common:actions.cancel")}
        </Button>
      )}

      {!isEditMode && hasLink("accept") && !isFinalReviewedState && !isCancelled && (
        <Button variant="primary" onClick={onAccept} disabled={isAccepting} icon={<CheckIcon />}>
          {t("tasks:details.actions.accept")}
        </Button>
      )}

      {!isEditMode && hasLink("reject") && !isFinalReviewedState && !isCancelled && (
        <Button variant="outline" onClick={onReject} disabled={isRejecting} icon={<XMarkIcon />}>
          {t("tasks:details.actions.reject")}
        </Button>
      )}

      {!isEditMode && hasLink("assign") && !isFinalReviewedState && !isCancelled && (
        <Button variant="outline" onClick={onAssign} icon={<UserPlusIcon />}>
          {t("tasks:forms.assign.title")}
        </Button>
      )}

      {!isEditMode && hasLink("reassign") && !isFinalReviewedState && !isCancelled && (
        <Button variant="outline" onClick={onReassign} icon={<UserPlusIcon />}>
          {t("tasks:forms.assign.title")}
        </Button>
      )}

      {!isEditMode && hasLink("update-progress") && !isFinalReviewedState && !isCancelled && (
        <Button variant="secondary" onClick={onUpdateProgress} icon={<ChartBarIcon />}>
          {t("tasks:details.actions.updateProgress")}
        </Button>
      )}

      {!isEditMode && hasLink("mark-completed") && !isFinalReviewedState && !isCancelled && (
        <Button
          variant="destructive"
          onClick={onMarkCompleted}
          disabled={isMarkingCompleted}
          icon={<CheckCircleIcon />}
        >
          {t("tasks:details.actions.markCompleted")}
        </Button>
      )}

      {!isEditMode && hasLink("request-extension") && !isFinalReviewedState && !isCancelled && (
        <Button variant="outline" onClick={onRequestExtension} icon={<ClockIcon />}>
          {t("tasks:details.actions.requestExtension")}
        </Button>
      )}

      {!isEditMode && hasLink("approve-extension") && !isFinalReviewedState && !isCancelled && (
        <Button variant="outline" onClick={onApproveExtension} icon={<CheckCircleIcon />}>
          {t("tasks:details.actions.approveExtension")}
        </Button>
      )}

      {!isEditMode && hasLink("request-more-info") && !isFinalReviewedState && !isCancelled && (
        <Button variant="outline" onClick={onRequestMoreInfo} icon={<InformationCircleIcon />}>
          {t("tasks:details.actions.requestInfo")}
        </Button>
      )}

      {hasLink("review-completed") && (
        <Button variant="primary" onClick={onReview} icon={<ClipboardDocumentCheckIcon />}>
          {t("tasks:details.actions.review")}
        </Button>
      )}
    </div>
  );
}

