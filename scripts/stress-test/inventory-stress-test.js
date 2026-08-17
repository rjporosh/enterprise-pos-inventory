// scripts/stress-test/inventory-stress-test.js
// Run: k6 run -e BASE_URL=http://localhost:5002 inventory-stress-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5002';

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
  const res = http.get(`${BASE_URL}/api/v1/products`);
  check(res, { 'products 200': (r) => r.status === 200 });
  sleep(0.1);
}
