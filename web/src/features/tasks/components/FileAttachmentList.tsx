"use client";

import { useTranslation } from "react-i18next";

import type { TaskAttachmentDto } from "@/features/tasks/types";
import { getAttachmentTypeString } from "@/features/tasks/value-objects";

import { FileAttachmentCard } from "./FileAttachmentCard";

interface FileAttachmentListProps {
  attachments: TaskAttachmentDto[];
  onDownload: (attachmentId: string) => void;
  onDelete?: (attachmentId: string) => void;
  canDelete?: (attachment: TaskAttachmentDto) => boolean;
  markedForDeletion?: Set<string>;
  className?: string;
}

export function FileAttachmentList({
  attachments,
  onDownload,
  onDelete,
  canDelete,
  markedForDeletion,
  className
}: FileAttachmentListProps) {
  const { t } = useTranslation();
  
  if (attachments.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        <p className="text-sm">{t("tasks:attachments.empty")}</p>
      </div>
    );
  }

  // Group attachments by type
  const managerUploaded = attachments.filter(
    (a) => getAttachmentTypeString(a.type) === "ManagerUploaded"
  );
  const employeeUploaded = attachments.filter(
    (a) => getAttachmentTypeString(a.type) === "EmployeeUploaded"
  );

  return (
    <div className={className}>
      {managerUploaded.length > 0 && (
        <div className="mb-6">
          <h4 className="text-sm font-semibold text-foreground mb-3">{t("tasks:attachments.managerUploaded")}</h4>
          <div className="space-y-2">
            {managerUploaded.map((attachment) => (
              <FileAttachmentCard
                key={attachment.id}
                attachment={attachment}
                onDownload={() => onDownload(attachment.id)}
                {...(onDelete && { onDelete: () => onDelete(attachment.id) })}
                canDelete={canDelete ? canDelete(attachment) : false}
                {...(markedForDeletion?.has(attachment.id) && { isMarkedForDeletion: true })}
              />
            ))}
          </div>
        </div>
      )}

      {employeeUploaded.length > 0 && (
        <div>
          <h4 className="text-sm font-semibold text-foreground mb-3">{t("tasks:attachments.employeeUploaded")}</h4>
          <div className="space-y-2">
            {employeeUploaded.map((attachment) => (
              <FileAttachmentCard
                key={attachment.id}
                attachment={attachment}
                onDownload={() => onDownload(attachment.id)}
                {...(onDelete && { onDelete: () => onDelete(attachment.id) })}
                canDelete={canDelete ? canDelete(attachment) : false}
                {...(markedForDeletion?.has(attachment.id) && { isMarkedForDeletion: true })}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

