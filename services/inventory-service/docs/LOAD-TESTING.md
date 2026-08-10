# Load Testing Guide

## Objective
Verify the service handles expected production load with acceptable performance.

## Tools

### k6 (Recommended for API Load Testing)
```bash
# Install
brew install k6

# Run test
k6 run scripts/load-test/inventory-load-test.js
```

### NBomber (C# Load Testing)
```bash
dotnet run --project scripts/load-test/InventoryLoadTest.csproj
```

---

## Test Scenarios

### Scenario 1: Product Catalog Read

```javascript
// scripts/load-test/inventory-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '2m', target: 100 },
    { duration: '5m', target: 100 },
    { duration: '2m', target: 200 },
    { duration: '5m', target: 200 },
    { duration: '2m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.01'],
  },
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

### Scenario 2: Product Creation (Write)

```javascript
export const options = {
  scenarios: [
    {
      executor: 'ramping-vus',
      scenario: 'create_product',
      startVUs: 0,
      stages: [
        { duration: '2m', target: 50 },
        { duration: '5m', target: 50 },
      ],
    },
  ],
};

export default function () {
  const payload = JSON.stringify({
    name: `Product ${__VU}`,
    sku: `SKU-${__VU}-${Date.now()}`,
    price: 1000,
  });

  const res = http.post('http://localhost:5002/api/v1/products', payload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(res, {
    'status is 200': (r) => r.status === 200,
  });
  sleep(1);
}
```

---

## Success Criteria

| Metric | Target | Acceptable |
|--------|--------|------------|
| p95 Response Time | < 200ms | < 500ms |
| p99 Response Time | < 500ms | < 1000ms |
| Error Rate | < 0.1% | < 1% |
| Throughput | > 1000 RPS | > 500 RPS |
| CPU Usage | < 70% | < 85% |
| Memory Usage | < 80% | < 90% |

---

## How to Run

```bash
# Start service
docker compose -f services/inventory-service/docker-compose.dev.yml up

# Run k6 test
k6 run scripts/load-test/inventory-load-test.js

# Run with Grafana dashboard
k6 run --out influxdb=http://localhost:8086/k6 scripts/load-test/inventory-load-test.js
```

---

## Analysis

1. Review k6 summary report
2. Check Grafana for resource utilization
3. Identify slow endpoints
4. Check database for slow queries
5. Optimize based on findings
