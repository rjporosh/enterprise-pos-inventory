# Roadmap — Enterprise POS & Inventory Platform

## Current Status

**Phase:** L — Complete (Release Hardening)
**Branch:** main
**Last Updated:** 2026-08-17

---

## Completed Phases

### ✅ Phase A: Repository Architecture
- Created EnterprisePOS.sln with 14 projects
- Established Clean Architecture structure for both services
- Configured independent test projects per service

### ✅ Phase B: Shared Infrastructure Foundation
- Shared Kernel: Result<T>, Guard, Entity, ValueObject, DomainEvent
- Shared Infrastructure: MediatR pipeline, provider abstraction, Serilog
- Docker, docker-compose, CI pipeline
- ADRs 001-007
- Release notes v0.1.0

### ✅ Phase C: Inventory Database Foundation
- InventoryDbContext with BaseDbContext inheritance
- Domain entities: Product, Category, Brand, Unit, Supplier, Warehouse
- EF Core migrations: InitialCreate + SeedInitialData
- Seed data: 5 units, 5 categories, 3 brands, 2 warehouses
- Entity configurations with indexes and foreign keys
- Unit tests: domain + validation tests
- Integration tests: Health check, database migration
- Functional tests: Release endpoint
- Database documentation, C4 architecture, Programmer's guide, Observability guide

### ✅ Phase D: Inventory Product/Catalog Foundation
- CreateProduct, GetProductById, GetAllProducts, UpdateProduct, DeleteProduct (soft)
- IProductRepository + ProductRepository
- ProductsController with full CRUD endpoints
- FluentValidation validators for all commands/queries
- ProductDto, ProductListItemDto, CreateProductRequest, UpdateProductRequest

### ✅ Phase E: Inventory Stock/Ledger Foundation
- Stock entity (per product per warehouse), StockMovement entity
- StockIn, StockOut, StockAdjustment, StockTransfer commands + handlers
- IStockRepository + StockRepository
- StocksController (list, get, in, out, adjust, transfer)
- GetAllStocks with filtering (low stock, out of stock, by product/warehouse)
- Low-stock and out-of-stock query support

### ✅ Phase F: POS Database Foundation
- PosDbContext with BaseDbContext inheritance
- Domain entities: Store, Cashier, CashRegister, CashSession, Customer, Sale, SaleItem, Payment
- EF Core configurations + hand-authored InitialCreate migration
- Repository interfaces + implementations for all aggregate roots
- PosDbContextDesignTimeFactory

### ✅ Phase G: POS Sales/Checkout Foundation
- CreateSale, AddSaleItem, RemoveSaleItem, CompleteSale, VoidSale, GetSaleById, GetAllSales
- OpenSession, CloseSession for cash register management
- SalesController, CashSessionsController, ReportsController
- ISaleEventPublisher / NullSaleEventPublisher (checkout never depends on RabbitMQ)
- Sale aggregate: RecalculateTotals, Complete, Void domain logic

### ✅ Phase H: POS ↔ Inventory Integration (RabbitMQ)
- SaleCompletedIntegrationEvent, SaleVoidedIntegrationEvent in shared-kernel
- RabbitMqSaleEventPublisher (POS) — durable topic exchange, only active when broker configured
- SaleEventsConsumer (Inventory BackgroundService) — idempotency inbox, DLQ, exponential backoff
- ProcessedIntegrationEvent inbox table (hand-authored migration)
- IWarehouseRepository — default warehouse discovery for stock deduction

### ✅ Phase I: Daily Reporting
- DailySalesReport entity (one row per store/date, idempotent upsert)
- DailySalesReportGenerator — aggregates totals + top-10 products
- DailySalesReportJob (BackgroundService, UTC midnight, 7-day catch-up on startup)
- GET /api/v1/reports/daily-sales endpoint

### ✅ Phase J: Observability
- CorrelationIdMiddleware (shared) — X-Correlation-Id propagation
- OpenTelemetry: OTLP tracing (opt-in) + Prometheus metrics (/metrics always on)
- ObservabilityExtensions.AddObservability wired into both services
- Optional EF query logging (Database:EnableQueryLogging)
- Observability guide, Prometheus alert rules, Grafana dashboard JSON

### ✅ Phase K: Testing / Load Testing
- k6 load test scripts: inventory-load-test.js, pos-load-test.js
- k6 stress test scripts: pos-stress-test.js, inventory-stress-test.js
- POS Unit Tests: Sale domain, SaleItem domain, CreateSaleHandler, AddSaleItem/CompleteSale/VoidSale handlers, GetSaleById
- POS Functional Tests: health, release, OpenAPI, Scalar, metrics endpoints
- STRESS-TESTING.md for both services
- LOAD-TESTING.md for POS service

### ✅ Phase L: Release Hardening
- **API Key authentication** — ApiKeyMiddleware (shared-infrastructure), disabled by default, bypass for /health /metrics /openapi /scalar
- **Rate limiting** — sliding-window (api/write/health/global policies), config-driven, disabled by default
- **RateLimitingExtensions.AddApiRateLimiting / UseApiRateLimiting** wired into both services
- **shared-infrastructure.csproj**: FrameworkReference added (eliminates NU1603 package conflicts), Serilog.Extensions.Hosting + Serilog.AspNetCore properly separated
- **appsettings.json**: ApiAuth + RateLimiting config sections for both services (both disabled by default for dev)
- **SECURITY.md**: threat model, secret management, CORS, headers, database security, upgrade path — for both services
- Build warnings fixed: unused `servicesConfig` variable removed from both Program.cs

---

## Milestone Summary

| Phase | Name | Status |
|-------|------|--------|
| A | Repository Architecture | ✅ Done |
| B | Shared Infrastructure Foundation | ✅ Done |
| C | Inventory Database Foundation | ✅ Done |
| D | Inventory Product/Catalog Foundation | ✅ Done |
| E | Inventory Stock/Ledger Foundation | ✅ Done |
| F | POS Database Foundation | ✅ Done |
| G | POS Sales/Checkout Foundation | ✅ Done |
| H | POS ↔ Inventory Integration | ✅ Done |
| I | Daily Reporting | ✅ Done |
| J | Observability | ✅ Done |
| K | Testing / Load Testing | ✅ Done |
| L | Release Hardening | ✅ Done |

---

## Post-Phase-L Work (future)

| Item | Priority | Description |
|------|----------|-------------|
| JWT/RBAC | High | Replace API key with proper Bearer tokens + role claims |
| Transactional outbox | Medium | Guarantee event delivery even on POS process crash |
| Warehouse-per-store mapping | Medium | Map POS StoreId → Inventory WarehouseId (not always "default") |
| Hand-authored migration regeneration | High | Regenerate 3 migrations with real `dotnet ef` tool |
| VoidedSalesCount in daily report | Low | Needs date-ranged ICashSessionRepository query |
| BaseEntity deduplication | Low | Collapse PosService.Domain.Common.BaseEntity + SharedKernel.BaseEntity |
| SQL Server / MySQL provider | Low | Extend IDbProviderFactory (currently PostgreSQL only) |

---

## Environment Requirements

- .NET 10 SDK
- PostgreSQL 16 (two independent databases: `pos_db`, `inventory_db`)
- RabbitMQ 3.13 (optional — services work without it)
- Docker & Docker Compose
- k6 (load/stress testing)
- OTLP collector + Prometheus + Grafana (optional, observability)
