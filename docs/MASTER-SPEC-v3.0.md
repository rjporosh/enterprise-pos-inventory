# Enterprise POS & Inventory SaaS
## Master Product & Engineering Specification

**Version:** 3.0.0
**Status:** Active
**Product Type:** Commercial Multi-Tenant SaaS
**Primary Market:** Small and medium retail businesses
**Architecture:** API-first, multi-tenant, cloud-ready
**Last Updated:** August 26, 2026

---

# 1. Product Vision

Build a professional commercial Point of Sale and Inventory Management SaaS platform that can be used by real-world retail businesses including:

- Clothing stores
- Burkha stores
- Motor-parts stores
- Electronics stores
- Grocery/mudi shops
- Pharmacies
- General retail stores
- Small specialty stores

The platform must allow a business to start with one store and later expand into multiple branches.

The system must prioritize:

- Fast checkout
- Reliable inventory
- Barcode-based selling
- Accurate financial records
- Easy reporting
- Multi-user operation
- Multi-branch operation
- Offline-capable POS
- Subscription-based SaaS
- Strong security
- Auditability
- Maintainability
- Scalability

The initial production customer will be used as a real-world validation environment.

---

# 2. Product Strategy

The platform shall use a generic retail core instead of creating separate applications for every industry.

Core functionality shall be reusable across industries.

Industry-specific capabilities shall be implemented through extensible product attributes/modules.

Examples:

Clothing:

- Size
- Color
- Fabric
- Design

Motor Parts:

- Part Number
- OEM Number
- Vehicle Make
- Vehicle Model
- Compatibility

Pharmacy:

- Batch
- Expiry
- Manufacturer
- Generic Name

Grocery:

- Unit
- Weight
- Quantity

The core architecture must not be tightly coupled to a single industry.

---

# 3. SaaS Architecture

The system shall support:

- Multiple tenants
- Tenant isolation
- Multiple stores per tenant
- Multiple warehouses
- Multiple registers
- Multiple users
- Roles
- Permissions
- Subscription plans
- Licensing
- Entitlements
- Trial periods
- Billing lifecycle
- Audit logs

A tenant represents an independent business/customer.

All tenant-owned data must be isolated.

Tenant A must never be able to access Tenant B data.

Authorization must be enforced server-side.

---

# 4. Organization Model

The logical hierarchy shall be:

Tenant
|
+-- Users
+-- Roles
+-- Permissions
|
+-- Stores
|   |
|   +-- Registers
|   +-- Cash Sessions
|   +-- Staff
|
+-- Warehouses
|
+-- Products
+-- Categories
+-- Brands
+-- Suppliers
+-- Customers
+-- Sales
+-- Returns
+-- Purchases
+-- Expenses
+-- Inventory
+-- Reports
+-- Settings

A tenant may have multiple branches.

Branch-level access must be permission controlled.

---

# 5. Authentication & Authorization

The platform shall support:

- Secure authentication
- JWT/access-token based API authentication where appropriate
- Refresh-token/session management
- Role-based authorization
- Permission-based authorization
- Tenant context
- Branch restrictions
- Register restrictions
- Audit logging
- Account lockout/protection
- Secure password policies
- Session revocation

Frontend authorization checks are only UX controls.

Backend authorization is authoritative.

---

# 6. Roles

Initial roles may include:

- Platform Administrator
- Tenant Owner
- Tenant Administrator
- Manager
- Cashier
- Inventory Operator
- Accountant

The permission system must be extensible.

Example permissions:

- product.read
- product.create
- product.update
- product.delete
- inventory.adjust
- inventory.transfer
- sale.create
- sale.void
- sale.return
- report.view
- user.manage
- branch.manage
- settings.manage

---

# 7. Product Management

Products shall support:

- Product name
- SKU
- Barcode
- Product code
- Category
- Brand
- Unit
- Purchase price
- Selling price
- Discount
- Tax
- Minimum stock
- Maximum stock
- Supplier
- Product image
- Active/inactive state

