"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";

import { clearClientAuthSession, setClientAuthSession } from "@/core/auth/session.client";
import type { AuthSession } from "@/core/auth/types";

interface AuthContextValue {
  session: AuthSession | undefined;
  isAuthenticated: boolean;
  setSession: (session: AuthSession) => void;
  clearSession: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps extends PropsWithChildren {
  initialSession?: AuthSession | null | undefined;
}

export function AuthProvider({ children, initialSession }: AuthProviderProps) {
  const [session, setSessionState] = useState<AuthSession | undefined>(() => initialSession ?? undefined);

  useEffect(() => {
    if (initialSession) {
      setClientAuthSession(initialSession.token, initialSession.user);
    }
  }, [initialSession]);

  const setSession = useCallback((nextSession: AuthSession) => {
    setSessionState(nextSession);
    setClientAuthSession(nextSession.token, nextSession.user);
  }, []);

  const clearSession = useCallback(() => {
    setSessionState(undefined);
    clearClientAuthSession();
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isAuthenticated: Boolean(session?.token),
      setSession,
      clearSession
    }),
    [session, setSession, clearSession]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }

  return context;
}

