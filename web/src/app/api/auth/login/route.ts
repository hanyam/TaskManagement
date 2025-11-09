import { NextResponse } from "next/server";
import { z } from "zod";

import { apiClient } from "@/core/api/client.server";
import type { ApiErrorResponse } from "@/core/api/types";
import { setServerAuthSession } from "@/core/auth/session.server";
import type { AuthSession } from "@/core/auth/types";
import type { AuthenticationResponse } from "@/features/auth/types";

const schema = z.object({
  azureAdToken: z.string().min(1, "Azure AD token is required")
});

export async function POST(request: Request) {
  const body = await request.json();
  const parsed = schema.safeParse(body);

  if (!parsed.success) {
    return NextResponse.json(
      {
        success: false,
        message: "Invalid payload",
        errors: parsed.error.issues.map((issue) => ({
          code: "VALIDATION_ERROR",
          message: issue.message,
          field: issue.path.join(".")
        }))
      },
      { status: 400 }
    );
  }

  try {
    const { data } = await apiClient.request<AuthenticationResponse>({
      path: "/authentication/authenticate",
      method: "POST",
      body: parsed.data,
      auth: false
    });

    const sessionUser: AuthSession["user"] = {
      id: data.user.id,
      email: data.user.email,
      displayName: data.user.displayName,
      ...(data.user.role ? { role: data.user.role } : {})
    };

    setServerAuthSession(data.accessToken, sessionUser, data.expiresIn);

    return NextResponse.json(
      {
        success: true,
        data: {
          user: data.user,
          token: data.accessToken,
          expiresIn: data.expiresIn
        }
      },
      {
        headers: {
          "Cache-Control": "no-store"
        }
      }
    );
  } catch (error) {
    const apiError = error as ApiErrorResponse;
    return NextResponse.json(
      {
        success: false,
        message: apiError.message,
        errors: apiError.details
      },
      {
        status: apiError.status || 500
      }
    );
  }
}

