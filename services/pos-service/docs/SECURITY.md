# Security Guide — POS Service

## Overview

The POS Service uses a layered security model designed to be **off by default for local development**
and **fully configurable for staging/production** without a rebuild. All security controls are
driven by `appsettings.json` / environment variables.

---

## 1. Authentication — API Key (Phase L)

### How it works

An `ApiKeyMiddleware` inspects every request for the `X-Api-Key` header before routing.

| Scenario | Behaviour |
|----------|-----------|
| `ApiAuth:Enabled = false` (default) | Middleware is a no-op; all requests pass through |
| `ApiAuth:Enabled = true`, header present and correct | Request proceeds |
| `ApiAuth:Enabled = true`, header missing or wrong | `401 Unauthorized` |
| `ApiAuth:Enabled = true`, `ApiAuth:ApiKey` not configured | `503 Service Unavailable` + warning log |

### Bypass paths (never require a key)

```
/health
/health/live
/health/ready
/metrics
/openapi
/scalar
/favicon
```

### Enabling (staging/production)

```bash
# via environment variable (recommended — never put the real key in appsettings.json)
APIAUTH__ENABLED=true
APIAUTH__APIKEY=<generate with: openssl rand -hex 32>
```

### Planned upgrade path

API key auth is a stepping-stone. The production roadmap (ADR to be written) plans:
- **Phase M:** JWT Bearer tokens (ASP.NET Core `AddAuthentication` + `AddJwtBearer`)
- **Phase N:** Role-based authorization (`[Authorize(Roles = "cashier,manager")]`) per controller

---

## 2. Rate Limiting (Phase L)

Implemented via ASP.NET Core's built-in `Microsoft.AspNetCore.RateLimiting` (sliding window).

### Policies

| Policy | Default limit | Window | Used on |
|--------|--------------|--------|---------|
| `api` | 100 req | 60 s | General GET endpoints |
| `write` | 30 req | 60 s | POST/PUT/DELETE endpoints |
| `health` | 300 req | 60 s | Health probes (never block infra) |
| Global | 500 req | 60 s | Fallback for unmapped endpoints |

### Enabling

```bash
RATELIMITING__ENABLED=true
# Override individual limits:
RATELIMITING__WRITE__PERMITLIMIT=20
RATELIMITING__API__WINDOWSECONDS=30
```

When the limit is exceeded the service returns `429 Too Many Requests`.

---

## 3. CORS

Configured with an allowlist of origins. Default: `http://localhost:4200` (Angular dev server).

```json
"Cors": {
  "AllowedOrigins": ["https://your-frontend.example.com"]
}
```

**Never use `AllowAnyOrigin()` in production.** The CORS policy always requires explicit origins, headers, and methods (`AllowCredentials()` is enabled for cookie/token flows).

---

## 4. Secret Management

### Development (local)
All secrets in `appsettings.json` are **placeholder values** (`postgres`/`postgres`,
`guest`/`guest`). These are safe only because the services bind to `localhost` in Docker Compose.

### Production: never commit real secrets
Use one of these:
- **Environment variables** (e.g. `DATABASE__CONNECTIONSTRING=...`) — simplest
- **Docker secrets** (`docker secret create`) — for Swarm/Compose deployments
- **AWS Secrets Manager / Azure Key Vault / HashiCorp Vault** — for cloud deployments
- **.NET User Secrets** (`dotnet user-secrets`) — for local developer machines

```bash
# Example: override connection string via env var (Docker Compose / Kubernetes)
DATABASE__CONNECTIONSTRING="Host=prod-db;Port=5432;Database=pos_db;Username=posapp;Password=<secret>"
RABBITMQ__PASSWORD=<secret>
APIAUTH__APIKEY=<secret>
```

---

## 5. Database Security

| Setting | Recommendation |
|---------|----------------|
| Connection string | Use a dedicated `posapp` role with minimal privileges (no DDL) |
| Migrations | Run with a separate `posmigrations` role that has DDL rights |
| Connection pooling | Set `Maximum Pool Size=50` per app instance in production |
| SSL | Add `SSL Mode=Require` to the connection string for remote PostgreSQL |

---

## 6. Headers

The `GlobalExceptionHandler` middleware returns `ProblemDetails` for all unhandled exceptions,
which prevents stack traces leaking to clients.

For production, add security headers via a reverse proxy (nginx/Caddy) or an additional middleware:

```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Content-Security-Policy: default-src 'none'
```

---

## 7. Dependency Security

Run this before every release:

```bash
dotnet list package --vulnerable --include-transitive
```

The OpenTelemetry package set was pinned in Phase J to versions that clear all known advisories
(GHSA-8785-wc3w-h8q6, GHSA-g94r-2vxg-569j, GHSA-4625-4j76-fww9). Re-run after any version bump.

---

## 8. Threat Model (summary)

| Threat | Control |
|--------|---------|
| Unauthenticated API access | API key middleware (Phase L); JWT (Phase M) |
| DoS via bulk requests | Rate limiting (Phase L) |
| Cross-origin data theft | CORS allowlist |
| Secret leakage in logs | Serilog `Destructure.ByIgnoring` + masked appsettings |
| SQL injection | Parameterised queries (EF Core — no raw SQL) |
| Event replay (duplicate stock deduction) | RabbitMQ consumer idempotency inbox (`ProcessedIntegrationEvent`) |
| Crash-between-complete-and-publish | Known gap — transactional outbox planned (ADR pending) |
