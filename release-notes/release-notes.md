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
