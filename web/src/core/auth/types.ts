export interface PersistedUser {
  id: string;
  email: string;
  displayName: string;
  role?: string;
}

export interface AuthSession {
  token: string;
  user: PersistedUser;
  expiresAt?: string;
}

