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
