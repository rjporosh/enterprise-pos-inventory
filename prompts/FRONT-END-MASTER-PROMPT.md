# MASTER PROMPT — ENTERPRISE POS & INVENTORY SaaS FRONTEND

You are working inside an existing enterprise POS + Inventory SaaS repository.

The repository already contains a substantial .NET backend with:

* Inventory Service
* POS Service
* PostgreSQL
* EF Core
* Clean Architecture
* CQRS/MediatR
* FluentValidation
* Redis
* RabbitMQ
* OpenTelemetry
* Serilog
* health checks
* integration events
* stock management
* sales/checkout
* cash sessions
* customers
* stores
* registers
* reporting foundation
* automated tests
* Docker/development infrastructure
* backend architecture documentation

DO NOT rewrite the backend unnecessarily.

Your job is to build the missing, production-grade frontend applications and only modify backend code when a genuine frontend-required API capability is missing.

---

# 1. PRIMARY OBJECTIVE

Create a professional, production-grade SaaS frontend under:

frontend/

with exactly two independent Next.js applications:

frontend/pos/
frontend/inventory/

The final result must look and behave like a real commercial product that can be sold to:

* grocery stores
* pharmacies
* clothing stores
* burkha stores
* electronics stores
* motor-parts stores
* general retail stores
* small shops
* medium businesses
* multi-store businesses
* enterprise retail organizations

The frontend must NOT look like a tutorial, demo, CRUD template, admin template, or AI-generated prototype.

It must feel like a polished commercial SaaS product.

---

# 2. NON-NEGOTIABLE TECHNOLOGY STACK

Use current stable versions compatible with the repository at implementation time.

Required:

* React 19
* Next.js with App Router
* TypeScript strict mode
* Redux Toolkit
* Redux-Saga
* modern CSS architecture
* Tailwind CSS or an equally production-grade styling system
* React Hook Form
* Zod
* accessible component primitives
* TypeScript API types
* ESLint
* Prettier
* unit testing
* integration testing
* Playwright end-to-end testing

Do NOT introduce unnecessary dependencies.

Prefer small, well-maintained libraries.

Do not use `any` unless there is a documented unavoidable reason.

Do not disable TypeScript strictness.

Do not suppress lint errors just to make the build pass.

---

# 3. ARCHITECTURE

Each frontend application must be independently maintainable.

Use a feature-oriented architecture.

Preferred structure:

frontend/
pos/
app/
components/
features/
store/
services/
hooks/
lib/
types/
config/
public/
tests/
docs/

inventory/
app/
components/
features/
store/
services/
hooks/
lib/
types/
config/
public/
tests/
docs/

Do not create one giant components directory containing the entire application.

Organize code by business capability.

Example:

features/
auth/
products/
barcode/
cart/
checkout/
customers/
cash-register/
reports/

Inventory may have:

features/
auth/
products/
categories/
brands/
units/
suppliers/
warehouses/
stock/
barcode/
reports/

---

# 4. API-FIRST DEVELOPMENT

Before writing UI code:

1. Inspect the existing backend source.
2. Inspect API controllers.
3. Inspect DTOs.
4. Inspect validators.
5. Inspect OpenAPI output if available.
6. Inspect backend documentation.
7. Identify all existing endpoints.
8. Map them into frontend API services.

Do not invent API contracts when an existing backend contract exists.

Create a centralized API client.

Requirements:

* typed requests
* typed responses
* centralized base URL
* authentication handling
* correlation/request ID support where applicable
* consistent error parsing
* ProblemDetails support
* request cancellation where appropriate
* retry only when safe
* timeout handling
* standardized API result handling

Create domain-specific API modules rather than scattering fetch calls throughout components.

Example:

services/api/
client.ts
auth-api.ts
products-api.ts
inventory-api.ts
sales-api.ts
customers-api.ts
reports-api.ts

---

# 5. STATE MANAGEMENT

Use Redux Toolkit + Redux-Saga deliberately.

Do NOT put every UI state into Redux.

Redux/Saga should manage application-wide workflows such as:

* authentication
* tenant context
* store context
* register context
* cashier session
* POS cart workflow
* checkout workflow
* background synchronization
* important notifications
* global application state

Use local state/form state for local UI concerns.

Organize Redux by feature.

Example:

store/
root-reducer.ts
root-saga.ts
store.ts

features/products/state/
products-slice.ts
products-saga.ts
products-selectors.ts

features/cart/state/
cart-slice.ts
cart-saga.ts
cart-selectors.ts

Do not create a single giant Redux slice.

