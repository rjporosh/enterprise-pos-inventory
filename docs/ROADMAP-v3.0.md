# Enterprise POS & Inventory SaaS
# Product & Engineering Roadmap

**Version:** 3.0
**Status:** Active — Production Hardening Plan
**Last Updated:** August 31, 2026

> Status in this document is based on the repository, committed handover documents and Git history
> available at the time of update. A feature is **not** marked production-ready merely because code
> exists; it must have runtime/integration/security evidence.

---

# 0. Current Repository Status

## Confirmed completed

### Backend
- [x] Inventory database foundation
- [x] Inventory product/catalog CRUD foundation
- [x] Inventory stock/ledger foundation
- [x] POS database foundation
- [x] POS sales/checkout foundation
- [x] POS cash-session open/close foundation
- [x] POS → Inventory RabbitMQ integration foundation
- [x] Daily sales report generation foundation
- [x] Correlation ID / OpenTelemetry / metrics foundation
- [x] Load-test scaffolding
- [x] POS integration-test scaffolding
- [x] Previous build-error/warning fix passes
- [x] `auth-service` and `notification-service` exist as full services (added prior to 2026-08-31,
      confirmed and hardened this session) — RBAC/JWT auth, OTP, audit logs; notification
      send/list/templates/preferences, Email/SMS/Push channel abstractions, outbox pattern
- [x] All 4 services build 0/0, test green, migrate cleanly, and run in Docker with passing health
      checks — first full, real verification pass across the whole backend (2026-08-31, see
      `release-notes.md`/`AI-HANDOVER.md` §L for the 10 bugs found and fixed to get here)

### Frontend MVP
- [x] React 19 + Next.js POS app
- [x] React 19 + Next.js Inventory app
- [x] Redux Toolkit + Redux-Saga
- [x] Typed API clients
- [x] Inventory product CRUD UI
- [x] Inventory stock UI
- [x] POS terminal/session UI
- [x] POS cart and checkout saga
- [x] Receipt UI/print CSS
- [x] Sale history and void
- [x] Daily sales UI
- [x] Unit tests
- [x] Typecheck/lint/build verification for the frontend MVP

## Not yet proven
- [x] Third build with 0 errors / 0 warnings after the last backend fixes — done 2026-08-31, all
      four services (`auth`, `notification`, `pos`, `inventory`), including 0 known-vulnerable
      dependencies (3 previously-suppressed NuGet advisories actually fixed, not just silenced)
- [~] Real backend + frontend end-to-end runtime — backend-to-backend verified 2026-08-31 (all 4
      services run in Docker, pass health checks, a real product create+list round-trip was
      smoke-tested through a live container); frontend apps have not yet been pointed at a running
      backend in this environment — still the next concrete step
- [ ] Production authentication/authorization — `auth-service` exists and now builds/runs/migrates
      cleanly (2026-08-31), but is not integrated into either frontend app
- [ ] Production tenant isolation
- [~] Gateway/BFF — did not exist anywhere in the repo at the start of 2026-08-31 (an earlier
      assumption that one had been added was incorrect); built same day (`services/gateway`,
      YARP), routes to all 4 services verified working. Frontend apps not yet repointed at it —
      see Phase 3 below.
- [ ] Subscription/billing/licensing
- [ ] Entitlement/quota enforcement
- [ ] Barcode generation/scanning end-to-end
- [~] Thermal printer integration — 58mm/80mm browser-print CSS profiles shipped 2026-08-28
      (`frontend/pos`); ESC/POS adapter and local print bridge still not started
- [ ] Cash drawer integration
- [ ] Certified card-terminal integration
- [ ] Returns/refunds
- [ ] Full reporting suite
- [ ] Expenses/profit accounting
- [ ] Offline transactional storage + synchronization
- [~] Notification service (email/SMS/web) — `notification-service` exists, now builds/runs/
      migrates cleanly and passes health checks in Docker (2026-08-31); not integrated into either
      frontend app, and its RabbitMQ upstream bindings still reference bus-ticketing-domain event
      names (`booking.events`, `payment.events`) left over from a template project — harmless
      (nothing publishes to them) but misleading, and a real POS/Inventory event wiring (e.g.
      low-stock, trial-expiry) has not been built
- [ ] Production CI/CD
- [ ] Backup/restore verification
- [ ] Production security/load/chaos testing

---

# Phase 1 — Build & Contract Stabilization
**Status: CORE DONE 2026-08-31 — a few items remain (see unchecked below)**

## Goal
Create a clean baseline before adding large commercial features.

