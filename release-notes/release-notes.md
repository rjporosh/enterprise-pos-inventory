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

## 2026-08-30 session

**Environment note:** same `node`/`npm`-only sandbox as the prior session — no `dotnet` SDK. This
was the exact next command from the prior session's handover: barcode generation for the
Inventory app, frontend-only.

**Verified (re-ran):** `frontend/inventory` — install, typecheck, lint, test, build all PASS
(11/11 routes, 15/15 tests, up from 12 as a new component test file was added). `frontend/pos` and
`services/*` were not touched this session.

**Shipped this session (frontend-only, fully verified):**
- Barcode label generation for the Inventory product catalog, using `jsbarcode` (new npm
  dependency) to render `Product.Barcode` as a scannable Code128 SVG:
  - Live preview on the product form as a barcode value is typed or scanned in.
  - A "Label" action on each row of the products list (shown only when that product has a
    barcode) opening a print-friendly label view.
  - A "Print barcode label" button on the product edit/detail page.
  - Code128 was used rather than a numeric symbology (EAN/UPC) because the backend stores
    `Barcode` as free text with no format constraint — Code128 encodes whatever was actually
    saved instead of rejecting it.
  - This covers barcode *generation/printing* only. Barcode *scanning* into the POS search box
    and barcode-aware backend search remain the pre-existing gap already tracked in
    `docs/API-GAPS.md` ("Barcode lookup / barcode-aware search") — unrelated and unchanged.

**Not attempted this session (needs `dotnet` + Docker):** everything else on the prior session's
remaining-tasks list — auth-service/notification-service frontend integration, the
license/subscription/trial engine, enterprise demo data seeding. See `AI-HANDOVER.md` §J for the
full breakdown.

Trust `AI-HANDOVER.md` and `git log` over this entry for anything more recent.

## 2026-08-31 session — Phase 1 build/test/migration/Docker baseline (first session with real dotnet + Docker)

**Environment note:** for the first time across all sessions recorded in this file, the sandbox had
a real **.NET 10 SDK (10.0.400) and Docker** available. Every fix below was verified against real
`dotnet build`/`dotnet test`/`dotnet ef`/`docker compose` runs, not read-only source inspection —
see `AI-HANDOVER.md` §L for the full list with exact evidence.

**Headline result:** all four services (`auth-service`, `notification-service`, `pos-service`,
`inventory-service`) now build with **0 errors / 0 warnings** (including 0 known-vulnerable
dependencies — 3 separate NuGet security advisories were silently `<NoWarn>`-suppressed rather than
fixed; all three are now actually fixed), all unit tests pass (48 + 18), all integration tests pass
against a real Postgres/RabbitMQ/Redis stack (7 + 1) **repeatably** (re-run twice with no
flakiness), and all four services run as Docker containers via a corrected root
`docker-compose.yml`, passing `/health` on every one of them.

**This was not a clean bill of health going in.** Ten independent, previously-undetected bugs were
found and fixed this session — several of them severe enough that they would have blocked or
crashed a real deployment. In order of how they'd surface to a real user:

1. **`InventoryService.API`/`PosService.API` used the plain `Microsoft.NET.Sdk` instead of
   `Microsoft.NET.Sdk.Web`.** `appsettings.json` was never copied to the build/publish output for
   either service — every real run (published binary, Docker container) would start with **zero
   configuration** and crash on the first request needing `Database:ConnectionString`. This is the
   single most severe bug found: it affected both core services and every way of running them
   except `dotnet run` from source with `ASPNETCORE_ENVIRONMENT` unset.
2. **MediatR's `ValidationBehavior<,>` pipeline was registered `AddSingleton` while depending on
   FluentValidation's `IValidator<T>`, which is Scoped.** This throws
   `"Cannot consume scoped service ... from singleton"` for *every* validated request the moment
   DI scope validation is enabled — which is exactly what happens in the `Development` environment
   that `docker-compose.yml` sets for every service. Fixed to `AddScoped`.
3. **`GetAllProductsHandler`'s DTO mapping dereferenced `p.Category.Name`/`p.Brand.Name`/
   `p.Unit.Symbol` directly, but `ProductRepository.GetPagedAsync` never `.Include()`d those
   navigation properties.** `GET /api/v1/products` — the products list, one of the most-hit
   endpoints in the whole product — throws `NullReferenceException` for every row the instant a
   real product exists. Only passed by accident in earlier sessions because the test database was
   empty. Fixed with the missing `.Include()` calls.
4. **`auth-service`'s design-time `IDesignTimeDbContextFactory` didn't match its runtime
   `AddDbContext` registration** (different database name, and missing the
   `.MigrationsHistoryTable("__ef_migrations_history", "auth")` override). Following the
   documented `dotnet ef database update` workflow applies migrations tracked in
   `public.__EFMigrationsHistory`; the running app checks `auth.__ef_migrations_history`, finds it
   empty, and tries to re-run every migration against tables that already exist — a **guaranteed
   crash-loop on first boot** after any by-the-book deploy. Fixed by aligning the design-time
   factory with the runtime configuration.
