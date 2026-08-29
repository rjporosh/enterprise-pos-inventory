# Release Notes — v0.1.0

**Release Date:** 2026-08-10
**Milestone:** Phase A + B — Repository Architecture and Shared Infrastructure Foundation
**Status:** Foundation

---

## What's New

### 🏗️ Repository Architecture
- Created solution file `EnterprisePOS.sln` with 14 projects
- Created two independently deployable services:
  - **POS Service** — `services/pos-service/`
  - **Inventory Service** — `services/inventory-service/`
- Each service follows Clean Architecture with:
  - Domain layer
  - Application layer
  - Infrastructure layer
  - API layer
  - Unit Tests
  - Integration Tests
  - Functional Tests

### 📦 Shared Kernel
- Created `shared/shared-kernel/` with enterprise-grade primitives:
  - `Result` / `Result<T>` — Result pattern for explicit success/failure handling
  - `ValidationError` — Validation error representation
  - `Guard` — Guard clause utilities
  - `Entity<TId>` — Base entity with audit fields
  - `ValueObject` — Value object base
  - `DomainEvent` / `IDomainEvent` — Domain event primitives
  - `IAggregateRoot` — Aggregate root marker
  - `IAuditableEntity` / `ITenantEntity` / `ISoftDeletable` — Cross-cutting entity interfaces
  - `Constants` — Cross-service constants
  - `IAuditContext` — Audit context for request-scoped audit data

### 🔧 Shared Infrastructure
- Created `shared/shared-infrastructure/` with:
  - `ValidationBehavior` — FluentValidation pipeline behavior for MediatR
  - `IDbProviderFactory` / `PostgreSqlProviderFactory` — Database provider abstraction
  - `DbContextFactory` / `IDbContextFactory` — DI-scoped DbContext factory
  - `SerilogConfiguration` — Structured logging factory with Seq/file sinks
  - `DependencyInjection` — Service registration extensions
  - `LoggingOptions` — Logging configuration options

### 🐳 Docker & Containerization
- `services/pos-service/Dockerfile` — Multi-stage Docker build for POS API
- `services/inventory-service/Dockerfile` — Multi-stage Docker build for Inventory API
- `services/pos-service/docker-compose.yml` — POS full stack (PostgreSQL + Redis + RabbitMQ + POS API)
- `services/inventory-service/docker-compose.yml` — Inventory full stack
- Root `docker-compose.yml` — Unified dev environment (PostgreSQL, Redis, RabbitMQ, Seq)

### 🌐 API Foundation
- Both services expose:
  - `GET /health` — Liveness probe
  - `GET /health/live` — K8s liveness
  - `GET /health/ready` — K8s readiness
  - `GET /api/v1/system/release` — Release/build information endpoint
  - `/swagger` — Swagger UI
  - `/scalar/v1` — Scalar API reference
  - `/openapi/v1.json` — OpenAPI specification

### 🛡️ Global Exception Handling
- `GlobalExceptionHandler` middleware in both services
- Consistent `application/problem+json` responses
- Correlation ID propagation via `X-Correlation-ID` header
- Structured error responses with traceId, correlationId, timestamp

### 🔐 Authentication & Authorization (Foundation)
- CORS configured with configurable allowed origins
- Response caching middleware registered
- HTTP context accessor registered for request-scoped operations

### 📋 Architecture Decisions
- `ADR-001-service-boundaries.md` — Two independent services
- `ADR-002-clean-architecture.md` — Clean Architecture + Vertical Slice + CQRS
- `ADR-003-database-provider-abstraction.md` — PostgreSQL primary with provider abstraction
- `ADR-004-pos-inventory-communication.md` — RabbitMQ event-driven communication
- `ADR-005-result-pattern.md` — Result pattern and error handling
- `ADR-006-multi-tenancy.md` — Multi-tenancy and audit strategy
- `ADR-007-observability.md` — Logging and observability strategy

### 🧪 Testing Foundation
- Unit test projects with xUnit, FluentAssertions, Moq
- Integration test projects with Respawn for database cleanup
- Functional test projects for API-level testing
- CI pipeline (`.github/workflows/ci.yml`) for build and test

---

## Database Changes

No database migrations yet. Databases are prepared in docker-compose:
- `pos_db` — PostgreSQL for POS Service
- `inventory_db` — PostgreSQL for Inventory Service

---

## API Endpoints Added