---

# 6. AUTHENTICATION

The frontend must be designed for a real SaaS authentication system.

Support architecture for:

* login
* logout
* refresh session
* protected routes
* authenticated API requests
* unauthorized handling
* session expiration
* loading state
* tenant selection if required
* store selection
* register selection
* cashier session

Never expose secrets in client-side code.

Never store sensitive credentials insecurely.

Use the backend's actual authentication contract when available.

If authentication backend endpoints are missing, document the exact backend contract that must be added.

Do not fabricate fake authentication.

---

# 7. MULTI-TENANCY

Treat this as a true SaaS application.

The frontend must understand:

Tenant
↓
Stores
↓
Registers
↓
Users/Roles
↓
Products/Inventory/Sales

The UI must never assume there is only one store.

Provide appropriate tenant/store context handling.

Never allow the frontend to select arbitrary tenant IDs and assume that is authorization.

Authorization must ultimately be enforced by the backend.

The frontend should only hide/disable capabilities based on known permissions for UX purposes.

---

# 8. POS APPLICATION

The POS application is the highest-priority frontend.

It must be optimized for:

* barcode scanners
* keyboard
* mouse
* touch screen
* low-latency operation
* cashier speed
* minimal clicks
* large readable controls
* responsive layouts

The main POS screen should feel like a real retail terminal.

Required capabilities:

* barcode scanning
* product search
* SKU search
* product lookup
* category browsing
* add product to cart
* change quantity
* remove item
* clear cart
* line discount
* cart discount
* tax display
* subtotal
* total
* customer selection
* hold/suspend sale
* resume sale
* checkout
* payment selection
* cash payment
* card
* mobile money
* store credit if supported by backend
* other payment methods
* change calculation
* sale completion
* receipt preview
* receipt printing
* receipt reprint
* void where permission allows
* sales history

The POS must remain usable on smaller screens.

---

# 9. BARCODE SCANNING

Barcode scanning is a core feature.

Implement scanner-first UX.

The barcode input must be:

* fast
* keyboard compatible
* automatically focusable
* resistant to accidental focus loss
* capable of handling scanner Enter suffixes
* debounced only when appropriate
* clearly indicate success/failure

Typical flow:

Scanner
↓
Barcode
↓
API lookup
↓
Product
↓
Cart
↓
Quantity +1

Do not force the cashier through a modal for normal barcode scanning.

When a barcode is successfully scanned:

* show product feedback
* update cart immediately
* optionally play a configurable success sound
* preserve scanner focus

When not found:

* show a clear error
* keep scanner focus
* allow manual product search
* provide an optional "create product" action only where permission allows

Do not fake barcode data.

Use the actual backend barcode field/API.

---

# 10. BARCODE GENERATION

Inventory application must provide a professional barcode generation workflow.

Support:

* generating an internal barcode where appropriate
* assigning barcode to product
* barcode preview
* barcode label layout
* print labels
* download/print where appropriate
* multiple labels
* configurable label quantities

Support common barcode formats when technically appropriate.

Do not pretend that every product should receive an arbitrary EAN/UPC.

Clearly distinguish:

* manufacturer barcode
* internally generated/store barcode

The UI must make this distinction clear.

---

# 11. PRODUCT MANAGEMENT

Create a professional product management interface.

Required:

* product list
* server-side pagination
* search
* filters
* sorting
* create
* edit
* view
* soft delete
* activation/deactivation
* barcode
* SKU
* category
* brand
* unit
* supplier
* cost price
* selling price
* discount
* tax
* reorder level
* stock visibility
* inventory tracking

Use professional data tables.

Include:

* column visibility
* pagination
* loading skeleton
* empty state
* error state
* retry
* confirmation dialogs
* keyboard accessibility

---

# 12. INVENTORY MANAGEMENT

Build interfaces for:

* stock overview
* stock details
* stock in
* stock out
* adjustment
* transfer
* warehouse selection
* low stock
* out of stock
* movement history

Use visual status indicators.

Do not use excessive colors.

Keep the design professional and restrained.

---

# 13. THERMAL RECEIPTS

Thermal printing is mandatory.

Support:

* 58mm receipt
* 80mm receipt
* browser print
* print preview
* receipt reprint
* receipt template configuration

The receipt should contain configurable:

* store name
* logo
* address
* phone
* invoice number
* date/time
* cashier
* register
* customer
* products
* quantity
* unit price
* discount
* tax
* subtotal
* total
* payment method
* amount tendered
* change
* footer

Build the receipt renderer independently from the POS screen.

Example:

