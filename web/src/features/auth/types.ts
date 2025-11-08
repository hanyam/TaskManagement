export interface AuthenticateUserRequest {
  azureAdToken: string;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string | null;
  role?: string | null;
}

export interface AuthenticationResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  user: UserDto;
}

