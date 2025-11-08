import { AUTH_TOKEN_COOKIE, AUTH_USER_COOKIE } from "@/core/auth/constants";
import type { PersistedUser } from "@/core/auth/types";

export const AUTH_TOKEN_STORAGE_KEY = AUTH_TOKEN_COOKIE;
export const AUTH_USER_STORAGE_KEY = AUTH_USER_COOKIE;

const isBrowser = typeof window !== "undefined";

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

export function setClientAuthSession(
  token: string,
  user: PersistedUser,
  options: { remember?: boolean; expiresInSeconds?: number } = {}
): void {
  if (!isBrowser) {
    return;
  }

  const { remember = true, expiresInSeconds = 60 * 60 * 6 } = options;
  const expirationDate = new Date(Date.now() + expiresInSeconds * 1000);

  window.localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, token);
  window.localStorage.setItem(AUTH_USER_STORAGE_KEY, JSON.stringify(user));

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

  document.cookie = `${AUTH_TOKEN_STORAGE_KEY}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Strict; Secure`;
  document.cookie = `${AUTH_USER_STORAGE_KEY}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Strict; Secure`;
}

