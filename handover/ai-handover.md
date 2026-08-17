# AI Handover — Enterprise POS & Inventory Backend

**Last Updated:** 2026-08-17T00:00:00+00:00 (patch session, see "Session: 2026-08-17 — Dev-environment & testability patch" below)
**Current Branch:** main
**Status:** ALL PHASES COMPLETE (A through L). Phase L was followed by an unverified
dev-environment/testability patch this session — **a real `dotnet build`/`dotnet test` run has
never once happened on this repo across any session.** Treat "0 errors, 0 warnings" claims below
as the last session's *source-reading* assessment, not a confirmed build.

---

## ⚠️ Read first: environment limitation (still true this session)

Every AI session, including this one, has run with **no .NET SDK installed and no
NuGet/apt/network access** (confirmed again this session: `dotnet` not found, `api.nuget.org`
returns 403 in this sandbox). All code changes are made from careful reading of the existing
source, cross-checked against official docs/searches where possible — never from an actual
compiler or test runner. **This is the single most important fact for the next agent**: nothing
in this repo's history has ever been build-verified by an AI. The very first thing the next
agent (or the user) should do in an environment with a real .NET 10 SDK is:

```bash
dotnet restore EnterprisePOS.sln
dotnet build EnterprisePOS.sln
dotnet test EnterprisePOS.sln
```

and fix whatever that surfaces — there will likely be at least a few compile errors from 12+
phases of never-compiled code, on top of the specific bugs already found and fixed below.

---

## Current Phase: DONE (functionally) / UNVERIFIED (mechanically)

All 12 phases (A–L) are complete at the source level. Both services are *designed* to be:
- **Independently runnable** (no cross-service dependency at startup)
- **Jointly runnable** (RabbitMQ bridges POS→Inventory stock sync)
- **Scalar/v1 API explorer** at `/scalar/v1` (dev mode) — **this session fixed the reason it
  wasn't showing up; see below**
- **0 build errors, 0 build warnings** — unverified, see warning above

---

## Session: 2026-08-17 — Dev-environment & testability patch

**User's request:** no `Properties/`/`launchSettings.json` in either API project, no Scalar/Swagger
UI showing up from `dotnet run`, make sure unit/integration/load/stress tests pass, update release
notes + this file.

**What I could and couldn't do:** No SDK/network in this sandbox (see warning above), so I did a
careful static/source review — verifying things like relative-path math, `.sln` structure, and
known ASP.NET Core testing semantics (confirmed default `WebApplicationFactory` environment via
web search) — rather than an actual build. Everything below is source-level, not compiler-verified.

### Root cause of "no Scalar/Swagger UI" — FOUND AND FIXED

Both `Program.cs` files only call `app.MapOpenApi()` / `app.MapScalarApiReference()` inside
`if (app.Environment.IsDevelopment())`. With no `Properties/launchSettings.json`, `dotnet run`
never sets `ASPNETCORE_ENVIRONMENT=Development`, so the app silently ran as `Production` and
skipped that whole block — no OpenAPI, no Scalar, nothing wrong with the code itself.

**Fix:** added `Properties/launchSettings.json` to both API projects:
- `services/pos-service/src/PosService.API/Properties/launchSettings.json` — port 5001, `launchUrl: scalar/v1`, `ASPNETCORE_ENVIRONMENT=Development` on all profiles (http/https/IIS Express).
- `services/inventory-service/src/InventoryService.API/Properties/launchSettings.json` — port 5002, same pattern.

Ports match what every other doc in the repo already assumed (`docker-compose.dev.yml`,
`ai-handover.md`'s own "How to run" section, `LOAD-TESTING.md`, `STRESS-TESTING.md`,
`PROGRAMMERS-GUIDE.md`, release notes) — verified this by grepping all of them before picking
5001/5002, not guessing.

### Other real bugs found by static review (would have failed a real build/test run)

1. **Both `*.FunctionalTests.csproj` had a broken `ProjectReference` path.**
   `..\..\..\src\PosService.API\PosService.API.csproj` (and the Inventory equivalent) resolves to
   `services/src/PosService.API/...`, which doesn't exist — the correct path only needs two `..`
   levels, not three (confirmed by literally resolving the path with Python, not by eye).
   `dotnet restore` would have failed on these two projects outright.
   **Fixed** in both csproj files (now `..\..\src\...`).

2. **Both `*.FunctionalTests` projects existed on disk but were never added to
   `EnterprisePOS.sln`.** `dotnet build/test EnterprisePOS.sln` was silently skipping them
   entirely — including `ReleaseEndpointTests.cs`, which is the test file that actually checks
   `/scalar/v1`, `/openapi/v1.json`, `/health`, `/metrics`, etc. **Fixed**: added both projects to
   the `.sln` (new GUIDs `4D060D9E-E9C9-42BB-8A3C-CBD2C25566D2` for POS,
   `58B7C45B-2FC2-41F1-98FC-6526F6D50AEE` for Inventory), nested under the right solution folders,
   with `Debug|Release` build configs. Verified the `.sln` is still structurally balanced
   (20 `Project`/`EndProject` pairs, 3 matched `GlobalSection`s, 20 unique GUIDs) — but **a real
   Visual Studio / `dotnet` load of this `.sln` has not happened**, so double-check it opens
   cleanly.