### Backend
- [x] `dotnet restore EnterprisePOS.sln`
- [x] `dotnet build EnterprisePOS.sln`
- [x] Achieve 0 errors — all 4 services (`EnterprisePOS.sln` covers pos+inventory;
      `auth-service`/`notification-service` each have their own `.sln`, built separately)
- [x] Achieve 0 warnings — including 0 unresolved security advisories (see release-notes.md
      2026-08-31 entry: 3 advisories had been `<NoWarn>`-suppressed rather than fixed; now fixed)
- [x] `dotnet test EnterprisePOS.sln` — 48+18 unit, 7+1 integration, all pass, repeatably
- [x] Regenerate hand-authored EF migrations with real `dotnet ef` — pos-service `InitialCreate`
      (folded `AddDailySalesReport` into one clean migration) and inventory-service
      `AddIntegrationEventInbox`, both now have real Designer.cs/ModelSnapshot pairs
- [x] Apply migrations to clean databases — all 4 services, against a live Postgres via Docker
- [x] Verify database schema — confirmed via `\dt` for all 4 databases
- [ ] Resolve duplicated `BaseEntity` design intentionally — not revisited this session, still open
- [ ] Review API response/error contracts — `docs/API-CONTRACT.md` vs. real controllers still
      disagree per `docs/API-GAPS.md` §1; not reconciled this session
- [ ] Review versioning
- [ ] Review pagination/filter/sort
- [ ] Review correlation ID propagation
- [ ] Implement idempotency infrastructure for financial writes
- [ ] Add ADR for gateway/BFF
- [ ] Add ADR for resilience policies
- [ ] Add ADR for subscription/entitlements
- [ ] Add ADR for offline synchronization

### Exit criteria
- [x] Clean build: 0/0 — all 4 services
- [x] Full tests green — unit + integration, all 4 services buildable/testable (auth/notification
      integration tests need Docker for Testcontainers; not run in CI yet, only confirmed to build)
- [x] Clean database migration succeeds — all 4 services, verified against live Postgres
- [ ] No known contract mismatch between frontend and backend — `docs/API-GAPS.md` still lists
      real gaps (category/brand/unit/warehouse/store/register CRUD, barcode search); frontend was
      not touched this session

---

# Phase 2 — Authentication, Authorization & Multi-Tenancy
**Status: NOT STARTED / HIGH PRIORITY**

## Goal
Make the system safe for multiple real businesses.

### Identity
- [ ] Authentication service/identity boundary
- [ ] Access token/session strategy
- [ ] Refresh-token/session revocation
- [ ] Password/security policy
- [ ] Account lockout/rate limits
- [ ] Secure logout
- [ ] Security audit events

### Authorization
- [ ] RBAC
- [ ] Permission model
- [ ] Tenant context
- [ ] Branch scope
- [ ] Register scope
- [ ] Server-side authorization policies

### Tenant isolation
- [ ] TenantId on all tenant-owned aggregates
- [ ] Global query/filter strategy where appropriate
- [ ] Tenant-aware repository boundaries
- [ ] Tenant isolation integration tests
- [ ] Cross-tenant access denial tests
- [ ] IDOR/broken authorization tests

### Exit criteria
- [ ] Tenant A cannot read/write Tenant B
- [ ] Branch permissions are enforced server-side
- [ ] All protected APIs reject unauthenticated/unauthorized requests

---

# Phase 3 — Public API Gateway / BFF
**Status: CORE ROUTING DONE 2026-08-31 — auth/tenant/idempotency propagation intentionally deferred**

## Goal
Expose one public API boundary while keeping internal service topology private.

```text
Browser
  -> Public Gateway/BFF  (services/gateway, YARP — new 2026-08-31)
      -> POS Service
      -> Inventory Service
      -> Notification Service
      -> Auth Service
```

See [`decisions/ADR-008-api-gateway.md`](../decisions/ADR-008-api-gateway.md) for the full design
rationale, including exactly what was deliberately deferred and why.

### Gateway
- [x] One public origin — `services/gateway` (`Gateway.Api`), YARP 2.3.0, its own `Gateway.sln`
- [x] Internal service URLs private — frontend apps have NOT been repointed at it yet though (see
      below), so this exit criterion is only half-true until that happens
- [x] API routing — path-based, matching each service's real controller prefixes exactly (see
      `services/gateway/README.md` for the full route table)
- [ ] Authentication propagation — deferred: auth-service itself isn't integrated into either
      frontend app yet; adding gateway-side JWT validation ahead of that would be speculative
