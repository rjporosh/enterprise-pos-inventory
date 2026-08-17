# Release Notes — POS Service

## v0.1.0

**Release Date:** 2026-08-10  
**Milestone:** Phase A + B — Repository Architecture and Shared Infrastructure Foundation  
**Build:** `20260810.001`  
**Environment:** Development

---

## Features

### Foundation
- [x] POS Service project structure with Clean Architecture
- [x] GlobalExceptionHandler with ProblemDetails responses
- [x] Serilog structured logging
- [x] Health checks (/health, /health/live, /health/ready)
- [x] OpenAPI/Scalar documentation
- [x] CORS configuration
- [x] Release information endpoint

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
- [x] Test project scaffolding (Unit, Integration, Functional)
- [x] CI pipeline configured

---

## Database Changes

No database schema yet. POS database foundation in Phase F.

---

## Bug Fixes

None in this release.

---

## Breaking Changes

None. This is an initial foundation release.

---

## API Changes

No public endpoints yet. Foundation endpoints only.

---

## Configuration Changes

- `appsettings.json` — Database connection string, CORS origins
- `Dockerfile` — Multi-stage Alpine build
- `docker-compose.dev.yml` — PostgreSQL, API service

---

## Migration Changes

No migrations yet.

---

## Test Results

| Test Suite | Status |
|-----------|--------|
| POS Unit Tests | ⏳ No tests yet (Phase G) |
| POS Integration Tests | ⏳ No tests yet (Phase H) |
| Build | ✅ Succeeded |

---

## Known Issues

- No database schema yet (Phase F)
- No sales/checkout endpoints yet (Phase G)
- No authentication/authorization (Phase B/C)

---

## Deployment Notes

```bash
docker compose -f services/pos-service/docker-compose.dev.yml up -d postgres
dotnet run --project services/pos-service/src/PosService.API
```

---

## Rollback Notes

Revert to commit before `a3e3ac8`.

---

## What to Test (QA)

1. Health endpoints return 200 OK
2. Release endpoint returns service information
3. Build succeeds with no errors
