# Security Guide — Inventory Service

## Overview

The Inventory Service uses the same layered security model as the POS Service — **off by default
for local development**, fully configurable via environment variables for staging/production.
Both services share the same `ApiKeyMiddleware` and `RateLimitingExtensions` from
`shared/shared-infrastructure`.

---

## 1. Authentication — API Key (Phase L)

### How it works

`ApiKeyMiddleware` inspects every request for the `X-Api-Key` header before routing.

| Scenario | Behaviour |
|----------|-----------|
| `ApiAuth:Enabled = false` (default) | Middleware is a no-op |
| `ApiAuth:Enabled = true`, correct key | Request proceeds |
| `ApiAuth:Enabled = true`, missing/wrong key | `401 Unauthorized` |
| `ApiAuth:Enabled = true`, no key configured | `503 Service Unavailable` |

### Bypass paths

```
/health  /health/live  /health/ready  /metrics  /openapi  /scalar  /favicon
```

### Enabling

```bash
APIAUTH__ENABLED=true
APIAUTH__APIKEY=$(openssl rand -hex 32)
```

### Planned upgrade path

- **Phase M:** JWT Bearer tokens
- **Phase N:** Role-based authorization (`warehouse-manager`, `stock-viewer`, `admin`)

---

## 2. Rate Limiting (Phase L)

| Policy | Default limit | Window | Applies to |
|--------|--------------|--------|------------|
| `api` | 100 req | 60 s | Product/stock read endpoints |
| `write` | 30 req | 60 s | Stock movements, product mutations |
| `health` | 300 req | 60 s | Health probes |
| Global | 500 req | 60 s | Unmapped fallback |

```bash
# Enable for staging:
RATELIMITING__ENABLED=true
```

Exceeded limit → `429 Too Many Requests`.

---

## 3. RabbitMQ Consumer Security

The `SaleEventsConsumer` (POS→Inventory integration) only activates when `RabbitMQ:Host` is
configured. Security considerations:

| Risk | Control |
|------|---------|
| Broker credential leakage | Inject via `RABBITMQ__PASSWORD` env var, not appsettings |
| Duplicate event processing | Idempotency inbox (`ProcessedIntegrationEvent` table) |
| Malformed message crash | `try/catch` around handler; message `Nack`-ed to dead-letter queue |
| Broker unreachable | Exponential-backoff reconnect; consumer disables gracefully if never connects |

---

## 4. CORS

Default allowed origin: `http://localhost:3000` (React dev server).

```json
"Cors": {
  "AllowedOrigins": ["https://inventory-ui.example.com"]
}
```

---

## 5. Secret Management

Same approach as POS Service — see `services/pos-service/docs/SECURITY.md §5`.

Inventory-specific secrets:

```bash
DATABASE__CONNECTIONSTRING="Host=prod-db;Database=inventory_db;Username=inventoryapp;Password=<secret>"
RABBITMQ__USERNAME=inventoryapp
RABBITMQ__PASSWORD=<secret>
APIAUTH__APIKEY=<secret>
```

**Use separate DB users per service** — `posapp` must never have access to `inventory_db` and
vice versa. The two databases share a PostgreSQL server in the dev `docker-compose.dev.yml` only
for convenience.

---

## 6. Database Security

Same recommendations as POS (dedicated role, minimal privileges, SSL in production).

Inventory-specific: the `inventory` schema is owned by the migration user (`inventorymigrations`);
the application user (`inventoryapp`) has only `SELECT/INSERT/UPDATE/DELETE` on tables in the
`inventory` schema.

---

## 7. Dependency Security

```bash
dotnet list package --vulnerable --include-transitive
```

---

## 8. Threat Model (summary)

| Threat | Control |
|--------|---------|
| Unauthenticated stock reads/writes | API key (Phase L); JWT/RBAC (Phase M/N) |
| DoS via bulk stock movement requests | Rate limiting — `write` policy (30 req/60 s) |
| Cross-origin data theft | CORS allowlist |
| Negative stock from duplicate sale events | Idempotency inbox + AvailableQuantity guard |
| RabbitMQ message injection | VHost-level ACL on the `pos.events` exchange (production config) |
| SQL injection | EF Core parameterised queries only |
| Stack trace leakage | GlobalExceptionHandler → ProblemDetails |
