/**
 * localStorage-backed access/refresh token persistence, read by both the Redux auth slice
 * (features/auth/slice.ts) and the API client's Authorization-header/401-refresh logic
 * (lib/api/client.ts). Split into its own module so client.ts can read/write tokens without
 * importing the Redux slice (which would create a circular import: slice -> api client -> slice).
 *
 * SSR-safe: every function no-ops (or returns null) when `window` isn't available, matching the
 * existing pattern in features/session/slice.ts (loadPersisted/persist).
 */

const STORAGE_KEY = "pos.auth.v1";

export interface StoredTokens {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
  userId: string;
  email: string;
  roles: string[];
}

export const tokenStorage = {
  load(): StoredTokens | null {
    if (typeof window === "undefined") return null;
    try {
      const raw = window.localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as StoredTokens) : null;
    } catch {
      return null;
    }
  },

  save(tokens: StoredTokens): void {
    if (typeof window === "undefined") return;
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(tokens));
  },

  clear(): void {
    if (typeof window === "undefined") return;
    window.localStorage.removeItem(STORAGE_KEY);
  },

  getAccessToken(): string | null {
    return tokenStorage.load()?.accessToken ?? null;
  },
};
