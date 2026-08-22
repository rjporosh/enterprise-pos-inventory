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

# Successful Response

Single resource:

{
  "success": true,
  "data": {},
  "meta": null
}

Collection:

{
  "success": true,
  "data": [],
  "meta": {
    "page": 1,
    "pageSize": 25,
    "totalCount": 100,
    "totalPages": 4
  }
}

---

# Error Response

Financial/business/API errors must expose stable machine-readable error codes.

Example:

{
  "type": "...",
  "title": "...",
  "status": 400,
  "code": "PRODUCT_NOT_FOUND",
  "detail": "Product was not found.",
  "traceId": "..."
}

Production errors must not expose internal stack traces or secrets.

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