The architecture must support future custom attributes.

Barcode requirements:

- Generate barcode
- Assign barcode
- Print barcode labels
- Search by barcode
- Scan barcode
- Validate barcode uniqueness
- Support common retail barcode formats
- Support manually entered barcodes

Barcode scanning must be optimized for fast POS operation.

---

# 8. POS

POS must support:

- Product search
- Barcode scanning
- Keyboard-friendly workflow
- Fast product lookup
- Cart
- Quantity changes
- Discounts
- Taxes
- Customer selection
- Hold sale
- Resume sale
- Remove item
- Cancel sale
- Complete sale
- Multiple payment methods
- Cash payment
- Card payment integration architecture
- Mobile payment/manual payment methods
- Change calculation
- Receipt generation
- Receipt reprint

POS must prioritize speed and reliability.

---

# 9. Sales

Every completed sale shall record:

- Tenant
- Store
- Register
- Cashier
- Customer where applicable
- Sale number
- Items
- Quantities
- Unit prices
- Discounts
- Taxes
- Total
- Payment methods
- Payment amounts
- Timestamp
- Device information where required
- Correlation ID
- Audit information

Financial transactions must be immutable after completion except through controlled correction workflows.

---

# 10. Product Returns & Refunds

Returns are mandatory production functionality.

The system shall support:

- Full sale return
- Partial sale return
- Return by receipt
- Return by sale number
- Return by product
- Return quantity validation
- Return reason
- Refund amount
- Refund method
- Inventory restoration
- Return history
- Manager approval where configured
- Return audit trail

A returned product must not simply modify the original sale.

Instead, create a separate return/refund transaction linked to the original sale.

Example:

Sale:

SALE-1001

Return:

RETURN-2001

Linked to:

SALE-1001

The system must prevent returning more quantity than was originally sold minus already returned quantity.

---

# 11. Inventory

Inventory shall support:

- Stock by warehouse
- Stock by branch
- Stock movements
- Stock ledger
- Stock adjustment
- Stock transfer
- Stock count
- Stock receiving
- Stock issue
- Low stock alerts
- Out-of-stock alerts
- Inventory history
- Inventory reconciliation
- Inventory valuation where supported

Inventory changes must be auditable.

POS sales must update inventory safely.

Concurrency must prevent overselling where inventory restrictions apply.

---

# 12. Purchasing

Support:

- Suppliers
- Purchase orders
- Purchase receiving
- Purchase items
- Purchase price
- Supplier invoice/reference
- Stock receiving
- Purchase history

Purchasing shall integrate with inventory.

---

# 13. Customers

Support:

- Customer profile
- Phone
- Email where applicable
- Address
- Purchase history
- Returns
- Outstanding balances where credit sales are supported

Customer data must be tenant-isolated.

---

# 14. Cash Register

Support:

- Registers
- Opening cash
- Cash session
- Cashier assignment
- Cash-in
- Cash-out
- Cash sales
- Returns
- Expected cash
- Actual cash
- Cash variance
- Register closing
- Reconciliation

Every cash operation must be auditable.

---

# 15. Cash Denomination Tracking

The architecture shall support denomination-aware cash management.

Initial configurable denominations may include:

- 1000
- 500
- 200
- 100
- 50
- 20
- 10
- 5

The system shall support:

- Opening denomination count
- Cash received denomination breakdown
- Cash payout denomination breakdown
- Closing denomination count
- Expected cash
- Actual cash
- Variance
- Cashier reconciliation

Example:

500 × 2
100 × 3
50 × 1

The system must preserve denomination records as transaction/audit information.

Important:

Denomination tracking must not claim to automatically determine counterfeit currency unless actual hardware/currency-validation infrastructure is integrated.

---

# 16. Receipt Printing

The platform shall support:

- Thermal receipt printing
- Standard receipt printing
- Reprint
- Print preview
- Configurable receipt template
- Store information
- Tax information
- Payment information
- Return information
- Barcode/QR where applicable

