import { toast } from "sonner";
import type { ApiErrorResponse } from "@/core/api/types";
import { debugError } from "@/core/debug/logger";

/**
 * Helper function to display API errors properly in toast notifications
 */
export function displayApiError(error: unknown, fallbackMessage: string) {
  debugError("API Error", error);
  
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
  toast.error(fallbackMessage);
}

