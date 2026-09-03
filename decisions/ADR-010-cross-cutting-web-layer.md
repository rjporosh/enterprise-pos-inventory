# ADR-010: `shared-web` — the unified cross-cutting web layer

**Date:** 2026-09-03
**Status:** Accepted (M1 C1–C6 implemented; C7 success-envelope migration pending)
**Decision Maker:** Principal Architect
**Supersedes parts of:** ADR-005 (Result pattern — the Result type is now the single one below),
ADR-007 (the exception-handling half)

## Context

Before this ADR the five services (`auth`, `notification`, `inventory`, `pos`, `gateway`) had
**three incompatible `Result` types**, **four near-duplicate global exception handlers** (the gateway
had none), **four correlation-id middlewares**, and only `notification` had a
`{success,message,data,errors,traceId,timestamp}` response envelope. Worse, `inventory`/`pos`
controllers read only `result.Error.Code` and silently dropped every FluentValidation field error —
so a bad form submit returned `400 {title:"VALIDATION_ERROR", detail:null}`.

The project brief requires: one API response contract, **all** validation errors in one response,
one centralized exception handler that never leaks internals, RFC 7807 where appropriate, and
resource-based localization (English default, Bangla) for every user-visible string.

## Decision

### 1. One leaf project: `shared/shared-web`

`<FrameworkReference Include="Microsoft.AspNetCore.App"/>` + `FluentValidation` + a `ProjectReference`
to `shared-kernel` only. Deliberately **does not** reference `shared-infrastructure` (EF Core, Npgsql,
MediatR, OpenTelemetry) so `auth`/`notification`/`gateway` can use it without dragging in a database
stack a reverse proxy or a Minimal-API service does not need. Namespace `SharedWeb`.

### 2. One `Result` — `SharedKernel.Result` / `Result<T>`, extended in place

`Error` gained an optional `Field` (3rd positional param, source-compatible) and
`NotFound/Conflict/Validation/InvalidState/Unexpected` factories. `Result`/`Result<T>` gained
`IReadOnlyList<Error> Errors` (every failure flattened) and `Failure(IEnumerable<Error>)`.
`notification`'s and `auth`'s local `Result`/`Error` types are deleted. Chosen over promoting
`notification`'s model because that would have renamed `Description → Message` across ~40 handlers and
67 unit-test assertions.

### 3. One response contract

```jsonc
// success
{ "success": true, "message": "...", "data": { }, "traceId": "...", "timestamp": "..." }
// failure — every error, never just the first
{ "success": false, "message": "...",
  "errors": [ { "code": "...", "field": "...", "message": "..." } ],
  "traceId": "...", "timestamp": "..." }
```

`ApiResponse<T>` / `ApiFailureResponse` / `ApiErrorItem` live in `shared-web`.
`ApiErrorItem.Of()` normalizes field names to match frontend inputs (drops the `Request.`/`Command.`
prefix FluentValidation adds, camelCases). `ControllerBaseExtensions.ToApiResult` (MVC) and
`MinimalApiResultExtensions.ToApiResult` (Minimal API) are the per-endpoint shims — **not** a global
filter, so health/OpenAPI/release endpoints stay untouched. `ResultEnvelopeMapper` is the single
Result→(status, body) function: all validation errors returned; `Error.Code` → HTTP status via an
exact map plus `*_NOT_FOUND→404` / `*_EXISTS→409` / `*_DELETED→404` / `*_EXCEEDED→409` conventions.

**Transitional bridge (removed in M1 C7):** the failure body currently also carries RFC 7807 aliases
(`type`, `title`, `detail`, `status`) so the existing frontend clients' `problem.detail ?? problem.title`
keep resolving while C1–C6 land with **zero** coordinated frontend change. Success responses stay
**raw** (bare `Guid` / DTO / `PagedResult` / 204) until C7 flips `wrapSuccess` in lockstep with the
frontend `lib/api/client.ts`.

### 4. One exception handler — `SharedWeb.PlatformExceptionHandler : IExceptionHandler`

`AddPlatformExceptionHandling()` + `app.UseExceptionHandler()` in all five services.
- `FluentValidation.ValidationException` → 400 with **every** error.
- `OperationCanceledException` on client abort → 499, no body.
- `TimeoutException` → 504, `UnauthorizedAccessException` → 403, `KeyNotFoundException` → 404.
- Per-service domain exceptions via a registered `IExceptionMapper` list
  (`AuthExceptionMapper`, `NotificationExceptionMapper`).
- Anything unmapped → **scrubbed RFC 7807 500**: generic `detail` (real message only in Development),
  never a stack trace, SQL, or connection string; always `traceId` + `X-Correlation-Id`; one
  structured `Error` log with method / endpoint / correlation id + a best-effort
  `RootCause` / `PossibleSolution` hint for `logs/runtime-errors/` (M3).
- `[ApiController]` automatic model-validation 400 is reshaped to the same envelope via
  `ConfigurePlatformApiBehavior()`.

`InvalidOperationException` / `ArgumentException` are **not** mapped to 400 (they are almost always a
real server bug — a genuine client-input rule should be a `Result.Failure` or a domain exception +
mapper). This is a deliberate change from the old inventory/pos handler.

### 5. One request-localization pipeline — `SharedWeb.PlatformLocalization`

`AddPlatformLocalization()` + `app.UsePlatformLocalization()` (framework `UseRequestLocalization`,
per-request, auto-restored — fixes `notification`'s old static `CultureInfo.CurrentCulture =` leak).
`PlatformRequestCultureProvider` resolves `?lang=` → `Accept-Language` → a `locale`/`culture` user
claim → `en`. Supported cultures: `en`, `bn` (add a code + a `PlatformMessages.<code>.resx` to
extend). Cross-cutting strings (envelope `Response.*`, generic `Error.*`) live in
`shared-web/src/Resources/PlatformMessages[.bn].resx`; a domain `Error.Code` message is localized
only when a resx entry named exactly after the code exists, otherwise the handler's own English
`Error.Description` is kept — so localization is added incrementally with **no handler changes**.
Per-service domain resx stay local (`notification`'s `Messages.resx`).

## Consequences

- **Positive:** one contract, one handler, one Result, one localization pipeline. The all-errors
  validation bug is fixed. Every service returns an identical failure shape and an identical
  scrubbed-500. Adding a language = one resx file. No frontend change was needed for C1–C6.
- **Negative:** `shared-web` is now a dependency of all five services (light — no DB stack).
  Full message translation is incremental (only the ~11 cross-cutting keys + FluentValidation
  built-ins are Bangla today; handler-authored domain messages and frontend strings are separate
  work — M4 covers the frontend).
- **Migration risk (C7):** moving success responses into `data` and dropping the RFC 7807 aliases is
  the one change that must be coordinated backend + `frontend/*/lib/api/client.ts` + integration
  tests, per app, one endpoint group per commit. Everything before C7 is frontend-safe by design.
