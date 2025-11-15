export * from "@/core/api/types";

// Export apiClient from the appropriate environment-specific module
export { apiClient } from "@/core/api/client.browser";
