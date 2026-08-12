# Observability Guide

Phase J of the roadmap. Both services (POS and Inventory) ship the same observability stack via
`shared-infrastructure`'s `AddObservability` extension, so this guide applies to either.

## What's wired up

- **Structured logs** — Serilog, JSON-formatted, to console + debug sink (`SerilogConfiguration`).
  Every log line is enriched with `Service` and `Environment`.
- **Correlation ID** — `CorrelationIdMiddleware` reads `X-Correlation-Id` from the incoming request
  (or generates one), pushes it onto the Serilog `LogContext` so it appears on every log line for that
  request, tags the current `Activity` so it shows up on the trace, and echoes it back on the response.
  Downstream calls (e.g. POS calling out, if it ever does) should forward this header.
- **Distributed tracing** — OpenTelemetry, ASP.NET Core + HttpClient instrumentation. Exports over OTLP
  to whatever collector/backend you point it at (Jaeger's OTLP receiver, Tempo, etc.) when
  `Observability:OtlpEndpoint` is configured. If it's not configured, tracing still runs in-process
  (useful for local `Activity`-based debugging) but nothing is exported anywhere.
- **Metrics** — OpenTelemetry ASP.NET Core + HttpClient + .NET runtime instrumentation, exposed on
  `/metrics` in Prometheus text format via `app.MapPrometheusScrapingEndpoint()`. Always on — no
  external dependency required to scrape it.
- **EF Core query logging** — off by default (would be noisy in production). Set
  `Database:EnableQueryLogging: true` in a service's `appsettings.{Environment}.json` to route EF's
  command-executed events through Serilog for debugging.

## Configuration

Add to `appsettings.json` (or an environment-specific override) per service:

```json
{
  "Observability": {
    "OtlpEndpoint": "http://localhost:4317"
  },
  "Database": {
    "EnableQueryLogging": false
  }
}
```

Leaving `Observability:OtlpEndpoint` unset is a valid, supported configuration — the service does not
require a collector to be running to start or to serve requests.

## Running a local collector stack

`docker-compose.yml` at the repo root should include (or be extended with) Jaeger for trace viewing and
Prometheus for metrics scraping. A minimal Jaeger container that exposes its OTLP gRPC receiver on 4317:

```yaml
jaeger:
  image: jaegertracing/all-in-one:1.62
  ports:
    - "16686:16686"  # UI
    - "4317:4317"    # OTLP gRPC receiver
```

And a Prometheus config scraping both services' `/metrics` endpoints — see
`docs/observability/prometheus-alerts.yml` for a starting set of alert rules (high error rate, high
p99 latency, RabbitMQ consumer disconnected) to load alongside your own `prometheus.yml` scrape config.

## Grafana

`docs/observability/grafana-dashboard.json` is a minimal starter dashboard (request rate, error rate,
p50/p95/p99 latency per service, .NET GC/heap metrics from the runtime instrumentation). Import it as a
starting point and extend with business metrics (sales/hour, checkout failures) as custom OpenTelemetry
`Meter`/`Counter` instruments are added to the CQRS handlers.

## Verification caveat

The OpenTelemetry package versions pinned in `shared-infrastructure.csproj` (1.9.0 core packages,
1.9.0-rc.1 for the still-prerelease Prometheus exporter) were selected from memory in an environment
with no network access to confirm against NuGet. Run `dotnet restore` and adjust versions if resolution
fails before relying on this in CI.
