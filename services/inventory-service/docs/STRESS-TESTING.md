# Stress Testing Guide

## Objective
Find the breaking point of the service and verify graceful degradation.

## Test Profile

```
Phase 1: Ramp up (1 → 500 users over 5 minutes)
Phase 2: Normal load (500 users for 10 minutes)
Phase 3: Spike (500 → 5000 users in 1 minute)
Phase 4: Stress (5000 users for 5 minutes)
Phase 5: Recovery (ramp down to 0 over 5 minutes)
```

---

## k6 Stress Test

```javascript
// scripts/stress-test/inventory-stress-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: [
    {
      executor: 'ramping-arrival-rate',
      scenario: 'stress',
      startRate: 10,
      timeUnit: '1s',
      preAllocatedVUs: 50,
      maxVUs: 10000,
      stages: [
        { duration: '5m', target: 100 },
        { duration: '10m', target: 1000 },
        { duration: '2m', target: 5000 },
        { duration: '5m', target: 0 },
      ],
    },
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'],
    http_req_failed: ['rate<0.05'],
  },
};

export default function () {
  const res = http.get('http://localhost:5002/api/v1/products');
  check(res, {
    'status is 200': (r) => r.status === 200,
  });
  sleep(0.1);
}
```

---

## Success Criteria

| Metric | Target | Failure |
|--------|--------|---------|
| Service Uptime | 100% | < 99% |
| Error Rate | < 1% | > 5% |
| Response Time p95 | < 2s | > 5s |
| Recovery Time | < 60s | > 120s |
| Data Integrity | 100% | Any corruption |

---

## Resource Monitoring

### CPU/Memory (via Grafana)
- Alert if CPU > 85% for 2 minutes
- Alert if Memory > 90%
- Alert if Database connections > 80% of pool

### Database
- Monitor connection pool usage
- Monitor query duration
- Monitor lock wait time
- Monitor deadlocks

---

## Execution

```bash
# Run stress test
k6 run scripts/stress-test/inventory-stress-test.js

# Monitor in parallel
# - Grafana: http://localhost:3000
# - Seq: http://localhost:5341
# - Jaeger: http://localhost:16686
```

---

## Analysis

1. Identify breaking point (max RPS before errors)
2. Verify graceful degradation (errors return proper status codes)
3. Verify recovery (service returns to normal after stress ends)
4. Document findings in stress test report