5. **`notification-service` throws `CultureNotFoundException` on every single request in its Docker
   image**, including `/health`. The `aspnet:10.0-alpine` base image ships with
   `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true` and no ICU data by default; this service's
   `LocalizationMiddleware`/`ResourceLocalizationService` do real culture-aware work
   (English/Bangla). Fixed by installing `icu-libs` and turning invariant mode back off in the
   Dockerfile.
6. **`Microsoft.OpenApi` 2.0.0 (high-severity, GHSA-v5pm-xwqc-g5wc) and `System.Security.Cryptography.Xml`
   9.0.0 (8 separate high-severity DoS advisories) were shipped in `inventory-service`'s and
   `pos-service`'s runtime output**, silently hidden behind `<NoWarn>NU1903</NoWarn>` rather than
   fixed. Pinned both to patched versions (2.7.5 and 9.0.18) and removed the suppression.
7. **Both integration test projects' `Testcontainers.*` packages were pinned to 4.0.0**, which pulls
   a vulnerable `SSH.NET` (high severity, GHSA-q939-rpr3-3284), also hidden behind `<NoWarn>`.
   Bumped to 4.14.0; fixed the resulting obsolete-constructor warnings from the version bump.
   Test-only `Azure.Identity`/`Microsoft.IdentityModel.*` transitive vulnerabilities (pulled in by
   `Microsoft.AspNetCore.Mvc.Testing`) were fixed the same way.
8. **`WebApplicationFactory<object>` in both services' integration test base classes** — `object`'s
   assembly has no entry point, so every integration test using it threw
   `InvalidOperationException` before making a single HTTP call. Fixed by exposing the top-level
   `Program` class as `public partial class Program {}` in both `Program.cs` files (the standard
   fix for this well-known ASP.NET Core minimal-API testing gap) and referencing `Program` instead
   of `object`.
9. **A hand-authored EF migration's seed data used `DateTime.Parse("2025-01-01T00:00:00Z")`**,
   which despite the `Z` suffix returns `DateTimeKind.Local` from `DateTime.Parse` (a well-known
   .NET footgun) — Npgsql then refuses to write it to a `timestamptz` column with
   `"a UTC DateTime is required"` on any machine not running in the UTC timezone. Fixed with
   explicit `new DateTime(..., DateTimeKind.Utc)`.
10. **`docker-compose.yml` at the repo root only defined infrastructure (Postgres/Redis/RabbitMQ/Seq)
    with no application containers at all**, referenced undeclared `redis-data`/`seq-data`
    volumes, and only ever created one of the four databases the platform actually needs. Rewritten
    to create all four databases (new `scripts/postgres/init/01-create-databases.sh`) and run all
    four services as containers. Two more services'-own `docker-compose.yml`/`.dev.yml` files had
    the *same* connection-string key name bug as #1's config-loading fix exposed
    (`ConnectionStrings__DefaultConnection`, which neither service's `Program.cs` ever reads —
    the real key is `Database:ConnectionString`) and one had a wrong Docker build context; all
    fixed. `pos-service`'s standalone compose file referenced a `rabbitmq` host with no `rabbitmq`
    service defined at all; added one. `inventory-service`'s `Dockerfile` had
    `ENV ASPNETCORE_URLS=http+:8080` (missing `//`) — fixed.

**Also fixed, lower severity:** CORS `AllowedOrigins` on `pos-service` and `auth-service` pointed at
Angular/Vite dev ports (4200/5173) left over from a different template project, not this repo's
actual Next.js apps; `notification-service` had no CORS policy configured at all despite needing
one for the planned in-app notification bell. All four now allow `localhost:3000`/`:3001`.
`.gitignore`'s `.env.*` pattern was silently excluding `.env.example` from every commit (both
frontend apps' example files, referenced by their own READMEs, had never actually been committed);
added a negation and recreated both files. `frontend/inventory` and `frontend/pos`'s READMEs had
their example `NEXT_PUBLIC_*_API_URL` ports swapped relative to every other doc in the repo. Added
`launchSettings.json` for `InventoryService.API`/`PosService.API` (previously only
`auth-service`/`notification-service` had one, at ports 5002/5001 matching the rest of the repo's
existing documentation). Hand-authored EF migrations for `pos-service` (`InitialCreate`,
`AddDailySalesReport`) and `inventory-service` (`AddIntegrationEventInbox`) were regenerated with
real `dotnet ef` tooling, producing proper matching `Designer.cs`/`ModelSnapshot.cs` pairs for the
first time.

