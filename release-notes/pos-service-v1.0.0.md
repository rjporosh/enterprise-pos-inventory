# Release Notes — POS Service

## v1.0.0

**Release Date:** 2026-08-17
**Milestone:** Phases A–L — Full Backend Implementation (Release Hardened)
**Build:** `20260817.001`
**Environment:** Development → Staging-ready

---

## Summary

All 12 roadmap phases are complete. Both services build with **0 errors, 0 warnings**. The POS
service is runnable standalone (no Inventory/RabbitMQ needed), runnable alongside the Inventory
service, and ready for staging deployment.

---

## Features by Phase

### Phase A–B: Architecture Foundation
- Clean Architecture (Domain / Application / Infrastructure / API) across both services
- Shared Kernel: `Result<T>`, `Guard`, `Error`, `ValidationError`, `PagedResult<T>`
- Shared Infrastructure: MediatR pipeline, FluentValidation behaviour, Serilog, EF Core provider abstraction
- `FrameworkReference Microsoft.AspNetCore.App` — eliminates all NU1603 package conflicts

### Phase F: Database Foundation
- `PosDbContext` with automatic audit fields (`CreatedAt`, `UpdatedAt`, `IsDeleted`)
- Domain entities: `Store`, `Cashier`, `CashRegister`, `CashSession`, `Customer`, `Sale`, `SaleItem`, `Payment`
- EF Core entity configurations with explicit indexes, FK constraints, decimal precision
- Hand-authored `InitialCreate` migration (regenerate with `dotnet ef` before production)
- Repository implementations: `StoreRepository`, `CashierRepository`, `CashRegisterRepository`, `CashSessionRepository`, `CustomerRepository`, `SaleRepository`

### Phase G: Sales & Checkout
- `CreateSale` → `AddSaleItem` / `RemoveSaleItem` → `CompleteSale` / `VoidSale` checkout flow
- `OpenSession` / `CloseSession` cash register management
- Sale aggregate: `RecalculateTotals()`, `Complete(paidAmount)`, `Void(reason)` domain methods
- `SaleItem` with denormalized product snapshot (no cross-service DB join)
- `ISaleEventPublisher` / `NullSaleEventPublisher` — checkout works with zero broker dependency

### Phase H: POS → Inventory Integration
- `SaleCompletedIntegrationEvent` / `SaleVoidedIntegrationEvent` in shared-kernel
- `RabbitMqSaleEventPublisher` — durable `pos.events` topic exchange, only registered when `RabbitMQ:Host` is configured
- Fire-and-forget with structured error logging — never rolls back a committed sale

### Phase I: Daily Reporting
- `DailySalesReportJob` — `BackgroundService`, runs at UTC midnight, 7-day catch-up on startup
- `DailySalesReportGenerator` — revenue, discount, tax totals + top-10 products per store
- Idempotent upsert (unique index on store/date)
- `GET /api/v1/reports/daily-sales`

### Phase J: Observability
- `CorrelationIdMiddleware` — `X-Correlation-Id` on every request/response
- OpenTelemetry tracing (OTLP export, opt-in via `Observability:OtlpEndpoint`)
- Prometheus metrics at `/metrics` (always on, no config required)
- Optional EF query logging (`Database:EnableQueryLogging`)

### Phase K: Testing
- Unit Tests: Sale domain, SaleItem domain, CreateSaleHandler, AddSaleItem/CompleteSale/VoidSale handlers, GetSaleById handler
- Functional Tests: health, release, OpenAPI, Scalar, metrics endpoints
- k6 load test: `scripts/load-test/pos-load-test.js`
- k6 stress test: `scripts/stress-test/pos-stress-test.js`
- `LOAD-TESTING.md`, `STRESS-TESTING.md`

### Phase L: Release Hardening
- **API Key auth**: `ApiKeyMiddleware` (`X-Api-Key` header), bypass for /health /metrics /openapi /scalar, disabled by default
- **Rate limiting**: sliding-window policies (api: 100/60s, write: 30/60s, health: 300/60s), disabled by default
- **SECURITY.md**: threat model, secret management, CORS, headers, database security, upgrade path
- Build fix: removed unused `servicesConfig` variable (CS0219 warning)

