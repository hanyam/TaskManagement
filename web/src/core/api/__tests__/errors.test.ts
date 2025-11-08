import { describe, expect, it } from "vitest";

import { createApiError, createNetworkError } from "@/core/api/errors";

describe("API error helpers", () => {
  it("maps validation envelope to ApiErrorResponse", () => {
    const error = createApiError({
      status: 400,
      envelope: {
        success: false,
        message: "Validation failed",
        errors: [
          {
            code: "VALIDATION_ERROR",
            message: "Title is required",
            field: "Title"
          }
        ]
      }
    });

    expect(error.status).toBe(400);
    expect(error.type).toBe("validation");
    expect(error.details).toHaveLength(1);
    expect(error.details[0]?.message).toBe("Title is required");
  });

  it("creates network error fallback", () => {
    const error = createNetworkError(new Error("Connection lost"));
    expect(error.type).toBe("network");
    expect(error.status).toBe(0);
  });
});

