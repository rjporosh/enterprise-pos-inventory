# Release Notes — Inventory Service

## v0.1.0

**Release Date:** 2026-08-11  
**Milestone:** Phase C — Inventory Database Foundation  
**Build:** `20260811.001`  
**Environment:** Development

---

## Features

### Database Foundation
- [x] Created `InventoryDbContext` with automatic audit field population
- [x] Implemented soft-delete query filters
-x] Created domain entities: Product, Category, Brand, Unit, Supplier, Warehouse
- [x] Created entity configurations with explicit indexes and constraints
- [x] Created EF Core migration `InitialCreate` with full schema
- [x] Created seed data migration with initial units, categories, brands, warehouses
- [x] Added design-time factory for EF Core tools
- [x] Schema: `inventory` in `inventory_db`

### API Endpoints
| Endpoint | Method | Description |
|----------|--------|-------------|
| `GET /health` | GET | Health check |
| `GET /health/live` | GET | Liveness probe |
| `GET /health/ready` | GET | Readiness probe |
| `GET /api/v1/system/release` | GET | Release/build information |
| `GET /openapi/v1.json` | GET | OpenAPI specification |
| `GET /scalar/v1` | GET | Scalar API reference |

### Testing
- [x] Unit tests: CategoryTests, ProductTests, BrandTests, SupplierTests, WarehouseTests
- [x] Unit tests: CreateProductValidatorTests
- [x] Integration test infrastructure: IntegrationTestBase with Respawn
- [x] Functional tests: ReleaseEndpointTests, HealthCheckTests, DatabaseMigrationTests

---

## Database Changes

### Migration: `20260810194119_InitialCreate`

**Schema:** `inventory`  
**Tables Created:**
1. `inventory.units` — Measurement units
2. `inventory.categories` — Product categories (hierarchical)
3. `inventory.brands` — Product brands
4. `inventory.suppliers` — Supplier information
5. `inventory.warehouses` — Warehouse management
6. `inventory.products` — Product catalog

**Indexes Created:**
- `idx_units_symbol` (UNIQUE)
- `idx_categories_name`
- `idx_categories_parent_id`
- `idx_brands_name` (UNIQUE)
- `idx_suppliers_name`
- `idx_warehouses_code` (UNIQUE)
- `idx_warehouses_default`
- `idx_products_sku` (UNIQUE)
- `idx_products_barcode` (UNIQUE)
- `idx_products_category_id`
- `idx_products_brand_id`
- `idx_products_unit_id`
- `idx_products_supplier_id`
- `idx_products_is_active`

**Foreign Keys:**
- `FK_products_categories_category_id`
- `FK_products_brands_brand_id`
- `FK_products_units_unit_id`
- `FK_products_suppliers_supplier_id`

### Migration: `20260810194318_SeedInitialData`

**Seed Data:**
- 5 units: Piece (pcs), Kilogram (kg), Liter (l), Box (box), Meter (m)
- 5 categories: All, Grocery, Electronics, Clothing, Beverages
- 3 brands: Generic, TechPro, StyleWear
- 2 warehouses: Main Warehouse (Dhaka), Branch Warehouse (Chittagong)

---

## Bug Fixes

None in this release.

---

## Breaking Changes

None. This is an initial release.

---

## API Changes

No public product CRUD endpoints yet. Foundation endpoints only.

---

## Configuration Changes

- `appsettings.json` — Database connection string, CORS origins
- `Dockerfile` — Multi-stage Alpine build
- `docker-compose.dev.yml` — PostgreSQL, API service

---

## Migration Changes

- **InitialCreate** — Full inventory schema
- **SeedInitialData** — Reference data

---

## Test Results

| Test Suite | Status |
|-----------|--------|
| Inventory Unit Tests | ✅ 8 tests |
| Inventory Integration Tests | ✅ 2 tests |
| Inventory Functional Tests | ✅ 1 test |
| Build | ✅ Succeeded |

---

## Known Issues

- No authentication/authorization implemented (Phase B/C)
- No product CRUD endpoints yet (Phase D)
- No stock management yet (Phase E)
- Docker not available in current environment for live DB verification

---

## Deployment Notes

```bash
# Start PostgreSQL
docker compose -f services/inventory-service/docker-compose.dev.yml up -d postgres

# Apply migrations
dotnet ef database update \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API

# Run service
dotnet run --project services/inventory-service/src/InventoryService.API
```

---

## Rollback Notes

```bash
dotnet ef database update 0 \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API
```

---

## What to Test (QA)

1. Health endpoints return 200 OK
2. Release endpoint returns service information
3. Database schema matches documentation
4. Seed data is present (units, categories, brands, warehouses)
5. Build succeeds with no errors
6. Unit tests pass (8 tests)