**Verification performed (all real, all this session):**
```
dotnet build EnterprisePOS.sln                       0 errors, 0 warnings
dotnet build (auth-service, notification-service)     0 errors, 0 warnings each
dotnet test EnterprisePOS.sln                         48+18 unit, 7+1 integration — all pass, twice in a row
dotnet ef database update  (all 4 services)           applied cleanly against a live Postgres
docker compose build (all 4 API images)               all succeed
docker compose up -d                                  all 4 containers healthy, GET /health = 200 on all 4
curl-based smoke test                                 create + list a real product through inventory-api container;
                                                       open-cash-session correctly rejected with a structured
                                                       ProblemDetails error (no seeded store/register — expected,
                                                       tracked in docs/API-GAPS.md)
```

**Not touched this session:** the API Gateway (confirmed not to exist anywhere in the repo — the
prior assumption that a YARP gateway had been added was incorrect), auth/notification frontend
integration, the license/subscription/trial engine. See `AI-HANDOVER.md` §L for the prioritized
next-steps list.

## 2026-08-31 session (continued) — API Gateway shipped (Phase 3 core routing)

Same session as the Phase 1 baseline above. Built `services/gateway` (`Gateway.Api`, YARP 2.3.0,
its own `Gateway.sln`) — see `decisions/ADR-008-api-gateway.md` for the full design rationale.

**Verified real:** `dotnet build`/`dotnet test` (3/3 hermetic tests) both 0 issues; `docker compose
build` + `up -d` brings up all 5 API containers (gateway + 4 services) healthy; real routing
confirmed through the running container — `GET localhost:5010/api/v1/products` reached
`inventory-api` and got a real response, `POST localhost:5010/api/v1/auth/login` reached
`auth-api`, `GET localhost:5010/health/services` correctly reports all 4 downstream services via
YARP's active health checks. Uses port **5010**, not 5000 — macOS's AirPlay Receiver claims 5000
by default, which would break `docker compose up` out of the box on every Mac (found by hitting
exactly that conflict).

