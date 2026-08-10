# ADR-007: Logging and Observability Strategy

**Date:** 2026-08-10
**Status:** Accepted
**Decision Maker:** Principal Architect

## Context

Enterprise systems require production-grade observability: structured logging, metrics, distributed tracing, and health checks.

## Problem

What observability stack should be used?

## Options Considered

### Option A: Standard ASP.NET logging only
- **Pros:** No external dependencies
- **Cons:** No structured logging, no query interface, no correlation across services

### Option B: Serilog + Seq + OpenTelemetry (Selected)
- **Pros:** Structured JSON logging, Seq for query, OpenTelemetry for distributed tracing, Serilog enrichers for machine name, thread, request ID
- **Cons:** Additional infrastructure (Seq), more configuration

### Option C: ELK Stack
- **Pros:** Powerful search and dashboards
- **Cons:** Heavy, more infrastructure, overkill for initial stages

## Decision

Use **Serilog + Seq + OpenTelemetry**:

```csharp
// Serilog with Seq sink (when Seq available)
// Falls back to file-based logging when Seq URL is not configured

.Enrich.WithProperty("Service", serviceName)
.Enrich.WithProperty("Environment", environment)
.Enrich.WithMachineName()
.Enrich.WithThreadId()
.WriteTo.Console(new JsonFormatter())     // Docker-friendly
.WriteTo.Seq(seqUrl)                       // When Seq configured
.WriteTo.File(path, rollingInterval: Daily) // Fallback
```

**Health checks:** `/health`, `/health/live`, `/health/ready` (K8s-ready)

**Correlation ID:** `X-Correlation-ID` header, propagated via middleware and logged in every request

**Structured logging policy:**
- Log all exceptions with: endpoint, HTTP method, service, class, method, correlation ID, exception details
- Never log: passwords, JWT secrets, API keys, payment credentials, PII

## Consequences

- **Positive:** Production-ready observability, queryable logs via Seq, distributed tracing via OpenTelemetry
- **Negative:** Seq infrastructure required for full query capabilities
- **Mitigation:** File-based logging as fallback; Seq is optional but recommended