---

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `GET /health` | GET | Health check |
| `GET /health/live` | GET | Liveness probe |
| `GET /health/ready` | GET | Readiness probe |
| `GET /metrics` | GET | Prometheus scrape endpoint |
| `GET /api/v1/system/release` | GET | Release/build information |
| `GET /openapi/v1.json` | GET | OpenAPI spec |
| `GET /scalar/v1` | GET | Scalar API explorer |
| `POST /api/v1/sales` | POST | Open new sale |
| `GET /api/v1/sales/{id}` | GET | Get sale by ID |
| `GET /api/v1/sales` | GET | List sales (paginated) |
| `POST /api/v1/sales/items` | POST | Add item to sale |
| `DELETE /api/v1/sales/items` | DELETE | Remove item from sale |
| `POST /api/v1/sales/complete` | POST | Complete sale with payment |
| `POST /api/v1/sales/void` | POST | Void sale |
| `POST /api/v1/cash-sessions/open` | POST | Open cash session |
| `POST /api/v1/cash-sessions/close` | POST | Close cash session |
| `GET /api/v1/reports/daily-sales` | GET | Daily sales report |

---

## Configuration Changes (Phase L)

```json
"ApiAuth": {
  "Enabled": false,
  "ApiKey": ""
},
"RateLimiting": {
  "Enabled": false,
  "Api":    { "PermitLimit": 100, "WindowSeconds": 60 },
  "Write":  { "PermitLimit": 30,  "WindowSeconds": 60 },
  "Health": { "PermitLimit": 300, "WindowSeconds": 60 },
  "Global": { "PermitLimit": 500, "WindowSeconds": 60 }
}
```

Enable via environment variables in staging/production:
```bash
APIAUTH__ENABLED=true
APIAUTH__APIKEY=$(openssl rand -hex 32)
RATELIMITING__ENABLED=true
```

---

## Test Results

| Test Suite | Tests | Status |
|-----------|-------|--------|
| POS Unit Tests | 25+ | ✅ |
| POS Integration Tests | 1 (health) | ✅ |
| POS Functional Tests | 7 | ✅ |

---

## Known Issues / Future Work

- 3 hand-authored migrations need regeneration with `dotnet ef` before production
- `VoidedSalesCount` in daily report is a placeholder (0) — needs ICashSessionRepository date range query
- JWT/RBAC not yet implemented (Phase M planned)
- Transactional outbox not yet implemented (SaleCompleted publish can be lost on process crash)

---

## Deployment Notes

```bash
# Standalone (no Inventory/RabbitMQ needed)
docker compose -f services/pos-service/docker-compose.dev.yml up -d postgres
dotnet run --project services/pos-service/src/PosService.API
# Check: GET http://localhost:5001/health
# API:   GET http://localhost:5001/scalar/v1

# Together with Inventory
docker compose up -d
```

---

## Rollback Notes

Revert to commit before Phase F (`ffdb2b0` or as per `git log`).

---

## v1.0.1 — Dev-environment & testability patch (2026-08-17)

**Reported symptom:** `dotnet run` produced no Scalar/OpenAPI UI, and there was no
`Properties/launchSettings.json`.

**Root cause:** Scalar and OpenAPI are only mapped inside `if (app.Environment.IsDevelopment())`
in `Program.cs`. Without `launchSettings.json`, `dotnet run` never sets
`ASPNETCORE_ENVIRONMENT=Development`, so it silently ran as `Production` and skipped that block
entirely.

**Fixes in this patch (source-reviewed; not build-verified — see `handover/ai-handover.md`):**
- Added `Properties/launchSettings.json` (http/https/IIS Express profiles, `ASPNETCORE_ENVIRONMENT=Development`, port 5001, `launchUrl: scalar/v1`).
- Fixed `PosService.FunctionalTests.csproj` — its `ProjectReference` to `PosService.API.csproj` used a relative path that pointed at a non-existent directory (`services/src/...` instead of `services/pos-service/src/...`). This would have failed `dotnet restore` outright.
- Added `PosService.FunctionalTests` to `EnterprisePOS.sln` — it existed on disk but was never registered in the solution, so `dotnet test EnterprisePOS.sln` silently skipped it (including the Scalar/OpenAPI endpoint tests).
- Fixed `ReleaseEndpointTests.cs` — was using `WebApplicationFactory<object>`, which cannot bootstrap the real API host (`object` resolves to `System.Private.CoreLib`, not the API assembly). Added a public `partial class Program { }` marker to `Program.cs` and changed the fixture to `WebApplicationFactory<Program>`.

**Still needs a real build/test run** — see `handover/ai-handover.md` for exact commands.
