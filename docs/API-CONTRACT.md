# API Contract

## Purpose

This document is the contract between backend services and frontend applications.

Frontend agents must not invent API endpoints.

Backend agents must not silently change existing API contracts.

---

# API Versioning

All public APIs must follow the project's selected versioning strategy.

Example:

/api/v1/...

---

# Response contract — CURRENT (as of M1 C1–C6, 2026-09-03)

Authoritative shape and mechanics: `docs/programmers-guide/api-response-contract.md` +
`decisions/ADR-010`. Summary:

## Failure (implemented, all 5 services)

```jsonc
{
  "success": false,
  "message": "…",                                   // localized (?lang / Accept-Language, en default)
  "errors": [ { "code": "…", "field": "…", "message": "…" } ],   // EVERY error, never just the first
  "traceId": "…",
  "timestamp": "…"
  // transitional until M1 C7: also "type","title","detail","status" (RFC7807 aliases)
}
```

HTTP status derives from the first error's `code` (`*_NOT_FOUND`→404, `*_EXISTS`→409, …).
Unhandled 500s are `application/problem+json`, RFC 7807, scrubbed (no stack trace / SQL / secrets).

## Success

Currently the **raw resource** (bare `Guid` / DTO / `PagedResult<T>` / `204`) — unchanged from the
original implementation. **M1 C7** wraps it as
`{ "success": true, "message": "…", "data": …, "traceId": "…", "timestamp": "…" }` in lockstep with
`frontend/*/src/lib/api/client.ts`. `PagedResult<T>` is `{ items, totalCount, pageNumber, pageSize }`.

---

# Pagination

All large collections must support:

- page
- pageSize
- totalCount
- totalPages

---

# Filtering

Endpoints should document supported filters explicitly.

---

# Sorting

Endpoints should document supported sortable fields.

---

# Idempotency

Required for financial operations such as:

- Sale completion
- Payment
- Refund
- Return
- Offline synchronization
- Cash operations where appropriate

The same idempotency key must never create duplicate financial transactions.

---

# Correlation IDs

Requests must propagate a correlation/trace identifier.

---

# Barcode

Barcode lookup must be optimized for POS use.

---

# Sales

Sale completion must be atomic from the business perspective.

---

# Returns

Returns must reference the original sale.

A return must never mutate the historical sale into a different financial event.

---

# Inventory

Inventory-changing operations must be concurrency-safe.

---

# Frontend Rule

If an endpoint is not documented here or in generated OpenAPI documentation, frontend agents must not invent it.

Backend changes affecting frontend contracts must update this document.