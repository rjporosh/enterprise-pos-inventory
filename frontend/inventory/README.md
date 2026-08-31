# Inventory — Enterprise POS & Inventory (frontend)

Admin/back-office app for product catalog and stock management. Built against `inventory-service`
only. See `docs/API-GAPS.md` at the repo root for exactly which endpoints exist and which don't.

## Purpose

- Manage the product catalog (create/edit/delete, pricing, SKU, barcode, reorder thresholds).
- Manage stock: receive (in), issue (out), adjust, and transfer between warehouses; see current
  stock levels with low/out-of-stock filtering.
- A dashboard with real counts (products, low-stock lines, out-of-stock lines) — no fabricated
  metrics.

## Setup

```bash
npm install
cp .env.example .env.local   # then set NEXT_PUBLIC_INVENTORY_API_URL to your running inventory-service
```

## Environment variables

| Variable | Required | Description |
|---|---|---|
| `NEXT_PUBLIC_INVENTORY_API_URL` | Yes | Base URL of `inventory-service` (e.g. `http://localhost:5002`). No default is baked in — the app throws a clear config error if unset rather than silently failing requests. |

## Development

```bash
npm run dev
```

## Production build

```bash
npm run build
npm start
```

## Testing / linting / type-checking

```bash
npm test         # vitest
npm run lint      # next lint (ESLint)
npm run typecheck # tsc --noEmit, strict mode
```

## Architecture

See `docs/inventory/ARCHITECTURE.md` for the full picture. Short version: Next.js App Router pages
dispatch Redux actions, redux-saga workers call a typed `lib/api/*.ts` client, results land back in
a Redux Toolkit slice, and the page renders off `useAppSelector`. See
`docs/inventory/PROGRAMMER-GUIDE.md` for the day-to-day patterns and
`docs/inventory/ADDING-A-CRUD.md` for a step-by-step guide to adding a new entity.

## Folder structure

```
src/
  app/                  # Next.js App Router pages (routing only — no business logic)
  components/
    ui/                 # Design system: Button, Input, Table, Modal, Toast, etc. + ui.css
    layout/             # AppShell, Sidebar
  features/
    products/           # slice.ts (state+saga), validation.ts, components/ProductForm.tsx
    stock/               # slice.ts (state+saga)
  lib/
    api/                 # client.ts (fetch wrapper), products.ts, stock.ts (typed endpoints)
    store/                # store.ts, hooks.ts, StoreProvider.tsx
```

## Known limitations (see docs/API-GAPS.md for full detail)

- Category/Brand/Unit/Supplier and Warehouse are entered as raw GUIDs in forms — there's no
  backend CRUD for these yet, so there's nothing to build a picker against.
- The dashboard only shows metrics computable from existing list endpoints (product/low-stock/
  out-of-stock counts). Revenue/sales data isn't available from inventory-service and isn't shown
  here — see the POS app's `/reports` page.
