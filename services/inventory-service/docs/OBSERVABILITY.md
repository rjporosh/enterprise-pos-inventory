# Observability Guide — Enterprise POS & Inventory

## 1. Structured Logging

### Serilog Configuration

All logs are structured JSON with the following fields:
- `@t` — Timestamp
- `@m` — Message template
- @r — Renderings
- `Level` — Log level (Information, Warning, Error, etc.)
- `Service` — Service name (pos-service, inventory-service)
- `Environment` — Development, Staging, Production
- `CorrelationId` — Request correlation ID
- `TraceId` — Distributed trace ID (when OpenTelemetry enabled)

### Example Log Entry

```json
{
  "@t": "2025-01-01T00:00:00.0000000Z",
  "@m": "Processing order {OrderId} with correlation {CorrelationId}",
  "@r": ["12345", "abc-123-def"],
  "Level": "Information",
  "Service": "pos-service",
  "Environment": "Production",
  "CorrelationId": "abc-123-def",
  "TraceId": "xyz-789"
}
```

### How to Query Logs

#### Seq
```
URL: http://localhost:5341
Default login: admin / admin123

Example queries:
- @Level = 'Error' AND @Service = 'inventory-service'
- @CorrelationId = 'your-correlation-id-here'
- @Service = 'pos-service' AND @t > now() - 1h
```

---

## 2. Metrics

### Prometheus Endpoint

```
GET /metrics
```

### Key Metrics

| Metric | Type | Description |
|--------|------|-------------|
| `http_requests_total` | Counter | Total HTTP requests by method, path, status |
| `http_request_duration_seconds` | Histogram | Request latency distribution |
| `database_query_duration_seconds` | Histogram | Database query latency |
| `background_job_executions_total` | Counter | Scheduled job executions by status |
| `inventory_products_total` | Gauge | Total product count |
| `inventory_low_stock_items` | Gauge | Products below reorder level |

### Prometheus Configuration

```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'inventory-service'
    static_configs:
      - targets: ['inventory-api:8080']
    metrics_path: '/metrics'
    scrape_interval: 15s
```

### Grafana Dashboard

```
URL: http://localhost:3000 (admin/admin)
Import dashboard ID: 14282 (ASP.NET Core Dashboard)

Key panels:
- Request rate (RPS)
- Response time (p50, p95, p99)
- Error rate
- Database connection pool
- Memory usage
- CPU usage
```

---

## 3. Distributed Tracing

### OpenTelemetry Configuration

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddEntityFrameworkCoreInstrumentation()
               .AddJaegerExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddPrometheusExporter();
    });
```

### Jaeger

```
URL: http://localhost:16686
Search by:
- Trace ID (from Correlation ID header)
- Service name (inventory-service)
- Operation name (HTTP GET /api/v1/products)
- Time range

View:
- Trace timeline
- Span durations
- Service dependencies
- Error spans
```

---

## 4. Health Checks

```
GET /health              # Overall health
GET /health/live         # Liveness probe (K8s)
GET /health/ready        # Readiness probe (K8s)
```

### Response

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "self", "status": "Healthy" }
  ]
}
```

---

## 5. Release Information Endpoint

```
GET /api/v1/system/release
```

### Response

```json
{
  "service": "inventory-service",
  "version": "1.0.0",
  "build": "20250101.001",
  "commit": "abc123def456",
  "environment": "Production",
  "apiVersion": "v1",
  "databaseMigration": "20250101_001",
  "features": ["products", "categories", "brands"],
  "releaseNotes": ["Initial release", "Added product catalog"],
  "knownIssues": ["None"]
}
```

**QA/SQA uses this endpoint to determine:**
- What version is deployed
- What features are enabled
- What changed since last release
- Database migration version
- Environment (Development/Staging/Production)

---

## 6. Correlation ID Propagation

Every request gets a correlation ID:
1. Middleware checks `X-Correlation-ID` header
2. If missing, generates new UUID
3. Adds to response headers
4. Logs in every log entry
5. Propagates to downstream services via HTTP headers

### Usage in Code

```csharp
// Access correlation ID
var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault();

// Log with correlation
_logger.LogInformation("Processing {EntityId} with correlation {CorrelationId}", 
    entityId, correlationId);
```

---

## 7. Idempotency

For write operations, client sends `Idempotency-Key` header:

```csharp
// In middleware or controller
var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
if (string.IsNullOrEmpty(idempotencyKey))
    return BadRequest("Idempotency-Key required");
```

Storage:
```csharp
public class IdempotencyKey
{
    public string Key { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
```

---

## 8. Exception Tracking

All exceptions are:
1. Logged with full context (endpoint, method, correlation ID, trace ID)
2. Converted to ProblemDetails response
3. Sent to Seq for search
4. Tracked in Jaeger for distributed tracing

### Example Error Log

```json
{
  "@t": "2025-01-01T00:00:00.0000000Z",
  "@m": "Unhandled exception: {CorrelationId} | Endpoint: {Endpoint} | Method: {Method} | Error: {Message}",
  "Level": "Error",
  "CorrelationId": "abc-123",
  "Endpoint": "/api/v1/products",
  "Method": "POST",
  "Exception": "System.ArgumentException: Name is required"
}
```

---

## 9. Troubleshooting Guide

### Slow Request
1. Find trace in Jaeger by Trace ID
2. Identify slow spans
3. Check database query duration in Seq
4. Check for missing indexes

### Exception Spike
1. Seq: `@Level = 'Error'` with time range
2. Group by `Exception` type
3. Correlate with deployment time
4. Check known issues in release endpoint

### Database Performance
1. Enable EF Core query logging
2. Search Seq for `Executed DbCommand`
3. Check for N+1 queries
4. Verify indexes exist

### Memory Leak
1. Grafana: Memory usage panel
2. Check for undisposed DbContext
3. Check for growing caches
4. Check for event handler leaks