components/printing/
ReceiptPreview.tsx
Receipt58mm.tsx
Receipt80mm.tsx
print-receipt.ts
receipt-types.ts

Never make printing logic part of a giant POS component.

---

# 14. CASH DENOMINATION ARCHITECTURE

There is an advanced planned feature for denomination-aware cash handling.

Design the frontend so this can be added cleanly.

Supported denominations should be configurable, with an initial Bangladesh-friendly set such as:

1000
500
200
100
50
20
10
5

Do not hardcode this throughout the application.

Use:

types:
CashDenomination

configuration:
cashDenominations

The eventual workflow should support:

Cash received
↓
Denomination count
↓
Total verified
↓
Sale payment
↓
Change calculation
↓
Transaction audit

Later cash drawer reconciliation can support:

Opening cash
+
Cash sales
+
Cash in
-------

## Cash out

# Refunds

Expected cash

Expected cash
vs
Actual denomination count
=========================

Variance

The frontend should be architected so this can be added without rewriting checkout.

If backend APIs for denomination tracking do not exist, document the missing API contract rather than inventing persistence.

---

# 15. REPORTING

Build a professional reporting area.

Support the UI architecture for:

* today
* yesterday
* custom range
* daily
* weekly
* half-monthly
* monthly
* half-yearly
* yearly

Reports should include:

* total sales
* revenue
* gross sales
* discounts
* tax
* net sales
* cash
* card
* mobile money
* other payments
* top-selling products
* low-stock products
* stock movement
* cashier performance
* register performance
* voided sales
* refunds
* profit where backend data supports it

Use reusable date-range controls.

Do not create six unrelated reporting pages.

Create reusable report infrastructure.

---

# 16. DASHBOARD

Create an executive dashboard with:

* today's sales
* today's transactions
* average transaction value
* gross revenue
* discounts
* tax
* payment breakdown
* top products
* low stock
* out of stock
* recent sales
* register status
* cashier status
* sales trend

Dashboard must not fire dozens of unnecessary requests.

Prefer consolidated reporting endpoints where available.

Clearly show data loading states.

---

# 17. RESPONSIVE DESIGN

The UI must work across:

* desktop
* laptop
* tablet
* POS touchscreen
* small mobile screens where appropriate

Do not simply shrink desktop layouts.

Create deliberate responsive layouts.

POS deserves a specialized responsive strategy.

Inventory/admin pages may use standard responsive dashboard layouts.

---

# 18. DESIGN SYSTEM

Create a consistent design system.

Define:

* typography
* spacing
* radius
* shadows
* form controls
* buttons
* badges
* tables
* dialogs
* dropdowns
* tabs
* cards
* alerts
* toast notifications
* skeleton loaders
* empty states
* error states

The visual language should be:

* modern
* professional
* clean
* restrained
* trustworthy
* enterprise
* fast
* accessible

Avoid:

* excessive gradients
* giant decorative illustrations
* excessive rounded cards
* childish colors
* unnecessary animations
* dashboard template aesthetics

Use animation only when it improves usability.

---

# 19. ACCESSIBILITY

Target WCAG 2.2 AA principles.

Requirements:

* keyboard navigation
* visible focus states
* semantic HTML
* labels
* accessible dialogs
* screen-reader support
* sufficient contrast
* accessible error messages
* reduced motion support
* proper table semantics

POS keyboard navigation is especially important.

---

# 20. ERROR HANDLING

Handle backend ProblemDetails correctly.

Create a centralized error normalization layer.

The UI must distinguish:

* validation error
* authentication error
* authorization error
* not found
* conflict
* network failure
* timeout
* server error

Never show raw stack traces to users.

Provide useful messages.

Log technical details appropriately without leaking secrets.

---

# 21. PERFORMANCE

Treat performance as a first-class requirement.

Use:

* Server Components where appropriate
* Client Components only where required
* lazy loading
* dynamic imports
* memoization where justified
* virtualized large lists when required
* optimized images
* minimal bundle size
* request deduplication
* efficient selectors
* stable callbacks where useful

Do not cargo-cult memoize everything.

POS interactions should feel instantaneous.

Barcode lookup must be optimized.

---

# 22. SECURITY

Never:

* expose secrets
* expose backend credentials
* trust client-side permissions
* trust client-provided tenant IDs
* store sensitive data unnecessarily
* log passwords/tokens/payment secrets

Use secure cookie/session patterns when compatible with backend architecture.

Implement route protection.

Implement permission-aware UI.

Remember:

FRONTEND AUTHORIZATION IS UX ONLY.