| Service | Endpoint | Method | Description |
|---------|----------|--------|-------------|
| POS | `/health` | GET | Health check |
| POS | `/health/live` | GET | Liveness probe |
| POS | `/health/ready` | GET | Readiness probe |
| POS | `/api/v1/system/release` | GET | Release information |
| POS | `/swagger` | GET | Swagger UI |
| POS | `/scalar/v1` | GET | Scalar API docs |
| Inventory | `/health` | GET | Health check |
| Inventory | `/health/live` | GET | Liveness probe |
| Inventory | `/health/ready` | GET | Readiness probe |
| Inventory | `/api/v1/system/release` | GET | Release information |
| Inventory | `/swagger` | GET | Swagger UI |
| Inventory | `/scalar/v1` | GET | Scalar API docs |

---

## Configuration Changes

- `.gitignore` — Root-level gitignore
- `.github/workflows/ci.yml` — GitHub Actions CI pipeline
- `services/pos-service/.gitignore` — POS service gitignore
- `services/pos-service/.dockerignore` — POS Docker ignore
- `services/inventory-service/.gitignore` — Inventory service gitignore
- `services/inventory-service/.dockerignore` — Inventory Docker ignore

---

## Migration Changes

None yet. Database migrations will begin in Phase C (Inventory) and Phase F (POS).

---

## Known Issues

- Database migrations not yet implemented (expected for Phase A/B)
- No domain entities yet (Phase C/D for Inventory, Phase F/G for POS)
- No authentication/authorization implemented (Phase B/C)
- `UseSnakeCaseNamingConvention()` call in `BaseDbContext` may cause issues if Npgsql package version mismatch
- Database provider abstraction has `NotImplementedException` for non-PostgreSQL providers

---

## Deployment Notes

Services can be started independently:

```bash
# POS Service
cd services/pos-service
dotnet run --project src/PosService.API/PosService.API.csproj

# Inventory Service
cd services/inventory-service
dotnet run --project src/InventoryService.API/InventoryService.API.csproj

# Or with Docker
docker compose -f services/pos-service/docker-compose.yml up --build
docker compose -f services/inventory-service/docker-compose.yml up --build
```

---

## Rollback Notes

Rollback to pre-Phase-A by reverting to commit `365f48e`.

---

## Test Results

No functional tests yet. CI pipeline runs build successfully for compilation.

| Test Suite | Status |
|-----------|--------|
| POS Unit Tests | Pending (Phase G) |
| POS Integration Tests | Pending (Phase H) |
| Inventory Unit Tests | Pending (Phase D) |
| Inventory Integration Tests | Pending (Phase E) |
| CI Build | ✅ Compiles |

---

## Next Steps

- Phase C: Inventory database foundation (entities, DbContext, migrations)
- Phase D: Inventory Product/Catalog foundation
- Phase E: Inventory Stock/Ledger foundation
- Phase F: POS database foundation
- Phase G: POS Sales/Checkout foundation
- Phase H: POS ↔ Inventory integration

---

## 2026-08-16 — Build Health Fix Pass

