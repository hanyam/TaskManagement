/**
 * Test helper functions for development/testing
 * These should NOT be used in production
 */

import { getClientSession, setClientAuthSession } from "@/core/auth/session.client";
import type { AuthSession } from "@/core/auth/types";

/**
 * Updates the user role in the current session (for testing purposes)
 * After calling the backend test endpoint /testing/current-user,
 * call this function to update the frontend session role
 * 
 * @param newRole - The new role to set (e.g., 'Employee', 'Manager', 'Admin')
 * @returns The updated session, or null if no session exists
 */
export function updateSessionRole(newRole: string): AuthSession | null {
  if (typeof window === "undefined") {
    return null;
  }

  const currentSession = getClientSession();
  if (!currentSession) {
    console.warn("No session found. Please log in first.");
    return null;
  }

  const updatedUser = {
    ...currentSession.user,
    role: newRole
  };

  const updatedSession: AuthSession = {
    ...currentSession,
    user: updatedUser
  };

  setClientAuthSession(
    currentSession.token,
    updatedUser,
    currentSession.expiresAt ? { expiresAt: currentSession.expiresAt } : {}
  );

  console.log("✅ Session role updated:", { oldRole: currentSession.user.role, newRole });
  
  return updatedSession;
}

/**
 * Exposes the updateSessionRole function to window for easy console access
 * Usage in browser console: window.updateSessionRole('Employee')
 */
if (typeof window !== "undefined") {
  (window as unknown as { updateSessionRole: typeof updateSessionRole }).updateSessionRole = updateSessionRole;
}

