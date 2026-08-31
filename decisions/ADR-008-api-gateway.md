# ADR-008: API Gateway

**Date:** 2026-08-31
**Status:** Accepted
**Decision Maker:** Principal Architect

## Context

Four independently deployable backend services exist (`auth-service`, `notification-service`,
`pos-service`, `inventory-service`), each on its own port, with no shared entry point. Every
frontend app that wants to call more than one service must know all of their individual addresses,
which:
- Exposes internal service topology to the browser (a config leak, and a mild security concern —
  any of the four ports is directly reachable from the public internet in a naive deployment).
- Gives every service's CORS policy, rate limiting, and correlation-ID handling to configure and
  keep consistent independently, rather than once.
- Has no single place to add cross-cutting concerns (auth propagation once real login exists,
  consistent error shaping, a single health view of "is the platform up") without repeating the
  work four times.

`docs/ROADMAP-v3.0.md` Phase 3 calls for exactly this: "one public origin," internal service URLs
private, health-aware routing, rate limiting, and resilience. Prior to this session, no gateway of
any kind existed anywhere in the repository (confirmed by a full-repo search) — Phase 3 was
correctly marked "NOT STARTED."

## Problem

What should sit at the public edge of the platform, and how should it route to the four services?

## Options Considered

### Option A: No gateway — frontend calls each service directly
- **Pros:** Simplest possible setup, zero extra moving parts.
- **Cons:** Exactly the problems in Context above; explicitly rejected by the existing roadmap's
  Phase 3 exit criteria ("Internal service URLs cannot be discovered from frontend configuration").

### Option B: Hand-rolled ASP.NET Core proxy (manual `HttpClient` forwarding)
- **Pros:** No new dependency, full control.
- **Cons:** Reimplementing request/response streaming, header forwarding, timeouts, health-aware
  load balancing, and active health checks by hand — all solved problems, not worth re-solving.

### Option C: YARP (Yet Another Reverse Proxy) — Selected
- **Pros:** Official Microsoft-maintained reverse proxy library for ASP.NET Core, config-driven
  routing (routes/clusters expressible entirely in `appsettings.json` or environment variables —
  no redeploy needed to repoint a destination), built-in active/passive health checks, already
  net10.0-compatible, integrates with the same ASP.NET Core middleware pipeline (Serilog, health
  checks, OpenTelemetry, rate limiting) every other service in this repo already uses.
- **Cons:** One more service to build/deploy/monitor.

### Option D: A hosted API management product (e.g. a cloud API gateway service)
- **Pros:** Managed, offloads infra.
- **Cons:** Vendor lock-in, cost, and this platform's deployment target has not been decided yet —
  premature to commit to a specific cloud vendor's gateway product this early.

## Decision

Add a new service, `services/gateway/` (`Gateway.Api`, its own `Gateway.sln` — same
one-project-per-solution pattern as `auth-service`/`notification-service`), built on **YARP
2.3.0**.

**Deliberately dependency-light — does NOT reference `shared-infrastructure`.** A gateway is a
pure edge/routing concern with no domain logic; `shared-infrastructure` pulls in EF Core, Npgsql,
MediatR, and FluentValidation, none of which a reverse proxy needs. The one piece worth reusing —
the `X-Correlation-Id` middleware contract — is duplicated locally (same "duplicate rather than
share for independent deployability" call already made for the two frontend apps in
`docs/AI-CODING-RULES.md`), not referenced.

**Routing is path-based against each service's real, existing controller/endpoint-group
prefixes** (`/api/v1/auth`, `/api/v1/admin` → auth; `/api/v1/notifications`,
`/api/v1/recipients/*/preferences`, `/api/v1/templates` → notification; `/api/v1/products`,
`/api/v1/stocks` → inventory; `/api/v1/sales`, `/api/v1/cash-sessions`, `/api/v1/reports` → pos).
No service's controller routes were renamed or given a service-name prefix to make routing
"cleaner" — that would be a breaking API change to four already-working services for a cosmetic
gain. The tradeoff is that a new controller group in any service needs one new route entry added
to the gateway's `appsettings.json`'s `ReverseProxy:Routes` section — an accepted, documented cost.

**What the gateway does today:**
- Reverse-proxies to all four services via YARP, config-driven clusters/routes.
- `GET /health` — the gateway's own liveness (does not depend on any downstream service).
- `GET /health/services` — fans out to all four services' `/health` and returns one combined
  JSON view (`docs/ROADMAP-v3.0.md`'s "health-aware routing" exit criterion).
- `GET /metrics` — Prometheus scrape endpoint (OpenTelemetry ASP.NET Core + HttpClient +
  runtime instrumentation), same pattern as the four services.
- Correlation ID propagation (`X-Correlation-Id`) — generated or forwarded on the way in, and the
  proxied downstream response's own copy of the header flows back out unchanged.
- A single configured-origins CORS policy and a general per-client-IP fixed-window rate limiter
  (`RateLimiting:PermitLimit`/`WindowSeconds` in config) at the edge, ahead of whatever
  per-endpoint limits an individual service adds itself.
- YARP active health checks per cluster (`ConsecutiveFailures` policy, `/health` on each
  destination) — a failing service's route stops being sent traffic rather than every request
  timing out against it.
- Structured logging via Serilog, now also shipping to Seq when `Seq:Url` is configured (see
  "Related fix" below) — console + Seq, same as the four services.

**What it deliberately does NOT do yet (explicitly out of scope for this ADR):**
- **Authentication/authorization propagation.** `auth-service` is not yet integrated into either
  frontend app (see `AI-HANDOVER.md` §L); adding JWT validation at the gateway ahead of that
  integration would be speculative, half-wired complexity. This is real, tracked future work, not
  an oversight.
- **Tenant context propagation.** No tenant isolation exists anywhere yet (Phase 2, not started).
- **Idempotency-key enforcement**, **circuit breakers**, and **request size limits** beyond
  Kestrel's defaults — YARP supports all of these via config once there's a concrete resilience
  requirement driving the specific thresholds; adding them speculatively now would be untested
  configuration nobody could verify against a real failure scenario.
- **Frontend apps have not been repointed at the gateway.** Both `frontend/inventory` and
  `frontend/pos` still call their respective services' ports directly. Repointing them is
  additive (change the two `NEXT_PUBLIC_*_API_URL` env vars) and deliberately left for a
  dedicated frontend-integration pass rather than bundled into this ADR's scope.

**Related fix bundled into this change:** none of the four existing services or `shared-infrastructure`'s
`SerilogConfiguration.CreateLogger` ever actually wired the `Serilog.Sinks.Seq` sink, despite the
`enterprise-seq` container having run in every `docker-compose.yml` stack since Phase J. Writing
the gateway's own Serilog bootstrap from scratch surfaced this gap; fixed for all five services
(gateway included) via an optional `Seq:Url` config key — purely additive, falls back to
console-only logging when unset, same pattern already used for the optional OTLP tracing endpoint.

## Consequences

- **Positive:** Single public origin (once frontends are repointed), consistent CORS/rate-limiting/
  correlation-ID handling in one place, a real health-aware routing layer, and the previously-dead
  Seq container now actually receives logs from all five services.
- **Negative:** One more service to build, test, deploy, and keep in sync with the other four's
  route prefixes when they change.
- **Mitigation:** Config-driven routing means most changes (repointing a destination, adding a
  route) need no code change or gateway redeploy in a real deployment with externalized config —
  only a config update.
