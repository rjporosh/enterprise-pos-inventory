# API response contract

Every service returns one shape. Defined in `shared/shared-web/src/ApiResponse.cs`;
see [`decisions/ADR-010`](../../decisions/ADR-010-cross-cutting-web-layer.md).

## Failure (final)

```jsonc
{
  "success": false,
  "message": "One or more validation errors occurred.",   // localized to the request culture
  "errors": [
    { "code": "VALIDATION_ERROR", "field": "sellingPrice", "message": "Selling price must be > 0." },
    { "code": "VALIDATION_ERROR", "field": "sku",          "message": "SKU is required." }
  ],
  "traceId": "…",
  "timestamp": "2026-09-03T19:03:12Z"
}
```

- **Always every error**, never just the first.
- `field` is omitted when the error is not tied to one input (business-rule / not-found).
- `field` is normalized to match a frontend form input (`Request.SellingPrice` → `sellingPrice`).
- HTTP status comes from the first error's `code` — see [result-pattern.md](result-pattern.md).

### Transitional note (until M1 C7)

The failure body currently **also** carries `type` / `title` / `detail` / `status` (RFC 7807
aliases) so existing frontend clients that read `problem.detail ?? problem.title` keep working
while the backend migration lands. These are removed in M1 C7 once
`frontend/*/src/lib/api/client.ts` reads `message` / `errors`. Do not build new clients against
them.

## Success

Today (until M1 C7) success is the **raw resource** — a bare `Guid`, a DTO, a `PagedResult<T>`,
or `204 No Content` — exactly as before. After C7:

```jsonc
{ "success": true, "message": "…", "data": { … }, "traceId": "…", "timestamp": "…" }
```

## Unhandled 500

`application/problem+json`, RFC 7807, scrubbed — see [exception-handling.md](exception-handling.md).

## Writing an endpoint

MVC (`inventory`/`pos`): `return this.ToApiResult(result);`
Minimal API (`auth`/`notification`): `return result.ToApiResult(httpContext);`
`[ApiController]` model-validation 400s are auto-reshaped by `ConfigurePlatformApiBehavior()`.
