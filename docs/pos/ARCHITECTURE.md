# POS Frontend — Architecture & Programmer Guide

Same stack and conventions as the Inventory app (Next.js 15 App Router, React 19, TypeScript
strict, Redux Toolkit + redux-saga, hand-rolled `components/ui/` design system) — read
`docs/inventory/ARCHITECTURE.md` and `docs/inventory/PROGRAMMER-GUIDE.md` first; this file only
covers what's POS-specific.

## Feature responsibilities

| Feature | Responsibility | Backend calls |
|---|---|---|
| `features/session` | Terminal identity (store/register/cashier IDs, saved to `localStorage`) and cash session open/close lifecycle. | `POST /cash-sessions/open`, `POST /cash-sessions/close` |
| `features/catalog` | Product search for the checkout screen. | `GET /api/v1/products` (on inventory-service, read-only) |
| `features/cart` | **Pure client-side** line-item state (add/remove/quantity). No saga, no backend calls — see `docs/API-GAPS.md` §3 for why. Has full unit test coverage (`__tests__/slice.test.ts`) since it's the one feature with real client-side business logic (merging duplicate adds, quantity-to-zero removal, subtotal calc). |
| `features/sale` | The checkout pipeline (create sale → add each cart line → complete → re-fetch for receipt) and void. Renders `Receipt`. | `POST /sales`, `POST /sales/items`, `POST /sales/complete`, `GET /sales/{id}`, `POST /sales/void` |

## Cash session lifecycle

```
/setup page
  1. Save terminal config (store/register/cashier) -> configSaved -> localStorage
  2. Open cash session -> cashSessionOpenRequested -> POST /cash-sessions/open
     -> cashSessionOpenSucceeded -> stores { id, registerId, cashierId, openingBalance, openedAt }
        in Redux AND localStorage
  3. (later) Close -> cashSessionCloseRequested -> POST /cash-sessions/close
     -> cashSessionCloseSucceeded -> clears openSession from Redux + localStorage
```

The main terminal page (`app/page.tsx`) guards on `config && openSession` both being present — if
either is missing, it shows an `EmptyState` pointing to `/setup` instead of a broken checkout UI.

## Checkout pipeline (the core POS flow)

`features/sale/slice.ts`, `checkoutWorker`:

```
checkoutRequested({ saleHeader, lines, payments })
  1. saleId = yield call(salesApi.create, saleHeader)            // Draft sale
  2. checkoutStageChanged("adding-items")
  3. for each cart line: yield call(salesApi.addItem, {...})     // sequential, in cart order
  4. checkoutStageChanged("completing")
  5. yield call(salesApi.complete, saleId, payments)              // 204, no body
  6. completedSale = yield call(salesApi.getById, saleId)         // re-fetch for the real totals
  7. checkoutSucceeded(completedSale)
```

The UI shows a stage-specific button label ("Starting sale…" / "Adding items…" / "Completing
sale…") during this so the cashier isn't staring at a single frozen spinner during a multi-step
network operation. **Never show "success" before step 6 resolves** — if any step throws, the whole
`try` is caught, `checkoutFailed(message)` is dispatched, and the cart is left intact so the
cashier can retry rather than losing their work.

## Receipt

`features/sale/components/Receipt.tsx` renders directly from the `Sale` DTO returned by
`GET /sales/{id}` — every line, subtotal, discount, tax, total, paid, change, and payment method
shown is a real field from that response. Print support is `window.print()` plus a
`@media print` rule that hides everything except `.receipt-print-area`. No dedicated receipt
endpoint exists (see `docs/API-GAPS.md`).

## Daily report

`app/reports/page.tsx` calls `GET /reports/daily-sales?storeId=&reportDate=` directly (not through
a saga — it's a single simple read with its own local status state, consistent with how the
Inventory app's dashboard does its multi-call summary fetch). Defaults `reportDate` to yesterday.
A `404` from the backend is treated as the expected "not generated yet" case (`status:
"not-found"`), not an error — see `docs/API-GAPS.md` §2/§3 for why this is correct today-of-writing
behavior, not a bug.

## What's intentionally NOT here

- No authentication (see `/setup`'s demo-access banner).
- No returns/partial refunds — only full void.
- No offline mode.
- No barcode-specific lookup (search is name/SKU only — see `lib/api/catalog.ts` file comment).
