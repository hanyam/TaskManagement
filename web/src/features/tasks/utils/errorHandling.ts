import { toast } from "sonner";
import type { ApiErrorResponse } from "@/core/api/types";
import { debugError } from "@/core/debug/logger";

// Track last displayed error to prevent duplicate toasts
let lastErrorToast: { message: string; timestamp: number } | null = null;
const TOAST_DEBOUNCE_MS = 2000; // Prevent duplicate toasts within 2 seconds

/**
 * Helper function to display API errors properly in toast notifications
 */
export function displayApiError(error: unknown, fallbackMessage: string) {
  debugError("API Error", error);
  
  let errorMessage: string | null = null;
  
  if (error && typeof error === "object") {
    const apiError = error as ApiErrorResponse;
    
    // Check if we have detailed errors array
    if ("details" in apiError && apiError.details && apiError.details.length > 0) {
      // Combine all error messages into one toast, deduplicating messages
      // Normalize messages (trim, lowercase) for better deduplication
      const messageMap = new Map<string, string>(); // Map normalized -> original
      apiError.details.forEach((detail) => {
        const originalMsg = detail.field 
          ? `${detail.field}: ${detail.message}` 
          : detail.message;
        if (originalMsg && originalMsg.trim()) {
          const normalized = originalMsg.trim().toLowerCase();
          // Only keep the first occurrence of each normalized message
          if (!messageMap.has(normalized)) {
            messageMap.set(normalized, originalMsg.trim());
          }
        }
      });
      // Get unique messages (preserving original casing)
      const uniqueMessages = Array.from(messageMap.values());
      if (uniqueMessages.length === 1) {
        errorMessage = uniqueMessages[0];
      } else if (uniqueMessages.length > 1) {
        errorMessage = uniqueMessages.join("; ");
      }
    }
    // Check if we have a message property (for both ApiErrorResponse and generic Error)
    else if ("message" in apiError && apiError.message) {
      errorMessage = apiError.message;
    }
    // Check if we have rawMessage (specific to ApiErrorResponse)
    else if ("rawMessage" in apiError && apiError.rawMessage) {
      errorMessage = apiError.rawMessage;
    }
  }
  
  // Use fallback if no error message found
  if (!errorMessage) {
    errorMessage = fallbackMessage;
  }
  
  // Normalize error message for comparison (trim and lowercase for better matching)
  const normalizedMessage = errorMessage.trim().toLowerCase();
  
  // Prevent duplicate toasts within the debounce window
  const now = Date.now();
  if (
    lastErrorToast &&
    lastErrorToast.message.toLowerCase() === normalizedMessage &&
    now - lastErrorToast.timestamp < TOAST_DEBOUNCE_MS
  ) {
    debugError("Skipping duplicate error toast", { message: errorMessage, lastToast: lastErrorToast });
    return; // Skip duplicate toast
  }
  
  lastErrorToast = { message: errorMessage, timestamp: now };
  toast.error(errorMessage);
}