**Status:** Zero-error/zero-warning verification in progress (see `handover/ai-handover.md` for current state — this file is a stale Phase A/B snapshot; trust the handover doc and `git log` over this file until it's fully reconciled).

Fixed all 71 build errors and 30 build warnings reported by a real `dotnet restore` / `dotnet build` run against the repo (this session's environment has no .NET SDK, so fixes were made by reading the exact compiler output the user supplied, then verified by reading the affected source — not guessed):

- 8x CS0104 (`PosService.Domain`): ambiguous `BaseEntity` between `PosService.Domain.Common.BaseEntity` and `SharedKernel.BaseEntity` — disambiguated with a type alias in each of the 8 affected files.
- 61x CS0234/CS0246 (`PosService.Application`): `PosService.Application.csproj` was missing its `ProjectReference` to `PosService.Domain` — added it.
- 2x errors (`shared-infrastructure`): missing `FluentValidation.DependencyInjectionExtensions` package reference (`AddValidatorsFromAssemblies`); wrong namespace for `DbLoggerCategory`.
- 30x NU1603/NU1902 warnings: `OpenTelemetry.Exporter.Prometheus.AspNetCore` was pinned to a version that was never published (`1.9.0-rc.1`); repinned the whole OpenTelemetry package set in `shared-infrastructure.csproj` to a verified, patched, mutually-compatible set (Extensions.Hosting/Exporter.OpenTelemetryProtocol 1.15.3, Instrumentation.AspNetCore 1.15.2, Instrumentation.Http/Runtime 1.15.1, Exporter.Prometheus.AspNetCore 1.15.3-beta.1), clearing 3 known moderate-severity advisories (GHSA-8785-wc3w-h8q6, GHSA-g94r-2vxg-569j, GHSA-4625-4j76-fww9).

No business logic, tests, or previously-working files touched. Full detail in `handover/ai-handover.md`.

---

## 2026-08-23 — Frontend MVP (Inventory + POS) built and verified

**Status:** First customer-demo-ready MVP frontend complete for both apps. Built against real,
already-verified backend endpoints only — see `docs/API-GAPS.md` for exact contract notes and
every documented gap. No backend files touched this session.

**Inventory app** (`frontend/inventory`): dashboard (real counts only), Products full CRUD
(list/search/filter/sort/paginate/create/edit/delete), Stock (list with low/out-of-stock filter,
in/out/adjustment/transfer). 16-component design system, Redux Toolkit + redux-saga throughout,
typed API clients written directly from the C# DTOs/controllers.

**POS app** (`frontend/pos`): terminal setup (explicit demo-access banner, no fake auth), cash
session open/close, product search + cart (client-side, unit-tested), checkout saga
(create sale → add items → complete → fetch for receipt), print-friendly receipt, sale history +
void, daily sales report (defaults to yesterday — backend generates reports overnight, not
on-demand; documented rather than faked).

**Verification (both apps):** `npm install`, `npm run typecheck`, `npm run lint`, `npm test`,
`npm run build` all PASS. Inventory: 10/10 routes build, 12/12 tests pass. POS: 5/5 routes build,
9/9 tests pass. Full command output recorded in `AI-HANDOVER.md` §F.

**Docs added:** `docs/API-GAPS.md`, `docs/AI-CODING-RULES.md`,
`docs/inventory/{ARCHITECTURE,PROGRAMMER-GUIDE,ADDING-A-CRUD}.md`,
`docs/pos/{ARCHITECTURE,README,PROGRAMMER-GUIDE}.md`, per-app `README.md`, root
`AI-HANDOVER.md`, root `NEXT-AI-PROMPT.md`.

**Known, documented gaps (not defects):** Category/Brand/Unit/Warehouse/Store/Register have no
backend CRUD, so forms use labeled manual-GUID input. Product search matches name/SKU, not
barcode. Daily report has no on-demand generation. Cash session has no GET (tracked in
localStorage). Full detail and priority ranking in `docs/API-GAPS.md`.

**Not verified:** live runtime against a running backend instance (none available in this
environment). See `AI-HANDOVER.md` §H for the recommended next step.

Trust `AI-HANDOVER.md` and `git log` over this entry for anything more recent.

---

## 2026-08-28 session

**Environment note:** this session's sandbox had `node`/`npm` only — no `dotnet` SDK, no NuGet
access. All `services/*` (.NET) work was read-only analysis; no backend code was built, run, or
modified. Full detail in `needed-credentials.md` (new file this session).

**Verified (re-ran, no regressions):** `frontend/inventory` and `frontend/pos` — install,
typecheck, lint, test, build all still PASS. Inventory 11/11 routes, 12/12 tests. POS 7/7 routes,
9/9 tests (route counts grew from the last recorded 10 and 5 as app pages were added in the prior
session; not a regression).

**Shipped this session (frontend-only, fully verified):**
- POS thermal receipt printing now supports real 58mm and 80mm paper profiles instead of one
  generic print stylesheet. `TerminalConfig.receiptPaperWidthMm` (58 | 80) is set on the Setup
  page, persisted to `localStorage` alongside the rest of terminal identity, and used by
  `features/sale/components/Receipt.tsx` to set `@page` size, printable width, and a monospace
  font sized per profile (48mm printable / 9pt for 58mm rolls, 72mm printable / 10.5pt for 80mm
  rolls). Existing saved configs without the field default to 80mm. This renders via the browser
  print dialog to any thermal printer with an OS print driver — it is not a raw ESC/POS bridge
  (that remains not-started, see `docs/ROADMAP-v3.0.md` Phase 7).

**Corrected, not shipped (documentation only):**
- `docs/API-GAPS.md` had two stale rows: it said no authentication existed anywhere and that
  multi-tenancy/subscriptions were unimplemented "beyond a marker interface." Both were true when
  written but predate commit `0e79624`, which added full `auth-service` (login/refresh/RBAC/audit/
  OTP) and `notification-service` (send/templates/preferences) — real, substantial services, just
  not yet called by either frontend app. The tenancy/subscription/trial finding was re-verified and
  is still accurate: nothing exists anywhere in the repo for licensing/subscriptions/trials.
- `needed-credentials.md` created — documents the `dotnet`-less sandbox constraint, that no demo
  credentials/seed data exist yet (seeding needs a live Postgres via `dotnet ef`, unavailable
  here), and the env vars that will be needed once auth is wired in.

**Not attempted this session (needs `dotnet` + Docker):** AuthService/NotificationService frontend
integration, the license/subscription/trial engine (does not exist in any form — this is
from-scratch backend work, not integration), enterprise demo data seeding. See `AI-HANDOVER.md`
§I for the full breakdown and exact next command.

Trust `AI-HANDOVER.md` and `git log` over this entry for anything more recent.
