/**
 * Typed fetch wrapper for the pos-service API. Same shape as the inventory-service client
 * (see frontend/inventory/src/lib/api/client.ts) — duplicated rather than shared so the two
 * apps stay independently deployable, per the MVP brief.
 *
 * Verified against SalesController / CashSessionsController / ReportsController directly:
 * responses are the raw resource (or PagedResult<T>), errors are RFC7807 ProblemDetails via
 * `Problem(...)`. Several mutating endpoints return 204 No Content (AddItem's 200/Guid and
 * RemoveItem/Complete/Void's 204 are intentionally different — see sales.ts).
 */

import { tokenStorage } from "@/lib/auth/tokenStorage";

const BASE_URL = process.env.NEXT_PUBLIC_POS_API_URL;
const AUTH_BASE_URL = process.env.NEXT_PUBLIC_AUTH_API_URL;

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly detail?: string;

  constructor(status: number, message: string, code?: string, detail?: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.detail = detail;
  }

  get isNotFound(): boolean {
    return this.status === 404;
  }
}

export class NetworkError extends Error {
  constructor(cause: unknown) {
    super("Could not reach the POS service. Check your connection and try again.");
    this.name = "NetworkError";
    this.cause = cause;
  }
}

interface ProblemDetailsShape {
  title?: string;
  detail?: string;
  status?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

type QueryValue = string | number | boolean | undefined | null;

function toQueryString(params?: Record<string, QueryValue>): string {
  if (!params) return "";
  const sp = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === "") continue;
    sp.set(key, String(value));
  }
  const qs = sp.toString();
  return qs ? `?${qs}` : "";
}

// Deduplicates concurrent refresh attempts: if several requests 401 at once, only one
// POST /api/v1/auth/refresh goes out, and every caller awaits the same result.
let refreshInFlight: Promise<string | null> | null = null;

async function refreshAccessToken(): Promise<string | null> {
  const stored = tokenStorage.load();
  if (!stored || !AUTH_BASE_URL) return null;

  refreshInFlight ??= (async () => {
    try {
      const response = await fetch(`${AUTH_BASE_URL}/api/v1/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        body: JSON.stringify({ refreshToken: stored.refreshToken }),
      });
      if (!response.ok) {
        tokenStorage.clear();
        return null;
      }
      const body = await response.json();
      tokenStorage.save({
        accessToken: body.accessToken,
        accessTokenExpiresAtUtc: body.accessTokenExpiresAtUtc,
        refreshToken: body.refreshToken,
        refreshTokenExpiresAtUtc: body.refreshTokenExpiresAtUtc,
        userId: body.userId,
        email: body.email,
        roles: body.roles ?? [],
      });
      return body.accessToken as string;
    } catch {
      return null;
    } finally {
      refreshInFlight = null;
    }
  })();

  return refreshInFlight;
}

/** Set once by the auth slice so a hard-expired session can force a redirect from this module, which has no React/router access of its own. */
let onSessionExpired: (() => void) | null = null;
export function registerSessionExpiredHandler(handler: () => void): void {
  onSessionExpired = handler;
}

async function request<T>(
  baseUrl: string | undefined,
  path: string,
  init?: RequestInit & { query?: Record<string, QueryValue>; skipAuth?: boolean; isRetry?: boolean }
): Promise<T> {
  if (!baseUrl) {
    throw new ApiError(0, "API base URL is not configured. Set it in .env.local.", "CONFIG_MISSING");
  }

  const { query, skipAuth, isRetry, ...rest } = init ?? {};
  const url = `${baseUrl}${path}${toQueryString(query)}`;

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    Accept: "application/json",
    ...(rest.headers as Record<string, string> | undefined),
  };
  const accessToken = skipAuth ? null : tokenStorage.getAccessToken();
  if (accessToken) {
    headers.Authorization = `Bearer ${accessToken}`;
  }

  let response: Response;
  try {
    response = await fetch(url, { ...rest, headers });
  } catch (err) {
    throw new NetworkError(err);
  }

  if (response.status === 401 && accessToken && !isRetry) {
    const newAccessToken = await refreshAccessToken();
    if (newAccessToken) {
      return request<T>(baseUrl, path, { ...init, isRetry: true });
    }
    onSessionExpired?.();
  }

  if (response.status === 204) return undefined as T;

  const text = await response.text();
  const body = text ? safeJsonParse(text) : undefined;

  if (!response.ok) {
    const problem = (body ?? {}) as ProblemDetailsShape;
    throw new ApiError(
      response.status,
      problem.detail ?? problem.title ?? `Request failed with status ${response.status}`,
      problem.title,
      problem.detail
    );
  }

  return body as T;
}

function safeJsonParse(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return undefined;
  }
}

export const posApiClient = {
  get: <T>(path: string, query?: Record<string, QueryValue>) =>
    request<T>(BASE_URL, path, { method: "GET", query }),
  post: <T>(path: string, body?: unknown) =>
    request<T>(BASE_URL, path, { method: "POST", body: body !== undefined ? JSON.stringify(body) : undefined }),
  delete: <T>(path: string, body?: unknown) =>
    request<T>(BASE_URL, path, { method: "DELETE", body: body !== undefined ? JSON.stringify(body) : undefined }),
};

const INVENTORY_BASE_URL = process.env.NEXT_PUBLIC_INVENTORY_API_URL;

/** Read-only client into inventory-service, used only for product lookup during checkout (ADR-001: POS never writes to Inventory's database, and only reads via its public API). */
export const inventoryApiClient = {
  get: <T>(path: string, query?: Record<string, QueryValue>) =>
    request<T>(INVENTORY_BASE_URL, path, { method: "GET", query }),
};

/** auth-service client — register/login skip attaching a (possibly stale) Authorization header. */
export const authApiClient = {
  post: <T>(path: string, body?: unknown, opts?: { skipAuth?: boolean }) =>
    request<T>(AUTH_BASE_URL, path, {
      method: "POST",
      body: body !== undefined ? JSON.stringify(body) : undefined,
      skipAuth: opts?.skipAuth,
    }),
  get: <T>(path: string) => request<T>(AUTH_BASE_URL, path, { method: "GET" }),
};
