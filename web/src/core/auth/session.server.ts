import { cookies } from "next/headers";

import { AUTH_TOKEN_COOKIE, AUTH_USER_COOKIE } from "@/core/auth/constants";
import type { PersistedUser } from "@/core/auth/types";

export function getServerAuthToken(): string | undefined {
  const cookieStore = cookies();
  return cookieStore.get(AUTH_TOKEN_COOKIE)?.value;
}

export function getServerUser(): PersistedUser | undefined {
  const cookieStore = cookies();
  const persisted = cookieStore.get(AUTH_USER_COOKIE)?.value;

  if (!persisted) {
    return undefined;
  }

  try {
    return JSON.parse(persisted) as PersistedUser;
  } catch {
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
  const cookieStore = cookies();

  cookieStore.delete(AUTH_TOKEN_COOKIE);
  cookieStore.delete(AUTH_USER_COOKIE);
}

