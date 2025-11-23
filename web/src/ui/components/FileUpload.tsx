"use client";

import { PaperClipIcon, XMarkIcon, CloudArrowUpIcon } from "@heroicons/react/24/outline";
import { useCallback, useState, useRef } from "react";
import { useTranslation } from "react-i18next";

import { Button } from "@/ui/components/Button";
import { cn } from "@/ui/utils/cn";

export interface FileUploadItem {
  file: File;
  id: string;
  progress?: number;
  error?: string;
}

interface FileUploadProps {
  files: FileUploadItem[];
  onFilesChange: (files: FileUploadItem[]) => void;
  maxSize?: number; // in bytes
  maxFiles?: number;
  accept?: Record<string, string[]>;
  disabled?: boolean;
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

const getFileIcon = (_fileName: string) => {
  return <PaperClipIcon className="h-5 w-5 text-muted-foreground" />;
};

export function FileUpload({
  files,
  onFilesChange,
  maxSize = 50 * 1024 * 1024, // 50MB default
  maxFiles,
  accept,
  disabled = false,
  className
}: FileUploadProps) {
  const { t } = useTranslation();
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFiles = useCallback(
    (fileList: FileList | File[]) => {
      if (disabled) return;

      const validateFile = (file: File): string | null => {
        if (file.size > maxSize) {
          return t("common:fileSize.exceeds", { size: formatFileSize(maxSize, t) });
        }
        return null;
      };

      const fileArray = Array.from(fileList);
      const newFiles: FileUploadItem[] = [];

      fileArray.forEach((file) => {
        const error = validateFile(file);
        const fileItem: FileUploadItem = {
          file,
          id: `${Date.now()}-${Math.random()}`
        };
        if (!error) {
          fileItem.progress = 0;
        }
        if (error) {
          fileItem.error = error;
        }
        newFiles.push(fileItem);
      });

      const updatedFiles = [...files, ...newFiles];
      if (maxFiles && updatedFiles.length > maxFiles) {
        updatedFiles.splice(maxFiles);
      }
      onFilesChange(updatedFiles);
    },
    [files, onFilesChange, maxFiles, disabled, maxSize]
  );

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (!disabled) {
      setIsDragging(true);
    }
  }, [disabled]);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      e.stopPropagation();
      setIsDragging(false);

      if (disabled) return;

      const droppedFiles = e.dataTransfer.files;
      if (droppedFiles.length > 0) {
        handleFiles(droppedFiles);
      }
    },
    [disabled, handleFiles]
  );

  const removeFile = (id: string) => {
    onFilesChange(files.filter((f) => f.id !== id));
  };

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      handleFiles(e.target.files);
    }
  };

  return (
    <div className={cn("space-y-4", className)}>
      <div
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
        className={cn(
          "relative rounded-lg border-2 border-dashed transition-colors",
          isDragging
            ? "border-primary bg-primary/5"
            : "border-border bg-muted/30 hover:border-primary/50",
          disabled && "opacity-50 cursor-not-allowed"
        )}
      >
        <input
          ref={fileInputRef}
          type="file"
          multiple
          onChange={handleFileInputChange}
          className="hidden"
          disabled={disabled}
          accept={accept ? Object.keys(accept).join(",") : undefined}
        />
        <div className="flex flex-col items-center justify-center p-8 text-center">
          <CloudArrowUpIcon
            className={cn(
              "h-12 w-12 mb-4",
              isDragging ? "text-primary" : "text-muted-foreground"
            )}
          />
          <p className="text-sm font-medium text-foreground mb-1">
            {isDragging ? t("tasks:attachments.upload.dropHere") : t("tasks:attachments.upload.dragDrop")}
          </p>
          <p className="text-xs text-muted-foreground mb-4">
            {t("tasks:attachments.upload.orClick")} ({t("tasks:attachments.upload.maxSize", { size: formatFileSize(maxSize, t) })})
          </p>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => fileInputRef.current?.click()}
            disabled={disabled}
          >
            {t("tasks:attachments.upload.selectFiles")}
          </Button>
        </div>
      </div>

      {files.length > 0 && (
        <div className="space-y-2">
          <h4 className="text-sm font-medium text-foreground">{t("common:attachments.selectedFiles", { count: files.length })}</h4>
          <div className="space-y-2">
            {files.map((fileItem) => (
              <div
                key={fileItem.id}
                className={cn(
                  "flex items-center gap-3 rounded-md border border-border bg-background p-3",
                  fileItem.error && "border-destructive"
                )}
              >
                <div className="flex-shrink-0">{getFileIcon(fileItem.file.name)}</div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-foreground truncate">{fileItem.file.name}</p>
                  <div className="flex items-center gap-2 mt-1">
                    <p className="text-xs text-muted-foreground">{formatFileSize(fileItem.file.size, t)}</p>
                    {fileItem.progress !== undefined && fileItem.progress < 100 && (
                      <div className="flex-1 max-w-xs">
                        <div className="h-1.5 bg-muted rounded-full overflow-hidden">
                          <div
                            className="h-full bg-primary transition-all duration-300"
                            style={{ width: `${fileItem.progress}%` }}
                          />
                        </div>
                      </div>
                    )}
                    {fileItem.error && (
                      <p className="text-xs text-destructive">{fileItem.error}</p>
                    )}
                  </div>
                </div>
                <button
                  type="button"
                  onClick={() => removeFile(fileItem.id)}
                  disabled={disabled}
                  className="flex-shrink-0 rounded-md p-1 text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
                >
                  <XMarkIcon className="h-4 w-4" />
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

