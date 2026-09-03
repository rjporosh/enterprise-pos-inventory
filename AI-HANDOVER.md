# AI Handover

Read this in full before touching `frontend/`. It is the source of truth for what's implemented.
Read `docs/API-GAPS.md` next. Do not trust an earlier chat summary over this file or over the
actual repository contents — if they disagree, the repository is correct.

## A. Project overview

This repo is an Enterprise POS & Inventory SaaS: a .NET microservices backend
(`services/inventory-service`, `services/pos-service`) that already existed, plus a frontend
(`frontend/inventory`, `frontend/pos`) built in this and prior sessions against the backend's
**real, currently-implemented** API surface only.

- **Inventory app** (`frontend/inventory`): back-office admin — product catalog CRUD, stock
  receive/issue/adjust/transfer, a real-metrics dashboard.
- **POS app** (`frontend/pos`): cashier-facing checkout — terminal setup, cash session
  open/close, product search + cart, sale completion + receipt, sale history + void, daily
  sales report.

**Stack (both apps, identical)**: Next.js 15 (App Router), React 19, TypeScript 5 (strict,
`noUncheckedIndexedAccess: true`), Redux Toolkit + redux-saga for all async/server state, a
hand-rolled CSS design system (no Tailwind/CSS-in-JS — see `components/ui/ui.css` +
`app/globals.css` tokens), Vitest + Testing Library for tests.

**Environment config**: each app reads its API base URL(s) from `.env.local`
(`NEXT_PUBLIC_INVENTORY_API_URL`, and for POS also `NEXT_PUBLIC_POS_API_URL`). No default is baked
in — see `.env.example` in each app.

**The two apps are independently deployable.** Shared code (the design system, the API client
shell shape) is deliberately duplicated between them rather than extracted into a shared package —
see `docs/AI-CODING-RULES.md` for why; don't "fix" this by merging them.

**Full architecture docs**: `docs/inventory/ARCHITECTURE.md`, `docs/inventory/PROGRAMMER-GUIDE.md`,
`docs/inventory/ADDING-A-CRUD.md`, `docs/pos/ARCHITECTURE.md`.

## B. Repository tree (frontend, as it exists right now)

```
frontend/
  inventory/
    src/
      app/                       # dashboard, products (list/new/[id]), stock (list/in/out/adjustment/transfer)
      components/ui/              # 16 design-system components + toastSlice + ui.css
      components/layout/           # AppShell, Sidebar
      features/products/            # slice.ts, validation.ts, components/ProductForm.tsx, __tests__/
      features/stock/                 # slice.ts, __tests__/
      lib/api/                          # client.ts, products.ts, stock.ts
      lib/store/                         # store.ts, hooks.ts, StoreProvider.tsx
    README.md, package.json, tsconfig.json, next.config.ts, .eslintrc.json,
    vitest.config.ts, vitest.setup.ts, .env.example
  pos/
    src/
      app/                       # setup, page.tsx (terminal), sales, reports
      components/ui/              # same 16 components, duplicated (see AI-CODING-RULES.md)
      components/layout/           # AppShell, Topbar
      features/catalog/             # slice.ts
      features/cart/                  # slice.ts, __tests__/ (9 tests)
      features/session/                # slice.ts (terminal config + cash session, localStorage-backed)
      features/sale/                     # slice.ts (checkout saga + void), components/Receipt.tsx
      lib/api/                             # client.ts, sales.ts, cashSessionsAndReports.ts, catalog.ts
      lib/store/                            # store.ts, hooks.ts, StoreProvider.tsx
    README.md, package.json, tsconfig.json, next.config.ts, .eslintrc.json,
    vitest.config.ts, vitest.setup.ts, .env.example

docs/
  API-GAPS.md                   # authoritative real-vs-documented backend contract notes
  AI-CODING-RULES.md            # rules for future AI agents working on this repo
  inventory/ARCHITECTURE.md, PROGRAMMER-GUIDE.md, ADDING-A-CRUD.md
  pos/ARCHITECTURE.md, README.md, PROGRAMMER-GUIDE.md (thin, points to ARCHITECTURE.md)

AI-HANDOVER.md                  # this file
NEXT-AI-PROMPT.md               # copy-paste prompt for the next agent
```

Everything listed above **exists in the repository right now** — this is not a plan, it's an
inventory of what was written and verified this session (see §E for verification results).

## C. Inventory app — precise checklist

| Item | Status |
|---|---|
| Scaffold (package.json, tsconfig, next.config, eslint, vitest config, .env.example) | DONE |
| API client (`lib/api/client.ts`) — fetch wrapper, `ApiError`/`NetworkError`, ProblemDetails parsing, 204 handling | DONE |
| Products API client (`lib/api/products.ts`) | DONE — typed from real `ProductDto`/`ProductListItemDto` |
| Stock API client (`lib/api/stock.ts`) | DONE — typed from real `StockDto`/`StockListItemDto`, all 4 movement types |
| Redux store + saga middleware (`lib/store/store.ts`, `hooks.ts`, `StoreProvider.tsx`) | DONE |
| Design system (Button, Field, Input, Select, SearchInput, Card, Badge, PageHeader, DataToolbar, EmptyState, ErrorState, Skeleton/TableSkeleton, Modal, ConfirmDialog, ToastStack, Pagination) | DONE |
| Layout (AppShell, Sidebar) | DONE |
| Dashboard (`app/page.tsx`) — real counts only (total products, low-stock, out-of-stock) | DONE |
| Products list (`app/products/page.tsx`) — search (debounced), status filter, sort, pagination, delete w/ confirm | DONE |
| Products create (`app/products/new/page.tsx`) | DONE |
| Products edit (`app/products/[id]/page.tsx`) | DONE |
| Products delete | DONE (part of list page) |
| Products validation (`features/products/validation.ts`) — mirrors backend FluentValidation | DONE |
| Stock list (`app/stock/page.tsx`) — low/out-of-stock filter, pagination | DONE |
| Stock in (`app/stock/in/page.tsx`) | DONE |
| Stock out (`app/stock/out/page.tsx`) | DONE |
| Stock adjustment (`app/stock/adjustment/page.tsx`) | DONE |
| Stock transfer (`app/stock/transfer/page.tsx`) | DONE |
| Loading/error/empty states throughout | DONE |
| Toast system | DONE |
| Tests | DONE — 12 tests (7 validation, 5 stock slice), all passing |
| Category/Brand/Unit/Warehouse pickers | **NOT DONE — no backend endpoint exists.** Forms use labeled manual-GUID text inputs instead. See `docs/API-GAPS.md`. This is intentional, not an oversight. |
| Barcode generation (product form live preview, list "Label" action, detail/edit "Print barcode label") | DONE — `features/products/components/BarcodeLabel.tsx` (Code128 SVG via `jsbarcode`) + `BarcodeLabelModal.tsx` (print view). Client-side only — renders whatever string is already stored in `Product.Barcode`; does not touch barcode *scanning* or POS barcode-aware search, which remain the backend gap tracked in `docs/API-GAPS.md`. |

## D. POS app — precise checklist

| Item | Status |
|---|---|
| Scaffold | DONE |
| API clients (`lib/api/client.ts`, `sales.ts`, `cashSessionsAndReports.ts`, `catalog.ts`) | DONE — typed from real `SaleDto`/`CashSessionDto`/`DailySalesReportDto`/`ProductListItemDto` |
| Redux store + saga middleware | DONE |
| Design system | DONE (duplicated from inventory app) |
| Layout (AppShell, Topbar showing session status badge) | DONE |
| Terminal setup — store/register/cashier config, saved to localStorage, with explicit "DEMO / DEVELOPMENT ACCESS" banner | DONE |
| Open cash session | DONE |
| Close cash session | DONE |
| Product search (name/SKU, scanner-friendly Enter-to-add) | DONE — **not** barcode-matching (backend gap, see API-GAPS.md) |
| Cart (add/remove/quantity, pure client state) | DONE, with 9 passing unit tests |
| Sale creation | DONE (as step 1 of the checkout saga, not a separate user action — see docs/pos/ARCHITECTURE.md) |
| Add/remove sale item | DONE — add is part of the checkout saga; the `removeItem` API method exists in `lib/api/sales.ts` but there is currently no UI calling it directly (cart quantity/remove edits happen pre-checkout, client-side only, and are pushed to the backend via `addItem` at checkout time) |
| Sale completion | DONE, with a multi-stage loading label (creating/adding items/completing) |
| Sale void | DONE — `app/sales/page.tsx`, list + void with a reason modal |
| Receipt (print-friendly, from real `SaleDto`) | DONE — `features/sale/components/Receipt.tsx`, `window.print()` + `@media print` |
| Sale history list | DONE — `app/sales/page.tsx` |
| Daily sales report | DONE — `app/reports/page.tsx`, defaults to yesterday, treats 404 as an expected "not generated yet" state, not an error |
| Tests | DONE for cart (9 tests); no additional test files were added for catalog/session/sale sagas this session — **TODO if more test coverage is wanted** |
| Loading/error/empty states throughout | DONE |

