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

const BASE_URL = process.env.NEXT_PUBLIC_POS_API_URL;

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

async function request<T>(
  baseUrl: string | undefined,
  path: string,
  init?: RequestInit & { query?: Record<string, QueryValue> }
): Promise<T> {
  if (!baseUrl) {
    throw new ApiError(0, "API base URL is not configured. Set it in .env.local.", "CONFIG_MISSING");
  }

  const { query, ...rest } = init ?? {};
  const url = `${baseUrl}${path}${toQueryString(query)}`;

  let response: Response;
  try {
    response = await fetch(url, {
      ...rest,
      headers: { "Content-Type": "application/json", Accept: "application/json", ...rest.headers },
    });
  } catch (err) {
    throw new NetworkError(err);
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
