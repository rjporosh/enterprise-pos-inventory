/**
 * Typed fetch wrapper for the inventory-service API.
 *
 * IMPORTANT — deviation from docs/API-CONTRACT.md:
 * The contract document describes a `{ success, data, meta }` envelope, but the actual
 * ProductsController / StocksController implementations return the resource (or
 * PagedResult<T>) directly, and return RFC7807 ProblemDetails on error via `Problem(...)`.
 * This client is written against the REAL behavior (verified by reading the controllers),
 * not the aspirational contract doc. See docs/API-GAPS.md, "API-CONTRACT.md drift".
 */

const BASE_URL = process.env.NEXT_PUBLIC_INVENTORY_API_URL;

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly detail?: string;
  readonly traceId?: string;

  constructor(status: number, message: string, code?: string, detail?: string, traceId?: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.detail = detail;
    this.traceId = traceId;
  }

  /** True for validation-shaped 400s the UI can show inline rather than as a toast. */
  get isValidation(): boolean {
    return this.status === 400;
  }

  get isNotFound(): boolean {
    return this.status === 404;
  }
}

export class NetworkError extends Error {
  constructor(cause: unknown) {
    super("Could not reach the inventory service. Check your connection and try again.");
    this.name = "NetworkError";
    this.cause = cause;
  }
}

interface ProblemDetailsShape {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  traceId?: string;
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
  path: string,
  init?: RequestInit & { query?: Record<string, QueryValue> }
): Promise<T> {
  if (!BASE_URL) {
    throw new ApiError(
      0,
      "NEXT_PUBLIC_INVENTORY_API_URL is not configured. Set it in .env.local.",
      "CONFIG_MISSING"
    );
  }

  const { query, ...rest } = init ?? {};
  const url = `${BASE_URL}${path}${toQueryString(query)}`;

  let response: Response;
  try {
    response = await fetch(url, {
      ...rest,
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
        ...rest.headers,
      },
    });
  } catch (err) {
    throw new NetworkError(err);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  const body = text ? safeJsonParse(text) : undefined;

  if (!response.ok) {
    const problem = (body ?? {}) as ProblemDetailsShape;
    throw new ApiError(
      response.status,
      problem.detail ?? problem.title ?? `Request failed with status ${response.status}`,
      problem.code ?? problem.title,
      problem.detail,
      problem.traceId
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

export const apiClient = {
  get: <T>(path: string, query?: Record<string, QueryValue>) => request<T>(path, { method: "GET", query }),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "POST", body: body !== undefined ? JSON.stringify(body) : undefined }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PUT", body: body !== undefined ? JSON.stringify(body) : undefined }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};