**Everything in the MVP brief's "WHAT MVP COMPLETE MEANS" section is implemented**: open session,
search, add, change qty, remove, complete, view result, void, close session (POS); create, edit,
delete, search/filter, stock in/out/adjustment/transfer (Inventory); daily sales view (Reports).

## E. Backend

**No backend changes were made.** `services/inventory-service` and `services/pos-service` are
untouched — confirm with `git status services/` (should be clean) and `git log -- services/` (no
new commits from this session touch that path). All gaps found are documented in
`docs/API-GAPS.md` as recommendations, not applied.

## F. Verification status (commands actually run this session)

### Inventory (`frontend/inventory`)

```
npm install     PASS (426 packages)
npm run typecheck  PASS (tsc --noEmit, no errors)
npm run lint       PASS (next lint — "No ESLint warnings or errors")
npm run build      PASS (10/10 routes compiled)
npm test           PASS (12/12 tests, 2 files)
```

### POS (`frontend/pos`)

```
npm install     PASS (426 packages)
npm run typecheck  PASS (tsc --noEmit, no errors — after fixing 3 noUncheckedIndexedAccess issues found by this same command)
npm run lint       PASS (next lint — "No ESLint warnings or errors")
npm run build      PASS (5/5 routes compiled)
npm test           PASS (9/9 tests, 1 file)
```

Both apps' `node_modules/`, `.next/`, and `.env.local` were removed before committing (kept out of
git per `.gitignore`; `.env.example` is committed instead).

**Not verified**: end-to-end runtime behavior against a live backend (no running instance of
`inventory-service`/`pos-service` was available in this environment to point the apps at). All
verification above is build-time (types, lint, unit tests, production bundle compiles). This is
the single most important remaining verification step before a real customer demo — see §H.

## G. Git history

Existing history (pre-dating this session) is fully preserved — no rebase, reset, squash, or
force-push was performed. New work is added as new commits on top, one per logical
phase/milestone, with descriptive messages (see `git log` after this handover's commit lands).
Confirm with `git log --oneline` — the pre-existing commits (backend build fixes, spec/roadmap
docs, etc.) remain exactly as they were.

## H. Recommended next step (single most valuable action)

Point `frontend/inventory/.env.local` and `frontend/pos/.env.local` at a running instance of both
backend services (`docker-compose up` at the repo root should bring up Postgres/Redis/RabbitMQ/Seq
per the existing `docker-compose.yml`; the two .NET services need to be run separately per their
own README/launch instructions, which this session did not touch) and manually walk through: open
a cash session → search a real product → complete a sale → view the receipt → view sale history →
void it → check the Inventory dashboard/stock pages against the same data. This will surface any
runtime issue that TypeScript/build/unit-tests cannot catch (e.g. an actual field-name mismatch
between what a handler returns at runtime vs. what its DTO class declares in source, which can
happen with things like computed/ignored properties). This exact task is item 3 in
`NEXT-AI-PROMPT.md`.

No known blocker prevents this from being demoed to a real customer for the currently-implemented
MVP scope, other than the manual-GUID friction for category/brand/unit/warehouse/store/register
documented in §C/§D and `docs/API-GAPS.md` — that's a real, known rough edge for a non-technical
operator, not a defect, and the highest-priority backend follow-up.

---

## I. Session addendum — 2026-08-28

**Scope of the incoming request**: continue toward a production-ready enterprise multi-tenant
SaaS (auth integration, notification integration, a license/subscription engine, thermal
printing, barcode scan+generation, enterprise demo data). This addendum records what was actually
verified this session, per this file's own rule of not marking things DONE without verification.

### Environment constraint (read this before continuing)

This session's sandbox had **`node`/`npm` only — no `dotnet` SDK, and network access limited to
npm/GitHub-style registries (no NuGet)**. Full detail and recommendation in `needed-credentials.md`
(new file, created this session). Net effect: everything below that touches
`services/*` could be **read and analyzed**, but not built, migrated, run, or verified.

### What was verified this session (commands actually run)

```
frontend/inventory: npm install, npm run typecheck, npm run lint, npm test (12/12), npm run build (11/11 routes)
frontend/pos:        npm install, npm run typecheck, npm run lint, npm test (9/9), npm run build (7/7 routes)
```
Both apps still build clean, unchanged from the prior session's checklists (§C/§D above remain
accurate). No frontend source was modified this session — this was a verification pass, not a
feature pass. Inventory now has 11 routes vs. the 10 documented in §F above (routing behavior was
not investigated further; not a regression, both `npm run build` and `npm test` are green).

### Key finding: §E above ("Backend: no changes") and `docs/API-GAPS.md`'s auth/tenancy rows were stale

`services/auth-service` and `services/notification-service` were added in commit `0e79624`
("auth-service and notification-service added"), which **postdates** this handover file and
`docs/API-GAPS.md`. Both are substantial, real implementations, not stubs:

- **auth-service** (213 `.cs` files): register/login/refresh/logout/me, change/forgot/reset
  password, OTP, security questions, audit logs, and full RBAC admin (permissions/modules/roles,
  role↔permission and user↔role assignment). Minimal-API `Endpoints` style, not MVC controllers.
- **notification-service** (134 `.cs` files): send/list/get/cancel/retry/soft-delete
  notifications, recipient preferences, templates, Email/SMS/Push channel abstractions, outbox
  pattern, scheduling jobs.

**Neither is integrated into either frontend app.** POS still uses a raw pasted `cashierId` GUID;
there is no login screen, token storage, or RBAC-aware UI in either app; there is no in-app
notification bell/panel anywhere.

**Also re-confirmed, still accurate**: there is **no license/subscription/trial/tenant-isolation
code anywhere in the repository** — a repo-wide grep for those terms across all four services
turns up only the pre-existing `BaseEntity` marker field. The "3-day trial / monthly subscription /
module entitlement" engine requested does not exist in any form and would be a from-scratch domain
model + migrations + middleware, needing `dotnet` to build and verify.

`docs/API-GAPS.md` has been corrected in place (two rows updated/added) rather than trusted as-is.

### Remaining tasks, in the order they'd need to happen

1. **Get a `dotnet` + Docker environment** (Claude Code desktop/terminal, or local machine) — this
   is the actual blocker, not a task. `docker-compose up` (Postgres/Redis/RabbitMQ/Seq) plus each
   service's own launch profile needs to run so `dotnet build`/`dotnet test`/`dotnet ef` can be
   verified, per §H above (still the single most valuable unverified step, now four services deep
   instead of two).
2. Wire `auth-service` into both frontend apps: shared JWT/refresh-token client, auth Redux slice,
   route guards, RBAC-aware UI, `cashierId`/`userId` derived from the token.
3. Wire `notification-service` into both frontend apps: notifications client/slice, bell/panel UI;
   wire inventory-service low-stock events (and any future trial/subscription events) to call it.
4. Design and build the license/subscription/entitlement engine from scratch (domain model,
   migrations, middleware, upgrade-flow UI) — this is new backend work, not integration.
5. Thermal receipt printing (58mm/80mm) and barcode generation are frontend-only concerns
   (`features/sale/components/Receipt.tsx` print CSS; a barcode-rendering library for the
   Inventory product form). **58mm/80mm receipt printing was implemented and verified this
   session** — see the "Shipped this session" entry below. Barcode generation (product labels)
   was not reached this session — genuinely next if continuing in a `node`-only environment.
6. Enterprise demo data seeding needs a live Postgres reachable by `dotnet ef`/migrations — blocked
   on item 1.

### Shipped this session (frontend-only, fully verified)

- **POS thermal receipt printing, real 58mm/80mm profiles.** `TerminalConfig.receiptPaperWidthMm`
  (`58 | 80`, default `80` for backward compatibility with previously-saved configs) added to
  `features/session/slice.ts`, exposed as a `<Select>` on the Setup page (`app/setup/page.tsx`),
  and consumed by `features/sale/components/Receipt.tsx` to set `@page` size, printable width
  (48mm/72mm — printable area, not roll width), and a monospace font sized per profile (9pt/10.5pt)
  in the print stylesheet. This is a browser-print-dialog-driven receipt, not a raw ESC/POS bridge
  — that item remains not-started per `docs/ROADMAP-v3.0.md` Phase 7 "Local print bridge."
  Re-verified clean after the change: `frontend/pos` typecheck/lint/test(9/9)/build(7/7 routes) all
  pass. Files changed: `features/session/slice.ts`, `app/setup/page.tsx`,
  `features/sale/components/Receipt.tsx`.


### Exact next command

**If continuing in a `node`-only sandbox** (no `dotnet`): implement barcode generation for the
Inventory product form (`frontend/inventory/src/features/products/components/ProductForm.tsx` +
product list/detail views) using a pure-JS barcode-rendering library reachable via npm, rendering
the existing `Product.Barcode` field as a scannable image/label. Verify with the same
typecheck/lint/test/build loop used this session.

