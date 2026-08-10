# Roadmap — Enterprise POS & Inventory Platform

## Current Status

**Phase:** C — Inventory Database Foundation  
**Branch:** main  
**Last Updated:** 2026-08-11

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
- Unit tests: 8 domain + validation tests
- Integration tests: Health check, database migration
- Functional tests: Release endpoint
- Database documentation complete
- C4 architecture documented
- Programmer's guide documented
- Observability guide documented
- Load/stress testing guides documented

---

## Upcoming Phases

### ⏳ Phase D: Inventory Product/Catalog Foundation
**Target:** Complete product CRUD APIs

Tasks:
- CreateProductCommand + Handler
- GetProductByIdQuery + Handler
- GetAllProductsQuery + Handler (pagination, filtering, sorting)
- UpdateProductCommand + Handler
- DeleteProductCommand (soft delete) + Handler
- UnitOfWork pattern (if needed)
- ProductDto, CategoryDto, BrandDto
- ProductController with full CRUD endpoints
- Integration tests for product CRUD
- Functional tests for product workflows
- Postman collection for product APIs

**Dependencies:** Phase C complete

---

### ⏳ Phase E: Inventory Stock/Ledger Foundation
**Target:** Stock management and audit trail

Tasks:
- Stock entity (current stock per product per warehouse)
- StockLedger entity (immutable transaction log)
- StockMovement entity (in/out adjustments)
- StockAdjustment (manual adjustments with approval)
- StockTransfer (between warehouses)
- StockCount (physical inventory counting)
- Low stock alerts
- Out of stock reporting
- Stock valuation (FIFO/weighted average)

**Dependencies:** Phase D complete

---

### ⏳ Phase F: POS Database Foundation
**Target:** POS service database schema

Tasks:
- POSDbContext
- Sale, SaleItem entities
- Payment, PaymentMethod entities
- Discount, Tax entities
- CashRegister, CashSession entities
- Expense entity
- Migration + seed data

**Dependencies:** Phase A + B complete

---

### ⏳ Phase G: POS Sales/Checkout Foundation
**Target:** Core POS operations

Tasks:
- CreateSaleCommand + Handler
- AddItemToSaleCommand + Handler
- RemoveItemFromSaleCommand + Handler
- ApplyDiscountCommand + Handler
- ProcessPaymentCommand + Handler
- CompleteSaleCommand + Handler
- VoidSaleCommand + Handler
- SaleController with full endpoints
- Integration tests
- Functional tests

**Dependencies:** Phase F complete

---

### ⏳ Phase H: POS ↔ Inventory Integration
**Target:** Service-to-service communication

Tasks:
- StockDeductedEvent (Inventory → POS)
- ProductUpdatedEvent (Inventory → POS)
- StockConfirmedEvent (POS → Inventory)
- RabbitMQ event bus implementation
- Idempotency key middleware
- Correlation ID propagation
- Circuit breaker for service calls
- Contract tests

**Dependencies:** Phase E + G complete

---

### ⏳ Phase I: Daily Reporting
**Target:** Scheduled business reports

Tasks:
- DailySalesReportJob (midnight UTC)
- Sales summary report
- Payment method breakdown
- Top-selling products
- Low stock products
- Cash summary
- Idempotent report generation
- Retry logic
- Recovery after restart

**Dependencies:** Phase G + H complete

---

### ⏳ Phase J: Observability
**Target:** Production monitoring

Tasks:
- OpenTelemetry integration
- Jaeger distributed tracing
- Prometheus metrics
- Grafana dashboards
- Alert rules
- Correlation ID in all logs
- Query logging

**Dependencies:** Phase A + B complete (can be done in parallel)

---

### ⏳ Phase K: Testing/Load Testing
**Target:** Performance validation

Tasks:
- k6 load test scripts
- NBomber load tests
- Stress test scenarios
- Performance benchmarks
- CI integration for performance tests

**Dependencies:** Phase I complete

---

### ⏳ Phase L: Release Hardening
**Target:** Production readiness

Tasks:
- Authentication/Authorization (JWT + RBAC)
- Rate limiting
- Input validation everywhere
- API versioning strategy
- Database provider tests (PostgreSQL, SQL Server)
- Backup/restore procedures
- Disaster recovery plan
- Security audit
- Penetration testing

**Dependencies:** All previous phases

---

## Parallel Tracks

These can be worked on in parallel with feature phases:

1. **Documentation** — Update after each phase
2. **Testing** — Write tests alongside features
3. **Observability** — Can start after Phase B
4. **Security** — Can start after Phase B
5. **Performance** — Can start after Phase D

---

## Milestone Summary

| Phase | Name | Status | Dependencies |
|-------|------|--------|--------------|
| A | Repository Architecture | ✅ Done | - |
| B | Shared Infrastructure Foundation | ✅ Done | A |
| C | Inventory Database Foundation | ✅ Done | B |
| D | Inventory Product/Catalog Foundation | ⏳ Next | C |
| E | Inventory Stock/Ledger Foundation | ⏳ Planned | D |
| F | POS Database Foundation | ⏳ Planned | A, B |
| G | POS Sales/Checkout Foundation | ⏳ Planned | F |
| H | POS ↔ Inventory Integration | ⏳ Planned | E, G |
| I | Daily Reporting | ⏳ Planned | G, H |
| J | Observability | ⏳ Planned | A, B |
| K | Testing/Load Testing | ⏳ Planned | I |
| L | Release Hardening | ⏳ Planned | All |

---

## Next Agent Command

```
Continue from Phase D: Inventory Product/Catalog Foundation.

1. Read handover/ai-handover.md
2. Verify: dotnet build EnterprisePOS.sln
3. Create Product CRUD commands, queries, handlers
4. Create ProductController
5. Add unit tests for handlers
6. Add integration tests for endpoints
7. Update release notes
8. Update handover/ai-handover.md
9. Git commit: feat(inventory): implement product catalog CRUD

Do NOT modify: shared/, services/pos-service/, existing ADRs
Acceptance: Product CRUD endpoints work, tests pass
```
