"use client";

import { PaperClipIcon, ArrowDownTrayIcon, TrashIcon } from "@heroicons/react/24/outline";
import { format } from "date-fns";
import { useTranslation } from "react-i18next";

import type { TaskAttachmentDto } from "@/features/tasks/types";
import { getAttachmentTypeString } from "@/features/tasks/value-objects";
import { Button } from "@/ui/components/Button";
import { cn } from "@/ui/utils/cn";

interface FileAttachmentCardProps {
  attachment: TaskAttachmentDto;
  onDownload: () => void;
  onDelete?: () => void;
  canDelete?: boolean;
  isMarkedForDeletion?: boolean;
  className?: string;
}

const formatFileSize = (bytes: number, t: (key: string) => string): string => {
  if (bytes === 0) return `0 ${t("common:fileSize.bytes")}`;
  const k = 1024;
  const sizes = [
    t("common:fileSize.bytes"),
    t("common:fileSize.kb"),
    t("common:fileSize.mb"),
    t("common:fileSize.gb")
  ];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${Math.round((bytes / Math.pow(k, i)) * 100) / 100} ${sizes[i]}`;
};

const getFileIcon = () => {
  return <PaperClipIcon className="h-5 w-5 text-muted-foreground" />;
};

export function FileAttachmentCard({
  attachment,
  onDownload,
  onDelete,
  canDelete = false,
  isMarkedForDeletion = false,
  className
}: FileAttachmentCardProps) {
  const { t } = useTranslation();
  const attachmentType = getAttachmentTypeString(attachment.type);

  return (
    <div
      className={cn(
        "flex items-center gap-3 rounded-lg border border-border bg-background p-4 hover:bg-muted/50 transition-colors",
        isMarkedForDeletion && "opacity-50 bg-destructive/5 border-destructive/30",
        className
      )}
    >
      <div className="flex-shrink-0">{getFileIcon()}</div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <p className={cn("text-sm font-medium truncate", isMarkedForDeletion ? "text-muted-foreground line-through" : "text-foreground")}>
            {attachment.originalFileName}
          </p>
          {isMarkedForDeletion ? (
            <span className="flex-shrink-0 text-xs px-2 py-0.5 rounded-full bg-destructive/20 text-destructive">
              {t("common:attachments.toBeDeleted")}
            </span>
          ) : (
            <span className="flex-shrink-0 text-xs px-2 py-0.5 rounded-full bg-primary/10 text-primary">
              {attachmentType === "ManagerUploaded" ? t("common:attachments.manager") : t("common:attachments.employee")}
            </span>
          )}
        </div>
        <div className="flex items-center gap-3 text-xs text-muted-foreground">
          <span>{formatFileSize(attachment.fileSize, t)}</span>
          <span>•</span>
          <span>{format(new Date(attachment.createdAt), "MMM d, yyyy")}</span>
          {attachment.uploadedByDisplayName && (
            <>
              <span>•</span>
              <span>{attachment.uploadedByDisplayName}</span>
            </>
          )}
        </div>
      </div>
      <div className="flex items-center gap-2 flex-shrink-0">
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={onDownload}
          icon={<ArrowDownTrayIcon className="h-4 w-4" />}
        >
          {t("tasks:attachments.actions.download")}
        </Button>
        {canDelete && onDelete && (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={onDelete}
            icon={<TrashIcon className="h-4 w-4" />}
            className="text-destructive hover:text-destructive hover:bg-destructive/10"
          >
            {t("tasks:attachments.actions.delete")}
          </Button>
        )}
      </div>
    </div>
  );
}