**If continuing with `dotnet` + Docker available** (needed for everything else on this list):
```
cd enterprise-pos-inventory
docker-compose up -d
dotnet restore EnterprisePOS.sln && dotnet build EnterprisePOS.sln
```
Then follow `docs/ROADMAP-v3.0.md` Phase 1 exit criteria before starting Phase 2 (auth/tenancy).

### Suggested commit message for this session

```
feat(pos): add 58mm/80mm thermal receipt paper profiles; docs: correct stale
auth/notification gaps, add needed-credentials.md

- features/session/slice.ts: TerminalConfig gains receiptPaperWidthMm (58|80,
  default 80 for backward compat with saved configs)
- app/setup/page.tsx: paper-width selector on terminal identity form
- features/sale/components/Receipt.tsx: real @page/printable-width/font-size
  print profiles per paper width, replacing the one-size print stylesheet
- docs/API-GAPS.md: auth-service and notification-service exist (added in 0e79624,
  after this doc was written) but are not yet integrated into either frontend app;
  re-confirm no license/subscription/tenant-isolation code exists anywhere
- needed-credentials.md: new — documents the dotnet/NuGet-less sandbox constraint
  hit this session, demo-credential placeholders (none exist yet), and env vars
- AI-HANDOVER.md, release-notes/release-notes.md: record session findings and
  the receipt-printing change

Verified: frontend/inventory and frontend/pos both pass typecheck/lint/test/build
(11/11 and 7/7 routes respectively, 12/12 and 9/9 tests). No backend source
touched — services/* could not be built/run in this sandbox (no dotnet SDK).
```

## J. Session addendum — 2026-08-30

**Scope of the incoming request**: the exact next command from §I above — implement barcode
generation for the Inventory product form in this `node`-only sandbox (no `dotnet`, same
constraint as §I; unchanged, see `needed-credentials.md`).

### What was verified this session (commands actually run)

```
frontend/inventory: npm install, npm run typecheck, npm run lint, npm test (15/15), npm run build (11/11 routes)
```

`frontend/pos` was not touched and not re-verified this session (no changes made there).

### Shipped this session (frontend-only, fully verified)

- **Barcode generation for the Inventory product form, list, and edit/detail page.** Added
  `jsbarcode` (a pure-JS Code128 SVG renderer) as a runtime dependency, `@types/jsbarcode` as a
  dev dependency. Code128 was chosen over EAN/UPC because `Product.Barcode` is free-text on the
  backend (see `lib/api/products.ts`), not a pre-validated numeric symbology, so encoding whatever
  was actually typed/scanned in is the only approach that won't reject legitimately-saved data.
  - `features/products/components/BarcodeLabel.tsx` (new) — renders an SVG barcode for a given
    value, with a "no barcode set" placeholder for empty values and a non-crashing error state if
    `jsbarcode` itself throws.
  - `features/products/components/BarcodeLabelModal.tsx` (new) — a print-friendly single-label
    modal, using the same visibility-based `@media print` pattern as
    `frontend/pos/src/features/sale/components/Receipt.tsx` (hide everything except the label,
    `window.print()`), so no popup window or new dependency for printing was needed.
  - `features/products/components/ProductForm.tsx` — live barcode preview under the Barcode field
    as the value is typed or scanned in.
  - `app/products/page.tsx` — a "Label" row action (shown only when that product has a barcode)
    opening the print modal.
  - `app/products/[id]/page.tsx` — a "Print barcode label" button in the page header when the
    loaded product has a barcode.
  - `vitest.config.ts` — added `esbuild: { jsx: "automatic" }`. This is the first `.tsx` test file
    in the project (`features/products/__tests__/BarcodeLabel.test.tsx`, 3 tests); without this,
    esbuild falls back to the classic JSX transform and fails with "React is not defined" in any
    file containing a JSX literal, since the project's own `tsconfig.json` uses `"jsx": "preserve"`
    (correct for Next/SWC, but not read by vitest's esbuild pipeline). Needed for this test to run,
    not just a style preference — future `.tsx` component tests benefit from the same fix.
  - This is purely a **label-printing / barcode-generation** feature (turning a stored barcode
    value into a scannable image). It does not touch barcode **scanning** or POS barcode-aware
    search — `docs/API-GAPS.md`'s existing "Barcode lookup / barcode-aware search" row (backend gap:
    `SearchTerm` doesn't match `Barcode`) is unrelated and still accurate; no change was needed
    there.
  - Re-verified clean: `frontend/inventory` typecheck/lint/test(15/15)/build(11/11 routes) all
    pass.

### Remaining tasks (unchanged from §I, still blocked on a `dotnet` + Docker environment)

Items 1–4 and 6 from §I's "Remaining tasks" list are unchanged. Item 5 (thermal printing +
barcode generation) is now **fully shipped** on the frontend side — thermal receipt printing in
the prior session, barcode label generation this session. The only related item still open is the
backend piece already tracked in `docs/API-GAPS.md` (wiring `Barcode` into the search filter, or
exposing `GetByBarcodeAsync`), which needs `dotnet` to build/verify and was out of scope for a
`node`-only session.

### Exact next command

**If continuing in a `node`-only sandbox** (no `dotnet`): there is no further frontend-only item
queued in §I's original list — everything reachable without a backend has now been shipped
(receipt printing, barcode generation). Confirm with the user before picking new frontend-only
scope; don't invent new feature work not requested.

**If continuing with `dotnet` + Docker available** (needed for everything else on this list):
```
cd enterprise-pos-inventory
docker-compose up -d
dotnet restore EnterprisePOS.sln && dotnet build EnterprisePOS.sln
```
Then follow `docs/ROADMAP-v3.0.md` Phase 1 exit criteria before starting Phase 2 (auth/tenancy),
and pick up the barcode-search backend gap in `docs/API-GAPS.md` as a small, high-value fix along
the way.

### Suggested commit message for this session

```
feat(inventory): add barcode label generation (form preview, list action,
detail print); test: enable automatic JSX runtime for vitest

- package.json: add jsbarcode (runtime) and @types/jsbarcode (dev)
- features/products/components/BarcodeLabel.tsx: new — Code128 SVG barcode
  renderer for Product.Barcode, with empty/error states
- features/products/components/BarcodeLabelModal.tsx: new — print-friendly
  label modal, reusing the Receipt.tsx print-CSS pattern
- features/products/components/ProductForm.tsx: live barcode preview
- app/products/page.tsx: per-row "Label" action when a barcode exists
- app/products/[id]/page.tsx: "Print barcode label" header action
- vitest.config.ts: esbuild.jsx = "automatic" (first .tsx test file in the
  project needed this; classic transform failed with "React is not defined")
- features/products/__tests__/BarcodeLabel.test.tsx: new, 3 tests
- AI-HANDOVER.md: §C checklist row + new §J session addendum

Verified: frontend/inventory passes typecheck/lint/test(15/15)/build(11/11
routes). frontend/pos and services/* untouched this session.
```

## K. Frontend-only checkpoint — 2026-08-30 (read this first if you are the next agent)

This section is the authoritative summary of what a `node`-only sandbox (no `.NET` SDK, no
Docker) was able to complete and verify across this and the prior session, and exactly what is
blocked pending a capable environment. It supersedes §I/§J for a quick status check; §I/§J remain
as the detailed session-by-session record.

### Completed and verified (frontend-only, real build/test/runtime evidence)

- **POS thermal receipt printing, 58mm/80mm paper profiles** — `TerminalConfig.receiptPaperWidthMm`,
  Setup page selector, `@page`/font-size print profiles in `Receipt.tsx`. Verified:
  `frontend/pos` install/typecheck/lint/test(9/9)/build(7/7 routes) — see §I.
- **Inventory barcode label generation** — live form preview, list "Label" action, detail/edit
  "Print barcode label" button, via `jsbarcode` (Code128 SVG). Verified: `frontend/inventory`
  install/typecheck/lint/test(15/15)/build(11/11 routes) — see §J.
- Both features are committed: `5d80db6` (receipt printing) and `c0e7237` (barcode generation) on
  `main`. `git log --oneline -20` shows the full recent history including these two commits.

### Not started / blocked by environment

The following require `.NET` SDK, Docker, Docker Compose, and PostgreSQL access, none of which
exist in this sandbox (`which dotnet docker docker-compose` all fail). **None of this has been
attempted, and no backend build/test/runtime result has been fabricated:**

```text
Blocked by environment:
- backend build/runtime verification (dotnet restore/build/test has never been run this session
  or the prior one — services/* has not been touched by either)
- API Gateway (existence/completeness not verified — needs inspection with dotnet available)
- auth-service integration into frontend/pos and frontend/inventory
- notification-service integration into either frontend app
- tenancy implementation/verification (server-side isolation cannot be tested without a running
  backend + database)
- licensing/subscription/trial engine (prior-session finding: does not exist in any form beyond a
  marker interface — needs re-verification against actual code once dotnet is available, not
  just trusted from this note)
- enterprise database seeding (needs `dotnet ef` against a live Postgres)
- backend barcode-search fix (`docs/API-GAPS.md`: `SearchTerm` does not match `Barcode` — fix is
  trivial but requires building/testing the actual service)
- Result-pattern standardization across services (requires inspecting actual service code with
  dotnet tooling)
- full Docker Compose stack verification
- i18n/localization (frontend and backend) — not started
```

