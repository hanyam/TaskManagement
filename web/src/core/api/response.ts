import type { ApiEnvelope } from "@/core/api/types";

export function parseEnvelope<T>(payload: unknown): ApiEnvelope<T> {
  if (!payload || typeof payload !== "object") {
    return {
      success: false,
      message: "Malformed response",
      errors: [],
      traceId: null
    };
  }

  const envelope = payload as Partial<ApiEnvelope<T>>;

  return {
    success: envelope.success ?? false,
    ...(Object.prototype.hasOwnProperty.call(envelope, "data") ? { data: envelope.data as T } : {}),
    message: envelope.message ?? null,
    errors: envelope.errors ?? [],
    traceId: envelope.traceId ?? null,
    ...(envelope.timestamp !== undefined ? { timestamp: envelope.timestamp } : {}),
    ...(envelope.links !== undefined ? { links: envelope.links } : {})
  };
}

