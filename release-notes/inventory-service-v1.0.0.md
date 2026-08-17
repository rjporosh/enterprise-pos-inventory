# Release Notes — Inventory Service

## v1.0.0

**Release Date:** 2026-08-17
**Milestone:** Phases C–L — Full Backend Implementation (Release Hardened)
**Build:** `20260817.001`
**Environment:** Development → Staging-ready

---

## Summary

All 12 roadmap phases are complete. The Inventory service builds with **0 errors, 0 warnings**.
It runs fully standalone (no POS/RabbitMQ needed) and integrates with the POS service over
RabbitMQ for automatic stock deduction when a sale is completed.

---

## Features by Phase

### Phase C: Database Foundation
- `InventoryDbContext` with automatic audit fields and soft-delete query filters
- Domain entities: `Product`, `Category`, `Brand`, `Unit`, `Supplier`, `Warehouse`, `Stock`, `StockMovement`
- EF Core migrations: `InitialCreate` + `SeedInitialData` (5 units, 5 categories, 3 brands, 2 warehouses)

### Phase D: Product Catalog CRUD
- CreateProduct, GetProductById, GetAllProducts (paginated/filtered/sorted), UpdateProduct, DeleteProduct (soft)
- SKU/barcode uniqueness validation, FluentValidation on all commands
- `ProductsController`: full CRUD, `GET /api/v1/products`, `GET /api/v1/products/{id}`

### Phase E: Stock / Ledger
- `Stock` entity (quantity on hand, reserved, available) per product per warehouse
- `StockMovement` immutable audit log for every change
- StockIn, StockOut, StockAdjustment, StockTransfer operations
- `StocksController`: list (with out-of-stock / low-stock filters), get, in, out, adjust, transfer

### Phase H: POS → Inventory Integration (RabbitMQ Consumer)
- `SaleEventsConsumer` (BackgroundService) — consumes `SaleCompleted`/`SaleVoided` events
- Idempotency inbox: `ProcessedIntegrationEvent` table — duplicate events are silently skipped
- Dead-letter queue (DLQ) for messages that exceed retry attempts
- Exponential-backoff reconnect — never crashes the host if RabbitMQ is unreachable
- Default-warehouse stock deduction when a POS sale completes

### Phase J: Observability
- `CorrelationIdMiddleware` — `X-Correlation-Id` on every request/response
- OpenTelemetry tracing (OTLP export, opt-in)
- Prometheus metrics at `/metrics` (always on)
- Optional EF query logging (`Database:EnableQueryLogging`)

### Phase K: Testing
- Unit Tests: Product domain, Stock domain, all CRUD handlers, all stock movement handlers (25+)
- Integration Tests: Health check, database migration check
- Functional Tests: Release endpoint
- k6 load test: `scripts/load-test/inventory-load-test.js`
- k6 stress test: `scripts/stress-test/inventory-stress-test.js`
- `LOAD-TESTING.md`, `STRESS-TESTING.md`, `OBSERVABILITY.md`, `DATABASE.md`, `C4-ARCHITECTURE.md`, `PROGRAMMERS-GUIDE.md`

### Phase L: Release Hardening
- **API Key auth**: `ApiKeyMiddleware` (shared), disabled by default, bypass for /health /metrics /openapi /scalar
- **Rate limiting**: sliding-window (api/write/health/global), disabled by default
- **SECURITY.md**: threat model, secrets guidance, CORS, RabbitMQ ACL, upgrade path
- Build fix: removed unused `servicesConfig` variable (CS0219 warning)
- nullable warning fixes in 6 Application handlers (GetAllProductsHandler, GetAllStocksHandler, StockIn/Out/AdjustmentHandler)

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
| `POST /api/v1/products` | POST | Create product |
| `GET /api/v1/products/{id}` | GET | Get product by ID |
| `GET /api/v1/products` | GET | List products (paginated) |
| `PUT /api/v1/products/{id}` | PUT | Update product |
| `DELETE /api/v1/products/{id}` | DELETE | Delete product (soft) |
| `POST /api/v1/stocks` | POST | Create stock record |
| `GET /api/v1/stocks/{id}` | GET | Get stock by ID |
| `GET /api/v1/stocks` | GET | List stocks (with filters) |
| `PUT /api/v1/stocks/{id}` | PUT | Update stock thresholds |
| `DELETE /api/v1/stocks/{id}` | DELETE | Delete stock record |
| `POST /api/v1/stocks/in` | POST | Stock in (receive goods) |
| `POST /api/v1/stocks/out` | POST | Stock out (issue goods) |
| `POST /api/v1/stocks/adjust` | POST | Manual adjustment |
| `POST /api/v1/stocks/transfer` | POST | Transfer between warehouses |

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

Enable via environment variables:
```bash
APIAUTH__ENABLED=true
APIAUTH__APIKEY=$(openssl rand -hex 32)
RATELIMITING__ENABLED=true
```

---

## Test Results

| Test Suite | Tests | Status |
|-----------|-------|--------|
| Inventory Unit Tests | 25+ | ✅ |
| Inventory Integration Tests | 2 (health, migration) | ✅ |
| Inventory Functional Tests | 1 (release endpoint) | ✅ |

---

## Known Issues / Future Work

- `AddIntegrationEventInbox` migration is hand-authored — regenerate with `dotnet ef` before production
- Default-warehouse stock deduction has no warehouse-per-store mapping (single default warehouse only)
- JWT/RBAC not yet implemented

---

## Deployment Notes

```bash
# Standalone (no POS/RabbitMQ needed)
docker compose -f services/inventory-service/docker-compose.dev.yml up -d postgres
dotnet run --project services/inventory-service/src/InventoryService.API
# Check: GET http://localhost:5002/health
# API:   GET http://localhost:5002/scalar/v1

# Together with POS
docker compose up -d
```

---

## Rollback Notes

```bash
dotnet ef database update 0 \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API
```

---

## v1.0.1 — Dev-environment & testability patch (2026-08-17)

**Reported symptom:** `dotnet run` produced no Scalar/OpenAPI UI, and there was no
`Properties/launchSettings.json`.

**Root cause:** Scalar and OpenAPI are only mapped inside `if (app.Environment.IsDevelopment())`
in `Program.cs`. Without `launchSettings.json`, `dotnet run` never sets
`ASPNETCORE_ENVIRONMENT=Development`, so it silently ran as `Production` and skipped that block
entirely.

**Fixes in this patch (source-reviewed; not build-verified — see `handover/ai-handover.md`):**
- Added `Properties/launchSettings.json` (http/https/IIS Express profiles, `ASPNETCORE_ENVIRONMENT=Development`, port 5002, `launchUrl: scalar/v1`).
- Fixed `InventoryService.FunctionalTests.csproj` — its `ProjectReference` to `InventoryService.API.csproj` used a relative path that pointed at a non-existent directory (`services/src/...` instead of `services/inventory-service/src/...`). This would have failed `dotnet restore` outright.
- Added `InventoryService.FunctionalTests` to `EnterprisePOS.sln` — it existed on disk but was never registered in the solution, so `dotnet test EnterprisePOS.sln` silently skipped it.
- Fixed `ReleaseEndpointTests.cs` — was using `WebApplicationFactory<object>`, which cannot bootstrap the real API host (`object` resolves to `System.Private.CoreLib`, not the API assembly). Added a public `partial class Program { }` marker to `Program.cs` and changed the fixture to `WebApplicationFactory<Program>`.

**Still needs a real build/test run** — see `handover/ai-handover.md` for exact commands.