**Also fixed:** `Serilog.Sinks.Seq` had never actually been wired on any of the four existing
services (nor `shared-infrastructure`'s shared `SerilogConfiguration`), despite the `enterprise-seq`
container running in every compose stack since Phase J — nothing had ever shipped logs to it.
Added an optional `Seq:Url` config key (same "optional, falls back gracefully" pattern as the
existing OTLP tracing config) to all five services now. Reconfirmed all four existing services
still build 0/0 after the change.

**Not done:** neither frontend app has been repointed at the gateway yet (still call each
service's port directly) — the natural next step, deliberately left out of this milestone's scope.
No auth/tenant propagation, circuit breakers, or retry policies at the gateway (see the ADR for
why — these correctly follow, not precede, auth/tenant work happening elsewhere). See
`AI-HANDOVER.md` §M for the full breakdown.

## 2026-08-31 session (continued) — both frontend apps repointed at the gateway

Closed the gap left by the gateway milestone above. `frontend/inventory/.env.example` and
`frontend/pos/.env.example` now default `NEXT_PUBLIC_*_API_URL` to `http://localhost:5010` (the
gateway) instead of each service's own port — zero code changes needed since the gateway proxies
every route these apps already call unchanged.

**Verified real, browser-driven (not just curl):** ran both apps' real `npm run dev` servers
against the live Docker gateway/backend stack and loaded pages in an actual headless browser
(Inventory dashboard + products list, POS terminal + setup) — 0 console errors, 0 failed requests,
real data round-tripped end-to-end through gateway → inventory-service. Then ran the full
definition-of-done loop for both apps: `typecheck`, `lint`, `test` (15/15 Inventory, 9/9 POS),
`build` — all green, no regressions from before this change.

**Found, not fixed:** `npm install` on both apps surfaced 8 pre-existing dependency vulnerabilities
(esbuild — moderate, via vite/vitest; postcss + sharp — high, via Next.js's own transitive deps).
The only fix path bumps Next.js from 15 to 16.3.3, a major-version upgrade across both apps —
correctly left for its own dedicated pass with full re-verification rather than a drive-by
`npm audit fix --force` mid-session. Tracked in `AI-HANDOVER.md` §M, not silently dropped.

## 2026-08-31 session (continued) — auth-service wired into both frontend apps

`auth-service` is now integrated into both `frontend/inventory` and `frontend/pos`: typed API
client, Redux auth slice + saga (login, logout, session hydration, automatic refresh-and-retry on
401), a `/login` page in each app, and an `AppShell` route guard. POS's Setup page no longer has a
manual "Cashier ID" GUID field — the cashier is now the signed-in user.

**Verified real, browser-driven** (not just curl or unit tests): registered a real user through
the live gateway, then drove both apps' actual `npm run dev` servers with a headless browser —
real login, real name/email shown post-login, real logout, and a route-guard redirect confirmed by
visiting a protected page while logged out. 0 console errors, 0 real failed requests throughout.

**Found and fixed along the way:** a previously-latent test-infrastructure bug — Node.js 22+'s
experimental built-in `localStorage` global shadows jsdom's own with a non-functional stub, which
crashed every test touching `localStorage` (the new auth slice tests, 9 per app, were the first to
do so). Fixed for both apps via `cross-env NODE_OPTIONS=--no-experimental-webstorage` on the `test`
script plus an explicit jsdom URL in `vitest.config.ts`.

**Full definition-of-done, both apps, all green:** `frontend/inventory` typecheck/lint/test
(24/24)/build (12/12 routes); `frontend/pos` typecheck/lint/test (18/18)/build (8/8 routes).

**Not done:** no register/forgot-password UI (login-only this pass), no RBAC-aware UI, no tenant
isolation anywhere yet (the necessary prerequisite for the licensing/subscription engine requested
in this project's brief). See `AI-HANDOVER.md` §N for the full breakdown and exact next steps.

## 2026-08-31 session (continued) — POS was unusable from scratch; fixed, plus a critical Id bug

Discovered, while preparing a product usage guide, that **the POS app was completely unusable in a
fresh deployment**: zero stores/registers existed anywhere and no endpoint could create one.
Added `POST/GET /api/v1/stores` and `/api/v1/registers` (the Domain/Repository layers already
existed in `pos-service`; only Application/API were missing) plus a new bridging endpoint,
`POST /api/v1/cashiers/ensure`, since pos-service's `Cashier` entity turned out to be completely
separate from auth-service's `User` (own database, ADR-001) — the earlier assumption that
`cashierId = auth User.Id` was wrong and caused every sale/session call to fail `CASHIER_NOT_FOUND`.

**A second, more severe bug surfaced while verifying the first fix**: the very first store ever
created came back with `id: Guid.Empty`. Root cause — `PosService.Domain.Common.BaseEntity`'s
constructor never generated an Id (unlike `InventoryService`'s and `SharedKernel`'s equivalents),
silently affecting **every entity in pos-service** (Store, CashRegister, Cashier, Sale, SaleItem,
CashSession, Customer, Payment) — a second insert of any of them would have violated its primary
key. This had been flagged in a much older handover as a known discrepancy and never fixed, and no
test had ever caught it (the sole POS integration test is a health check). Fixed with a one-line
constructor addition and new regression assertions.

**Verified real, browser-driven, full loop**: logged in, entered a real store/register, saved
(triggering the cashier-ensure call), opened a cash session with a real balance — topbar correctly
showed "SESSION OPEN · 500.00 OPENING", confirmed in Postgres directly with real non-empty,
correctly-linked IDs throughout. Full backend suite after all changes: 48+19 unit, 7+1 integration,
all passing, 0 build warnings.

See `AI-HANDOVER.md` §O for the full writeup, including two Docker-image-not-rebuilt process
mistakes made and self-caught along the way (worth reading if you hit a mysterious 404 after a
code change that definitely compiled).

## 2026-08-31 session (continued) — GUIDE.md, and a full design for tenancy/licensing

Added `GUIDE.md` at the repo root: a start-from-zero usage walkthrough (run the stack, register an
account, add a product, set up a POS terminal, complete a sale). Every command in it was actually
run and verified against the live stack this session — including an honest "Known limitations"
section (no sign-up UI, no picker UI for reference data, no subscription/tenancy/RBAC yet) rather
than glossing over gaps.

Added `decisions/ADR-009-tenancy-and-licensing.md`: a concrete, file-path-level design for the
tenant isolation + subscription/licensing/trial engine this project's brief calls for (3-day
trial, POS-only/Inventory-only/Combined plans, product-count-based tiers, configurable pricing).
**Design only — not implemented.** Covers where Tenant lives (`auth-service`, created at
registration, a new `tenant_id` JWT claim), where Plan/Subscription/trial-expiry lives (a new
`services/billing-service`), how enforcement happens (in `pos-service`/`inventory-service`
themselves, not the gateway, via a synchronous entitlements check), and what's explicitly out of
scope (a real payment provider integration).

### Session totals

Five commits, each a real, independently-verified milestone: Phase 1 backend baseline (10 bugs
fixed), API Gateway, frontend-repointed-at-gateway, auth integration (both apps), and Store/
Register/Cashier CRUD (2 more bugs fixed, including one that broke every entity in `pos-service`).
**13 previously-undetected bugs found and fixed in total this session**, several severe enough to
have blocked any real deployment or usage — see `AI-HANDOVER.md` §L–§P for the complete list and
evidence. All five backend services build 0 errors/0 warnings, all tests pass (67+ unit, 8
integration), all run in Docker with passing health checks, and the full login → catalog → POS
checkout path was verified end-to-end through a real browser against the live stack.