3. **Both `ReleaseEndpointTests.cs` used `WebApplicationFactory<object>`.** This is broken:
   `WebApplicationFactory<TEntryPoint>` needs `TEntryPoint` to resolve to the actual API
   assembly so it can find the entry point and content root; `object` resolves to
   `System.Private.CoreLib`, so this would not have bootstrapped the real app (likely a
   fixture-construction failure or requests hitting nothing meaningful). Neither `Program.cs`
   exposed a public `Program` type (top-level-statement `Program` is `internal` by default).
   **Fixed**: added `public partial class Program { }` to the end of both `Program.cs` files, and
   changed both test fixtures to `WebApplicationFactory<Program>`. Confirmed via web search that
   `WebApplicationFactory` defaults to the `Development` environment (a historical bug where this
   didn't apply to minimal-hosting apps, dotnet/aspnetcore#33889, was fixed years before this
   .NET 10 codebase) — so once bootstrapped correctly, these tests should exercise the same
   Scalar/OpenAPI code path as a real `dotnet run`.

I did **not** exhaustively re-read every line of business logic, every unit test, or the k6
load/stress scripts' assertions this session — I spot-checked the k6 scripts' `BASE_URL`s (5001/
5002, consistent) and confirmed `Scalar.AspNetCore 2.2.0`'s `/scalar/v1` route naming via web
search, but did not verify e.g. that `CreateSaleHandlerTests.cs`'s mocks actually compile against
current handler signatures, or run any k6 script (`k6` isn't installed here either).

### What's still unverified / left for the next agent

Everything. No command in this repo's history has ever actually been run. In priority order:

1. **`dotnet restore EnterprisePOS.sln && dotnet build EnterprisePOS.sln`** — fix whatever
   compile errors surface. Given 12 phases + this patch were all written blind, expect some.
2. **`dotnet test EnterprisePOS.sln`** — unit + integration + functional tests. Integration tests
   need Postgres up (`docker compose -f services/pos-service/docker-compose.dev.yml up -d postgres`
   and the Inventory equivalent, or `docker compose up -d` for both + RabbitMQ).
3. **Manually confirm Scalar UI**: `dotnet run --project services/pos-service/src/PosService.API`
   then open `http://localhost:5001/scalar/v1` in a browser — this is the concrete proof the
   original complaint is resolved. Repeat for Inventory on port 5002.
4. **Load tests**: `k6 run -e BASE_URL=http://localhost:5001 ... scripts/load-test/pos-load-test.js`
   (needs real `STORE_ID`/`REGISTER_ID`/`CASHIER_ID`/`CASH_SESSION_ID` seed data — check
   `pos-load-test.js`'s header comment) and the Inventory equivalent.
5. **Stress tests**: `k6 run scripts/stress-test/pos-stress-test.js` and the Inventory equivalent
   (same seed-data caveat).
6. Once green, bump release notes from `v1.0.1` to whatever's next and remove the "unverified"
   caveats added in this patch.
7. The pre-existing "Known Issues" list below (hand-authored migrations, placeholder
   `VoidedSalesCount`, no transactional outbox, default-warehouse-only, `BaseEntity` duplication)
   is still untouched — none of it was in scope for this session's request.

---

## This session (2026-08-17): Phase K + L completion

### Phase K — Testing (completed)

**New test files:**

| File | Tests | What's covered |
|------|-------|----------------|
| `PosService.UnitTests/Sales/CreateSaleHandlerTests.cs` | 6 | CreateSaleHandler — valid path, inactive store, null store, inactive register, inactive cashier, null session |
| `PosService.UnitTests/Sales/SaleHandlerTests.cs` | 14 | AddSaleItemHandler (4), CompleteSaleHandler (5), VoidSaleHandler (4), GetSaleByIdHandler (1) |
| `PosService.UnitTests/Domain/SaleTests.cs` | 8 | Sale aggregate — create, RecalculateTotals, Complete, Void, edge cases |
| `PosService.UnitTests/Domain/SaleItemTests.cs` | 7 | SaleItem — LineTotal, Quantity, Discount, Tax, Guard throws |
| `PosService.FunctionalTests/ReleaseEndpointTests.cs` | 7 | health, release, OpenAPI, Scalar, health/live, health/ready, metrics |

**Test infrastructure fixes:**

- `PosService.FunctionalTests.csproj` — added missing `Microsoft.AspNetCore.Mvc.Testing 10.0.0`
- `InventoryService.FunctionalTests/ReleaseEndpointTests.cs` — added missing `using FluentAssertions;`

**Stress test scripts + docs:**

- `scripts/stress-test/pos-stress-test.js` — k6 checkout stress test (ramp 1→5000 VUs)
- `scripts/stress-test/inventory-stress-test.js` — k6 product-list stress test
- `services/pos-service/docs/STRESS-TESTING.md` — mirrors Inventory's existing guide

### Phase L — Release Hardening (completed)

**New shared-infrastructure files:**

| File | Purpose |
|------|---------|
| `shared/shared-infrastructure/src/Authentication/ApiKeyMiddleware.cs` | X-Api-Key header check; bypass /health /metrics /openapi /scalar; off by default |
| `shared/shared-infrastructure/src/RateLimiting/RateLimitingExtensions.cs` | Sliding-window limiter (api/write/health/global); config-driven; off by default |

**shared-infrastructure.csproj changes:**

1. Added `<FrameworkReference Include="Microsoft.AspNetCore.App" />` — all ASP.NET Core 10 types available without NuGet duplicates
2. Removed explicit `Microsoft.Extensions.*` and `Microsoft.EntityFrameworkCore` PackageReferences — provided by FrameworkReference
3. Removed `Serilog.Extensions.Hosting` standalone — already bundled in `Serilog.AspNetCore 9.0.0`

**Both Program.cs wired up:**
- `AddApiRateLimiting(builder.Configuration)` in DI
- `UseMiddleware<ApiKeyMiddleware>()` in pipeline (after exception handler, before CORS)
- `UseApiRateLimiting(app.Configuration)` in pipeline (after CORS, before routing)
- Removed unused `var servicesConfig = ...` (was a CS0219 warning)

**Both appsettings.json** updated with `ApiAuth` + `RateLimiting` sections (both `Enabled: false` by default).

**Documentation:**
- `services/pos-service/docs/SECURITY.md` — threat model, auth, rate limiting, CORS, secrets, DB security
- `services/inventory-service/docs/SECURITY.md` — same + RabbitMQ consumer security
- `docs/ROADMAP.md` — all phases marked done, future work listed
- `release-notes/pos-service-v1.0.0.md` — comprehensive release notes all phases
- `release-notes/inventory-service-v1.0.0.md` — comprehensive release notes all phases

---

## Known Issues (not blocking)

### 1. Three hand-authored migrations need regeneration before production

```bash
# POS InitialCreate
dotnet ef migrations add InitialCreate --force \
  --project services/pos-service/src/PosService.Infrastructure \
  --startup-project services/pos-service/src/PosService.API

# POS AddDailySalesReport
dotnet ef migrations add AddDailySalesReport --force \
  --project services/pos-service/src/PosService.Infrastructure \
  --startup-project services/pos-service/src/PosService.API

# Inventory AddIntegrationEventInbox
dotnet ef migrations add AddIntegrationEventInbox --force \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API
```

Do this before adding any further EF migrations.

### 2. VoidedSalesCount in DailySalesReport is placeholder (0)
Needs a date-ranged ICashSessionRepository query.

### 3. No transactional outbox
A POS crash between "sale committed" and "event published" loses that event silently.

### 4. Default warehouse only
Inventory always deducts from `IsDefault` warehouse. No per-store warehouse mapping.

### 5. BaseEntity duplication
`PosService.Domain.Common.BaseEntity` and `SharedKernel.BaseEntity` are near-duplicates.
Fixed with `using` alias — the duplication itself is tracked but not collapsed.

---

## How to run

```bash
# Build + test
dotnet restore EnterprisePOS.sln
dotnet build EnterprisePOS.sln
dotnet test EnterprisePOS.sln

# POS alone
docker compose -f services/pos-service/docker-compose.dev.yml up -d postgres
dotnet run --project services/pos-service/src/PosService.API
# http://localhost:5001/scalar/v1   <- API explorer
# http://localhost:5001/health      <- health
# http://localhost:5001/metrics     <- Prometheus

# Inventory alone
docker compose -f services/inventory-service/docker-compose.dev.yml up -d postgres
dotnet run --project services/inventory-service/src/InventoryService.API
# http://localhost:5002/scalar/v1

# Together with RabbitMQ
docker compose up -d

# Enable Phase L security (staging/prod — use env vars, never commit keys)
export APIAUTH__ENABLED=true
export APIAUTH__APIKEY=$(openssl rand -hex 32)
export RATELIMITING__ENABLED=true
```

---

## Do NOT

- Don't re-add `Microsoft.Extensions.*` or `Microsoft.EntityFrameworkCore` as `PackageReference` in `shared-infrastructure.csproj` — now from FrameworkReference.
- Don't set `ApiAuth:Enabled=true` or `RateLimiting:Enabled=true` in `appsettings.json` — use env vars.
- Don't add new EF migrations until the 3 hand-authored ones are regenerated with real `dotnet ef`.
- Don't touch `decisions/ADR-*.md` — none needed changing this session.
- Don't remove `Properties/launchSettings.json` from either API project or delete
  `ASPNETCORE_ENVIRONMENT=Development` from its profiles — that's the fix for the Scalar/Swagger
  UI not showing up.
- Don't revert `WebApplicationFactory<Program>` back to `WebApplicationFactory<object>` in either
  `ReleaseEndpointTests.cs`, and don't remove the `public partial class Program { }` marker from
  either `Program.cs` — the tests can't bootstrap the host without it.
- Don't assume "0 build errors, 0 warnings" anywhere in this file is a confirmed fact — see the
  environment-limitation warning at the top. Every session including this one has been unable to
  compile anything.