Printer integration must be abstracted so different printer environments can be supported.

---

# 17. Reporting

Reporting is a core feature.

The platform shall provide:

- Today sales
- Yesterday sales
- Daily sales
- Weekly sales
- Half-monthly sales
- Monthly sales
- Half-yearly sales
- Yearly sales
- Custom date-range reports
- Product sales
- Top-selling products
- Slow-moving products
- Stock reports
- Low-stock reports
- Out-of-stock reports
- Purchase reports
- Return reports
- Refund reports
- Payment reports
- Cashier reports
- Register reports
- Branch reports
- Profit reports
- Expense reports
- Tax reports

Reports must support filtering by:

- Date
- Store
- Branch
- Cashier
- Product
- Category
- Payment method

Reporting queries must be optimized for production workloads.

---

# 18. Dashboard

Dashboard should provide:

- Today's sales
- Today's transactions
- Average transaction value
- Top products
- Low stock
- Out of stock
- Returns
- Refunds
- Expenses
- Gross profit where data permits
- Cash status
- Branch comparison

Dashboard data should use optimized queries/read models where necessary.

---

# 19. Offline POS

Offline POS is a required product capability.

The POS application must be capable of continuing essential sales operations during temporary internet outages.

Offline architecture shall support:

- Local POS database/storage
- Cached products
- Cached prices
- Cached configuration
- Device identity
- Local transaction IDs
- Offline sales queue
- Synchronization
- Retry
- Duplicate detection
- Idempotency
- Conflict handling
- Synchronization acknowledgement

Financial transactions must never be duplicated during synchronization.

The backend shall be authoritative after successful synchronization.

---

# 20. Synchronization

Every offline transaction shall have a stable client-generated identity.

Example:

deviceId + localTransactionId

The backend must support idempotent synchronization.

Repeated submission of the same transaction must not create duplicate sales.

Synchronization must be observable and auditable.

---

# 21. SaaS Subscription

The system shall support:

- Plans
- Trials
- Subscriptions
- Subscription periods
- Subscription status
- Payment providers
- Payment webhooks
- Entitlements
- License state
- Usage limits

Initial commercial model:

3-day free trial.

After successful payment:

1-month subscription period.

The trial duration must be configurable.

The backend must be authoritative for subscription status.

---

# 22. Licensing

Licensing shall support:

- Tenant license
- Subscription status
- Trial status
- Expiration
- Grace period if configured
- Feature entitlements
- User limits
- Branch limits
- Register limits

Frontend must never be trusted to determine whether a tenant is licensed.

---

# 23. Payment Architecture

The platform shall abstract payment providers.

Do not implement card processing internally.

The architecture must support future integrations with:

- Payment gateways
- Acquirers
- Card terminals
- Mobile payment providers

Card PIN and sensitive card authentication data must never be handled by the application unless explicitly required by a compliant certified integration.

Terminal architecture:

POS
→ Payment Provider/Terminal Integration
→ Payment Processor
→ Bank/Card Network

The POS should receive a payment result/reference rather than sensitive card credentials.

---

# 24. Multi-Currency & Localization

Architecture shall support:

- Currency
- Locale
- Time zone
- Tax configuration
- Number formatting
- Date formatting

Initial deployment may use BDT and Asia/Dhaka.

The domain model must not hard-code BDT assumptions.

---

# 25. Tax

Tax configuration shall support:

- Tax rates
- Inclusive/exclusive tax
- Product-level tax
- Store-level tax
- Tax reporting

Tax rules must be configurable.

---

# 26. Audit

Audit logs shall record important actions:

- Login
- Logout
- Product creation
- Product modification
- Inventory adjustment
- Stock transfer
- Sale
- Sale void
- Return
- Refund
- Cash movement
- Register opening
- Register closing
- User changes
- Permission changes
- Subscription changes

Audit records should include:

- User
- Tenant
- Timestamp
- Action
- Entity
- Entity ID
- Correlation ID
- Relevant metadata

---

# 27. API Standards

APIs must have:

