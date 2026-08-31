import { authApiClient } from "./client";

export interface TokenPair {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  userId: string;
  email: string;
  roles: string[];
}

export interface LoginInput {
  email: string;
  password: string;
}

export interface RegisterInput {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
}

export interface CurrentUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string | null;
  isEmailVerified: boolean;
  createdAtUtc: string;
  lastLoginAtUtc: string | null;
  roles: string[];
}

// Verified against services/auth-service/src/AuthService.Api/Endpoints/AuthEndpoints.cs directly.
export const authApi = {
  login: (input: LoginInput) => authApiClient.post<TokenPair>("/api/v1/auth/login", input, { skipAuth: true }),
  register: (input: RegisterInput) =>
    authApiClient.post<TokenPair>("/api/v1/auth/register", input, { skipAuth: true }),
  refresh: (refreshToken: string) =>
    authApiClient.post<TokenPair>("/api/v1/auth/refresh", { refreshToken }, { skipAuth: true }),
  logout: (refreshToken: string) => authApiClient.post<void>("/api/v1/auth/logout", { refreshToken }),
  me: () => authApiClient.get<CurrentUser>("/api/v1/auth/me"),
};
