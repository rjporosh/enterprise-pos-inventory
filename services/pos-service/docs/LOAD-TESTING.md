# Load Testing Guide (POS)

## Objective
Verify the POS checkout flow handles expected in-store + multi-register load with acceptable
performance, independent of whether Inventory or RabbitMQ are reachable.

## Tools

### k6 (Recommended)
```bash
# Install
brew install k6

# Seed one Store/Cashier/CashRegister and open a CashSession first (see scripts/load-test/pos-load-test.js
# header — this phase doesn't expose reference-data creation over the API yet), then:
k6 run \
  -e BASE_URL=http://localhost:5001 \
  -e STORE_ID=<seeded-store-id> \
  -e REGISTER_ID=<seeded-register-id> \
  -e CASHIER_ID=<seeded-cashier-id> \
  -e CASH_SESSION_ID=<opened-session-id> \
  scripts/load-test/pos-load-test.js
```

## Test Scenario: Full Checkout (Open → Add Item → Complete)

See `scripts/load-test/pos-load-test.js`. Each virtual user repeats: open a sale against the shared
cash session, add one line item, complete the sale with exact cash payment. This exercises the write
path most representative of real POS load — Inventory and RabbitMQ do not need to be running for this
to succeed, per the PRIMARY GOAL that POS works standalone.

## Success Criteria

| Metric | Target | Acceptable |
|--------|--------|------------|
| p95 Response Time | < 300ms | < 800ms |
| p99 Response Time | < 800ms | < 1500ms |
| Error Rate | < 0.1% | < 1% |
| Throughput | > 200 checkouts/sec | > 100 checkouts/sec |

Checkout latency budget is looser than Inventory's read-heavy catalog target since each checkout is
three sequential writes (create → add item → complete) rather than one read.

## How to Run

```bash
docker compose -f services/pos-service/docker-compose.dev.yml up
k6 run -e BASE_URL=http://localhost:5001 -e STORE_ID=... -e REGISTER_ID=... -e CASHIER_ID=... -e CASH_SESSION_ID=... scripts/load-test/pos-load-test.js
```

## Isolation Verification Under Load

Two variants worth running explicitly, matching the roadmap's failure-scenario requirements:

1. **RabbitMQ stopped**: stop the broker, re-run the load test. Checkout throughput/latency should be
   unaffected — `CompleteSaleHandler`/`VoidSaleHandler` treat a publish failure as non-fatal (logged,
   not thrown). Cross-check `SaleCompleted`/`SaleVoided` publish failures in the logs afterward.
2. **Inventory stopped entirely**: same expectation — POS has no runtime call into Inventory at all in
   the checkout path, so this should show zero difference from the baseline run.

## Analysis

1. Review the k6 summary report (p50/p95/p99, error rate, iterations/sec).
2. Cross-reference with the Grafana dashboard (`docs/observability/grafana-dashboard.json`) for
   Postgres connection pool saturation and .NET GC pressure during the run.
3. If p99 degrades under the 50-VU stage, check `CompleteSaleHandler`'s three sequential DB round-trips
   (create → add item → complete are three separate HTTP calls/transactions in this test, matching how
   a real POS terminal UI would call the API) — batching is a possible future optimization if this
   becomes a bottleneck.
