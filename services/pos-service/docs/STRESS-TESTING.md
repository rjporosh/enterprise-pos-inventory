# Stress Testing Guide — POS Service

## Objective

Find the breaking point of the POS service and verify graceful degradation under extreme load.
The POS checkout flow (CreateSale → AddSaleItem → CompleteSale) is the critical path; it must
degrade predictably and recover quickly.

## Test Profile

```
Phase 1: Ramp up      (1 → 500 VUs over 5 minutes)
Phase 2: Normal load  (500 VUs for 10 minutes)
Phase 3: Spike        (500 → 5000 VUs in 1 minute)
Phase 4: Stress       (5000 VUs for 5 minutes)
Phase 5: Recovery     (ramp down to 0 over 5 minutes)
```

---

## k6 Stress Test

```javascript
// scripts/stress-test/pos-stress-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5001';
const STORE_ID  = __ENV.STORE_ID  || '00000000-0000-0000-0000-000000000001';
const REG_ID    = __ENV.REG_ID    || '00000000-0000-0000-0000-000000000002';
const CASHIER_ID= __ENV.CASHIER_ID|| '00000000-0000-0000-0000-000000000003';
const SESSION_ID= __ENV.SESSION_ID || '00000000-0000-0000-0000-000000000004';

export const options = {
  scenarios: {
    stress: {
      executor: 'ramping-arrival-rate',
      startRate: 10,
      timeUnit: '1s',
      preAllocatedVUs: 50,
      maxVUs: 10000,
      stages: [
        { duration: '5m',  target: 100  },
        { duration: '10m', target: 1000 },
        { duration: '2m',  target: 5000 },
        { duration: '5m',  target: 0    },
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<2000'],
    http_req_failed:   ['rate<0.05'],
  },
};

export default function () {
  // ── 1. Open a sale ────────────────────────────────────────────────────────
  const openRes = http.post(
    `${BASE_URL}/api/v1/sales`,
    JSON.stringify({
      storeId:      STORE_ID,
      registerId:   REG_ID,
      cashierId:    CASHIER_ID,
      cashSessionId:SESSION_ID,
      customerId:   null,
    }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  const ok = check(openRes, {
    'create sale: 200 OK': (r) => r.status === 200,
  });
  if (!ok) {
    sleep(0.1);
    return;
  }

  const saleId = JSON.parse(openRes.body);

  // ── 2. Add a line item ────────────────────────────────────────────────────
  const addRes = http.post(
    `${BASE_URL}/api/v1/sales/items`,
    JSON.stringify({
      saleId,
      productId:   '00000000-0000-0000-0000-000000000010',
      productName: 'Stress Widget',
      sku:         'STR-001',
      unitPrice:   99.99,
      quantity:    2,
    }),
    { headers: { 'Content-Type': 'application/json' } },
  );
  check(addRes, { 'add item: 200 OK': (r) => r.status === 200 });

  // ── 3. Complete the sale ──────────────────────────────────────────────────
  const completeRes = http.post(
    `${BASE_URL}/api/v1/sales/complete`,
    JSON.stringify({
      saleId,
      payments: [{ method: 'Cash', amount: 200.00, referenceNumber: null }],
    }),
    { headers: { 'Content-Type': 'application/json' } },
  );
  check(completeRes, { 'complete sale: 204 No Content': (r) => r.status === 204 });

  sleep(0.1);
}
```

---

## Read-only Stress Test (GET endpoints only)

For a lighter read-only profile that stresses the reporting and list endpoints:

```javascript
// scripts/stress-test/pos-readonly-stress-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5001';

export const options = {
  scenarios: {
    reads: {
      executor: 'constant-arrival-rate',
      rate: 500,
      timeUnit: '1s',
      duration: '10m',
      preAllocatedVUs: 100,
      maxVUs: 5000,
    },
  },
  thresholds: {
    http_req_duration: ['p(99)<500'],
    http_req_failed:   ['rate<0.01'],
  },
};

export default function () {
  const endpoints = [
    '/api/v1/sales?pageNumber=1&pageSize=20',
    '/api/v1/reports/daily-sales',
    '/health',
    '/api/v1/system/release',
  ];
  const url = endpoints[Math.floor(Math.random() * endpoints.length)];
  const res = http.get(`${BASE_URL}${url}`);
  check(res, { '2xx': (r) => r.status >= 200 && r.status < 300 });
  sleep(0.05);
}
```

---

## Success Criteria

| Metric | Target | Failure Threshold |
|--------|--------|-------------------|
| Service Uptime | 100% | < 99% |
| Error Rate | < 1% | > 5% |
| Response Time p95 | < 2 s | > 5 s |
| Response Time p99 | < 5 s | > 10 s |
| Recovery Time | < 60 s | > 120 s |
| Data Integrity | 100% | Any corruption |

---

## Resource Monitoring During Stress

### CPU/Memory (via Grafana)
- Alert if CPU > 85% for 2 minutes
- Alert if Memory > 90%
- Alert if DB connections > 80% of pool

### Database Indicators
- Monitor connection pool utilisation (`pg_stat_activity`)
- Monitor query duration (via EF query logging or OTEL traces)
- Monitor lock waits and deadlocks (`pg_locks`)

### RabbitMQ (if integrated)
- Monitor queue depth on `pos.events` exchange
- Alert if consumer lag > 1000 messages

---

## Execution

```bash
# Full checkout stress test
k6 run \
  -e BASE_URL=http://localhost:5001 \
  -e STORE_ID=<uuid> \
  -e REG_ID=<uuid> \
  -e CASHIER_ID=<uuid> \
  -e SESSION_ID=<uuid> \
  scripts/stress-test/pos-stress-test.js

# Read-only stress test
k6 run scripts/stress-test/pos-readonly-stress-test.js

# Monitor in parallel:
#   Grafana:  http://localhost:3000
#   Jaeger:   http://localhost:16686
#   Metrics:  http://localhost:5001/metrics
```

---

## Analysis Checklist

1. Identify breaking point (max RPS before error rate exceeds 1%)
2. Confirm graceful degradation — errors must return proper ProblemDetails responses (not HTML / 500 panics)
3. Confirm no sale records were corrupted (check `SELECT status, count(*) FROM pos.sales GROUP BY status`)
4. Confirm recovery: after load drops, p95 must return to < 200 ms within 60 s
5. Document findings in the stress test report and commit to `docs/stress-test-results/`
