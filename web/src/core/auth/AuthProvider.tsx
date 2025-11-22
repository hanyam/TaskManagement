"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";

import { clearClientAuthSession, getClientAuthToken, getClientUser, setClientAuthSession } from "@/core/auth/session.client";
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
      return initialSession;
    }
    
    // If no server session, check localStorage on client side
    if (typeof window !== "undefined") {
      const token = getClientAuthToken();
      const user = getClientUser();
      if (token && user) {
        return { token, user };
      }
    }
    
    return undefined;
  });

  // Sync with localStorage on mount and when initialSession changes
  useEffect(() => {
    if (initialSession) {
      // Server session takes precedence - sync to localStorage
      setClientAuthSession(initialSession.token, initialSession.user);
      setSessionState(initialSession);
    } else if (typeof window !== "undefined") {
      // No server session - check localStorage
      const token = getClientAuthToken();
      const user = getClientUser();
      if (token && user) {
        // Found session in localStorage - use it
        const localStorageSession = { token, user };
        if (!session || session.token !== token) {
          setSessionState(localStorageSession);
        }
      } else if (!token && session) {
        // No token in localStorage but session exists in state - clear it
        setSessionState(undefined);
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
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

