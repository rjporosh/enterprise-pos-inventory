# API Gateway

The single public entry point for the platform's four backend services
(`auth-service`, `notification-service`, `inventory-service`, `pos-service`), built on
[YARP](https://microsoft.github.io/reverse-proxy/) (Yarp.ReverseProxy). See
[`decisions/ADR-008-api-gateway.md`](../../decisions/ADR-008-api-gateway.md) for the full design
rationale and what is explicitly out of scope today.

## What's here

Just one project — a gateway has no domain logic to layer:

| Project | Responsibility |
|---|---|
| `Gateway.Api` | YARP reverse proxy, correlation ID, CORS, rate limiting, health checks, metrics |
| `Gateway.Tests` | Hermetic tests (health/404/metrics) — no downstream service required to run them |

## Routes

Path-based, matching each service's real controller/endpoint-group prefixes exactly (see
`src/Gateway.Api/appsettings.json`'s `ReverseProxy` section for the authoritative list):

| Path prefix | Routed to |
|---|---|
| `/api/v1/auth/*`, `/api/v1/admin/*` | `auth-service` |
| `/api/v1/notifications/*`, `/api/v1/recipients/*/preferences`, `/api/v1/templates/*` | `notification-service` |
| `/api/v1/products/*`, `/api/v1/stocks/*` | `inventory-service` |
| `/api/v1/sales/*`, `/api/v1/cash-sessions/*`, `/api/v1/reports/*` | `pos-service` |

Adding a new controller/endpoint group in any service needs one new route entry added here —
there is no shared URL convention (like a `/inventory/*` prefix) that would make this automatic,
by deliberate choice (see the ADR — renaming existing routes for that would be a breaking change).

## Endpoints on the gateway itself

| Method | Route | Description |
|---|---|---|
| GET | `/health` | Gateway's own liveness — does not depend on any downstream service |
| GET | `/health/services` | Fans out to all 4 services' `/health`, returns one combined JSON view |
| GET | `/metrics` | Prometheus scrape endpoint |

## Running locally

```bash
# 1. Start the 4 backend services first (from repo root) — either via Docker:
docker compose up -d
# ...or run each with `dotnet run` per its own README.

# 2. Run the gateway
cd services/gateway
dotnet run --project src/Gateway.Api
# → http://localhost:5010
```

In Docker Compose (`docker-compose.yml` at the repo root), the gateway runs as `gateway-api` on
host port **5010** (not 5000 — macOS's AirPlay Receiver claims 5000 by default, which would break
`docker compose up` out of the box on every Mac).

## Running tests

```bash
cd services/gateway
dotnet test Gateway.sln
```

## Configuration

All config lives in `src/Gateway.Api/appsettings.json`, overridable via environment variables
(matching the double-underscore convention every other service in this repo uses). The Docker
Compose file overrides the four cluster destination addresses to the containers' internal
hostnames (`auth-api`, `notification-api`, `inventory-api`, `pos-api`) instead of `localhost`.

| Key | Default | Notes |
|---|---|---|
| `ReverseProxy:Clusters:*:Destinations:destination1:Address` | `http://localhost:<port>/` | One per service |
| `Cors:AllowedOrigins` | `localhost:3000`, `:3001` | The two Next.js frontend apps' dev ports |
| `RateLimiting:PermitLimit` / `:WindowSeconds` | `200` / `60` | Per-client-IP fixed window, at the edge |
| `Seq:Url` | `http://localhost:5341` | Optional — omit to log console-only |
| `Observability:OtlpEndpoint` | unset | Optional distributed tracing export |

## Not yet done (see the ADR for why)

- Frontend apps still call each service's port directly — not yet repointed at the gateway.
- No authentication/authorization propagation (auth-service isn't integrated into either frontend
  app yet either — this naturally follows that, not the other way around).
- No tenant context propagation (no tenant isolation exists anywhere yet).
