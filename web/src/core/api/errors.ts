import type { ApiEnvelope, ApiErrorDetail, ApiErrorResponse, ApiErrorType } from "@/core/api/types";

interface CreateApiErrorOptions {
  status: number;
  envelope?: ApiEnvelope<unknown>;
  fallbackMessage?: string;
  cause?: unknown;
}

const ERROR_CODE_MAPPING: Record<string, ApiErrorType> = {
  VALIDATION_ERROR: "validation",
  NOT_FOUND: "notFound",
  CONFLICT: "conflict",
  FORBIDDEN: "forbidden",
  UNAUTHORIZED: "unauthorized",
  INTERNAL_ERROR: "internal"
};

function mapErrorType(code?: string | null): ApiErrorType {
  if (!code) {
    return "unknown";
  }

  return ERROR_CODE_MAPPING[code] ?? "unknown";
}

function toErrorDetails(envelope?: ApiEnvelope<unknown>): ApiErrorDetail[] {
  if (!envelope?.errors?.length) {
    if (envelope?.message) {
      return [
        {
          code: "UNKNOWN",
          message: envelope.message,
          field: undefined
        }
      ];
    }

    return [];
  }

  return envelope.errors.map((error) => ({
    code: error.code,
    message: error.message,
    field: error.field
  }));
}

export function createApiError({
  status,
  envelope,
  fallbackMessage,
  cause
}: CreateApiErrorOptions): ApiErrorResponse {
  const details = toErrorDetails(envelope);
  const primaryError = details[0];
  const type = primaryError?.code ? mapErrorType(primaryError.code) : mapErrorType(undefined);

  const error = new Error(primaryError?.message ?? envelope?.message ?? fallbackMessage ?? "Request failed");
  error.name = "ApiError";

  return Object.assign(error, {
    status,
    type,
    details,
    traceId: envelope?.traceId ?? undefined,
    rawMessage: envelope?.message ?? null,
    cause
  });
}

export function createNetworkError(cause: unknown): ApiErrorResponse {
  const error = new Error("Network error");
  error.name = "ApiError";

  return Object.assign(error, {
    status: 0,
    type: "network" as const,
    details: [],
    traceId: undefined,
    rawMessage: undefined,
    cause
  });
}

