import type { ApiEnvelope } from "@/core/api/types";

export function parseEnvelope<T>(payload: unknown): ApiEnvelope<T> {
  if (!payload || typeof payload !== "object") {
    return {
      success: false,
      message: "Malformed response",
      errors: [],
      traceId: undefined,
      timestamp: undefined,
      data: undefined
    };
  }

  const envelope = payload as Partial<ApiEnvelope<T>>;

  return {
    success: envelope.success ?? false,
    data: envelope.data,
    message: envelope.message ?? null,
    errors: envelope.errors ?? [],
    traceId: envelope.traceId ?? null,
    timestamp: envelope.timestamp
  };
}