### Next environment must provide

```text
.NET SDK (version — check global.json / .csproj TargetFramework once inspecting)
Docker
Docker Compose
PostgreSQL access (via docker-compose or otherwise)
Node.js/npm (already required and available; frontend work depends on it too)
```

### Exact next-agent instructions

Do repository inspection first — not blind implementation:

```bash
git status
git branch --show-current
git log --oneline -20
docker --version
docker compose version
dotnet --version
cd enterprise-pos-inventory
docker-compose up -d
dotnet restore EnterprisePOS.sln
dotnet build EnterprisePOS.sln
dotnet test
```

Then read, in this order, before touching any backend code:

```text
docs/ROADMAP-v3.0.md
docs/API-GAPS.md
AI-HANDOVER.md (this file, §K then §I/§J for detail)
```

Then inspect the actual source — existing `auth-service`/`notification-service` implementations,
domain models, migrations, and API contracts — to determine what genuinely exists vs. what the
docs claim, before designing or implementing anything. Proceed in the phase order already agreed
with the user: Phase 1 exit criteria → Phase 2 auth/tenancy → barcode-search fix → auth/notification
integration → licensing/subscription/trial engine → demo data seeding → full test/Docker
verification → documentation update. Report against each phase's exit criteria with real
build/test/runtime evidence before moving to the next.

---

## L. Phase 1 backend baseline — 2026-08-31 (read this first if you are the next agent)

**This is the first session with a real `.NET 10 SDK (10.0.400)` and Docker available.** Every
prior session's "cannot verify, no dotnet/Docker" caveat is now obsolete for anything covered
below. §K above is now superseded for backend status; it remains accurate for the frontend-only
history.

### Headline result

All four backend services (`auth-service`, `notification-service`, `pos-service`,
`inventory-service`) now:
- Build with **0 errors, 0 warnings** — including **0 unresolved NuGet security advisories**
  (three had been silently `<NoWarn>`-suppressed rather than fixed; all three are now actually
  fixed, see below).
- Pass their full test suites: 48 (Inventory) + 18 (POS) unit tests, 7 (Inventory) + 1 (POS)
  integration tests against a **real** Postgres/RabbitMQ/Redis stack, confirmed **repeatable**
  (ran twice back-to-back with no flakiness — this matters because two of the bugs below only
  reproduced on a *second* run against a populated database).
- Have real, tool-generated EF Core migrations (no more hand-authored migrations missing
  Designer.cs/ModelSnapshot pairs) applied to a live Postgres instance, schema-verified via `\dt`.
- Run as Docker containers (new images for `auth-service`/`notification-service`; existing ones for
  `pos-service`/`inventory-service` fixed) via a corrected root `docker-compose.yml`, all four
  passing `GET /health` → 200.
- Were smoke-tested end-to-end through a live container: created and listed a real product via
  `inventory-api`'s HTTP API (not a unit test — an actual `curl` against the Docker container),
  and confirmed `pos-api` correctly rejects an unseeded store/register with a structured
  `ProblemDetails` error rather than crashing.

**None of this was true at the start of the session.** Ten independent, previously-undetected bugs
were found and fixed to get here — several severe enough to crash any real deployment. Full detail
in `release-notes/release-notes.md`'s 2026-08-31 entry; summarized by severity:

**Would have crashed or silently misconfigured a real deployment:**
1. `InventoryService.API`/`PosService.API` used plain `Microsoft.NET.Sdk` instead of
   `Microsoft.NET.Sdk.Web` → `appsettings.json` never shipped in the build/publish output → every
   real run (Docker, published binary) started with zero configuration. **This is the single most
   severe bug found** — it means no one had ever actually run either core service from a
   Docker image or `dotnet publish` output before this session, only `dotnet run` from source.
2. MediatR's `ValidationBehavior<,>` was `AddSingleton` while depending on FluentValidation's
   Scoped `IValidator<T>` → every validated request throws in any environment with DI scope
   validation on, which `docker-compose.yml`'s `ASPNETCORE_ENVIRONMENT=Development` triggers for
   all four services.
3. `GetAllProductsHandler` dereferences `p.Category.Name`/`p.Brand.Name`/`p.Unit.Symbol` but
   `ProductRepository.GetPagedAsync` never `.Include()`d those navigations → `GET
   /api/v1/products` (the products list) throws `NullReferenceException` the instant any product
   exists. Only ever passed in earlier sessions because the test DB was empty.
4. `auth-service`'s design-time `IDesignTimeDbContextFactory` didn't match its runtime
   `AddDbContext` config (different DB name, missing `.MigrationsHistoryTable("__ef_migrations_history",
   "auth")`) → following the documented `dotnet ef database update` workflow leaves the app's own
   migration-history check empty, so it re-runs every migration against tables that already exist
   on first boot → guaranteed crash-loop. Reproduced live: `auth-api` was crash-looping (exit 139)
   until this was fixed and the (empty, no real data yet) `auth_service` database was recreated.
5. `notification-service` throws `CultureNotFoundException` on **every** request in its Docker
   image, including `/health` — the Alpine base image ships `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true`
   with no ICU data, and this service does real culture-aware work for its English/Bangla
   localization. Fixed with `apk add icu-libs` + turning invariant mode back off.

**Security (silently suppressed via `<NoWarn>`, not actually fixed, until now):**
6. `Microsoft.OpenApi` 2.0.0 (high, GHSA-v5pm-xwqc-g5wc) and `System.Security.Cryptography.Xml`
   9.0.0 (8 separate high-severity DoS advisories) were shipping in `inventory-service`'s and
   `pos-service`'s runtime output. Pinned to 2.7.5 / 9.0.18.
7. Both integration test projects pinned `Testcontainers.*` to a version pulling vulnerable
   `SSH.NET` (high, GHSA-q939-rpr3-3284), plus test-only `Azure.Identity`/`Microsoft.IdentityModel.*`
   vulnerabilities from `Microsoft.AspNetCore.Mvc.Testing`. All pinned to patched versions.

**Test infrastructure (masked real bugs by never actually running):**
8. `WebApplicationFactory<object>` in both services' integration test bases — `object` has no
   entry-point assembly, so every integration test threw `InvalidOperationException` before making
   an HTTP call. Fixed by adding `public partial class Program {}` to both `Program.cs` files (the
   standard ASP.NET Core fix) and referencing `Program` instead of `object`. **This is why bugs #2
   and #3 above were never caught before** — the tests that would have caught them couldn't run at
   all.
9. A hand-authored migration's seed data used `DateTime.Parse("2025-01-01T00:00:00Z")`, which
   despite the `Z` returns `DateTimeKind.Local` (classic .NET footgun) — fails against Postgres
   `timestamptz` outside the UTC timezone. Fixed with explicit `DateTimeKind.Utc` construction.

**Infrastructure-as-config:**
10. Root `docker-compose.yml` defined only infra containers (Postgres/Redis/RabbitMQ/Seq), no
    application services at all, referenced undeclared `redis-data`/`seq-data` volumes, and only
    ever created one of the four databases needed. Rewritten: new
    `scripts/postgres/init/01-create-databases.sh` creates all four DBs on first boot; all four
    services now run as containers with correct env var names (see next point). Two more
    per-service compose files had the *same* connection-string key bug exposed by #1's fix
    (`ConnectionStrings__DefaultConnection`, which neither `Program.cs` ever reads — the real key
    is `Database:ConnectionString`) and one had a wrong Docker build context (`context: .` instead
    of `context: ../..`, meaning the Dockerfile's repo-root-relative `COPY` paths would never
    resolve). `pos-service`'s standalone `docker-compose.yml` referenced a `rabbitmq` host with no
    `rabbitmq` service defined. `inventory-service`'s `Dockerfile` had `ENV
    ASPNETCORE_URLS=http+:8080` (missing `//`, a typo that happened to still partially resolve).
    `services/notification-service/Dockerfile`'s build stage needed `apk add libc6-compat` — its
    `Grpc.Tools`-generated `.proto` build step ships a glibc-linked `protoc` that can't run on
    Alpine's musl libc without it (fails with a misleading "No such file or directory").

**Lower severity, also fixed:** CORS `AllowedOrigins` on `pos-service`/`auth-service` pointed at
Angular/Vite ports (4200/5173) left over from a different template project; `notification-service`
had no CORS policy at all. All four now allow `localhost:3000`/`:3001` (the real Next.js apps'
ports). `.gitignore`'s `.env.*` pattern was silently excluding `.env.example` from every commit —
both frontend apps' example files (referenced by their own READMEs) had never actually been
committed in any session; fixed the pattern and recreated both files. Both frontend READMEs had
their example `NEXT_PUBLIC_*_API_URL` ports swapped relative to the rest of the repo's docs. Added
`launchSettings.json` for `InventoryService.API`/`PosService.API` (ports 5002/5001, matching every
other doc) — previously only `auth-service`/`notification-service` had one.

