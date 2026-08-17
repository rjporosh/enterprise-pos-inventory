# Programmer's Guide — Enterprise POS & Inventory

## 1. How to Add a New CRUD Feature

### Example: Adding "Product" CRUD to Inventory Service

#### Step 1: Create Domain Entity

**File:** `services/inventory-service/src/InventoryService.Domain/Products/Product.cs`

```csharp
using InventoryService.Domain.Common;
using SharedKernel;

namespace InventoryService.Domain.Products;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public Product() { }

    public Product(string name, string sku, decimal price)
    {
        Name = SharedKernel.Guard.NotNullOrEmpty(name, nameof(name));
        Sku = SharedKernel.Guard.NotNullOrEmpty(sku, nameof(sku));
        Price = SharedKernel.Guard.NotNegative(price, nameof(price));
    }
}
```

**Rules:**
- Inherit from `BaseEntity` (provides Id, audit fields, soft delete, tenant)
- Use `SharedKernel.Guard` for validation in constructors/methods
- Keep domain logic in the entity (methods that modify state)
- No EF Core attributes or DbContext references in Domain layer

#### Step 2: Create Command/Query

**File:** `services/inventory-service/src/InventoryService.Application/Products/CreateProduct/CreateProductCommand.cs`

```csharp
using MediatR;
using SharedKernel;

namespace InventoryService.Application.Products.CreateProduct;

public record CreateProductCommand(string Name, string Sku, decimal Price) : IRequest<Result<Guid>>;
```

**File:** `services/inventory-service/src/InventoryService.Application/Products/GetProductById/GetProductByIdQuery.cs`

```csharp
using MediatR;
using SharedKernel;

namespace InventoryService.Application.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
```

#### Step 3: Create Validator

**File:** `services/inventory-service/src/InventoryService.Application/Products/CreateProduct/CreateProductValidator.cs`

```csharp
using FluentValidation;

namespace InventoryService.Application.Products.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
```

#### Step 4: Create Handler

**File:** `services/inventory-service/src/InventoryService.Application/Products/CreateProduct/CreateProductHandler.cs`

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace InventoryService.Application.Products.CreateProduct;

public class CreateProductHandler(
    ILogger<CreateProductHandler> logger,
    InventoryDbContext context) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product(request.Name, request.Sku, request.Price);
        context.Products.Add(product);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created product {ProductId}", product.Id);
        return product.Id;
    }
}
```

#### Step 5: Create DTO

**File:** `services/inventory-service/src/InventoryService.Application/Products/ProductDto.cs`

```csharp
namespace InventoryService.Application.Products;

public record ProductDto(Guid Id, string Name, string Sku, decimal Price);
```

#### Step 6: Create Entity Configuration

**File:** `services/inventory-service/src/InventoryService.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InventoryService.Domain.Products;
using SharedKernel;

namespace InventoryService.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : BaseEntityConfiguration<Product, Guid>
{
    public ProductConfiguration() : base("products") { }

    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
        builder.HasIndex(p => p.Sku).HasDatabaseName("idx_products_sku").IsUnique();
    }
}
```

#### Step 7: Register in DbContext

```csharp
// InventoryDbContext.cs - Already handled by ApplyConfigurationsFromAssembly
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
}
```

#### Step 8: Create Controller

**File:** `services/inventory-service/src/InventoryService.API/Controllers/ProductsController.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace InventoryService.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController(IMediator mediator, ILogger<ProductsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        if (result.IsFailure) return NotFound(result.Error);
        return Ok(result.Value);
    }
}
```

#### Step 9: Create Migration

```bash
dotnet ef migrations add AddProductTable \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API \
  --output-dir Migrations

dotnet ef database update \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API
```

#### Step 10: Write Tests

```csharp
// Unit test
[Fact]
public async Task CreateProduct_ShouldReturnProductId()
{
    // Arrange
    var handler = new CreateProductHandler(logger, context);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
}

// Integration test
[Fact]
public async Task CreateProduct_ShouldPersistToDatabase()
{
    // Use Respawn to reset database
    // Test full flow through DbContext
}
```

---

## 2. How to Add a Scheduled Job

### Example: Daily Sales Report Job

#### Step 1: Create Job Class

```csharp
// services/pos-service/src/PosService.Application/Jobs/DailySalesReportJob.cs
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace PosService.Application.Jobs;