- Consistent naming
- API versioning strategy
- Validation
- Pagination
- Filtering
- Sorting
- Correlation IDs
- Idempotency where required
- Consistent success response structure
- RFC-compatible error responses
- OpenAPI documentation

Financial operations must support idempotency.

---

# 28. Error Handling

Errors must never expose:

- Secrets
- Passwords
- Tokens
- Database credentials
- Internal stack traces in production

Errors must have stable machine-readable codes.

Example:

PRODUCT_NOT_FOUND

SALE_ALREADY_COMPLETED

INSUFFICIENT_STOCK

RETURN_QUANTITY_EXCEEDED

SUBSCRIPTION_EXPIRED

TENANT_ACCESS_DENIED

---

# 29. Security

Required:

- Tenant isolation
- Authentication
- Authorization
- Input validation
- Rate limiting
- Secure secrets
- HTTPS
- Secure headers
- Audit logging
- Protection against injection
- Protection against broken authorization
- Protection against replay/duplicate financial transactions

Security must be server-enforced.

---

# 30. Rate Limiting

Rate limiting shall exist at appropriate layers.

Different limits may apply to:

- Authentication
- Password reset
- Public APIs
- Product search
- Barcode lookup
- Financial operations

Distributed rate limiting should be supported when multiple API instances are deployed.

Redis may be used for distributed coordination.

---

# 31. Scalability

The architecture shall support horizontal API scaling.

Target architecture:

CDN/WAF
→ Load Balancer
→ API instances
→ Redis
→ Message broker
→ PostgreSQL

Infrastructure should scale when customer load requires it.

Do not introduce unnecessary infrastructure before it is required.

---

# 32. Observability

Production system must support:

- Structured logging
- Correlation IDs
- Metrics
- Distributed tracing
- Health checks
- Readiness checks
- Liveness checks
- Error monitoring
- Performance monitoring

---

# 33. Backup & Disaster Recovery

Production must have:

- Automated database backups
- Backup retention
- Restore testing
- Migration strategy
- Disaster recovery documentation

A backup that has never been restored successfully must not be considered verified.

---

# 34. Testing

Required:

- Unit tests
- Integration tests
- API tests
- Database tests
- Authorization tests
- Tenant isolation tests
- Financial transaction tests
- Inventory concurrency tests
- Return/refund tests
- Idempotency tests
- Offline synchronization tests
- Load tests
- Regression tests

Critical financial workflows must have automated tests.

---

# 35. Frontend

Required frontend architecture:

- React 19
- Next.js
- TypeScript
- Redux
- Redux-Saga
- Production-grade state management
- Typed API clients
- Form validation
- Error boundaries
- Permission-aware UI
- Responsive design
- Accessibility
- POS keyboard optimization
- Barcode scanner support
- Thermal printing integration
- Offline capability architecture

Frontend projects:

frontend/
├── pos/
└── inventory/

The POS application must optimize speed of sale.

The Inventory application must optimize management workflows.

---

# 36. Documentation

Every major subsystem must contain:

- README
- Architecture overview
- ADRs
- Folder structure
- C4 diagrams
- Sequence diagrams where useful
- Developer guide
- API documentation
- Testing guide
- Deployment guide

Every new CRUD domain must document how another developer can implement the next CRUD safely.

---

# 37. AI Development Rules

AI agents must:

1. Read MASTER-SPEC.md.
2. Read ROADMAP.md.
3. Read relevant ADRs.
4. Read handover documentation.
5. Inspect existing implementation.
6. Never assume missing functionality exists.
7. Never rewrite working systems unnecessarily.
8. Preserve architecture.
9. Add tests with features.
10. Update documentation.
11. Update roadmap status.
12. Report blockers.
13. Never claim production readiness without evidence.

---

# 38. Definition of Production Ready

The system is production-ready only when:

