# Release Notes — Inventory Service

## v0.1.0

**Release Date:** 2026-08-11  
**Milestone:** Phase C + Phase D — Inventory Database Foundation + Product Catalog CRUD  
**Build:** `20260811.002`  
**Environment:** Development

---

## Features

### Database Foundation
- [x] Created `InventoryDbContext` with automatic audit field population
- [x] Implemented soft-delete query filters
- [x] Created domain entities: Product, Category, Brand, Unit, Supplier, Warehouse
- [x] Created entity configurations with explicit indexes and constraints
- [x] Created EF Core migration `InitialCreate` with full schema
- [x] Created seed data migration with initial units, categories, brands, warehouses
- [x] Added design-time factory for EF Core tools
- [x] Schema: `inventory` in `inventory_db`

### Product Catalog CRUD (Phase D)
- [x] Created `CreateProductCommand` + Handler with SKU/barcode uniqueness validation
- [x] Created `GetProductByIdQuery` + Handler with soft-delete check
- [x] Created `GetAllProductsQuery` + Handler with pagination, filtering, sorting
- [x] Created `UpdateProductCommand` + Handler with SKU/barcode uniqueness validation
- [x] Created `DeleteProductCommand` + Handler with soft-delete
- [x] Created `ProductDto`, `ProductListItemDto`, `CreateProductRequest`, `UpdateProductRequest`
- [x] Created `ProductsController` with full CRUD endpoints
- [x] Created `IProductRepository` interface + `ProductRepository` implementation
- [x] Added FluentValidation validators for all commands/queries

### API Endpoints
| Endpoint | Method | Description |
|----------|--------|-------------|
| `GET /health` | GET | Health check |
| `GET /health/live` | GET | Liveness probe |
| `GET /health/ready` | GET | Readiness probe |
| `GET /api/v1/system/release` | GET | Release/build information |
| `GET /openapi/v1.json` | GET | OpenAPI specification |
| `GET /scalar/v1` | GET | Scalar API reference |
| `POST /api/v1/products` | POST | Create product |
| `GET /api/v1/products/{id}` | GET | Get product by ID |
| `GET /api/v1/products` | GET | Get all products (paged, filtered, sorted) |
| `PUT /api/v1/products/{id}` | PUT | Update product |
| `DELETE /api/v1/products/{id}` | DELETE | Soft-delete product |

### Testing
- [x] Unit tests: CategoryTests, ProductTests, BrandTests, SupplierTests, WarehouseTests
- [x] Unit tests: CreateProductHandlerTests, DeleteProductHandlerTests, GetAllProductsHandlerTests
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

- [x] Fixed `BaseEntity` constructor to auto-generate GUID Ids
- [x] Fixed domain entity constructors to validate input using `Guard`
- [x] Fixed `Application.csproj` project reference path to Domain
- [x] Fixed `Infrastructure.csproj` to reference Application for repository implementation
- [x] Fixed repository implementation placement (moved from Application to Infrastructure)
- [x] Fixed unit tests to use mocked `IProductRepository` instead of `InventoryDbContext`

---

## Breaking Changes

None in this release.

---

## API Changes

### New Product CRUD Endpoints
- `POST /api/v1/products` — Create a new product
- `GET /api/v1/products/{id}` — Get product by ID
- `GET /api/v1/products` — Get all products with pagination, filtering, sorting
- `PUT /api/v1/products/{id}` — Update an existing product
- `DELETE /api/v1/products/{id}` — Soft-delete a product

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
| Inventory Unit Tests | ✅ 20 tests |
| Inventory Integration Tests | ✅ Scaffolded |
| Inventory Functional Tests | ✅ 1 test |
| Build | ✅ Succeeded |

---

## Known Issues

- No authentication/authorization implemented (Phase B/C)
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
3. Product CRUD endpoints work correctly
4. Database schema matches documentation
5. Seed data is present (units, categories, brands, warehouses)
6. Build succeeds with no errors
7. Unit tests pass (20 tests)