### What genuinely is NOT done — don't re-derive this from hope, read `docs/API-GAPS.md`

- **No API Gateway exists anywhere in the repo.** A prior message in this conversation's context
  assumed a YARP gateway had been added — it has not. `grep -ril yarp` across the whole repo
  returns nothing. This is genuinely Phase 3, not started.
- `auth-service`/`notification-service` are still not integrated into either frontend app — no
  login screen, no token storage, no notification bell/panel. `cashierId` in POS is still a raw
  pasted GUID.
- No license/subscription/trial/tenant-isolation code exists anywhere (re-confirmed, not
  re-checked exhaustively this session — the prior sessions' repo-wide grep finding stands).
- `notification-service`'s RabbitMQ `UpstreamBindings` config still lists bus-ticketing-domain
  event names (`booking.events`, `payment.events`) — harmless (nothing publishes to them) but
  copy-pasted from a template project, not real POS/Inventory event wiring. No inventory low-stock
  → notification wiring exists.
- Category/Brand/Unit/Warehouse/Store/Register CRUD still don't exist (per `docs/API-GAPS.md`,
  unchanged this session) — the products list smoke test above worked *because* the earlier
  `SeedInitialData` migration seeded 5 real units/categories/brands with known GUIDs; a real
  operator still has no UI or endpoint to create their own.
- Frontend apps were not touched, built, or pointed at a live backend this session.

### Exact commands to reproduce this session's verification

```bash
cd enterprise-pos-inventory

# Backend build/test
dotnet build EnterprisePOS.sln                    # expect 0 errors, 0 warnings
dotnet test EnterprisePOS.sln                     # expect 48+18 unit, 7+1 integration, all pass
cd services/auth-service && dotnet build AuthService.sln && cd ../..
cd services/notification-service && dotnet build NotificationService.sln && cd ../..

# Docker full stack
docker compose up -d                              # brings up postgres/redis/rabbitmq/seq + all 4 APIs
docker compose ps                                 # all should show "Up" / healthy
curl http://localhost:5100/health                 # auth-api      -> 200
curl http://localhost:5300/health                 # notification  -> 200
curl http://localhost:5002/health                 # inventory-api -> 200
curl http://localhost:5001/health                 # pos-api       -> 200

# Migrations (only needed against a fresh Postgres — the compose stack above
# auto-creates all 4 databases via scripts/postgres/init/01-create-databases.sh,
# but does NOT auto-apply migrations; run these once per fresh database)
export PATH="$PATH:$HOME/.dotnet/tools"   # dotnet-ef global tool
dotnet ef database update --project services/pos-service/src/PosService.Infrastructure --startup-project services/pos-service/src/PosService.API
dotnet ef database update --project services/inventory-service/src/InventoryService.Infrastructure --startup-project services/inventory-service/src/InventoryService.API
dotnet ef database update --project services/auth-service/src/AuthService.Infrastructure --startup-project services/auth-service/src/AuthService.Api
dotnet ef database update --project services/notification-service/src/NotificationService.Infrastructure --startup-project services/notification-service/src/NotificationService.Api
```

### Exact next command

Per `docs/ROADMAP-v3.0.md`'s "Delivery Order", the next items are (2) Auth + tenant isolation and
(3) Gateway + rate limiting + resilience — both genuinely not started. Recommended order, since a
gateway is much simpler to design correctly once you know what it's routing auth context *to*:

1. **Design and build the YARP API Gateway** (new service, `services/gateway/` or
   `gateway/` at repo root — decide based on whether it should be versioned/deployed like the other
   four services). Routes to all 4 services, correlation ID propagation (the pattern already
   exists in `shared/shared-infrastructure` — reuse it), consistent error mapping, health-aware
   routing. This is pure addition, no risk to existing services.
2. **Wire `auth-service` into both frontend apps** — shared JWT/refresh-token client, auth Redux
   slice, route guards, derive `cashierId`/`userId` from the token instead of a pasted GUID.
3. **Then** tenant isolation (`TenantId` on aggregates, tenant-scoped repositories, cross-tenant
   IDOR tests) and the licensing/subscription/trial engine described in this conversation's
   opening prompt (3-day trial, POS-only/Inventory-only/combined plans, product-count-based
   tiers) — this is real new domain modeling, not integration, and should follow tenant isolation
   since entitlements are naturally tenant-scoped.

Do NOT re-verify Phase 1 from scratch — it's done and documented above with real evidence. Do
re-run the build/test commands above once before starting new work, to confirm nothing regressed
between sessions.

---

## M. API Gateway shipped — 2026-08-31 (same session as §L)

Built `services/gateway` (`Gateway.Api`, YARP 2.3.0, its own `Gateway.sln`) per
`decisions/ADR-008-api-gateway.md` — read that ADR first, it has the full rationale and exactly
what was deliberately deferred (auth/tenant propagation, circuit breakers, retries) and why.

**Verified real, not just "should work":**
- `dotnet build Gateway.sln` → 0 errors, 0 warnings, 0 vulnerable packages.
- `dotnet test Gateway.sln` → 3/3 hermetic tests pass (health, 404 on unmatched route, metrics).
- `docker compose build gateway-api` → succeeds; `docker compose up -d` → all 5 API containers
  (gateway + 4 services) healthy.
- Real routing verified through the **running Docker container** (not a mock): `GET
  localhost:5010/api/v1/products` returned a real (empty) paged result from `inventory-api`; `POST
  localhost:5010/api/v1/auth/login` reached `auth-api` and got its real validation response; `GET
  localhost:5010/health/services` returned all 4 downstream services as Healthy via YARP's active
  health checks.
- Port **5010**, not 5000 — macOS's AirPlay Receiver claims 5000 by default; using it would break
  `docker compose up` out of the box on every Mac. Discovered by hitting exactly that conflict.

**Also fixed in the same pass:** `SharedInfrastructure.Logging.SerilogConfiguration.CreateLogger`
and both `auth-service`/`notification-service`'s inline Serilog setup never actually wired the
`Serilog.Sinks.Seq` sink, despite the `enterprise-seq` container running in every compose stack
since Phase J — nothing had ever shipped logs to it. Added an optional `Seq:Url` config key
(same "optional, falls back gracefully" pattern as the existing OTLP tracing endpoint) to all
five services now (four existing + gateway). Rebuilt and reconfirmed all four existing services
still build 0/0 after this change.