- Build passes
- Tests pass
- Critical workflows pass
- Tenant isolation is verified
- Authentication is secure
- Authorization is verified
- POS checkout works
- Barcode workflow works
- Inventory updates correctly
- Returns work correctly
- Refunds work correctly
- Receipts work
- Reports work
- Cash reconciliation works
- Audit logs work
- Offline synchronization is verified
- Duplicate transaction prevention works
- Database backup/restore is tested
- Monitoring works
- Rate limiting works
- Deployment is reproducible
- Documentation is complete

---

# 39. Definition of Done

A feature is not complete until:

- Code implemented
- Tests implemented
- Tests passing
- API documented
- Frontend integrated where applicable
- Authorization verified
- Tenant isolation verified
- Error handling implemented
- Logging implemented
- Documentation updated
- ADR added when architectural decisions change
- ROADMAP updated
- Handover updated

---

# 40. Product Release Strategy

Release order:

1. Retail Core
2. First production store
3. Multi-branch
4. Offline POS
5. Subscription/Licensing
6. Payment integrations
7. Advanced analytics
8. AI capabilities
9. Internationalization
10. Additional industry modules

The platform must prioritize real customer reliability over feature count.

---

# 41. Product Packaging & Independent Services

POS and Inventory are independent commercial capabilities.

A tenant/customer may subscribe to:

- Inventory only
- POS only
- POS + Inventory

Each service must remain independently deployable and independently usable.

The service boundary is also a data boundary:

- POS Service owns POS data and its own PostgreSQL database.
- Inventory Service owns Inventory data and its own PostgreSQL database.
- One service must never directly query another service's database.
- Cross-service data movement must use versioned APIs or asynchronous integration events.
- Shared kernel/contracts must not become a shared persistence model.

The initial architecture therefore uses:

```text
Tenant
 ├── POS entitlement ──> POS Service ──> pos_db
 └── Inventory entitlement ──> Inventory Service ──> inventory_db
```

A customer who purchases only Inventory must not be forced to activate POS.
A customer who purchases only POS must not be forced to activate Inventory.

Feature/plan entitlements must be enforced server-side at API boundaries.

---

# 42. Public API Gateway / BFF Boundary

The browser/client must not know internal microservice hostnames, ports, credentials, or topology.

Required boundary:

```text
Browser
   |
   v
Public HTTPS API Gateway / BFF
   |
   +--> POS Service (private network)
   |
   +--> Inventory Service (private network)
   |
   +--> Notification Service (private network)
   |
   +--> Subscription/Billing Service (private network)
```

The frontend must call one public origin, for example `/api/v1/...`.

Internal service URLs must be configuration-only and must never be exposed as `NEXT_PUBLIC_*`
or equivalent browser-visible configuration.

The gateway must provide:

- Authentication/token validation where appropriate
- Tenant context propagation
- Correlation ID propagation
- Idempotency key propagation
- Request routing
- API version enforcement
- Rate limiting
- Request size limits
- Security headers
- Timeout policy
- Retry policy only for safe/idempotent operations
- Circuit breaker
- Health-aware routing
- Consistent error mapping
- Audit/observability metadata

The gateway must not become a business-logic dumping ground.

---

# 43. Rate Limiting & Device Identity

Rate limiting must work for authenticated tenants/users and for anonymous/public endpoints.

Required dimensions may include:

- Tenant ID
- User ID
- API key/client ID where applicable
- Device ID
- IP address
- Endpoint/operation

**Important browser constraint:** a web browser cannot reliably expose the physical MAC address to a remote server.
Therefore the production design must not depend on a browser MAC address.

Instead use a server-issued/registered device identity combined with:

```text
tenantId + userId + deviceId + IP + route
```

where appropriate.

A native POS terminal/device integration may have a stronger hardware identity, but the MAC address must
not be treated as a secure authentication factor.

Redis-backed distributed rate limiting should be used when multiple gateway/API instances are deployed.

---

# 44. Circuit Breaker, Retry & Resilience

Service-to-service calls must use explicit resilience policies.

Required:

- Timeout
- Circuit breaker
- Bounded retry
- Exponential backoff
- Jitter
- Bulkhead/concurrency protection where appropriate