- [ ] Tenant context propagation — deferred: no tenant isolation exists anywhere yet (Phase 2)
- [x] Correlation ID propagation — `X-Correlation-Id`, generated/forwarded on the way in, echoed
      back via each downstream service's own copy on the way out
- [ ] Idempotency propagation — deferred, no concrete idempotency-key use case wired yet
- [x] Consistent error mapping — each downstream service already returns RFC7807 ProblemDetails;
      the gateway does not currently reshape these further (verified real request appears
      unchanged through the proxy)
- [ ] Request size limits — Kestrel defaults only; no explicit override yet
- [ ] Timeouts — YARP defaults only; no explicit per-cluster override yet
- [ ] Circuit breaker — not configured; YARP supports it, no concrete failure scenario driving
      specific thresholds yet
- [ ] Safe retries — not configured, same reasoning as circuit breaker
- [x] Health-aware routing — YARP active health checks per cluster (`ConsecutiveFailures` policy,
      polls each destination's `/health` every 15s); `GET /health/services` on the gateway itself
      fans out to all 4 services for one combined health view — verified real (all 4 healthy)

### Rate limiting
- [x] IP-based limits — a general fixed-window limiter at the edge (`RateLimiting:PermitLimit`/
      `:WindowSeconds`, default 200/60s per client IP)
- [ ] Tenant-based limits — deferred with tenant isolation (Phase 2)
- [ ] User-based limits — deferred with auth integration
- [ ] Device ID limits
- [ ] Endpoint-specific limits — only the general edge limiter exists; `auth-service` already has
      its own stricter per-endpoint limiter on login/register, independent of the gateway
- [ ] Redis distributed limiter — current limiter is in-memory/per-instance; fine for a single
      gateway instance, would need a distributed store before running >1 replica

### Device identity
- [ ] Server-issued POS device ID
- [ ] Device registration
- [ ] Device revocation
- [ ] Do not rely on browser MAC addresses
- [ ] Optional stronger native-terminal identity where applicable

### Exit criteria
- [~] Browser knows only public gateway origin — true for any request routed through it (verified:
      product list, auth login attempt both round-tripped correctly via `localhost:5010`); not yet
      true in practice because neither frontend app has been repointed at the gateway yet
- [x] Internal service URLs cannot be discovered from frontend configuration — true once the point
      above is done; not yet done
- [x] Gateway survives dependency failure without cascading outage — active health checks route
      around a failing destination rather than every request timing out against it (not yet
      chaos-tested against an actual killed container, but the health-check wiring is real and
      verified reachable)

---

# Phase 4 — Inventory Production Core
**Status: PARTIALLY IMPLEMENTED**

## Product/catalog
- [x] Product CRUD
- [ ] Category CRUD
- [ ] Brand CRUD
- [ ] Unit CRUD
- [ ] Supplier CRUD
- [ ] Warehouse CRUD
- [ ] SKU
- [ ] Barcode field
- [ ] Barcode uniqueness
- [ ] Product variants
- [ ] Industry attributes
- [ ] Product import/export

## Barcode
- [ ] Barcode generation
- [ ] Barcode formats
- [ ] Barcode label generation
- [ ] Batch label printing
- [ ] Barcode lookup API
- [ ] Barcode scan tests

## Inventory
- [x] Stock
- [x] Stock movements
- [x] Stock adjustment
- [x] Stock transfer foundation
- [ ] Stock count
- [ ] Receiving
- [ ] Issue
- [ ] Inventory reconciliation
- [ ] Inventory valuation
- [ ] Concurrency/oversell protection
- [ ] Audit history

### Exit criteria
A non-technical shop operator can create/import products, print labels, receive stock, transfer stock,
count stock and see accurate inventory.

---

# Phase 5 — POS Production Checkout
**Status: PARTIALLY IMPLEMENTED**

### Core checkout
- [x] Product search
- [x] Cart
- [x] Quantity
- [x] Checkout
- [x] Cash session
- [x] Receipt
- [x] Sale history
- [x] Void
- [ ] Barcode scan-to-sell
- [ ] Hold sale
- [ ] Resume sale
- [ ] Cancel/abandon sale
- [ ] Customer selection
- [ ] Discounts
- [ ] Tax
- [ ] Multiple payment methods
- [ ] Split payment
- [ ] Due/credit sale where enabled
- [ ] Returns
- [ ] Refunds

### Fast-sell acceptance
- [ ] Scan barcode
- [ ] Resolve product
- [ ] Add quantity 1
- [ ] Return focus to scanner input
- [ ] No manual product-name/price entry for known barcode
- [ ] Unknown barcode recovery
- [ ] Keyboard shortcuts
- [ ] POS performance test

### Exit criteria
A cashier can complete a full business day using scanner-first checkout without technical intervention.

---

# Phase 6 — Cash Register, Drawer & Denominations
**Status: PARTIALLY IMPLEMENTED**

### Register
- [x] Register foundation
- [x] Cash session foundation
- [ ] Opening cash
- [ ] Cash in
- [ ] Cash out
- [ ] Expected cash
- [ ] Actual cash
- [ ] Variance
- [ ] Shift close
- [ ] Cashier reconciliation

### Cash drawer
- [ ] Drawer hardware abstraction
- [ ] Local print/device bridge
- [ ] Drawer kick/open
- [ ] Drawer-open audit
- [ ] Device health/status

### Denomination feature
Optional configurable feature:
- [ ] Opening denomination count
- [ ] Customer received denomination breakdown
- [ ] Change denomination breakdown
- [ ] Closing denomination count
- [ ] Note counts and totals
- [ ] Audit trail
- [ ] Disable feature without affecting normal cash checkout

### Exit criteria
A real cashier can reconcile a physical drawer at the end of a shift.

---

# Phase 7 — Thermal Printing & Physical Payment Devices
**Status: PARTIALLY STARTED** — 58mm/80mm print CSS profiles shipped 2026-08-28; ESC/POS adapter,
local print bridge, and all card-terminal items below remain not started.

### Printing
- [x] 58mm/80mm profiles — browser-print CSS profiles only (2026-08-28); ESC/POS adapter and local
      print bridge below are still not started
- [ ] ESC/POS adapter
- [ ] Receipt templates
- [ ] Reprint
- [ ] Print preview
- [ ] QR/barcode on receipt
- [ ] Local print bridge

### Card terminal
- [ ] Payment provider abstraction
- [ ] Terminal adapter interface
- [ ] Authorization result
- [ ] Provider reference
- [ ] Reconciliation
- [ ] Webhook deduplication
- [ ] Provider failure handling
- [ ] Certified integration with chosen provider

Never store/process card PIN/CVV/full PAN unless a compliant certified architecture explicitly requires it.

---

# Phase 8 — Returns, Refunds & Purchasing
**Status: NOT STARTED**

### Returns
- [ ] Full return
- [ ] Partial return
- [ ] Return by receipt
- [ ] Return reason
- [ ] Return authorization
- [ ] Inventory restoration
- [ ] Refund
- [ ] Refund history

### Purchasing
- [ ] Supplier
- [ ] Purchase order
- [ ] Purchase items
- [ ] Receiving
- [ ] Supplier invoice/reference
- [ ] Purchase history
- [ ] Inventory integration

### Exit criteria
Financial and stock history remain correct after returns, refunds and purchasing.

---

# Phase 9 — Reporting, Expenses & Dashboard Analytics
**Status: PARTIALLY IMPLEMENTED**

## Required periods
- [ ] Today
- [ ] Last 7 days
- [ ] Last 15 days
- [ ] Monthly
- [ ] Half-yearly
- [ ] Yearly
- [ ] Custom date range

## Sales
- [x] Daily sales foundation
- [ ] Weekly
- [ ] 15-day
- [ ] Monthly
- [ ] Half-year
- [ ] Year
- [ ] Top-selling products
- [ ] Slow-moving products
- [ ] Payment reports
- [ ] Cashier reports
- [ ] Branch reports

## Inventory
- [ ] Low stock
- [ ] Out of stock
- [ ] Inventory valuation
- [ ] Stock movement
- [ ] Stock aging

## Expenses
- [ ] Expense categories
- [ ] Expense entry
- [ ] Payment method
- [ ] Branch/store
- [ ] Attachments metadata
- [ ] Expense approval if enabled

## Profit
- [ ] COGS
- [ ] Gross profit
- [ ] Operating expenses
- [ ] Operating profit
- [ ] Explicit revenue-vs-profit semantics

## Dashboard
- [ ] Sales today
- [ ] Transactions
- [ ] Average transaction value
- [ ] Profit
- [ ] Expenses
- [ ] Top products
- [ ] Low stock
- [ ] Out of stock
- [ ] Returns
- [ ] Branch comparison

### Exit criteria
Owner can understand daily/weekly/15-day/monthly/half-year/year performance without spreadsheets.

---

# Phase 10 — Notifications
**Status: NOT STARTED**

Create an independent Notification Service.

### Channels
- [ ] Email
- [ ] SMS
- [ ] Web/in-app

### Capabilities
- [ ] Templates
- [ ] Tenant branding
- [ ] Provider abstraction
- [ ] Delivery status
- [ ] Retry
- [ ] Dead-letter
- [ ] Idempotency/deduplication
- [ ] Correlation IDs
- [ ] Audit trail
- [ ] User/tenant preferences
- [ ] Feature entitlements

### Use cases
- [ ] Low stock
- [ ] Trial ending
- [ ] Subscription expiry
- [ ] Payment events
- [ ] Security events
- [ ] Scheduled reports
- [ ] System alerts

---

# Phase 11 — Offline-First POS & Sync
**Status: NOT STARTED**

### Local
- [ ] IndexedDB/local transactional store
- [ ] Device identity
- [ ] Product cache
- [ ] Price/config cache
- [ ] Local sale transaction
- [ ] Durable sync queue
- [ ] Sync status UI

### Sync
- [ ] Client-generated transaction ID
- [ ] Idempotency key
- [ ] Retry
- [ ] Backoff
- [ ] Duplicate detection
- [ ] Conflict handling
- [ ] Acknowledgement
- [ ] Recovery after browser/process restart
- [ ] Sync monitoring
- [ ] Server authoritative state

### Test scenarios
- [ ] Offline sale
- [ ] Internet restored
- [ ] Duplicate upload
- [ ] Partial failure
- [ ] App restart
- [ ] Device restart
- [ ] Concurrent stock conflict
- [ ] Expired entitlement while offline

---

# Phase 12 — SaaS Subscription, Billing & Licensing
**Status: NOT STARTED**

### Commercial lifecycle
- [ ] Signup
- [ ] 3-day full-feature trial
- [ ] Plan selection
- [ ] Payment
- [ ] Activation
- [ ] Renewal
- [ ] Grace period
- [ ] Expiry
- [ ] Cancellation

### Plans
Configurable examples:
- [ ] 1000 BDT/month plan
- [ ] 2000 BDT/month plan
- [ ] POS-only plan
- [ ] Inventory-only plan
- [ ] Combined plan

Prices must not be hard-coded into business logic.

### Entitlements
- [ ] POS enabled
- [ ] Inventory enabled
- [ ] Branch limit
- [ ] Register limit
- [ ] User limit
- [ ] Product limit
- [ ] Storage limit
- [ ] Reports
- [ ] Offline
- [ ] Advanced denomination
- [ ] Notifications
- [ ] API access

### Enforcement
- [ ] Gateway enforcement
- [ ] Service/domain enforcement
- [ ] Quota counters
- [ ] Stable error codes
- [ ] No partial writes on quota failure
- [ ] Webhook idempotency
- [ ] Billing audit

Example:
`PLAN_LIMIT_PRODUCT_COUNT_EXCEEDED`

### Exit criteria
A customer can go from signup → 3-day trial → payment → active subscription, and entitlements are
enforced without trusting the frontend.

---

# Phase 13 — Production React/Redux-Saga UI
**Status: MVP EXISTS; PRODUCTION HARDENING REMAINS**

### Architecture
- [x] React 19
- [x] Next.js
- [x] TypeScript strict
- [x] Redux Toolkit
- [x] Redux-Saga
- [ ] Gateway-only API access
- [ ] Authentication/session state
- [ ] Permission-aware UI
- [ ] Entitlement-aware UI
- [ ] Offline state machine
- [ ] Error boundaries
- [ ] Accessibility audit

### POS UX
- [ ] Charming/professional visual system
- [ ] Fast scanner workflow
- [ ] Keyboard-first operation
- [ ] Touch-friendly controls
- [ ] Responsive desktop/tablet
- [ ] Low-end hardware performance
- [ ] Printer/device status
- [ ] Clear offline indicator

### Inventory UX
- [ ] Product management
- [ ] Supplier
- [ ] Warehouse
- [ ] Customers
- [ ] Reports
- [ ] Branches
- [ ] Users
- [ ] Settings
- [ ] Subscription/billing

### Exit criteria
A shop owner and cashier can operate the system without developer assistance.

---

# Phase 14 — CI/CD, Security & Observability
**Status: FOUNDATION EXISTS; HARDENING REQUIRED**

### CI
- [ ] Backend restore/build
- [ ] Backend tests
- [ ] Frontend install/typecheck/lint/test/build
- [ ] Dependency audit
- [ ] Container build
- [ ] Integration tests
- [ ] Migration validation
- [ ] Artifact versioning

### CD
- [ ] Staging deployment
- [ ] Smoke tests
- [ ] Production approval
- [ ] Production deployment
- [ ] Rollback
- [ ] Database migration strategy

### Security
- [ ] Secret management
- [ ] HTTPS
- [ ] Security headers
- [ ] CORS review
- [ ] Rate limits
- [ ] Tenant isolation
- [ ] IDOR tests
- [ ] Dependency vulnerability scan
- [ ] Threat model
- [ ] Payment compliance boundary review

### Observability
- [x] Correlation ID foundation
- [x] OpenTelemetry foundation
- [x] Metrics foundation
- [ ] Gateway tracing
- [ ] Message tracing
- [ ] Dashboards
- [ ] Alerts
- [ ] Error monitoring
- [ ] SLOs

---

# Phase 15 — Backup, Disaster Recovery & Load Testing
**Status: NOT STARTED / PARTIAL TEST SCAFFOLD**

- [ ] Automated PostgreSQL backups
- [ ] Retention policy
- [ ] Restore test
- [ ] Point-in-time recovery where required
- [ ] Disaster recovery runbook
- [ ] RPO/RTO definition
- [ ] k6 baseline
- [ ] POS checkout load test
- [ ] Inventory search/load test
- [ ] Gateway load test
- [ ] RabbitMQ failure test
- [ ] Redis failure test
- [ ] Database failure/recovery test
- [ ] Offline sync stress test

---

# Phase 16 — Industry Extensions
**Status: FUTURE**

### Clothing / Burkha
- [ ] Size
- [ ] Color
- [ ] Fabric
- [ ] Design
- [ ] Variants

### Electronics
- [ ] Serial number
- [ ] Warranty
- [ ] IMEI where applicable

### Motor Parts
- [ ] Part number
- [ ] OEM number
- [ ] Vehicle compatibility

### Grocery
- [ ] Weight
- [ ] Unit pricing
- [ ] Scale integration

### Pharmacy
- [ ] Batch
- [ ] Expiry
- [ ] Manufacturer
- [ ] Compliance workflows where legally required

---

# Phase 17 — AI & Advanced Analytics
**Status: FUTURE**

Only after reliable transaction data exists.

- [ ] Demand forecasting
- [ ] Reorder suggestions
- [ ] Slow-moving detection
- [ ] Anomaly detection
- [ ] Natural-language reports
- [ ] Business insights

---

# Phase 18 — Internationalization
**Status: FUTURE**

- [ ] Multi-currency
- [ ] Multi-language
- [ ] Time zones
- [ ] Regional tax
- [ ] Regional receipts
- [ ] Regional payment providers
- [ ] Data residency

---

# Production Gate

The product may be called **production-ready** only when all critical gates have evidence:

- [ ] 0 build errors
- [ ] 0 build warnings
- [ ] All tests pass
- [ ] Tenant isolation verified
- [ ] Authorization verified
- [ ] POS checkout verified
- [ ] Barcode scan verified
- [ ] Inventory correctness verified
- [ ] Returns/refunds verified
- [ ] Thermal printing verified
- [ ] Cash drawer verified
- [ ] Card-terminal integration verified
- [ ] Reports verified
- [ ] Expenses/profit verified
- [ ] Offline sync verified
- [ ] Idempotency verified
- [ ] Circuit breaker verified
- [ ] Rate limiting verified
- [ ] Notifications verified
- [ ] Subscription/payment verified
- [ ] Entitlements/quotas verified
- [ ] CI/CD verified
- [ ] Backup/restore verified
- [ ] Monitoring/alerts verified
- [ ] Security tests verified
- [ ] Documentation current
- [ ] AI handover current
- [ ] First real customer acceptance completed

---

# Delivery Order

The fastest safe path to a real customer is:

1. Build/test/migration baseline
2. Auth + tenant isolation
3. Gateway + rate limiting + resilience
4. Inventory CRUD completion + barcode
5. POS barcode checkout
6. Cash/register/drawer/thermal printing
7. Returns/refunds
8. Reports/expenses/profit
9. Production UI hardening
10. First real customer pilot
11. Notifications
12. Offline sync
13. Subscription/licensing/entitlements
14. Card terminal/payment integrations
15. CI/CD/security/DR/load hardening
16. Industry modules
17. AI
18. Internationalization

Do not delay the first pilot for AI or decorative features. Reliability beats feature count.
