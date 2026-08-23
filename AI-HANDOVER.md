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
