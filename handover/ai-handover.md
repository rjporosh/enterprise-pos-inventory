# AI Handover — Enterprise POS & Inventory Backend

**Last Updated:** 2026-08-16T01:00:00+00:00
**Current Branch:** main
**Last Commit:** 0a48145 fix(build): resolve remaining 2 errors + 6 warnings from second build pass

---

## ⚠️ Read this first: environment limitation that shaped every session so far

Every session so far — including this one — has run in an environment with **no .NET SDK installed and
no network access to NuGet/apt** (confirmed again this session: `dotnet` is absent, `apt-get install
dotnet-sdk-8.0` returns `403 Forbidden`). This session was different in one important way though: **the
user ran `dotnet restore` / `dotnet build` themselves on their own machine and pasted the full output**
(30 warnings, 71 errors, itemized by file/line). That real compiler output was used to find and fix every
issue below — not guesswork. This is a meaningfully stronger basis than the "should be correct" caveat
in earlier versions of this doc, but it is still **only as verified as the next build the user runs**. If
you are picking this up with SDK access, `dotnet build EnterprisePOS.sln` is the very first thing to run,
and if new errors appear, treat it exactly like this session did: get the exact output, fix precisely,
don't guess broadly.

---

## Current Phase

Roadmap phases **F, G, H, I, J are complete**. **Phase K is partially complete** (stopped mid-phase at
the user's explicit request — see below). **Phase L has not been started.** This session's entire scope
was the build-health fix pass below — **Phase K/L work itself has not been touched this session.**

---

## This session (2026-08-16): build-health fix pass

**Commit `18d84c0`.** The user ran `dotnet restore`/`dotnet build` and got **30 warnings, 71 errors**.
Pasted the full output. All of it was traced to **5 root causes** and fixed with the smallest possible
change per cause — no rewrites, no deletions of working code, no touching of business logic or tests:

| # | Errors/Warnings | File(s) | Root cause | Fix |
|---|---|---|---|---|
| 1 | 8 errors (CS0104) | 8 files in `PosService.Domain` (`Customer.cs`, `Store.cs`, `Sale.cs`, `SaleItem.cs`, `Cashier.cs`, `CashSession.cs`, `CashRegister.cs`, `Payment.cs`) | `PosService.Domain.Common.BaseEntity` and `SharedKernel.BaseEntity` are both in scope (the latter needed for `SharedKernel.Guard`), so bare `BaseEntity` was ambiguous | Added `using BaseEntity = PosService.Domain.Common.BaseEntity;` alias to each file. Did **not** remove `using SharedKernel;` (still needed for `Guard`) and did **not** delete either `BaseEntity` class — both may still be intentionally separate; flagged below for a product decision. |
| 2 | 61 errors (CS0234/CS0246) | `PosService.Application.csproj` | The csproj had **no `ProjectReference` to `PosService.Domain` at all** — every Domain type (`Sale`, `Customer`, `Store`, `CashRegister`, `CashSession`, `DailySalesReport`, `SaleStatus`, `PaymentMethodType`, etc.) failed to resolve throughout the entire Application layer as a cascading result of this one missing line | Added the missing `<ProjectReference Include="..\PosService.Domain\PosService.Domain.csproj" />` |
| 3 | 1 error (CS1061) | `shared/shared-infrastructure/src/DependencyInjection.cs` | `AddValidatorsFromAssemblies` is an extension method from the **`FluentValidation.DependencyInjectionExtensions`** package, which was never referenced — only core `FluentValidation` was | Added `<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />` to `shared-infrastructure.csproj` (matches the existing `FluentValidation` core version) |
| 4 | 1 error (CS0234) | `shared/shared-infrastructure/src/Persistence/DbContextFactory.cs` | Wrong namespace: code referenced `Microsoft.EntityFrameworkCore.Diagnostics.DbLoggerCategory`, which doesn't exist — the real type is `Microsoft.EntityFrameworkCore.DbLoggerCategory` | Fixed the fully-qualified reference (one line) |
| 5 | 30 warnings (NU1603 + NU1902) | `shared/shared-infrastructure/src/shared-infrastructure.csproj` | `OpenTelemetry.Exporter.Prometheus.AspNetCore` was pinned to `1.9.0-rc.1`, **a version that was never published**, so NuGet silently substituted `1.10.0-beta.1` (NU1603 on every project in the solution) and the resulting/transitive `OpenTelemetry.Api` 1.10.0 + `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.9.0 carried 3 known moderate-severity advisories (NU1902): `GHSA-8785-wc3w-h8q6`, `GHSA-g94r-2vxg-569j`, `GHSA-4625-4j76-fww9` | Repinned the **entire OpenTelemetry package set** in one place to a verified, mutually-compatible, patched set (confirmed via NuGet.org + GitHub advisory pages, not guessed): `OpenTelemetry.Extensions.Hosting` **1.15.3**, `OpenTelemetry.Instrumentation.AspNetCore` **1.15.2**, `OpenTelemetry.Instrumentation.Http` **1.15.1**, `OpenTelemetry.Instrumentation.Runtime` **1.15.1**, `OpenTelemetry.Exporter.OpenTelemetryProtocol` **1.15.3**, `OpenTelemetry.Exporter.Prometheus.AspNetCore` **1.15.3-beta.1** (this exporter has never had a stable release — 1.15.3-beta.1 is the beta that's version-locked to core 1.15.3 per its own release notes) |

**RabbitMQ.Client 6.8.1 API usage was re-checked this session** (`RabbitMqSaleEventPublisher.cs`,
`SaleEventsConsumer.cs`) against the errors reported — it produced **zero errors** in the user's build
output, and a careful re-read confirms the `IModel`/`CreateModel`/`BasicPublish(...)`/
`AsyncEventingBasicConsumer`/`DispatchConsumersAsync` calls do match the real v6.x API surface. The
"written from memory, unverified" caveat on this file from the previous session's handover can be
considered resolved — **once the user's next build confirms 0 errors there too.**

**One open question flagged, not resolved:** `PosService.Domain.Common.BaseEntity` and
`SharedKernel.BaseEntity` are near-identical duplicates (the only difference: `SharedKernel.BaseEntity`'s
constructor sets `Id = Guid.NewGuid()`, `PosService.Domain.Common.BaseEntity`'s doesn't). Inventory's
equivalent (`InventoryService.Domain.Common.BaseEntity`) also duplicates `SharedKernel.BaseEntity` but
never hit this ambiguity because Inventory's domain files don't also `using SharedKernel;`. This looks
like unintentional duplication from an earlier phase, not a deliberate design choice — worth a follow-up
decision (collapse to one, or document why both exist) but **deliberately not touched this session**
since it wasn't broken and touching it risks exactly the kind of regression the user asked to avoid.

### Exact command to verify this fix

```bash
cd <repo-root>
dotnet restore EnterprisePOS.sln
dotnet build EnterprisePOS.sln
```

**Update — user re-ran the build and confirmed 71/30 → 2/6.** Both services' Domain/Application/
Infrastructure/API layers compiled clean, plus InventoryService's full test suite. Two remaining issues,
fixed in commit `0a48145`:

| # | Errors/Warnings | File(s) | Root cause | Fix |
|---|---|---|---|---|
| 6 | 2 errors (CS0234/CS0246) | `PosService.IntegrationTests.csproj` | Missing `Microsoft.AspNetCore.Mvc.Testing` package reference — this is what `WebApplicationFactory<>` comes from. `InventoryService.IntegrationTests.csproj` already had it; POS's equivalent never did | Added `<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.0" />` (same version Inventory's test project already uses) |
| 7 | 6 warnings (nullable/unused, `InventoryService.Application`) | `GetAllProductsHandler.cs`, `GetAllProductsValidator.cs`, `GetAllStocksHandler.cs`, `StockOutHandler.cs`, `StockInHandler.cs`, `StockAdjustmentHandler.cs` | Pre-existing nullable-reference and unused-parameter warnings, unrelated to the OpenTelemetry/DI fixes above — these are genuine (if minor) code issues that were always there, just never surfaced because the build never got that far before | `query.SortBy ?? "name"` at the one call site (matches the query's own default); null-forgiving `sortBy!` inside a `.Must()` lambda already guarded by `.When(!IsNullOrWhiteSpace(...))` that the compiler can't trace across; `logger` in `GetAllStocksHandler` was genuinely never used — added a real `LogInformation` call matching the sibling `GetAllProductsHandler`'s pattern, rather than deleting the parameter; `saved!` in all 3 stock-movement handlers where `saved` is provably non-null whenever `savedMovement` is non-null (same ternary-guard pattern in each), but the compiler can't trace it |

**Both services are now expected to build with 0 errors, 0 warnings.** This has not yet been confirmed
by a third build run — that's the very next step for whoever picks this up next.

### Exact command to verify this second fix

```bash
cd <repo-root>
dotnet restore EnterprisePOS.sln
dotnet build EnterprisePOS.sln
```

If this comes back 0/0 for real: **cross off Step 1 in "Next Exact Task" below and move straight to
Step 2** (regenerating the 3 hand-authored migrations). If new errors/warnings appear, they're new
information — same method as both passes above: paste exact output, fix precisely, don't touch anything
not implicated.

---

## What actually got done this session, in commit order

1. **`3939da9` fix(shared):** wired up MediatR handler discovery and DbContext DI registration.
   Two genuine, pre-existing runtime bugs (present since the Phase D/E commits, before this session
   started) that would have made **every existing Inventory endpoint throw at runtime**:
   - `AddSharedInfrastructure()` only ever registered MediatR handlers from the SharedInfrastructure
     assembly itself — never from `InventoryService.Application`/`PosService.Application` — so
     `mediator.Send(...)` had no handler to resolve, for anything.
   - `InventoryDbContext` was never registered in the DI container at all, so `IProductRepository`/
     `IStockRepository` (which take it via constructor injection) could never be constructed.
   Fixed by extending `AddSharedInfrastructure` to accept the calling service's Application assembly,
   and by registering the DbContext through the existing `IDbContextFactory` abstraction (ADR-003).

2. **`ffdb2b0` feat(pos): implement database foundation** (Phase F). Domain entities (`Store`,
   `Cashier`, `CashRegister`, `CashSession`, `Customer`, `Sale`, `SaleItem`, `Payment`), EF
   configurations, `PosDbContext`, design-time factory, repository interfaces + implementations, a
   hand-authored `InitialCreate` migration, domain unit tests. Also fixed `PosService.Infrastructure.csproj`,
   which was missing its `ProjectReference` to `PosService.Application` (same class of gap as bug #1 above).

3. **`8946c8c` feat(pos): implement sales checkout flow** (Phase G). Full CQRS slice: `CreateSale`,
   `AddSaleItem`, `RemoveSaleItem`, `CompleteSale`, `VoidSale`, `GetSaleById`, `GetAllSales`,
   `OpenSession`/`CloseSession` for cash sessions. `SalesController` + `CashSessionsController`.
   `ISaleEventPublisher`/`NullSaleEventPublisher` seam added here so checkout has zero dependency on a
   message broker by default.

4. **`079fd49` feat(integration): POS→Inventory stock sync over RabbitMQ** (Phase H).
   `SaleCompletedIntegrationEvent`/`SaleVoidedIntegrationEvent` contracts in shared-kernel.
   `RabbitMqSaleEventPublisher` (POS) publishes to a durable `pos.events` topic exchange; only
   registered when `RabbitMQ:Host` is configured. `SaleEventsConsumer` (Inventory, `BackgroundService`)
   consumes with a dead-letter queue, exponential-backoff reconnect (never crashes the host), and
   idempotency via a new `ProcessedIntegrationEvent` inbox table. Added minimal `IWarehouseRepository`
   to resolve Inventory's default warehouse for stock deduction/reversal.

5. **`c013e79` feat(pos): implement daily sales reporting job** (Phase I). `DailySalesReport` entity
   (one row per store/date, unique-indexed → idempotent), `DailySalesReportGenerator` (aggregates
   completed sales into revenue/discount/tax/payment-method totals + top-10 products),
   `DailySalesReportJob` (`BackgroundService`, runs at UTC midnight, 7-day catch-up scan on startup,
   per-store failure isolation). `GET /api/v1/reports/daily-sales`.
   **Known gap, not hidden:** `VoidedSalesCount` and `CashSessionSummaryJson` are placeholders
   (0 / empty array) — needs a date-ranged query on `ICashSessionRepository` that doesn't exist yet.

6. **`1a20b68` feat(observability):** (Phase J). `CorrelationIdMiddleware` (shared), OpenTelemetry
   tracing (OTLP export, opt-in via `Observability:OtlpEndpoint`) + metrics (`/metrics`, always on) via
   `ObservabilityExtensions.AddObservability`, both wired into both `Program.cs` files. Optional EF
   query logging via `Database:EnableQueryLogging`. `docs/observability/` guide + starter Prometheus
   alert rules + Grafana dashboard JSON.
   **Verification caveat:** the OpenTelemetry package versions in `shared-infrastructure.csproj`
   (1.9.0 core, 1.9.0-rc.1 Prometheus exporter) were chosen from memory with no NuGet access to confirm
   — run `dotnet restore` and adjust if resolution fails.

7. **`14bd1d0` test(load):** (Phase K, **partial** — stopped here). Added
   `scripts/load-test/inventory-load-test.js` (the script Inventory's `LOAD-TESTING.md` already
   documented but that was never actually committed) and a new `scripts/load-test/pos-load-test.js`
   (full checkout flow). Added `services/pos-service/docs/LOAD-TESTING.md`. Added
   `PosService.IntegrationTests/IntegrationTestBase.cs` + `HealthCheckTests.cs`, mirroring Inventory's
   existing pattern exactly (including its `WebApplicationFactory<object>` typing, which looks
   questionable but matches precedent — flagged, not fixed, since fixing it would mean touching
   Inventory's already-committed test infra too, which felt out of scope for a POS-focused pass).

---

## Bugs fixed (all pre-existing, found by reading, not by a failing build)

| Bug | File(s) | Fix |
|---|---|---|
| MediatR never discovered per-service handlers | `shared/shared-infrastructure/src/DependencyInjection.cs` | `AddSharedInfrastructure(params Assembly[])` |
| `InventoryDbContext` never registered in DI | `InventoryService.API/Program.cs` | Registered via `IDbContextFactory` |
| `PosService.Infrastructure.csproj` missing `ProjectReference` to `PosService.Application` | same file | Added the reference |

---

## Migrations that need regeneration before real use — important

**Three migrations in this session were hand-authored** because no `dotnet-ef` tooling was available:

1. `services/pos-service/src/PosService.Infrastructure/Migrations/20260812000000_InitialCreate.cs`
2. `services/pos-service/src/PosService.Infrastructure/Migrations/20260812020000_AddDailySalesReport.cs`
3. `services/inventory-service/src/InventoryService.Infrastructure/Migrations/20260812010000_AddIntegrationEventInbox.cs`

Each has `[DbContext]`/`[Migration]` attributes directly on the migration class so it's discoverable by
`dotnet ef database update` **without** a paired `Designer.cs`/`*ModelSnapshot.cs` — those auto-generated
files were deliberately **not** hand-forged (too easy to get subtly wrong in a way that's hard to debug
later; better to have the real tool generate them). Each migration's doc comment has the exact
`dotnet ef migrations add --force` command to run to regenerate it properly. **Do this before adding
any further migrations to either service**, or the tooling will get confused about the true model state.

---

## Files touched this session (see `git log` for full commit bodies)

Rather than duplicate `git log --stat` here, run:
```bash
git log --stat 3939da9^..14bd1d0
```
to see every file this session touched, grouped by commit/phase.

---

## Remaining Work

- **Phase K (Testing/Load Testing) — finish it:**
  - Deeper POS integration tests exercising `SalesController`/`CashSessionsController` end-to-end
    (needs DB seeding + whatever reset strategy — Respawn or similar — Inventory's fuller integration
    tests beyond `HealthCheckTests` use; **not yet inspected**, check
    `services/inventory-service/tests/InventoryService.IntegrationTests/DatabaseMigrationTests.cs` and
    any other integration tests there first).
  - Inventory's `LOAD-TESTING.md` mentions an NBomber C# load-testing project
    (`scripts/load-test/InventoryLoadTest.csproj`) that has never actually existed. Either build it or
    correct the doc to stop referencing it.
  - Add a stress-testing pass for POS analogous to `services/inventory-service/docs/STRESS-TESTING.md`
    (that file was not read this session — check it exists and what it documents before assuming scope).

- **Phase L (Release Hardening) — not started at all:**
  - Authentication/authorization (JWT). Neither service has any auth today — every endpoint is
    anonymous. Check ADRs for whether one was planned (ADR-004 covers communication, not auth — may be
    worth checking if there's an ADR for auth, or writing one).
  - Rate limiting.
  - Security review pass (secrets in appsettings.json are dev-only placeholders — `postgres`/`postgres`,
    `guest`/`guest` — fine for local docker-compose, not for anything else).
  - CORS is already partially configured (`Cors:AllowedOrigins` in appsettings) — verify it's actually
    wired into `Program.cs` correctly for both services.

- **Zero-build-warning/error acceptance criterion — cannot be verified in this environment.**
  The task's hard acceptance criterion (`dotnet build EnterprisePOS.sln` → 0 errors, 0 warnings, all
  tests passing) has **never been checked against any of this session's work**. This is the single
  most important thing to do next.

- **Reconcile `docs/ROADMAP.md` against actual state.** It was not updated this session (deprioritized
  in favor of code + this handover, given the "stop and package" instruction). It still shows Phase F
  onward as not-started. Update it phase-by-phase against what's listed above.

- **Release notes.** `release-notes/pos-service-v0.1.0.md` was not updated this session either.

---

## Next Exact Task

**Step 1 — confirm 0 errors / 0 warnings with a third build run:**

```bash
cd <repo-root>
dotnet restore EnterprisePOS.sln
dotnet build EnterprisePOS.sln
```

Two fix passes have run against real `dotnet build` output so far (71/30 → 2/6 → fixes applied,
unconfirmed). This step is just re-running the build to confirm the second pass actually worked — if it
comes back 0/0, cross this step off and move to Step 2. If not, paste the output and fix precisely, same
method as both passes above.

**Step 2 — regenerate the three hand-authored migrations** per the commands in their doc comments (see
"Migrations that need regeneration" above), then:

```bash
dotnet ef database update --project services/pos-service/src/PosService.Infrastructure --startup-project services/pos-service/src/PosService.API

dotnet ef migrations add AddInventoryChanges \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API

dotnet ef database update --project services/inventory-service/src/InventoryService.Infrastructure --startup-project services/inventory-service/src/InventoryService.API
```

**Step 3 — run the full test suite** and fix failures:

```bash
dotnet test EnterprisePOS.sln
```

**Step 4 — the three isolation scenarios** the original task specified, now that there's something to
actually test:
- POS alone (no Inventory, no RabbitMQ) — `docker compose -f services/pos-service/docker-compose.dev.yml up`, hit `/api/v1/sales`, `/api/v1/cash-sessions/open`.
- Inventory alone — same idea.
- All three together — complete a POS sale, confirm Inventory's stock decrements via the RabbitMQ event.

**Step 5 — pick up Phase K/L** per "Remaining Work" above.

---

## Do NOT

- Don't re-run `dotnet ef migrations add` for POS's `InitialCreate` or Inventory's
  `AddIntegrationEventInbox`/`AddStockAndStockMovement` without first reading the doc comment on the
  relevant hand-authored migration — the `--force` flag is required and intentional there.
- Don't assume `docs/ROADMAP.md` is current — it wasn't touched this session; trust this handover and
  `git log` over it until it's reconciled.
- Don't touch `decisions/ADR-*.md` — none needed changing this session; the integration design
  (optional RabbitMQ, no cross-service DB access) fits ADR-001/ADR-003 as written.

---

## Architectural decisions this session made that aren't yet in an ADR

Worth writing up as ADRs (or folding into existing ones) rather than leaving as implicit code decisions:

1. **Sale line items carry a denormalized product snapshot** (`ProductName`/`Sku` at time of sale) and
   only `ProductId` as a bare reference into Inventory — POS never queries Inventory's database or API
   synchronously during checkout. The caller (POS terminal UI) is responsible for having already
   resolved product details before calling `AddSaleItem`.
2. **RabbitMQ integration is entirely additive and optional**, gated by `RabbitMQ:Host` presence in
   config on both sides. `NullSaleEventPublisher` is the default; `SaleEventsConsumer` self-disables
   (logs and returns) if unconfigured, and reconnects with backoff rather than crashing if the broker
   is unreachable after being configured.
3. **Stock deduction from a POS sale always targets Inventory's "default" warehouse** (`Warehouse.IsDefault`).
   There's no per-store-to-per-warehouse mapping — POS doesn't know about Inventory's warehouse concept
   at all. This is a reasonable default but may not fit a multi-warehouse-per-store retail model; flag
   for product/architecture review.
4. **Idempotency for the RabbitMQ consumer** is a simple inbox table (`ProcessedIntegrationEvent`) keyed
   by `EventId`, not a full outbox pattern on the publisher side. The publisher does not persist
   outbound events before publishing — a POS process crash between "sale completed" and "event
   published" would silently lose that one event. Worth a follow-up if stronger delivery guarantees are
   needed (transactional outbox is the standard fix).

---

## Environment Requirements (unchanged)

- .NET 10 SDK
- PostgreSQL 16 (two independent databases: `pos_db`, `inventory_db`)
- RabbitMQ 3.13 (optional — see above)
- Docker & Docker Compose
- k6 (load testing)
- An OTLP collector (Jaeger or similar) + Prometheus + Grafana (optional, observability)

---

## Full commit history (this session)

```
3939da9 fix(shared): wire up MediatR handler discovery and DbContext DI registration
ffdb2b0 feat(pos): implement database foundation
8946c8c feat(pos): implement sales checkout flow
079fd49 feat(integration): implement POS-to-Inventory stock sync over RabbitMQ
c013e79 feat(pos): implement daily sales reporting job
1a20b68 feat(observability): add distributed tracing, metrics, correlation IDs, and query logging
14bd1d0 test(load): add k6 load test scripts and POS integration test scaffold
```

Read each commit's full message (`git show --stat <sha>` / `git log -1 <sha>`) for the detailed
per-commit rationale — they were written to be as informative as this document.