**Update, same session:** both frontend apps have now been repointed at the gateway too —
`frontend/inventory/.env.example` and `frontend/pos/.env.example` both default to
`http://localhost:5010`. Verified with real running `npm run dev` servers for both apps
(browser-automation skill: page loads, 0 console errors, 0 failed requests, real data round-tripped
through gateway → inventory-service) plus the full typecheck/lint/test/build loop for both apps —
all green, no regressions. `npm install` on both apps surfaced 8 pre-existing frontend dependency
vulnerabilities (esbuild/postcss/sharp, via vite and Next.js's own transitive deps) — **not fixed
this session**: the only available fix bumps Next.js to 16.3.3, a major-version upgrade across both
apps that needs its own dedicated pass with full re-verification, not a drive-by fix. Tracked here,
not silently ignored.

### What's genuinely still not done (don't re-derive from hope)

- No auth/tenant context propagation at the gateway (see ADR-008 — this correctly follows, not
  precedes, actual auth/tenant work being done elsewhere first).
- No circuit breaker, retry policy, or explicit request-size/timeout overrides configured — YARP
  supports all of these but none have a concrete failure scenario driving specific values yet.
- Gateway has not been chaos-tested (e.g. killing a downstream container mid-request) — the
  active-health-check wiring is real and verified *reachable*, but "does it actually route around
  a genuinely dead container without dropping in-flight requests" has not been exercised.
- Frontend dependency vulnerabilities (esbuild/postcss/sharp — see above) — needs a dedicated
  Next.js 15→16 upgrade pass for both apps, not attempted this session.

### Exact next command

Per `docs/ROADMAP-v3.0.md`'s Delivery Order, next is auth integration:

1. Wire `auth-service` into both frontend apps (shared JWT/refresh-token client, auth Redux slice,
   route guards, derive `cashierId`/`userId` from the token instead of a pasted GUID).
2. Then tenant isolation + the licensing/subscription/trial engine described in this
   conversation's opening prompt — see §L's "Exact next command" for the fuller breakdown.

---

## N. Auth integration for both frontend apps — 2026-08-31 (same session as §L/§M)

Item 1 above is now done. `auth-service` is wired into both `frontend/inventory` and
`frontend/pos`: a typed `lib/api/auth.ts` client, a `features/auth/slice.ts` Redux slice (login,
register-ready-but-no-UI-yet, logout, session hydration from localStorage, automatic
refresh-and-retry on 401 in `lib/api/client.ts`), a `/login` page in each app matching the existing
design system, and an `AppShell` route guard that redirects an unauthenticated visitor to `/login`
and back once signed in. POS's Setup page no longer has a manual "Cashier ID" text field — the
cashier is now the signed-in user's ID (`useAppSelector(s => s.auth.user?.id)`); Store/Register
remain manual GUIDs, which is a separate, still-open, correctly-documented backend gap
(`docs/API-GAPS.md` — no Store/Register CRUD exists).

### Verified real, browser-driven — not just curl or unit tests

Using the `browser-automation` skill against real running `npm run dev` servers for both apps,
pointed at the live Docker gateway/backend stack:
- Registered a real user via `POST :5010/api/v1/auth/register` (`demo@enterprise-pos.test` /
  `P@ssw0rd123!` — this account now exists in the dev `auth_service` database; harmless to leave,
  useful for future manual testing).
- **Inventory**: filled and submitted the real login form → redirected to `/` → sidebar shows the
  real fetched name ("Dana Owner") and email → clicked logout → toast shown, redirected to
  `/login` → visited `/products` directly while logged out → route guard redirected back to
  `/login` (confirms the guard works, not just the happy path).
- **POS**: same login flow → topbar shows the user and a logout button → Setup page's Cashier
  field shows the signed-in account's email, read-only, no GUID input anywhere on the page.
- 0 console errors, 0 real failed requests in every run (the `net::ERR_ABORTED` entries in each
  script's output are Next.js JS-chunk/HMR requests aborted by the client-side navigation that
  follows login/logout — a normal artifact of testing through fast client-side redirects, not a
  bug; nothing business-relevant failed).

### A real, previously-latent test-infra bug found and fixed along the way

Both apps' `npm test` **crashed on every test that touches `localStorage`** the first time one was
written (the new `features/auth/__tests__/slice.test.ts`, 9 tests each app) —
`TypeError: Cannot read properties of undefined (reading 'removeItem')`. Root cause: **Node.js
22+'s experimental built-in `--experimental-webstorage` global `localStorage`** (on by default in
this environment's Node 26) shadows jsdom's own `window.localStorage` with a non-functional stub
outside a `--localstorage-file`-backed process. No test before this session ever touched
`localStorage`, so nothing had surfaced it. Fixed for both apps: `cross-env NODE_OPTIONS=
--no-experimental-webstorage` prefixed onto the `test` npm script (new `cross-env` devDependency,
for Windows/Mac/Linux portability — matches this repo's stated cross-platform support), plus an
explicit `environmentOptions.jsdom.url` in `vitest.config.ts` (a related, smaller jsdom
opaque-origin gotcha, fixed defensively even though the Node flag alone was the actual fix).

### Verified (full definition-of-done loop, both apps, after all changes)

```
frontend/inventory: typecheck, lint, test (24/24 — 15 pre-existing + 9 new auth tests), build (12/12 routes)
frontend/pos:        typecheck, lint, test (18/18 — 9 pre-existing + 9 new auth tests), build (8/8 routes)
```

### What's genuinely still not done

- **No register/forgot-password/reset-password UI** in either app — `authApi.register` exists in
  the client but there's no `/register` page calling it yet. Login-only was the scope for this
  pass; the demo user above was created via a direct API call, not through a UI.
- **RBAC-aware UI** — every authenticated user sees the same UI regardless of role. `auth-service`
  has a full permissions/modules/roles admin API; nothing in either frontend app reads or acts on
  role/permission claims yet beyond "is signed in at all."
- **No tenant isolation anywhere** — a signed-in user's requests aren't scoped to a tenant/business
  in any way server-side. This is the load-bearing prerequisite for the licensing/subscription
  engine requested in this conversation's opening prompt, and is next.
- **`auth-service`'s own JWT issuer/audience/role defaults still carry bus-ticketing-template
  branding** (`iss: https://identity.bus-ticketing.local`, `aud: bus-ticketing-api`, new users
  default to role `"Customer"`) — cosmetic/internally-consistent (doesn't break token
  validation, since issuer/audience only need to match between issuing and validating code within
  the same service), but worth a cleanup pass before this is customer-facing. Not fixed this
  session — out of scope for a frontend integration pass.

### Exact next command

Per `docs/ROADMAP-v3.0.md`'s Delivery Order: tenant isolation, then the licensing/subscription/
trial engine (3-day trial, POS-only/Inventory-only/Combined plans, product-count-based tiers,
1000/2000 BDT pricing) described in this conversation's opening prompt. Suggested concrete first
step: add a `TenantId`-aware `IDbContextFactory`/query-filter pattern to `inventory-service` and
`pos-service` (the column already exists on every entity per ADR-006 — see `shared/shared-kernel/
src/ITenantEntity.cs` — it has just never been read or enforced), then design the
Tenant/Subscription/Plan domain as a new bounded context (likely a new `services/billing-service`,
matching this repo's one-service-per-solution pattern) before wiring entitlement checks into the
gateway or per-service middleware.

---

## O. Store/Register/Cashier CRUD, and a critical PosService entity-Id bug — 2026-08-31 (same session)

While preparing a "how to use this from scratch" guide, discovered that **the POS app was
completely unusable in a fresh deployment**: zero stores/registers existed anywhere and no
endpoint could create one (`REGISTER_NOT_FOUND` on every open-session attempt — confirmed, not
guessed). The Domain/Repository layers for `Store` and `CashRegister` already existed in
`pos-service`; only the Application (CQRS) and API (Controller) layers were missing. Added both:

- `POST/GET /api/v1/stores`, `POST/GET /api/v1/registers` — full CQRS slices
  (`PosService.Application/{Stores,Registers}/...`), matching the existing `CreateSale`-style
  pattern exactly (Result<T>, FluentValidation, `Problem()`-shaped errors).
- `POST /api/v1/cashiers/ensure` — a **new bridging concept**, not in the original plan: wiring
  auth into the POS frontend initially set `cashierId = <auth-service User.Id>`, but pos-service's
  `Cashier` is its own entity in its own database (ADR-001) with zero relationship to
  auth-service's identity model — every sale/session call failed `CASHIER_NOT_FOUND` the moment a
  real store/register existed to get past the earlier blocker. Fixed with a get-or-create endpoint
  keyed on `Username` = the signed-in user's email (idempotent, no new migration needed — reused
  the existing unique index on `Cashier.Username`). The Setup page now calls this once per
  save and stores the resulting pos-service `CashierId`, not the auth `User.Id`.
- Gateway routes added for all three (`/api/v1/stores`, `/api/v1/registers`, `/api/v1/cashiers`).

### A second critical, previously-undetected bug found while verifying the above

The **first** store ever created came back with `id: "00000000-0000-0000-0000-000000000000"` —
`Guid.Empty`. Root cause: `PosService.Domain.Common.BaseEntity`'s constructor never called
`Id = Guid.NewGuid()` (unlike `InventoryService.Domain.Common.BaseEntity`'s and
`SharedKernel.BaseEntity`'s equivalents, both of which do). **This affected every single entity in
pos-service** — Store, CashRegister, Cashier, Sale, SaleItem, CashSession, Customer, Payment — all
of them extend this base class. A second insert of any entity type would have violated its primary
key's uniqueness constraint. This bug had been flagged as a known discrepancy in a much older
handover entry ("worth a follow-up decision, deliberately not touched") but never actually fixed,
and — critically — **nothing had ever caught it**: `PosService.IntegrationTests` has exactly one
test (`HealthCheckTests`, added in a much earlier phase), no test ever exercised a real Postgres
insert-then-read-Id round trip for any POS entity until this session's manual verification of the
new Store endpoint surfaced it directly.

Fixed with a one-line constructor addition (`services/pos-service/src/PosService.Domain/Common/
BaseEntity.cs`), plus new regression assertions in `PosService.UnitTests/Domain/{StoreTests,
CashRegisterAndSessionTests}.cs` (`entity.Id.Should().NotBe(Guid.Empty)`, and that two instances
get different Ids) so this can't silently regress again.

### Verified real, browser-driven, full loop (not just curl)

Using the `browser-automation` skill against the live Docker gateway/backend stack: logged in as
the `demo@enterprise-pos.test` user created in §N, navigated to Setup, entered a real (freshly
created, via `curl`) store/register GUID pair, clicked "Save terminal identity" (which triggers the
new cashier-ensure call), then opened a cash session with a real opening balance. The topbar
correctly flipped from "NO CASH SESSION OPEN" to "SESSION OPEN · 500.00 OPENING", and a
"Cash session opened." toast appeared. Confirmed in Postgres directly: the `cash_sessions` row has
a real non-empty `id`, correctly linked `cashier_id`/`register_id`, and `status = 'Open'`.

**Two silly but real process mistakes along the way, both self-caught and fixed**: forgot to
rebuild the `gateway-api` and `pos-api` Docker images after adding the new routes/controller (the
first end-to-end attempt 404'd purely from stale container images, not a code bug) — rebuilt both,
re-verified, confirmed working. Worth remembering for whoever continues: **after any backend code
change, `docker compose build <service> && docker compose up -d <service>` before testing through
Docker** — `dotnet build` succeeding does not mean the running container has the new code.

### Verified (full backend suite, after all changes in §L/§M/§N/§O)

```
dotnet build EnterprisePOS.sln          0 errors, 0 warnings
dotnet test EnterprisePOS.sln           48 (Inventory) + 19 (POS, was 18 — +1 regression test) unit,
                                         7 (Inventory) + 1 (POS) integration — all pass
```

### What's genuinely still not done

- **No frontend picker UI for Store/Register** — still a paste-a-GUID field, now backed by a real
  API instead of nothing. Building the picker (a dropdown fed by `GET /api/v1/stores`) is a small,
  clearly-scoped next step if continuing frontend polish.
- **Cashier's `FullName` shows the user's email, not their real name**, in the one test run above —
  a timing issue (the Setup page's cashier-ensure call can fire before the async `GET /api/v1/auth/me`
  profile fetch from login finishes populating `firstName`/`lastName` in Redux state). Cosmetic
  only — `Username` (email) is what actually identifies the cashier — not fixed this session, low
  priority.
- Same "Not yet done" items as §L/§M/§N: no tenant isolation, no licensing/subscription engine, no
  RBAC-aware UI, no register/forgot-password UI, frontend dependency vulnerabilities need a Next.js
  major-version upgrade pass.

### Exact next command

Same as §N: tenant isolation, then the licensing/subscription/trial engine. Additionally now
unblocked and worth doing opportunistically: a Store/Register picker UI in the POS frontend's
Setup page (small, high-value, no backend work needed — the API exists now).

---

## P. Session summary and handoff — 2026-08-31 (end of session)

This session started from a repo where the last real verification was frontend-only (no
`dotnet`/Docker available in any prior session). This session had both, for the first time, and
used them to take the platform from "backend never actually run" to "all five services build,
test, migrate, run in Docker, and were exercised end-to-end through a real browser" — while
finding and fixing **13 previously-undetected bugs**, several severe enough to have blocked any
real deployment or usage. Full detail is in §L through §O above and in
`release-notes/release-notes.md`; this section is the one-page summary.

### What shipped this session, in order

1. **Phase 1 backend baseline** (§L) — all 4 pre-existing services (`auth`, `notification`, `pos`,
   `inventory`) build 0/0 (including 0 unresolved security advisories), all tests pass repeatably,
   EF migrations regenerated with real tooling and applied to a live Postgres, all 4 run in Docker
   with passing health checks. Ten bugs fixed to get here — see §L for the full list; the two most
   severe: `inventory-service`/`pos-service` used the wrong SDK so `appsettings.json` never shipped
   in any real build, and a MediatR pipeline behavior's DI lifetime bug broke every validated
   endpoint the moment scope validation was on (i.e. in the exact `Development` environment Docker
   uses).
2. **API Gateway** (§M) — `services/gateway` (new, YARP), routing to all 4 services, verified real
   end-to-end through the running container. Also fixed: `Serilog.Sinks.Seq` had never been wired
   on any service despite Seq running in every compose stack since a much earlier phase.
3. **Frontend repointed at the gateway** (§M/end) — both apps' `.env.example` now default to the
   gateway; verified with real browser-driven `npm run dev` sessions, 0 console errors.
4. **Auth integration** (§N) — both frontend apps now have real login/logout/session/route-guards
   against `auth-service`. Verified real, browser-driven, full loop (register → login → protected
   page → logout → route-guard redirect). Found and fixed a real test-infra bug along the way
   (Node.js 22+'s experimental `localStorage` global shadowing jsdom's).
5. **Store/Register/Cashier CRUD + a critical entity-Id bug** (§O) — the POS app was **completely
   unusable in a fresh deployment** before this (zero stores/registers existed, no endpoint could
   create one). Fixed, plus discovered along the way that `PosService.Domain.Common.BaseEntity`
   never generated an `Id` — every entity in `pos-service` was being inserted with `Guid.Empty`,
   which would have broken on the second insert of any entity type. Fixed with a one-line
   constructor change plus new regression tests. Verified with a full real browser-driven flow:
   login → create store/register via API → Setup page → cashier auto-resolved → open cash session
   → confirmed in Postgres with real, correctly-linked IDs throughout.
6. **`GUIDE.md`** — a start-from-zero usage walkthrough, every command in it actually run and
   verified this session, with an honest "Known limitations" section rather than glossing over
   gaps.
7. **`decisions/ADR-009-tenancy-and-licensing.md`** — a concrete, file-path-level design for the
   tenant isolation + subscription/licensing/trial engine this project's brief asks for (3-day
   trial, POS-only/Inventory-only/Combined plans, product-count-based tiers). **Design only, not
   implemented** — see below.

### What did NOT get built this session (in priority order for whoever continues)

1. **Tenant isolation + the licensing/subscription/trial engine.** Fully designed in ADR-009 with
   exact file paths, entity shapes, and build order — implementing it should not require
   re-deriving the design. This is the single most consequential remaining gap relative to this
   project's stated commercial goal (a licensed SaaS product) — every account today has permanent,
   unlimited access to everything.
2. **RBAC-aware UI.** `auth-service` has a full roles/permissions/modules admin API; neither
   frontend app reads or acts on role claims beyond "signed in or not."
3. **A "awesome," visually distinctive UI pass** on the core Inventory/POS screens (dashboard,
   product list, POS terminal). The login pages built this session are genuinely polished and
   match the existing design system; the pre-existing core screens (dashboard cards, product
   table, POS cart/checkout) were not redesigned this session — they were functional before this
   session and remain so, just not restyled. This is an open-ended, iterative design task better
   done with direct user feedback on specific screens than guessed at in one pass — flagging it
   honestly rather than claiming a redesign that didn't happen.
4. **Store/Register/Category/Brand/Unit picker UI** in the frontend (the backend APIs for
   Store/Register now exist as of this session; Category/Brand/Unit still have no CRUD API at all,
   only fixed seed data — see `docs/API-GAPS.md`).
5. **Frontend dependency vulnerabilities** (esbuild/postcss/sharp, 8 advisories) — fix requires a
   Next.js 15→16 major-version upgrade across both apps, correctly left for its own dedicated pass.
6. Register/forgot-password/reset-password frontend pages (backend endpoints exist, unused).
7. Notification-service integration into either frontend app (no bell/panel UI), and no real event
   wiring from inventory/pos into notification-service (e.g. low-stock alerts) — `notification-
   service`'s RabbitMQ bindings still reference bus-ticketing-template event names that nothing
   publishes to.

### Exact next command for whoever continues

```bash
cd enterprise-pos-inventory

# 1. Confirm nothing regressed since this session (should all still be true):
dotnet build EnterprisePOS.sln && dotnet test EnterprisePOS.sln
cd services/auth-service && dotnet build AuthService.sln && cd ../..
cd services/notification-service && dotnet build NotificationService.sln && cd ../..
cd services/gateway && dotnet build Gateway.sln && dotnet test Gateway.sln && cd ../..
docker compose up -d && sleep 15 && curl http://localhost:5010/health/services

# 2. Read the design before writing any code:
#    decisions/ADR-009-tenancy-and-licensing.md
#    Then implement in the order the ADR specifies: auth-service's Tenant entity + JWT claim
#    first (everything else depends on the claim existing), then services/billing-service,
#    then enforcement in pos-service/inventory-service, then frontend trial banner/upgrade page.

# 3. After each milestone: rebuild the affected Docker image(s) before testing through Docker
#    (docker compose build <service> && docker compose up -d <service>) — this session hit that
#    exact mistake twice (§O) and it produces a confusing 404 that looks like a routing bug.
```

Do not re-verify Phases described in §L–§O from scratch — they are done and documented with real
evidence. Do re-run the commands above once to confirm nothing regressed between sessions before
starting new work.

---

## Q. Cross-cutting foundation (M1) — 2026-09-03 / 2026-09-04

**Environment:** real .NET 10.0.400 SDK + Docker, all 5 services running in compose.

This session started the "production hardening" plan agreed with the user (all 5 tracks — full
localization, cross-cutting hardening, multi-tenancy, licensing, usability/barcode — plus offline
sync, depth-first, commit to `main`). The full plan is at
`~/.claude/plans/you-are-the-principle-deep-sprout.md`. **Milestone M1 (backend cross-cutting
foundation) is C1–C6 + C8 done; C7 remains.** Milestones M2–M10 not started.

### What shipped (7 commits, each independently verified)

| Commit | What |
|---|---|
| `e3dce39` | **C1** `shared/shared-web` leaf project + multi-error `SharedKernel.Result`/`Error` (optional `Field`, `Errors` list, `Failure(IEnumerable<Error>)`). `ApiResponse<T>`/`ApiFailureResponse`/`ResultEnvelopeMapper`/`ControllerBaseExtensions`/`MinimalApiResultExtensions`. Added to all 4 solutions. 23 Docker-free mapper tests. |
| `92126fb` | **C2** `SharedWeb.PlatformExceptionHandler` (`IExceptionHandler`) + `IExceptionMapper` — wired into inventory/pos/gateway; deleted both `GlobalExceptionHandler.cs`; middleware order harmonized (`CorrelationId → SerilogRequestLogging → UseExceptionHandler → …`). Scrubbed RFC7807 500, all-errors 400 for thrown `ValidationException`, 504/403/404/499 built-ins. |
| `7409bb3` | **C3** Every inventory/pos controller failure branch → `this.ToApiResult(result)` (the shared mapper). **Fixes the bug where FluentValidation field errors were silently discarded.** `ConfigurePlatformApiBehavior()` reshapes `[ApiController]` 400s too. `ApiErrorItem.Of()` normalizes field names (`Request.CostPrice`→`costPrice`). Success responses **unchanged** (raw). |
| `e673ef8` | **C4** notification-service: deleted its local `Result`/`Error`/`ApiResponse`; 28 files now `using SharedKernel;`; `.Message`→`.Description`; `ResultExtensions` is a thin adapter over `SharedWeb.MinimalApiResultExtensions`. |
| `f061cb3` | **C5** auth + notification: deleted both `ExceptionHandlingMiddleware.cs`; `AuthExceptionMapper`/`NotificationExceptionMapper`. All 5 services now emit one failure shape + one scrubbed-500. |
| `e1a3dcb` | **C6** `SharedWeb.PlatformLocalization` — `AddPlatformLocalization()`/`UsePlatformLocalization()` in all 5. `?lang`→`Accept-Language`→user-claim→`en`; cultures en/bn. `PlatformMessages[.bn].resx`. **Dockerfile fix: `apk add icu-libs` + `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false`** in inventory/pos/gateway/auth (was crash-looping exit 139 on `CultureNotFoundException`). Deleted notification `LocalizationMiddleware`, auth dead `ILocalizationService`. |
| `1956ffb` | **C8** `decisions/ADR-010-cross-cutting-web-layer.md`, `docs/programmers-guide/` (7 guides), root `MIGRATIONS.md` (+ terse AI cheat-block), `docs/API-CONTRACT.md`/`API-GAPS.md` updated to reality. |

### Root causes fixed

1. **All-errors validation was broken** (`inventory`/`pos`): the shared `ValidationBehavior`
   collected every FluentValidation failure into `Result.ValidationErrors`, but **every controller
   read only `result.Error.Code`** (`"VALIDATION_ERROR"`) and `result.Error.Description` (`null`)
   via `return Problem(...)`. Fixed in C3 by routing every failure through `ResultEnvelopeMapper`
   which emits `result.Errors`. Verified: `POST /api/v1/products {}` now returns all 3 errors with
   camelCase `field`s.
2. **3 incompatible `Result` types + 4 exception handlers + gateway had none** — consolidated onto
   `SharedKernel.Result` + `SharedWeb.PlatformExceptionHandler` (C1–C5).
3. **Localization crash in Docker (exit 139)**: `aspnet:*-alpine` runs globalization-invariant
   (no ICU); `new RequestCulture("en")` throws. `notification`'s Dockerfile already had the ICU
   fix; the other 4 did not. Added in C6.
4. **notification's `LocalizationMiddleware` set `CultureInfo.CurrentCulture =` statically**
   (leaks across requests on a pooled thread) — replaced by framework `UseRequestLocalization`
   (per-request, auto-restored).

### How it stays frontend-safe (the "bridge")

C1–C6 required **zero** frontend change. The failure envelope carries the new
`{success,message,errors[]}` **and** transitional RFC7807 aliases (`type`/`title`/`detail`/`status`),
so the existing `frontend/*/src/lib/api/client.ts` (`problem.detail ?? problem.title`) keeps
resolving — and validation 400s go from `detail:null` to a real per-field message. **Success
responses are still raw** (bare `Guid`/DTO/`PagedResult`/`204`). Verified: `frontend/inventory`
(24/24 tests) + `frontend/pos` (18/18) `typecheck`/`lint`/`test`/`build` all green at C3, unchanged
by C4–C6 (backend-only).

### Verification performed this session (all real)

```
dotnet build EnterprisePOS.sln / AuthService.sln / NotificationService.sln / Gateway.sln  -> 0 errors, 0 warnings (all)
dotnet test:
  SharedWeb.Tests            38   (mapper + exception handler + localization)
  InventoryService.UnitTests 48   + IntegrationTests 7   (real Postgres)
  PosService.UnitTests       19   + IntegrationTests 1
  Gateway.Tests               3
  NotificationService        27 unit + 5 integration (Testcontainers Postgres/RabbitMQ)
  AuthService                37 unit + 7/9 integration  (see PRE-EXISTING FAILURES below)
docker compose build (all 5) + up -d  -> all 5 containers Up + /health/services all Healthy
curl through gateway :5010:
  - 404 product / 404 sale       -> unified envelope + bridge aliases
  - POST /products {}            -> 400, ALL validation errors, camelCase fields
  - POST /products?lang=bn {}    -> message = "এক বা একাধিক ভ্যালিডেশন ত্রুটি হয়েছে।" (Bangla), all errors
  - POST /auth/login (bad creds) -> 401, envelope + title="The email or password is incorrect."
frontend/inventory + frontend/pos: npm ci + typecheck + lint + test + build  -> all green (24/24, 18/18)
```

### PRE-EXISTING failures (NOT caused this session — verified by `git stash` + test at HEAD `e673ef8`)

- `AuthService.IntegrationTests.AuthApiTests.Admin_ListPermissions_ReturnsSuccess` — a fresh
  "Customer" user calls `GET /api/v1/admin/permissions`, expects 200, gets 403. Needs an RBAC seed
  (or the test relaxed). **Fails identically before M1.**
- `AuthService.IntegrationTests.AuthApiTests.SecurityQuestions_ConfigureAndVerify_ReturnsSuccess` —
  posts a random question id, expects 204, gets 400. Needs a seeded security question. **Fails
  identically before M1.**

### What is NOT done

- **M1 C7 — success-envelope migration (the one coordinated frontend+backend change).** Move
  success responses into `{success:true, data:…}`, unwrap `body.data` in both
  `frontend/*/src/lib/api/client.ts`, update `InventoryService.IntegrationTests/Products/
  ProductsControllerTests.cs` (`ReadFromJsonAsync<ApiResponse<Guid>>`) + the auth/notification
  integration assertions that read raw success bodies. One sub-commit per endpoint group, per app,
  full frontend DoD + browser QA each. **Final sub-commit: drop the RFC7807 `title`/`detail`/
  `status` aliases from the failure body.** Mechanism: `ControllerBaseExtensions.ToApiResult` /
  `MinimalApiResultExtensions` already have a `wrapSuccess` flag (default false for MVC) — flip it.
- **M2** DB provider factory (config-switchable Postgres/SqlServer/MySQL/SQLite) — `shared`
  factory still `throw new NotImplementedException` for non-Postgres.
- **M3** structured file logging (`logs/runtime-errors`, `build-errors`, `query-logs`) + EF query
  interceptor + graceful dependency-failure logging + DB/Redis/RabbitMQ health checks on
  inventory/pos + rate limiting on inventory/pos.
- **M4** frontend localization (`next-intl`, en/bn, both apps).
- **M5** multi-tenancy (JWT bearer in pos/inventory, `Tenant` + `tenant_id` claim, EF query
  filters, cross-tenant tests). ADR-009 has the file-path plan.
- **M6** licensing/billing (`services/billing-service`, trial/plans/entitlements). ADR-009.
- **M7** Category/Brand/Unit/Warehouse CRUD + frontend pickers (kills "paste a GUID").
- **M8** barcode scan-to-sell + on-demand "today" report + cash-session GET.
- **M9** sales idempotency (`Idempotency-Key`, Redis). **M10** offline POS + sync.
- The auth domain messages + FluentValidation `.WithMessage("literal")` strings are English-only
  (localization is keyed-incremental — add a resx entry named after the `Error.Code`, or make the
  validator use `IStringLocalizer`; no handler change needed).

### NEXT AGENT COMMAND

```bash
cd ~/Downloads/porosh/enterprise-pos-inventory

# 1. confirm the baseline (should match this handover exactly)
dotnet build EnterprisePOS.sln && dotnet test EnterprisePOS.sln          # 0/0, 38+48+19 unit, 1+7 integ
cd services/gateway && dotnet build Gateway.sln && dotnet test Gateway.sln && cd ../..
cd services/notification-service && dotnet build NotificationService.sln && dotnet test NotificationService.sln && cd ../..   # 27+5
cd services/auth-service && dotnet build AuthService.sln && dotnet test AuthService.sln && cd ../..                            # 37 unit, 7/9 integ (2 pre-existing)
docker compose up -d && sleep 15 && curl http://localhost:5010/health/services                                                # all Healthy

# 2. read: ~/.claude/plans/you-are-the-principle-deep-sprout.md  (the full plan + progress)
#          decisions/ADR-010-cross-cutting-web-layer.md          (M1 design + the C7 spec)
#          docs/programmers-guide/api-response-contract.md

# 3. do M1 C7 first (finishes M1): flip wrapSuccess:true in ToApiResult call sites service-by-service,
#    each in the SAME commit as the matching frontend/<app>/src/lib/api/client.ts unwrap +
#    integration-test update + full frontend DoD + browser QA. Then M2 (DB provider factory).
```
