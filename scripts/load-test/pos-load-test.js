// POS checkout flow load test: open a sale, add a line item, complete it end to end.
// Run: k6 run -e BASE_URL=http://localhost:5001 -e STORE_ID=... -e REGISTER_ID=... -e CASHIER_ID=... -e CASH_SESSION_ID=... scripts/load-test/pos-load-test.js
//
// Prerequisites (this script does not seed reference data, since Store/Cashier/CashRegister creation
// isn't exposed over CQRS endpoints in this phase — see docs/pos-service/handover). Seed via direct DB
// insert or a future admin endpoint, open one CashSession, then pass its ID as CASH_SESSION_ID so every
// virtual user checks out against the same open register session.
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5001';
const STORE_ID = __ENV.STORE_ID;
const REGISTER_ID = __ENV.REGISTER_ID;
const CASHIER_ID = __ENV.CASHIER_ID;
const CASH_SESSION_ID = __ENV.CASH_SESSION_ID;

export const options = {
  stages: [
    { duration: '1m', target: 20 },
    { duration: '3m', target: 20 },
    { duration: '1m', target: 50 },
    { duration: '3m', target: 50 },
    { duration: '1m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<800'],
    http_req_failed: ['rate<0.01'],
  },
};

const jsonHeaders = { headers: { 'Content-Type': 'application/json' } };

export default function () {
  if (!STORE_ID || !REGISTER_ID || !CASHIER_ID || !CASH_SESSION_ID) {
    throw new Error('STORE_ID, REGISTER_ID, CASHIER_ID, and CASH_SESSION_ID env vars are required — see script header.');
  }

  // 1. Open a sale
  const createRes = http.post(
    `${BASE_URL}/api/v1/sales`,
    JSON.stringify({ storeId: STORE_ID, registerId: REGISTER_ID, cashierId: CASHIER_ID, cashSessionId: CASH_SESSION_ID }),
    jsonHeaders,
  );
  const createOk = check(createRes, { 'create sale: status 200': (r) => r.status === 200 });
  if (!createOk) {
    sleep(1);
    return;
  }
  const saleId = createRes.json();

  // 2. Add a line item (synthetic product; POS never validates against Inventory's DB directly)
  const addItemRes = http.post(
    `${BASE_URL}/api/v1/sales/items`,
    JSON.stringify({
      saleId,
      productId: '00000000-0000-0000-0000-000000000001',
      productName: 'Load Test Widget',
      sku: 'LOADTEST-WIDGET',
      unitPrice: 9.99,
      quantity: 1 + (__VU % 3),
    }),
    jsonHeaders,
  );
  check(addItemRes, { 'add item: status 200': (r) => r.status === 200 });

  // 3. Complete the sale with exact cash payment
  const completeRes = http.post(
    `${BASE_URL}/api/v1/sales/complete`,
    JSON.stringify({ saleId, payments: [{ method: 1, amount: 9.99 * (1 + (__VU % 3)), referenceNumber: null }] }),
    jsonHeaders,
  );
  check(completeRes, { 'complete sale: status 204': (r) => r.status === 204 });

  sleep(1);
}
