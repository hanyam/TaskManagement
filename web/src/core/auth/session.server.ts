import { cookies } from "next/headers";

import { AUTH_TOKEN_COOKIE, AUTH_USER_COOKIE } from "@/core/auth/constants";
import type { PersistedUser } from "@/core/auth/types";

/**
 * Decodes JWT token payload (server-side compatible)
 * Works in both Node.js and Edge Runtime
 */
function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    if (!token || typeof token !== "string") {
      return null;
    }

    const parts = token.split(".");
    if (parts.length !== 3) {
      return null;
    }

    // Try Buffer first (Node.js), fallback to atob (Edge Runtime)
    let decoded: string;
    try {
      // Node.js environment
      if (typeof Buffer !== "undefined") {
        decoded = Buffer.from(parts[1], "base64").toString("utf-8");
      } else {
        // Edge Runtime environment
        decoded = atob(parts[1]);
      }
    } catch {
      return null;
    }

    const payload = JSON.parse(decoded);
    return payload as Record<string, unknown>;
  } catch {
    return null;
  }
}

/**
 * Checks if a token is expired
 * @param token JWT token string
 * @returns true if token is expired or will expire within 1 minute
 */
function isTokenExpired(token: string): boolean {
  try {
    const payload = decodeJwtPayload(token);
    if (payload?.exp && typeof payload.exp === "number") {
      // JWT exp is in seconds, convert to milliseconds
      const expirationTime = payload.exp * 1000;
      // Consider expired if less than 1 minute remaining (buffer for clock skew)
      return expirationTime - Date.now() < 60_000;
    }
    // If we can't determine expiration, assume it's valid (let backend decide)
    return false;
  } catch {
    // If validation fails, assume valid (let backend decide)
    return false;
  }
}

export function getServerAuthToken(): string | undefined {
  try {
    const cookieStore = cookies();
    const token = cookieStore.get(AUTH_TOKEN_COOKIE)?.value;
    
    if (!token) {
      return undefined;
    }

    // Check if token is expired
    if (isTokenExpired(token)) {
      // Don't clear here - let the caller handle it to avoid multiple cookies() calls
      // Just return undefined to indicate token is invalid
      return undefined;
    }

    return token;
  } catch {
    // If anything fails, return undefined (safe fallback)
    return undefined;
  }
}

export function getServerUser(): PersistedUser | undefined {
  try {
    const cookieStore = cookies();
    const persisted = cookieStore.get(AUTH_USER_COOKIE)?.value;

    if (!persisted) {
      return undefined;
    }

    try {
      return JSON.parse(decodeURIComponent(persisted)) as PersistedUser;
    } catch {
      return undefined;
    }
  } catch {
    // If cookies() fails, return undefined
    return undefined;
  }
}

export function setServerAuthSession(token: string, user: PersistedUser, expiresInSeconds = 60 * 60 * 6): void {
  const cookieStore = cookies();
  const expiration = new Date(Date.now() + expiresInSeconds * 1000);
  const serializedUser = encodeURIComponent(JSON.stringify(user));

  cookieStore.set({
    name: AUTH_TOKEN_COOKIE,
    value: token,
    path: "/",
    httpOnly: true,
    sameSite: "strict",
    secure: true,
    expires: expiration
  });

  cookieStore.set({
    name: AUTH_USER_COOKIE,
    value: serializedUser,
    path: "/",
    httpOnly: false,
    sameSite: "strict",
    secure: true,
    expires: expiration
  });
}

export function clearServerAuthSession(): void {
  try {
    const cookieStore = cookies();

    cookieStore.delete(AUTH_TOKEN_COOKIE);
    cookieStore.delete(AUTH_USER_COOKIE);
  } catch {
    // Ignore errors when clearing cookies (might fail in some contexts)
  }
}

