// scripts/stress-test/pos-stress-test.js
// Run: k6 run -e BASE_URL=http://localhost:5001 -e STORE_ID=<uuid> ... pos-stress-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL  = __ENV.BASE_URL   || 'http://localhost:5001';
const STORE_ID  = __ENV.STORE_ID   || '00000000-0000-0000-0000-000000000001';
const REG_ID    = __ENV.REG_ID     || '00000000-0000-0000-0000-000000000002';
const CASHIER_ID= __ENV.CASHIER_ID || '00000000-0000-0000-0000-000000000003';
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
  const headers = { 'Content-Type': 'application/json' };

  // Open sale
  const openRes = http.post(
    `${BASE_URL}/api/v1/sales`,
    JSON.stringify({ storeId: STORE_ID, registerId: REG_ID, cashierId: CASHIER_ID, cashSessionId: SESSION_ID, customerId: null }),
    { headers },
  );
  const ok = check(openRes, { 'create sale 200': (r) => r.status === 200 });
  if (!ok) { sleep(0.1); return; }

  const saleId = JSON.parse(openRes.body);

  // Add item
  const addRes = http.post(
    `${BASE_URL}/api/v1/sales/items`,
    JSON.stringify({ saleId, productId: '00000000-0000-0000-0000-000000000010', productName: 'Widget', sku: 'WID-001', unitPrice: 99.99, quantity: 2 }),
    { headers },
  );
  check(addRes, { 'add item 200': (r) => r.status === 200 });

  // Complete
  const completeRes = http.post(
    `${BASE_URL}/api/v1/sales/complete`,
    JSON.stringify({ saleId, payments: [{ method: 'Cash', amount: 200.00, referenceNumber: null }] }),
    { headers },
  );
  check(completeRes, { 'complete 204': (r) => r.status === 204 });

  sleep(0.1);
}