Rules:

- Never blindly retry financial writes.
- Idempotent/read operations may be retried according to policy.
- Financial writes require idempotency keys and must be safe under repeated delivery.
- Circuit breakers must fail fast when a dependency is unhealthy.
- RabbitMQ consumers must reconnect with backoff and dead-letter poison messages.
- External payment providers must use provider-safe idempotency and webhook deduplication.

Polly or the .NET resilience pipeline may be used.

---

# 45. Correlation IDs & Idempotency

Every externally initiated request should carry or receive:

- `X-Correlation-ID`

Every financial or otherwise non-idempotent operation that can be retried must support:

- `Idempotency-Key`

The server must persist idempotency state for the required retention period and bind it to the
tenant/user/operation scope.

The same key with the same semantic request must return the original result rather than creating a
second sale/payment/stock mutation.

Reusing an idempotency key with a materially different request payload must be rejected.

Correlation IDs must propagate:

```text
Browser
 -> Gateway
 -> Service
 -> Message Broker
 -> Consumer
```

Logs, traces, audit records, integration events and error responses should retain correlation context.

---

# 46. Barcode Generation, Scanning & Label Printing

Barcode capability is a first-class retail feature.

Required:

- Barcode generation
- Barcode assignment
- Barcode uniqueness validation
- Barcode lookup
- Keyboard/scanner wedge support
- Camera scanning where supported
- Enter-to-add fast checkout
- Barcode label generation
- Batch label printing
- Product/SKU association
- Common retail barcode formats
- Configurable barcode templates

Fast-sell flow:

```text
Scan
 -> lookup
 -> add product
 -> default quantity 1
 -> focus scanner/input again
```

A cashier should not need to manually enter product name, SKU or price when the barcode resolves.

If the barcode is unknown, the UI must provide a fast, permission-aware recovery flow.

---

# 47. Thermal Printing & Cash Drawer

POS hardware integrations must be abstracted behind interfaces.

Required hardware integration capabilities:

- ESC/POS thermal printer support
- Receipt printing
- Reprint
- Print preview/fallback
- Cash drawer kick/open
- Drawer-open audit event
- Configurable printer per register
- Printer connectivity/status where the platform can reliably detect it

The architecture should support a local print bridge/agent when browser security prevents direct hardware
access.

The cloud API must not assume a browser can directly open a USB/serial cash drawer.

---

# 48. Card Terminal / Physical POS Device

The application must support physical card acceptance through certified payment-terminal/provider
integrations.

Required abstraction:

```text
POS
 -> Payment Adapter
 -> Payment Gateway / Terminal SDK
 -> Certified Processor
 -> Bank/Card Network
```

The application must receive:

- Authorization result
- Provider transaction/reference ID
- Amount
- Currency
- Status
- Reconciliation metadata

The application must not store or process:

- Card PIN
- CVV
- Full PAN

unless an explicitly certified integration requires it and the applicable compliance controls are
implemented.

Payment providers must be pluggable so one provider does not contaminate the domain model.

---

# 49. Expenses & Financial Reporting

Expenses are a first-class domain concept.

Support:

- Expense category
- Expense amount
- Payment method
- Store/branch
- Cash register where applicable
- Supplier/payee
- Reference
- Notes
- Attachment metadata
- User
- Timestamp
- Approval state where required

Reports must include:

- Today
- Last 7 days
- Last 15 days
- Month
- Half-year
- Year
- Custom range

"Weekly" means a seven-day period by the configured business timezone.
"15-day" is an explicit rolling/period report, not a mislabeled half-month report.

Profit must be clearly defined, for example:

```text
Gross Profit = Net Sales - Cost of Goods Sold
Operating Profit = Gross Profit - Operating Expenses
```

The UI must never label revenue as profit.

---

# 50. Cash Denomination Audit

Denomination tracking is optional per tenant/register and must be configurable.

When enabled, the system may record:

Customer payment:

```text
1000 x 2
500 x 1
100 x 2
```

