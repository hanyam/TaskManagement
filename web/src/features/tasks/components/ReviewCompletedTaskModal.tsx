"use client";

import { Dialog, DialogPanel, DialogTitle, Description } from "@headlessui/react";
import { StarIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";
import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import type { ApiErrorResponse } from "@/core/api/types";
import { useReviewCompletedTaskMutation } from "@/features/tasks/api/queries";
import { Button } from "@/ui/components/Button";
import { FormFieldError } from "@/ui/components/FormFieldError";
import { Label } from "@/ui/components/Label";

const reviewCompletedTaskSchema = z.object({
  decision: z.enum(["accept", "reject", "sendBack"], {
    required_error: "Please select a decision"
  }),
  rating: z.number().min(1).max(5, "Rating must be between 1 and 5"),
  feedback: z.string().max(1000, "Feedback cannot exceed 1000 characters").optional().nullable()
});

type ReviewCompletedTaskFormValues = z.infer<typeof reviewCompletedTaskSchema>;

interface ReviewCompletedTaskModalProps {
  taskId: string;
  taskTitle: string;
  isOpen: boolean;
  onClose: () => void;
}

export function ReviewCompletedTaskModal({
  taskId,
  taskTitle,
  isOpen,
  onClose
}: ReviewCompletedTaskModalProps) {
  const { t } = useTranslation("tasks");
  const [rating, setRating] = useState<number>(0);
  const [hoveredRating, setHoveredRating] = useState<number>(0);
  const reviewMutation = useReviewCompletedTaskMutation(taskId);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    watch,
    setValue,
    reset
  } = useForm<ReviewCompletedTaskFormValues>({
    resolver: zodResolver(reviewCompletedTaskSchema),
    defaultValues: {
      decision: "accept",
      rating: 0,
      feedback: ""
    }
  });

  const decision = watch("decision");

  const onSubmit = async (data: ReviewCompletedTaskFormValues) => {
    try {
      await reviewMutation.mutateAsync({
        accepted: data.decision === "accept",
        rating: data.rating,
        feedback: data.feedback || null,
        sendBackForRework: data.decision === "sendBack"
      });

      toast.success(t("forms.reviewCompleted.success"));
      reset();
      setRating(0);
      onClose();
    } catch (error) {
      console.error("Review error:", error);
      
      // Extract and display all API errors
      if (error && typeof error === "object") {
        const apiError = error as ApiErrorResponse;
        
        // Check if we have detailed errors array
        if ("details" in apiError && apiError.details && apiError.details.length > 0) {
          // Display all error messages from the API
          apiError.details.forEach((detail) => {
            const errorMessage = detail.field 
              ? `${detail.field}: ${detail.message}` 
              : detail.message;
            toast.error(errorMessage);
          });
          return; // Exit after displaying errors
        }
        
        // Check if we have a message property (for both ApiErrorResponse and generic Error)
        if ("message" in apiError && apiError.message) {
          toast.error(apiError.message);
          return;
        }
        
        // Check if we have rawMessage (specific to ApiErrorResponse)
        if ("rawMessage" in apiError && apiError.rawMessage) {
          toast.error(apiError.rawMessage);
          return;
        }
      }
      
      // Final fallback for unknown errors
      toast.error(t("forms.reviewCompleted.error"));
    }
  };

  const handleClose = () => {
    reset();
    setRating(0);
    onClose();
  };

  const handleRatingClick = (value: number) => {
    setRating(value);
    setValue("rating", value, { shouldValidate: true });
  };

  return (
    <Dialog open={isOpen} onClose={handleClose} className="relative z-50">
      <div className="fixed inset-0 bg-black/40" aria-hidden="true" />
      
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="mx-auto max-w-lg w-full rounded-xl border border-border bg-background shadow-lg">
          <div className="flex items-center justify-between border-b border-border px-6 py-4">
            <DialogTitle className="text-lg font-semibold text-foreground">
              {t("forms.reviewCompleted.title")}
            </DialogTitle>
            <button
              type="button"
              onClick={handleClose}
              className="rounded-md text-muted-foreground hover:text-foreground transition-colors"
            >
              <XMarkIcon className="h-5 w-5" />
            </button>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="px-6 py-4">
            <Description className="text-sm text-muted-foreground mb-4">
              {t("forms.reviewCompleted.description")}
            </Description>

            <div className="mb-4 p-3 bg-muted rounded-md">
              <p className="text-sm font-medium text-foreground">
                {taskTitle}
              </p>
            </div>

            {/* Decision Radio Buttons */}
            <div className="mb-6">
              <Label htmlFor="decision">{t("forms.reviewCompleted.decision")}</Label>
              <div className="mt-2 space-y-2">
                <label className="flex items-center gap-3 p-3 rounded-md border border-border cursor-pointer transition-colors hover:bg-muted">
                  <input
                    {...register("decision")}
                    type="radio"
                    value="accept"
                    className="h-4 w-4 text-primary focus:ring-primary"
                  />
                  <span className="text-sm text-foreground">
                    {t("forms.reviewCompleted.decisionOptions.accept")}
                  </span>
                </label>
                
                <label className="flex items-center gap-3 p-3 rounded-md border border-border cursor-pointer transition-colors hover:bg-muted">
                  <input
                    {...register("decision")}
                    type="radio"
                    value="reject"
                    className="h-4 w-4 text-destructive focus:ring-destructive"
                  />
                  <span className="text-sm text-foreground">
                    {t("forms.reviewCompleted.decisionOptions.reject")}
                  </span>
                </label>
                
                <label className="flex items-center gap-3 p-3 rounded-md border border-border cursor-pointer transition-colors hover:bg-muted">
                  <input
                    {...register("decision")}
                    type="radio"
                    value="sendBack"
                    className="h-4 w-4 text-warning focus:ring-warning"
                  />
                  <span className="text-sm text-foreground">
                    {t("forms.reviewCompleted.decisionOptions.sendBack")}
                  </span>
                </label>
              </div>
              {errors.decision?.message && (
                <FormFieldError message={errors.decision.message} />
              )}
            </div>

            {/* Rating */}
            <div className="mb-6">
              <Label htmlFor="rating">{t("forms.reviewCompleted.fields.rating")}</Label>
              <div className="mt-2 flex items-center gap-1">
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    key={star}
                    type="button"
                    onClick={() => handleRatingClick(star)}
                    onMouseEnter={() => setHoveredRating(star)}
                    onMouseLeave={() => setHoveredRating(0)}
                    className="focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 rounded"
                  >
                    {star <= (hoveredRating || rating) ? (
                      <StarIconSolid className="h-8 w-8 text-warning" />
                    ) : (
                      <StarIcon className="h-8 w-8 text-muted-foreground" />
                    )}
                  </button>
                ))}
                <span className="ml-2 text-sm text-muted-foreground">
                  {rating > 0 ? `${rating}/5` : t("forms.reviewCompleted.fields.ratingRequired")}
                </span>
              </div>
              {errors.rating?.message && (
                <FormFieldError message={errors.rating.message} />
              )}
            </div>

            {/* Feedback */}
            <div className="mb-6">
              <Label htmlFor="feedback">{t("forms.reviewCompleted.fields.feedback")}</Label>
              <textarea
                {...register("feedback")}
                id="feedback"
                rows={4}
                className="mt-2 flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm transition placeholder:text-muted-foreground focus-visible:border-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:cursor-not-allowed disabled:bg-muted disabled:text-muted-foreground"
                placeholder={t("forms.reviewCompleted.fields.feedbackPlaceholder")}
              />
              {errors.feedback?.message && (
                <FormFieldError message={errors.feedback.message} />
              )}
            </div>

            {/* Actions */}
            <div className="flex justify-end gap-3 pt-4 border-t border-border">
              <Button
                type="button"
                variant="outline"
                onClick={handleClose}
                disabled={isSubmitting}
                icon={<XMarkIcon />}
              >
                {t("forms.reviewCompleted.actions.cancel")}
              </Button>
              <Button
                type="submit"
                disabled={isSubmitting || rating === 0}
                variant={
                  decision === "accept"
                    ? "primary"
                    : decision === "reject"
                    ? "destructive"
                    : "secondary"
                }
              >
                {isSubmitting ? (
                  <span className="flex items-center gap-2">
                    <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                    </svg>
                    {t("forms.reviewCompleted.actions.submit")}
                  </span>
                ) : (
                  t("forms.reviewCompleted.actions.submit")
                )}
              </Button>
            </div>
          </form>
        </DialogPanel>
      </div>
    </Dialog>
  );
}