public class DailySalesReportJob(
    ILogger<DailySalesReportJob> logger,
    ISalesReportService reportService) : IHostedService, IDisposable
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Daily Sales Report Job started");
        // Schedule at midnight UTC
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddDays(1);
        var dueTime = nextRun - now;
        
        _timer = new Timer(GenerateReport, null, dueTime, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private async void GenerateReport(object? state)
    {
        try
        {
            logger.LogInformation("Generating daily sales report for {Date}", DateTime.UtcNow.Date.AddDays(-1));
            var report = await reportService.GenerateDailyReport(DateTime.UtcNow.Date.AddDays(-1));
            logger.LogInformation("Daily report generated successfully. Total sales: {Total}", report.TotalSales);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate daily sales report");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
```

#### Step 2: Register in Program.cs

```csharp
builder.Services.AddHostedService<DailySalesReportJob>();
```

#### Rules for Scheduled Jobs:
- Must be idempotent (safe to run multiple times)
- Must log execution start/end/failure
- Must handle application restarts gracefully
- Must use UTC for all time calculations
- Must be testable (inject services, avoid static state)

---

## 3. How to Add a Background Service

Same as scheduled job, but for continuous processing:

```csharp
public class InventorySyncService : BackgroundService
{
    private readonly ILogger<InventorySyncService> _logger;
    
    public InventorySyncService(ILogger<InventorySyncService> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Process queue
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

Register:
```csharp
builder.Services.AddHostedService<InventorySyncService>();
```

---

## 4. How to Add a Database Migration

```bash
# 1. Ensure PostgreSQL is running
docker compose -f services/inventory-service/docker-compose.dev.yml up -d postgres

# 2. Create migration
dotnet ef migrations add AddNewTable \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API \
  --output-dir Migrations

# 3. Review generated migration
cat services/inventory-service/src/InventoryService.Infrastructure/Migrations/[timestamp]_AddNewTable.cs

# 4. Apply migration
dotnet ef database update \
  --project services/inventory-service/src/InventoryService.Infrastructure \
  --startup-project services/inventory-service/src/InventoryService.API

# 5. Verify
psql -U postgres -d inventory_db -c "\dt inventory.*"
```

---

## 5. How to Run Tests

```bash
# Run all tests
dotnet test EnterprisePOS.sln --configuration Release

# Run specific test project
dotnet test services/inventory-service/tests/InventoryService.UnitTests/InventoryService.UnitTests.csproj
dotnet test services/inventory-service/tests/InventoryService.IntegrationTests/InventoryService.IntegrationTests.csproj
dotnet test services/inventory-service/tests/InventoryService.FunctionalTests/InventoryService.FunctionalTests.csproj

# Run with coverage
dotnet test services/inventory-service/tests/InventoryService.UnitTests/InventoryService.UnitTests.csproj \
  --collect:"Code Coverage" \
  --results-directory ./test-results

# Run specific test
dotnet test --filter "FullyQualifiedName~CreateProduct_ShouldReturnProductId"

# Run in parallel
dotnet test --parallel
```

---

## 6. How to Check Logs

### Seq (Primary)
```bash
# Start Seq
docker compose up seq

# Access at: http://localhost:5341
# Search for errors:
#   @Level = 'Error'
#   @Service = 'inventory-service'
#   @CorrelationId = 'your-correlation-id'
```

### File-based (Fallback)
```bash
tail -f services/inventory-service/logs/inventory-service-.log
```

### Structured JSON Fields to Search:
- `@t` — Timestamp
- `@m` — Message
- `@r` — Properties
- `Level` — Log level
- `Service` — Service name
- `Environment` — Environment
- `CorrelationId` — Request correlation ID
- `TraceId` — Distributed trace ID

---

## 7. How to Check Metrics

### Prometheus (Future)
```
# Metrics endpoint: /metrics
# Key metrics:
#   - http_request_duration_seconds
#   - http_requests_total
#   - database_query_duration_seconds
#   - background_job_executions_total
```

### Grafana Dashboard
```
URL: http://localhost:3000 (admin/admin)
Dashboards:
  - Service health
  - Request rate/latency
  - Database performance
  - Error rates
```

---

## 8. How to Perform Load Testing

### k6 Script Example

```javascript
// scripts/load-test/inventory-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '2m', target: 100 },  // Ramp up to 100 users
    { duration: '5m', target: 100 },  // Stay at 100 users
    { duration: '2m', target: 200 },  // Ramp up to 200 users
    { duration: '5m', target: 200 },  // Stay at 200 users
    { duration: '2m', target: 0 },    // Ramp down
  ],
};

export default function () {
  const res = http.get('http://localhost:5002/api/v1/products');
  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
  sleep(1);
}
```

Run:
```bash
k6 run scripts/load-test/inventory-load-test.js
```

### NBomber (C#)
```csharp
// scripts/load-test/InventoryLoadTest.cs
var scenario = Scenario.Create("load_test_inventory", async context =>
{
    var client = new HttpClient();
    var response = await client.GetAsync("http://localhost:5002/api/v1/products");
    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
})
.WithLoadSimulation(LoadSimulation.NewRamp(TimeSpan.FromMinutes(2), 100));

NBomberRunner.RegisterScenarios(scenario).Run();
```

---

## 9. How to Perform Stress Testing

### Objective
Find breaking point and verify graceful degradation.

### Test Profile
- Ramp: 1 → 1000 users over 10 minutes
- Hold: 1000 users for 10 minutes
- Spike: 1000 → 5000 users in 1 minute
- Monitor: CPU, memory, error rate, response time

### Success Criteria
- Error rate < 1% at normal load
- Response time p95 < 1s at normal load
- Service recovers within 60s after stress ends
- No data corruption

### k6 Stress Test
```javascript
export const options = {
  scenarios: [
    {
      executor: 'ramping-arrival-rate',
      scenario: 'stress',
      startRate: 10,
      timeUnit: '1s',
      preAllocatedVUs: 50,
      maxVUs: 5000,
      stages: [
        { duration: '5m', target: 100 },
        { duration: '10m', target: 1000 },
        { duration: '2m', target: 5000 },
        { duration: '5m', target: 0 },
      ],
    },
  ],
};
```

---

## 10. Correlation ID and Idempotency

### Correlation ID

Every request gets a correlation ID automatically via middleware:

```csharp
// Middleware automatically generates if not present
// Access in any service:
var correlationId = _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault();

// Log with correlation:
_logger.LogInformation("Processing order {OrderId} with correlation {CorrelationId}", 
    orderId, correlationId);
```

### Idempotency Key

For write operations (POST/PUT), client sends `Idempotency-Key` header:

```csharp
// In controller or middleware
var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
if (string.IsNullOrEmpty(idempotencyKey))
    return BadRequest("Idempotency-Key header required");

// Check if already processed
var existing = await _context.IdempotencyKeys.FindAsync(idempotencyKey);
if (existing != null) return Ok(existing.Result);

// Process and store
var result = await ProcessCommand(command);
_context.IdempotencyKeys.Add(new IdempotencyKey { Key = idempotencyKey, Result = result });
await _context.SaveChangesAsync();
```

---

## 11. Database Abstraction

### Provider Selection (Configuration-driven)

```json
// appsettings.json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Port=5432;Database=inventory_db;Username=postgres;Password=postgres"
  }
}
```

### Adding a New Provider

1. Implement `IDbProviderFactory`:
```csharp
public class SqlServerProviderFactory : IDbProviderFactory
{
    public string ProviderName => "SQL Server";
    
    public DbContextOptionsBuilder UseProvider(DbContextOptionsBuilder builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
        return builder;
    }
}
```

2. Register in `DbContextServiceCollectionExtensions`:
```csharp
"sqlserver" => new SqlServerProviderFactory()
```

3. Update `appsettings.json`:
```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=..."
  }
}
```

No business logic changes required.

---

## 12. Query Logging

Enable EF Core query logging:

```csharp
// In DbContext options
optionsBuilder.UseNpgsql(connectionString)
    .EnableSensitiveDataLogging(false)  // Never enable in production
    .LogTo(Console.WriteLine, LogLevel.Information);
```

Structured query log includes:
- Executed DbCommand
- Parameters (when not sensitive)
- Duration
- Correlation ID (via Enrich.WithProperty)

---

## 13. Centralized Exception Handler

All exceptions are caught by `GlobalExceptionHandler` middleware:

```csharp
public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

Response format:
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Failed",
  "status": 400,
  "detail": "Name is required",
  "instance": "/api/v1/products",
  "traceId": "abc123",
  "correlationId": "xyz789",
  "errors": { "Name": ["Name is required"] },
  "timestamp": "2025-01-01T00:00:00Z"
}
```

---

## 14. Testing Strategy

### Unit Tests (xUnit + FluentAssertions + Moq)
- Domain entity behavior
- Command/Query handlers
- Validators
- Business rules

### Integration Tests (Respawn + WebApplicationFactory)
- Full API workflows
- Database round-trips
- Transaction rollback between tests

### Functional Tests
- End-to-end API scenarios
- Authentication flows
- Error scenarios

### Run All
```bash
dotnet test EnterprisePOS.sln --verbosity normal
```
