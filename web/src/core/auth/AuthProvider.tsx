"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";

import { clearClientAuthSession, getClientSession, setClientAuthSession } from "@/core/auth/session.client";
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
  // Initialize session from server (cookies) or client (localStorage)
  const [session, setSessionState] = useState<AuthSession | undefined>(() => {
    // First, try to use server-provided session
    if (initialSession) {
      // Sync to localStorage immediately (synchronously) so API client can access it
      if (typeof window !== "undefined") {
        setClientAuthSession(
          initialSession.token,
          initialSession.user,
          initialSession.expiresAt
            ? {
                expiresAt: initialSession.expiresAt
              }
            : {}
        );
      }
      return initialSession;
    }
    
    // If no server session, check localStorage on client side
    // getClientSession validates expiration and returns null if expired
    if (typeof window !== "undefined") {
      const clientSession = getClientSession();
      if (clientSession) {
        return clientSession;
      }
    }
    
    return undefined;
  });

  // Sync with localStorage on mount and when initialSession changes
  useEffect(() => {
    if (initialSession) {
      // Server session takes precedence - ensure it's synced to localStorage
      // (Already synced in useState initializer, but ensure it's still in sync)
      if (typeof window !== "undefined") {
        const clientSession = getClientSession();
        if (!clientSession || clientSession.token !== initialSession.token) {
          setClientAuthSession(
            initialSession.token,
            initialSession.user,
            initialSession.expiresAt
              ? {
                  expiresAt: initialSession.expiresAt
                }
              : {}
          );
        }
      }
      setSessionState(initialSession);
    } else if (typeof window !== "undefined") {
      // No server session - check localStorage (validates expiration)
      const clientSession = getClientSession();
      if (clientSession) {
        // Found valid session in localStorage - use it
        if (!session || session.token !== clientSession.token) {
          setSessionState(clientSession);
        }
      } else if (session) {
        // No valid session in localStorage but session exists in state - clear it
        setSessionState(undefined);
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialSession]);

  const setSession = useCallback((nextSession: AuthSession) => {
    setSessionState(nextSession);
    setClientAuthSession(
      nextSession.token,
      nextSession.user,
      nextSession.expiresAt
        ? {
            expiresAt: nextSession.expiresAt
          }
        : {}
    );
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

