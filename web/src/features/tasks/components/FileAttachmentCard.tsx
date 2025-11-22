"use client";

import { PaperClipIcon, ArrowDownTrayIcon, TrashIcon } from "@heroicons/react/24/outline";
import { format } from "date-fns";

import type { TaskAttachmentDto } from "@/features/tasks/types";
import { getAttachmentTypeString } from "@/features/tasks/value-objects";
import { Button } from "@/ui/components/Button";
import { cn } from "@/ui/utils/cn";

interface FileAttachmentCardProps {
  attachment: TaskAttachmentDto;
  onDownload: () => void;
  onDelete?: () => void;
  canDelete?: boolean;
  className?: string;
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return "0 Bytes";
  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
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
  className
}: FileAttachmentCardProps) {
  const attachmentType = getAttachmentTypeString(attachment.type);

  return (
    <div
      className={cn(
        "flex items-center gap-3 rounded-lg border border-border bg-background p-4 hover:bg-muted/50 transition-colors",
        className
      )}
    >
      <div className="flex-shrink-0">{getFileIcon()}</div>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <p className="text-sm font-medium text-foreground truncate">{attachment.originalFileName}</p>
          <span className="flex-shrink-0 text-xs px-2 py-0.5 rounded-full bg-primary/10 text-primary">
            {attachmentType === "ManagerUploaded" ? "Manager" : "Employee"}
          </span>
        </div>
        <div className="flex items-center gap-3 text-xs text-muted-foreground">
          <span>{formatFileSize(attachment.fileSize)}</span>
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
          Download
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
            Delete
          </Button>
        )}
      </div>
    </div>
  );
}