BACKEND AUTHORIZATION IS SECURITY.

---

# 23. TESTING

Every major feature must have tests.

Minimum:

Unit tests
Integration tests
Component tests where useful
Playwright E2E tests

Critical E2E flows:

1. Login
2. Product creation
3. Product search
4. Barcode scan
5. Add item to cart
6. Change quantity
7. Checkout
8. Cash payment
9. Change calculation
10. Receipt preview
11. Receipt printing flow
12. Sale history
13. Inventory stock in
14. Inventory stock adjustment
15. Stock transfer
16. Low-stock filtering
17. Authorization behavior
18. Logout/session expiry

Do not write superficial tests that only assert that a component renders.

---

# 24. DOCUMENTATION

Each frontend application MUST contain:

docs/

including:

docs/README.md
docs/ARCHITECTURE.md
docs/DEVELOPERS-GUIDE.md
docs/PROJECT-STRUCTURE.md
docs/API-INTEGRATION.md
docs/TESTING.md
docs/DEPLOYMENT.md

Also:

docs/adr/

with ADRs for significant decisions.

Examples:

ADR-001-frontend-architecture.md
ADR-002-redux-saga.md
ADR-003-api-client.md
ADR-004-authentication.md
ADR-005-barcode-scanning.md
ADR-006-thermal-printing.md
ADR-007-pos-state-management.md
ADR-008-multi-tenancy.md

Also create C4 documentation:

docs/c4/
context.md
container.md
component.md
deployment.md

Use Mermaid diagrams where appropriate.

---

# 25. DEVELOPER GUIDE

The developer guide must explain how to add a new CRUD feature.

For example:

"How to add a new Product-like CRUD feature"

Document:

1. API type
2. API service
3. feature folder
4. Redux slice if needed
5. Saga if needed
6. selectors
7. list page
8. form
9. validation schema
10. detail page
11. permissions
12. tests
13. documentation
14. navigation registration

A new developer should be able to add a complete CRUD module by following the guide.

---

# 26. FOLDER STRUCTURE DIAGRAM

Every frontend must document its folder structure.

Example:

frontend/
pos/
app/
components/
features/
services/
store/
hooks/
lib/
types/
config/
tests/
docs/

Explain the purpose of every important directory.

---

# 27. C4 DIAGRAM

Create:

System Context
Container
Component
Deployment

for each frontend.

Also document the relationship:

POS Frontend
↓
POS API
↓
POS Database

POS API
↓
RabbitMQ
↓
Inventory API

Inventory Frontend
↓
Inventory API
↓
Inventory Database

---

# 28. CODE QUALITY

Follow these rules:

* strict TypeScript
* small focused components
* no giant components
* no giant hooks
* no duplicated API logic
* no duplicated validation logic
* no magic strings
* no magic numbers
* meaningful names
* explicit types
* reusable primitives
* feature boundaries
* predictable imports
* no circular dependencies
* no dead code
* no commented-out code
* no TODO placeholders for core functionality
* no fake/mock production behavior

If something cannot be completed because the backend lacks an endpoint, explicitly document it.

Do not silently fake the feature.

---

# 29. BACKEND GAP ANALYSIS

While building the frontend, continuously compare required product functionality against the existing backend.

If a required capability is genuinely missing, produce:

docs/BACKEND-GAPS.md

For every gap document:

Feature
Current backend capability
Missing endpoint/entity
Suggested endpoint
Request DTO
Response DTO
Validation
Authorization requirement
Persistence requirement
Reason frontend needs it

Only modify backend code if necessary and safe.

Do not redesign working backend architecture.

---

# 30. DO NOT INVENT FEATURES AS IF THEY ALREADY EXIST

This is extremely important.

If backend has:

GET /api/v1/reports/daily-sales

do not pretend that:

GET /api/v1/reports/yearly-sales

already exists.

If barcode generation is missing, document it.

If authentication is missing, document it.

If denomination persistence is missing, document it.

The frontend must remain truthful to the actual backend.

---

# 31. PRODUCTION UX

Every screen must have:

Loading state
Empty state
Error state
Success state
Permission state

Tables must handle:

* large datasets
* pagination
* sorting
* filtering
* network failures

Forms must handle:

* validation
* server validation
* dirty state
* submit loading
* duplicate/conflict errors
* cancellation
* successful save

---

# 32. POS UX RULES

POS is not a normal CRUD application.

Optimize for:

FAST
SIMPLE
KEYBOARD-FIRST
SCANNER-FIRST
TOUCH-FRIENDLY

