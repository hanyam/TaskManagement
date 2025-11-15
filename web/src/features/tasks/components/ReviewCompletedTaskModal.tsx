"use client";

import { Dialog, DialogPanel, DialogTitle, Description } from "@headlessui/react";
import { XMarkIcon, StarIcon } from "@heroicons/react/24/outline";
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
      <div className="fixed inset-0 bg-black/30" aria-hidden="true" />
      
      <div className="fixed inset-0 flex items-center justify-center p-4">
        <DialogPanel className="mx-auto max-w-lg w-full rounded-lg bg-white dark:bg-gray-800 shadow-xl">
          <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 px-6 py-4">
            <DialogTitle className="text-lg font-semibold text-gray-900 dark:text-gray-100">
              {t("forms.reviewCompleted.title")}
            </DialogTitle>
            <button
              type="button"
              onClick={handleClose}
              className="rounded-md text-gray-400 hover:text-gray-500 dark:hover:text-gray-300"
            >
              <XMarkIcon className="h-5 w-5" />
            </button>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="px-6 py-4">
            <Description className="text-sm text-gray-600 dark:text-gray-400 mb-4">
              {t("forms.reviewCompleted.description")}
            </Description>

            <div className="mb-4 p-3 bg-gray-50 dark:bg-gray-700/50 rounded-md">
              <p className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {taskTitle}
              </p>
            </div>

            {/* Decision Radio Buttons */}
            <div className="mb-6">
              <Label htmlFor="decision">{t("forms.reviewCompleted.decision")}</Label>
              <div className="mt-2 space-y-2">
                <label className="flex items-center space-x-3 p-3 rounded-md border border-gray-200 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/50">
                  <input
                    {...register("decision")}
                    type="radio"
                    value="accept"
                    className="h-4 w-4 text-emerald-600 focus:ring-emerald-500"
                  />
                  <span className="text-sm text-gray-700 dark:text-gray-300">
                    {t("forms.reviewCompleted.decisionOptions.accept")}
                  </span>
                </label>
                
                <label className="flex items-center space-x-3 p-3 rounded-md border border-gray-200 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/50">
                  <input
                    {...register("decision")}
                    type="radio"
                    value="reject"
                    className="h-4 w-4 text-red-600 focus:ring-red-500"
                  />
                  <span className="text-sm text-gray-700 dark:text-gray-300">
                    {t("forms.reviewCompleted.decisionOptions.reject")}
                  </span>
                </label>
                
                <label className="flex items-center space-x-3 p-3 rounded-md border border-gray-200 dark:border-gray-700 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/50">
                  <input
                    {...register("decision")}
                    type="radio"
                    value="sendBack"
                    className="h-4 w-4 text-orange-600 focus:ring-orange-500"
                  />
                  <span className="text-sm text-gray-700 dark:text-gray-300">
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
                      <StarIconSolid className="h-8 w-8 text-amber-400" />
                    ) : (
                      <StarIcon className="h-8 w-8 text-gray-300 dark:text-gray-600" />
                    )}
                  </button>
                ))}
                <span className="ml-2 text-sm text-gray-600 dark:text-gray-400">
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
                className="mt-2 block w-full rounded-md border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:border-primary focus:ring-primary"
                placeholder={t("forms.reviewCompleted.fields.feedbackPlaceholder")}
              />
              {errors.feedback?.message && (
                <FormFieldError message={errors.feedback.message} />
              )}
            </div>

            {/* Actions */}
            <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
              <Button
                type="button"
                variant="secondary"
                onClick={handleClose}
                disabled={isSubmitting}
              >
                {t("forms.reviewCompleted.actions.cancel")}
              </Button>
              <Button
                type="submit"
                disabled={isSubmitting || rating === 0}
                className={
                  decision === "accept"
                    ? "bg-emerald-600 hover:bg-emerald-700"
                    : decision === "reject"
                    ? "bg-red-600 hover:bg-red-700"
                    : "bg-orange-600 hover:bg-orange-700"
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

