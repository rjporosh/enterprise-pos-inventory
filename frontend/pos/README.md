# POS — Enterprise POS & Inventory (frontend)

Fast retail checkout app. Built against `pos-service`, with a read-only dependency on
`inventory-service` for product search. See `docs/API-GAPS.md` at the repo root for exactly which
endpoints exist and which don't.

## Purpose

- Set up a terminal (store/register/cashier identity — no login exists yet, see below) and open a
  cash session.
- Search products by name/SKU, build a cart, complete a sale with a payment method, and print a
  receipt.
- View sale history and void a completed sale.
- View the daily sales report (generated overnight by the backend — see the in-app note on that
  page and `docs/API-GAPS.md` §2/§3).

## Setup

```bash
npm install
cp .env.example .env.local
# set NEXT_PUBLIC_POS_API_URL and NEXT_PUBLIC_INVENTORY_API_URL to your running services
```

## Environment variables

| Variable | Required | Description |
|---|---|---|
| `NEXT_PUBLIC_POS_API_URL` | Yes | Base URL of `pos-service` (e.g. `http://localhost:5002`). |
| `NEXT_PUBLIC_INVENTORY_API_URL` | Yes | Base URL of `inventory-service`, used read-only for product search at checkout. |

## Development / build / test

```bash
npm run dev
npm run build && npm start
npm test
npm run lint
npm run typecheck
```

## Demo access (no authentication yet)

The backend has no login. The `/setup` page is explicitly labeled "DEMO / DEVELOPMENT ACCESS —
AUTHENTICATION NOT YET PROVIDED BY BACKEND" and asks for store/register/cashier IDs as raw GUIDs.
This is intentional, not a shortcut hiding a bug — see `docs/API-GAPS.md`.

## Folder structure

```
src/
  app/
    setup/            # terminal identity + open/close cash session
    page.tsx           # main terminal: search, cart, checkout, receipt
    sales/              # sale history + void
    reports/            # daily sales report
  components/ui/        # design system (duplicated from inventory app on purpose — see AI-CODING-RULES.md)
  components/layout/     # Topbar, AppShell
  features/
    catalog/              # product search slice+saga (read-only against inventory-service)
    cart/                  # pure client-side cart slice (no backend calls) + tests
    session/                # terminal config + cash session open/close (persisted to localStorage)
    sale/                    # checkout saga (create sale -> add items -> complete -> fetch for receipt) + void + Receipt component
  lib/api/                    # client.ts, sales.ts, cashSessionsAndReports.ts, catalog.ts
  lib/store/                   # store.ts, hooks.ts, StoreProvider.tsx
```

## Known limitations (see docs/API-GAPS.md for full detail)

- No barcode-specific search — the search box matches product name/SKU only.
- No "today" daily report — the backend generates reports overnight; the report page defaults to
  yesterday's date.
- Cash session state is tracked in `localStorage` after open/close since there's no GET endpoint
  for it — it can't detect a session opened on another device.
- Returns/partial refunds are not implemented (backend only supports full void).