Total received must equal the declared payment amount.

Change returned:

```text
500 x 1
100 x 2
```

The transaction must preserve:

- Denomination value
- Quantity
- Direction (received/returned/opening/closing)
- Total
- Cashier
- Register
- Sale/session reference
- Timestamp
- Correlation ID

The system should calculate expected change, but denomination selection may remain cashier-controlled.

This feature is audit/accounting support; it is not counterfeit detection.

If disabled, ordinary cash amount/change calculation must continue normally.

---

# 51. Subscription, Licensing & Entitlement Enforcement

Commercial lifecycle:

```text
Signup
 -> 3-day full-feature trial
 -> Choose plan
 -> Payment
 -> Active subscription
 -> Renewal
 -> Grace/expired state
```

Initial trial:

- 3 calendar days
- Full product features during trial, subject to configurable abuse controls

Plans must be configurable.

Example plan dimensions:

- POS enabled
- Inventory enabled
- Number of branches
- Number of registers
- Number of users
- Product count
- Storage
- Reports
- Offline capability
- Advanced cash denomination
- Notifications
- API access
- Advanced analytics

Example commercial constraint:

```text
Plan A:
1000 BDT/month
maximum 100 product types

Plan B:
2000 BDT/month
maximum 500 product types
```

These are examples, not hard-coded business values.

The entitlement engine must support:

- Feature flags
- Numeric quotas
- Boolean capabilities
- Expiration
- Grace periods
- Add-ons
- Usage metering

Enforcement must exist server-side at the service/gateway/domain boundary.

Frontend checks are only UX.

When a quota is exceeded, return a stable machine-readable error such as:

`PLAN_LIMIT_PRODUCT_COUNT_EXCEEDED`

Do not silently corrupt, delete, or partially create data.

Subscription/payment webhooks must be idempotent.

---

# 52. Notification Platform

Notifications must be an independent capability.

Channels:

- Email
- SMS
- In-app/web notification

Use cases:

- Low stock
- Sale events
- Payment events
- Subscription expiry
- Trial ending
- Password/security events
- Reports
- System alerts

The notification service must support:

- Template management
- Tenant branding
- Provider abstraction
- Delivery status
- Retry
- Exponential backoff
- Dead-letter handling
- Provider failure isolation
- Idempotency/deduplication
- Correlation IDs
- Audit trail

A tenant must be able to enable/disable notification categories/channels according to plan and settings.

The system must not claim SMS/email delivery success merely because a message was queued.

---

# 53. Offline-First POS Synchronization

Offline POS is not merely local caching.

The POS device must maintain a local transactional store, preferably IndexedDB for the web application or
a stronger local database in a native/desktop POS shell.

Local records require:

- Tenant ID
- Device ID
- Local transaction ID
- Created timestamp
- Schema/version
- Sync state
- Retry count
- Last error
- Server transaction ID after acknowledgement

Synchronization:

```text
Offline transaction
 -> durable local queue
 -> connectivity restored
 -> upload
 -> backend idempotency check
 -> authoritative commit
 -> acknowledgement
 -> local record marked synced
```

Rules:

- No duplicate sale on repeated upload.
- Failed sync must remain recoverable.
- Conflicts must be explicit.
- The server is authoritative after successful synchronization.
- Product/price cache must have freshness/expiry policy.
- Offline selling must obey the capabilities/limits explicitly cached for the device.
- High-risk operations may be disabled offline if they cannot be safely reconciled.

Offline mode must be tested with network loss, process restart, duplicate submission, partial failure and
recovery.

---

# 54. CI/CD & Release Engineering

Production delivery must include:

- Pull request validation
- Backend restore/build/test
- Frontend install/typecheck/lint/test/build
- Security/dependency checks
- Container image build
- Migration validation
- Integration tests
- Artifact/version generation
- Deployment
- Smoke tests
- Rollback strategy

Every phase/feature/fix must have a professional Conventional Commit-style message, for example:

