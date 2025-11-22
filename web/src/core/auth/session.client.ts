import { AUTH_TOKEN_COOKIE, AUTH_USER_COOKIE } from "@/core/auth/constants";
import type { AuthSession, PersistedUser } from "@/core/auth/types";

export const AUTH_TOKEN_STORAGE_KEY = AUTH_TOKEN_COOKIE;
export const AUTH_USER_STORAGE_KEY = AUTH_USER_COOKIE;
const SESSION_STORAGE_KEY = "tm.session";

const isBrowser = typeof window !== "undefined";

/**
 * Decodes JWT token to extract expiration time
 * @param token JWT token string
 * @returns Expiration timestamp in milliseconds, or null if invalid
 */
function getTokenExpiration(token: string): number | null {
  try {
    const parts = token.split(".");
    if (parts.length !== 3) {
      return null;
    }

    const payload = JSON.parse(atob(parts[1]));
    if (payload.exp) {
      // JWT exp is in seconds, convert to milliseconds
      return payload.exp * 1000;
    }

    return null;
  } catch {
    return null;
  }
}

/**
 * Checks if a token is expired
 * @param token JWT token string
 * @param expiresAt Optional ISO string expiration date from session
 * @returns true if token is expired or will expire within 1 minute
 */
export function isTokenExpired(token: string, expiresAt?: string): boolean {
  // Check expiresAt from session first (more reliable)
  if (expiresAt) {
    const expirationDate = new Date(expiresAt);
    // Consider expired if less than 1 minute remaining (buffer for clock skew)
    return expirationDate.getTime() - Date.now() < 60_000;
  }

  // Fallback to JWT expiration claim
  const tokenExp = getTokenExpiration(token);
  if (tokenExp) {
    // Consider expired if less than 1 minute remaining
    return tokenExp - Date.now() < 60_000;
  }

  // If we can't determine expiration, assume it's valid (let backend decide)
  return false;
}

export function getClientAuthToken(): string | undefined {
  if (!isBrowser) {
    return undefined;
  }

  return window.localStorage.getItem(AUTH_TOKEN_STORAGE_KEY) ?? undefined;
}

export function getClientUser(): PersistedUser | undefined {
  if (!isBrowser) {
    return undefined;
  }

  const persisted = window.localStorage.getItem(AUTH_USER_STORAGE_KEY);
  if (!persisted) {
    return undefined;
  }

  try {
    return JSON.parse(persisted) as PersistedUser;
  } catch {
    return undefined;
  }
}

/**
 * Gets the full session from localStorage (including expiresAt)
 * Also validates that the token is not expired
 */
export function getClientSession(): AuthSession | null {
  if (!isBrowser) {
    return null;
  }

  const token = getClientAuthToken();
  const user = getClientUser();

  if (!token || !user) {
    return null;
  }

  // Try to get expiresAt from stored session
  let expiresAt: string | undefined;
  try {
    const storedSession = window.localStorage.getItem(SESSION_STORAGE_KEY);
    if (storedSession) {
      const parsed = JSON.parse(storedSession) as AuthSession;
      expiresAt = parsed.expiresAt;
    }
  } catch {
    // Ignore parsing errors
  }

  // Check if token is expired
  if (isTokenExpired(token, expiresAt)) {
    // Token is expired, clear it
    clearClientAuthSession();
    return null;
  }

  return {
    token,
    user,
    ...(expiresAt ? { expiresAt } : {})
  };
}

export function setClientAuthSession(
  token: string,
  user: PersistedUser,
  options: { remember?: boolean; expiresInSeconds?: number; expiresAt?: string } = {}
): void {
  if (!isBrowser) {
    return;
  }

  const { remember = true, expiresInSeconds = 60 * 60 * 6 } = options;
  const expirationDate = new Date(Date.now() + expiresInSeconds * 1000);
  const expiresAt = options.expiresAt ?? expirationDate.toISOString();

  window.localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, token);
  window.localStorage.setItem(AUTH_USER_STORAGE_KEY, JSON.stringify(user));

  // Store full session with expiresAt for validation
  const session: AuthSession = { token, user, expiresAt };
  window.localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));

  const cookieDirectives = [
    `${AUTH_TOKEN_STORAGE_KEY}=${token}`,
    "Path=/",
    "SameSite=Strict",
    "Secure",
    `Expires=${expirationDate.toUTCString()}`
  ];

  document.cookie = cookieDirectives.join("; ");
  document.cookie = [
    `${AUTH_USER_STORAGE_KEY}=${encodeURIComponent(JSON.stringify(user))}`,
    "Path=/",
    "SameSite=Strict",
    "Secure",
    `Expires=${expirationDate.toUTCString()}`
  ].join("; ");

  if (!remember) {
    document.cookie = `${AUTH_TOKEN_STORAGE_KEY}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Strict; Secure`;
    document.cookie = `${AUTH_USER_STORAGE_KEY}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Strict; Secure`;
  }
}

export function clearClientAuthSession(): void {
  if (!isBrowser) {
    return;
  }

  window.localStorage.removeItem(AUTH_TOKEN_STORAGE_KEY);
  window.localStorage.removeItem(AUTH_USER_STORAGE_KEY);
  window.localStorage.removeItem(SESSION_STORAGE_KEY);

  document.cookie = `${AUTH_TOKEN_STORAGE_KEY}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Strict; Secure`;
  document.cookie = `${AUTH_USER_STORAGE_KEY}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Strict; Secure`;
}

