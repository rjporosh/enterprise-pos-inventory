# Enterprise POS & Inventory SaaS
# Product & Engineering Roadmap

**Version:** 3.0
**Status:** Active — Production Hardening Plan
**Last Updated:** August 26, 2026

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
- [ ] Third build with 0 errors / 0 warnings after the last backend fixes
- [ ] Real backend + frontend end-to-end runtime
- [ ] Production authentication/authorization
- [ ] Production tenant isolation
- [ ] Gateway/BFF
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
- [ ] Notification service (email/SMS/web)
- [ ] Production CI/CD
- [ ] Backup/restore verification
- [ ] Production security/load/chaos testing

---

# Phase 1 — Build & Contract Stabilization
**Status: IN PROGRESS / NEXT**

## Goal
Create a clean baseline before adding large commercial features.

### Backend
- [ ] `dotnet restore EnterprisePOS.sln`
- [ ] `dotnet build EnterprisePOS.sln`
- [ ] Achieve 0 errors
- [ ] Achieve 0 warnings
- [ ] `dotnet test EnterprisePOS.sln`
- [ ] Regenerate hand-authored EF migrations with real `dotnet ef`
- [ ] Apply migrations to clean databases
- [ ] Verify database schema
- [ ] Resolve duplicated `BaseEntity` design intentionally
- [ ] Review API response/error contracts
- [ ] Review versioning
- [ ] Review pagination/filter/sort
- [ ] Review correlation ID propagation
- [ ] Implement idempotency infrastructure for financial writes
- [ ] Add ADR for gateway/BFF
- [ ] Add ADR for resilience policies
- [ ] Add ADR for subscription/entitlements
- [ ] Add ADR for offline synchronization

### Exit criteria
- [ ] Clean build: 0/0
- [ ] Full tests green
- [ ] Clean database migration succeeds
- [ ] No known contract mismatch between frontend and backend

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
**Status: NOT STARTED / HIGH PRIORITY**

## Goal
Expose one public API boundary while keeping internal service topology private.

```text
Browser
  -> Public Gateway/BFF
      -> POS Service
      -> Inventory Service
      -> Notification Service
      -> Billing/Subscription Service
```

### Gateway
- [ ] One public origin
- [ ] Internal service URLs private
- [ ] API routing
- [ ] Authentication propagation
- [ ] Tenant context propagation
- [ ] Correlation ID propagation
- [ ] Idempotency propagation
- [ ] Consistent error mapping
- [ ] Request size limits
- [ ] Timeouts
- [ ] Circuit breaker
- [ ] Safe retries
- [ ] Health-aware routing

### Rate limiting
- [ ] IP-based limits
- [ ] Tenant-based limits
- [ ] User-based limits
- [ ] Device ID limits
- [ ] Endpoint-specific limits
- [ ] Redis distributed limiter

### Device identity
- [ ] Server-issued POS device ID
- [ ] Device registration
- [ ] Device revocation
- [ ] Do not rely on browser MAC addresses
- [ ] Optional stronger native-terminal identity where applicable

### Exit criteria
- [ ] Browser knows only public gateway origin
- [ ] Internal service URLs cannot be discovered from frontend configuration
- [ ] Gateway survives dependency failure without cascading outage

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