Avoid unnecessary confirmation dialogs.

Do not require users to click through multiple screens to sell one product.

A normal sale should feel approximately like:

Scan
Scan
Scan
Pay
Print

---

# 33. INVENTORY UX RULES

Inventory is more information-dense.

Prioritize:

* tables
* filters
* search
* bulk actions
* clear status indicators
* fast navigation
* predictable forms
* audit visibility

---

# 34. INDUSTRY EXTENSIBILITY

Do not hardcode the frontend around one industry.

The core product must support generic retail.

Architect optional product attributes so future modules can support:

Clothing:
size/color/material

Pharmacy:
batch/expiry/manufacturer

Motor parts:
OEM/part compatibility

Burkha:
size/color/material/design

Do not create separate applications for each industry.

---

# 35. INTERNATIONALIZATION READINESS

Even if English is the initial language:

* centralize labels
* avoid hardcoded UI strings everywhere
* prepare for localization
* prepare for RTL
* support configurable currency
* support date/time formatting
* support decimal formatting

The initial product should work well for Bangladesh and BDT while remaining architecturally extensible.

---

# 36. IMPLEMENTATION STRATEGY

Work in phases.

PHASE 1:
Analyze backend and create frontend architecture.

PHASE 2:
Create POS and Inventory Next.js applications.

PHASE 3:
Build shared design primitives independently in each application unless a true shared package is justified.

PHASE 4:
Implement API integration.

PHASE 5:
Implement authentication/session architecture.

PHASE 6:
Implement Inventory.

PHASE 7:
Implement POS.

PHASE 8:
Implement barcode workflow.

PHASE 9:
Implement printing architecture.

PHASE 10:
Implement reporting/dashboard.

PHASE 11:
Implement advanced cash-denomination-ready architecture.

PHASE 12:
Implement tests.

PHASE 13:
Security/performance/accessibility audit.

PHASE 14:
Documentation.

---

# 37. IMPORTANT: DO NOT STOP AT SCAFFOLDING

Do not return:

"Created the project structure."

The result must contain real working screens.

At minimum, implement real UI flows for:

Inventory:

* login architecture
* dashboard
* products
* create product
* edit product
* product detail
* stock
* stock movements
* warehouses
* suppliers
* barcode

POS:

* login architecture
* register/session
* POS checkout
* barcode input
* product search
* cart
* customer
* payment
* checkout result
* receipt preview
* sales history
* reports/dashboard

Use the real backend APIs wherever they exist.

---

# 38. DEFINITION OF DONE

The work is NOT complete until:

* both frontend apps build successfully
* TypeScript passes
* ESLint passes
* formatting passes
* unit tests pass
* integration tests pass where configured
* Playwright critical flows pass where backend is available
* no critical accessibility issues remain
* no obvious console errors
* no fake API implementations remain
* API errors are handled
* loading states exist
* empty states exist
* responsive layouts work
* POS scanner workflow works
* barcode assignment workflow works
* receipt preview works
* print architecture works
* documentation is complete
* ADRs exist
* C4 diagrams exist
* folder structure documentation exists
* developer CRUD guide exists
* backend gaps are documented
* production configuration is documented

---

# 39. FINAL VALIDATION

Before finishing, run the appropriate checks for both applications.

Examples:

npm/pnpm/yarn install
build
lint
typecheck
test
e2e

Use the repository's actual package manager.

Do not claim success if a command was not actually run.

Report:

* what was implemented
* what backend APIs were consumed
* what backend gaps remain
* build status
* lint status
* typecheck status
* test status
* E2E status
* production blockers
* recommended next steps

---

# 40. MOST IMPORTANT PRINCIPLE

This is not a UI mockup.

This is not a portfolio project.

This is not a prototype.

Build it as a real SaaS POS + Inventory product intended to be deployed to real stores and sold commercially.

Prioritize:

CORRECTNESS
SECURITY
PERFORMANCE
ACCESSIBILITY
MAINTAINABILITY
AUDITABILITY
SIMPLE UX
FAST POS OPERATION
EXTENSIBILITY

Do not sacrifice architecture for visual polish.

Do not sacrifice usability for architectural purity.

Do not fake backend capabilities.

Do not rewrite working backend code without justification.

Inspect first.
Plan second.
Implement third.
Test fourth.
Document fifth.

Begin by inspecting the entire repository and producing a concise implementation plan and backend capability matrix.

Then implement the frontend projects under:

frontend/pos
frontend/inventory

without waiting for additional instructions unless a genuinely blocking ambiguity is encountered.