- `feat(pos): add barcode checkout`
- `fix(inventory): prevent negative stock`
- `feat(billing): enforce product quota`
- `test(saas): verify tenant isolation`
- `docs(architecture): document gateway boundary`

Never rewrite, squash, reset or force-push existing Git history unless the repository owner explicitly
requests it.

---

# 55. Documentation & AI Handover Contract

Every service and frontend application must contain:

- `README.md`
- `architecture` documentation
- `programmer/developer guide`
- `testing guide`
- `deployment guide`
- `API documentation`
- `ADR` references
- `release-notes.md`
- `ai-handover.md`

The root repository must contain an authoritative `AI-HANDOVER.md`.

Before an AI agent stops because of token/context limits, it MUST update handover documentation with:

1. What was completed.
2. What was changed.
3. What was fixed.
4. Root cause of each important bug.
5. Verification commands and results.
6. Known risks/regressions.
7. Exact remaining work.
8. Exact next command/prompt for the next agent.

The next agent must be able to resume without guessing.

No feature is considered complete if its documentation/handover/roadmap status is stale.

---

# 56. Frontend Production Architecture

The current frontend target is:

- React 19
- Next.js
- TypeScript strict mode
- Redux Toolkit
- Redux-Saga
- Feature-based architecture
- Typed API clients
- Responsive design
- Accessibility
- Keyboard-optimized POS
- Error boundaries
- Offline storage/sync architecture
- Permission-aware UI
- Entitlement-aware UI

POS and Inventory applications remain independently deployable.

Frontend must consume the public gateway/BFF origin rather than direct internal service URLs.

The UI must be:

- Charming but not decorative at the expense of speed
- Mobile/tablet/desktop responsive
- Keyboard friendly
- Scanner friendly
- Accessible
- Fast on low-end shop hardware
- Clear enough for non-technical shop owners

---

# 57. Production Acceptance Matrix

Before declaring production readiness, verify all of the following with evidence:

### Architecture
- [ ] POS and Inventory independently deployable
- [ ] Independent service databases
- [ ] No cross-service DB access
- [ ] Public gateway hides internal service URLs
- [ ] Circuit breaker and timeout policies verified
- [ ] Retry policy is safe for financial operations

### SaaS
- [ ] Tenant isolation tests pass
- [ ] Branch isolation tests pass
- [ ] POS-only entitlement works
- [ ] Inventory-only entitlement works
- [ ] Combined entitlement works
- [ ] 3-day trial works
- [ ] Subscription activation works
- [ ] Expiry/grace works
- [ ] Product/user/branch/register quotas work

### POS
- [ ] Barcode generation
- [ ] Barcode scan-to-sell
- [ ] Cash checkout
- [ ] Card terminal integration path
- [ ] Thermal printing
- [ ] Cash drawer
- [ ] Returns/refunds
- [ ] Denomination tracking optional path
- [ ] Offline sale
- [ ] Offline sync
- [ ] Duplicate prevention

### Inventory
- [ ] Product CRUD
- [ ] Stock ledger
- [ ] Stock adjustment
- [ ] Transfer
- [ ] Receiving
- [ ] Low/out-of-stock
- [ ] Concurrency/oversell protection
- [ ] Audit

### Reporting
- [ ] Today
- [ ] 7 days
- [ ] 15 days
- [ ] Monthly
- [ ] Half-yearly
- [ ] Yearly
- [ ] Custom
- [ ] Sales
- [ ] Expenses
- [ ] COGS/profit
- [ ] Top products
- [ ] Low stock

### Notifications
- [ ] Email
- [ ] SMS
- [ ] Web/in-app
- [ ] Retry
- [ ] Provider failure isolation
- [ ] Delivery status
- [ ] Deduplication

### Quality
- [ ] 0 build errors
- [ ] 0 build warnings
- [ ] Tests pass
- [ ] No regressions
- [ ] CI green
- [ ] Security checks green
- [ ] Backup/restore verified
- [ ] Observability verified
- [ ] Documentation current
- [ ] Handover current
