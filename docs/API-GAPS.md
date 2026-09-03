# API Gaps & Backend/Frontend Contract Notes

Generated from a direct reading of the backend source (controllers, DTOs, handlers, validators,
migrations) — not from `docs/API-CONTRACT.md`, `docs/MASTER-SPEC.md`, or `docs/ROADMAP.md`, which
describe a larger product than what's implemented. Where those documents and the real code
disagree, this file follows the real code, and the disagreement is called out explicitly below.

The frontend (`frontend/inventory`, `frontend/pos`) is built strictly against what's documented
here. It does not call, mock, or assume any endpoint not listed under "What exists."

---

## 1. Endpoint inventory (verified by reading the controllers directly)

### inventory-service

| Method | Path | Notes |
|---|---|---|
| POST | `/api/v1/products` | Returns the new product's `Guid` id, 200. |
| GET | `/api/v1/products/{id}` | `ProductDto`, 404 if not found. |
| GET | `/api/v1/products` | `PagedResult<ProductListItemDto>`. Filters: `pageNumber`, `pageSize`, `categoryId`, `brandId`, `isActive`, `searchTerm`, `sortBy` (name/sku/price/createdat), `sortDescending`. |
| PUT | `/api/v1/products/{id}` | 204. Body `id` must match route `id`. |
| DELETE | `/api/v1/products/{id}` | 204. |
| POST | `/api/v1/stocks` | Create a stock record for a product/warehouse. |
| GET | `/api/v1/stocks/{id}` | `StockDto`. |
| GET | `/api/v1/stocks` | `PagedResult<StockListItemDto>`. Filters: `productId`, `warehouseId`, `lowStock`, `outOfStock`. |
| PUT | `/api/v1/stocks/{id}` | Update reorder/max levels. |
| DELETE | `/api/v1/stocks/{id}` | 204. |
| POST | `/api/v1/stocks/in` | Stock-in movement. |
| POST | `/api/v1/stocks/out` | Stock-out movement. |
| POST | `/api/v1/stocks/adjustment` | Signed quantity adjustment. |
| POST | `/api/v1/stocks/transfer` | Move stock between two warehouses. |

### pos-service

| Method | Path | Notes |
|---|---|---|
| POST | `/api/v1/sales` | Opens a Draft sale, returns the sale's `Guid` id. |
| GET | `/api/v1/sales/{id}` | Full `SaleDto` (items + payments). |
| GET | `/api/v1/sales` | `PagedResult<SaleListItemDto>`. Filters: `storeId`, `cashierId`, `status`, `fromDate`, `toDate`. |
| POST | `/api/v1/sales/items` | Add a line item. Returns the new item's `Guid` id, 200. |
| DELETE | `/api/v1/sales/items` | Remove a line item. 204. Body-based delete (`{ saleId, saleItemId }`), not a path param. |
| POST | `/api/v1/sales/complete` | **204, no body.** Fetch the sale again via GET to get the finalized totals/receipt data. |
| POST | `/api/v1/sales/void` | 204. |
| POST | `/api/v1/cash-sessions/open` | Returns the new session's `Guid` id. |
| POST | `/api/v1/cash-sessions/close` | 204. |
| GET | `/api/v1/reports/daily-sales?storeId=&reportDate=` | 404 `REPORT_NOT_FOUND` if the report hasn't been generated yet (see §3 below — this is the normal case for "today"). |

### Response/error shape — UPDATED 2026-09-03 (M1 C1–C6)

- **Failure responses are now the unified envelope** `{ success:false, message, errors:[{code,field,
  message}], traceId, timestamp }` on **all 5 services** (was: 3 different shapes). **Every**
  validation error is returned — the old bug where `inventory`/`pos` returned
  `{title:"VALIDATION_ERROR", detail:null}` and dropped every field message is **fixed** (M1 C3).
  Transitional RFC7807 aliases (`type`/`title`/`detail`/`status`) are still on the failure body so
  the current frontend keeps working; removed in M1 C7.
- Messages are **localized** (en default, bn via `?lang=`/`Accept-Language`) — M1 C6.
- **Success responses are still the raw resource** (or `PagedResult<T>`, or `204`). M1 C7 wraps
  them as `{ success:true, data, … }` coordinated with `frontend/*/lib/api/client.ts`.
- `docs/API-CONTRACT.md` and `docs/programmers-guide/api-response-contract.md` now match reality.

**Severity: was Medium (doc drift) — now largely CLOSED.** Remaining: the success-envelope
migration (C7) and dropping the RFC7807 aliases.

---

## 2. Endpoints referenced by docs/MASTER-SPEC.md, ROADMAP.md, or FRONTEND-MASTER-PROMPT.md that
   **do not exist in the backend**

None of these are implemented by the frontend. Each is a documented, intentional gap.

| Missing capability | Why the frontend needs it | Expected shape (proposed) | Depends on | Priority |
|---|---|---|---|---|
| **Category / Brand / Unit CRUD** | `Product` requires `categoryId`, `brandId`, `unitId` as GUIDs, but there's no endpoint to list/create them. The Products form currently accepts these as raw GUID text input with an inline warning. This is the single biggest usability blocker for a non-technical shop owner. | `GET/POST /api/v1/categories`, same for `brands`, `units` — simple id+name(+symbol for unit) CRUD, list unpaginated (these are reference data, expected to be small). | None — additive. | **High** |
| **Warehouse CRUD** | `Stock` requires `warehouseId`. Same problem as above for every stock operation (in/out/adjustment/transfer) and for `Sale`'s implicit register→store→warehouse chain. | `GET/POST /api/v1/warehouses`. | None — additive. | **High** |
| **Store / Register CRUD — CLOSED 2026-08-31** | Was a hard blocker, not just friction: zero stores/registers existed anywhere and no endpoint could create one, so a fresh deployment's POS app was completely unusable (confirmed: `REGISTER_NOT_FOUND` on every open-session attempt, and manually inserting rows was the only workaround). `POST/GET /api/v1/stores` and `POST/GET /api/v1/registers` now exist (Domain/Repository layers already existed — only the Application/API layers were missing). The frontend's Setup page still has no picker UI (paste a GUID, or create one via the API directly) — that UI is the next, lower-priority step. | — | None — additive; verified real end-to-end (create store → create register → open cash session → complete two sales) via the running Docker stack + a real browser session. | Done (API); UI picker still open |
| **POS `cashierId` ≠ auth-service `User.Id` — found and closed 2026-08-31** | Wiring auth into the POS frontend initially set `cashierId = <authenticated user's auth-service User.Id>`, but pos-service's `Cashier` is its own entity in its own database (ADR-001) with no relationship to auth-service's identity at all — every sale/session call failed with `CASHIER_NOT_FOUND`. Fixed with a bridging endpoint, `POST /api/v1/cashiers/ensure` (get-or-create by `Username` = the user's email, idempotent), called once from the Setup page after a store is chosen; the frontend now stores the resulting pos-service `CashierId`, not the auth `User.Id`. | — | None — additive. | Done |
| **Authentication / current user — CLOSED 2026-08-31** | `services/auth-service` is now integrated into both frontend apps: `lib/api/auth.ts` (login/register/refresh/logout/me), a `features/auth/slice.ts` Redux slice with a saga, a `/login` page in each app, an `AppShell` route guard, and a 401-triggers-refresh-then-retry interceptor in each app's `lib/api/client.ts`. POS's `cashierId` is now derived from the signed-in user (`user.id`) — the manual "Cashier ID" GUID field is gone from the Setup page (Store/Register GUIDs remain manual — see the CRUD row below, unrelated and still open). Verified real, browser-driven end-to-end for both apps: register → login → protected page renders with the real user's name → logout → route guard redirects an unauthenticated visit to `/products` (Inventory) back to `/login`. | — | Needs `auth-service` running (Docker or `dotnet run`) at whatever URL `NEXT_PUBLIC_AUTH_API_URL` points to — the gateway (`:5010`) by default. | Done |
| **Barcode lookup / barcode-aware search** | `Product.Barcode` exists and is stored, but `GetAllProductsQuery`'s `SearchTerm` filter only matches `Name` and `Sku` (verified in `ProductRepository`, the `Where` clause is `Name.Contains || Sku.Contains`). There is an **unused** `GetByBarcodeAsync` method on the repository with no controller route calling it. A USB barcode scanner typing into the POS search box will only find a product if the barcode text happens to also match the name/SKU. | Either wire `GetByBarcodeAsync` to `GET /api/v1/products/by-barcode/{barcode}`, or add `Barcode` to the existing `SearchTerm` `Where` clause. The latter is a one-line change and is the recommended fix. | None — the repository method already exists. | **High** — cheap fix, real UX impact. |
| **On-demand / "today" daily sales report** | `DailySalesReportJob` is a background service that generates one `DailySalesReport` row per store per UTC calendar day, at UTC midnight, with a 7-day catch-up window on restart. There is no "generate now" endpoint. `GET /api/v1/reports/daily-sales?reportDate=<today>` will 404 with `REPORT_NOT_FOUND` for the entire current day, every day, by design. | A `POST /api/v1/reports/daily-sales/generate?storeId=&reportDate=` (idempotent, same underlying `GenerateIfMissingAsync`) that the frontend can call on-demand for "today," or a live/unmaterialized query path that computes the same aggregate without persisting it. | `DailySalesReportGenerator.GenerateIfMissingAsync` already contains the aggregation logic and just needs an HTTP trigger. | **High** — "how did today go" is a core daily question for a shop owner and currently cannot be answered same-day. |
| **Cash session GET (by id / list / "current open session")** | The frontend cannot ask the backend "is there an open session for register X" — it can only open/close blind. The POS app currently tracks the session it opened in `localStorage` as a local source of truth, which cannot detect a session opened on another device, and can drift if closed elsewhere. | `GET /api/v1/cash-sessions/{id}`, `GET /api/v1/cash-sessions?registerId=&status=Open`. | None — additive, `CashSessionDto` already exists in the DTOs. | **High** |
| Returns / partial refunds | `SalesController` only has `Void` (all-or-nothing, Draft/Completed → Voided). No partial-return or refund endpoint. | Out of scope for this MVP per the product brief — documented here for the roadmap, not blocking. | New domain concept. | Medium (deferred) |
| Receipt-specific endpoint | No dedicated receipt/print endpoint. The frontend builds a print-friendly receipt directly from the `SaleDto` returned by `GET /api/v1/sales/{id}`, which has enough data (items, payments, totals) for a basic receipt. | N/A — current `SaleDto` is sufficient for MVP receipts. | — | Low (not currently a gap) |
| Weekly/monthly/yearly/top-products-standalone/cashier/branch/profit/expense reports | Only `daily-sales` exists. | Out of scope for this MVP per the product brief. | New reporting endpoints. | Deferred |
| Multi-tenancy / branch switching, subscriptions/entitlements, offline sync | **Re-verified 2026-08-28, still true**: a repo-wide search for `subscription\|license\|tenant\|trial\|entitlement` across all four services finds only the pre-existing `BaseEntity`/`ITenantEntity` marker field — no License/Subscription/Plan/Trial entity, no tenant-scoping middleware, no entitlement checks anywhere, including in the newer `auth-service`/`notification-service`. `auth-service`'s "Modules" concept is a permission-grouping construct for RBAC, not a billing/entitlement module. | Out of scope for this MVP per the product brief; requires a from-scratch domain model + migrations if taken on. | Large, separate initiative — needs `dotnet` to build/migrate/verify. | Deferred |
| Notification service integration | `services/notification-service` now exists (added alongside `auth-service` in `0e79624`) with real `/api/v1/notifications` (send/list/get/cancel/retry/soft-delete), `/preferences`, `/templates` endpoints and Email/SMS/Push channel abstractions. Neither frontend app calls it — no in-app notification bell/panel, no low-stock or trial-expiry triggers wired from inventory-service/pos-service into it. | Add a notifications client + Redux slice + UI (bell icon, panel/list) to both apps; wire inventory-service low-stock events and any future trial/subscription events to call `POST /api/v1/notifications`. | `notification-service` needs `dotnet` to build/run/verify. | High (correctly documented as "not yet started" in `docs/ROADMAP-v3.0.md` Phase 10) |

---

## 3. Why the frontend is built the way it is, given the above

- **Category/Brand/Unit/Warehouse/Store/Register fields are plain GUID text inputs**, each with an
  inline hint explaining that management UI isn't available yet. This was a deliberate choice over
  fabricating a dropdown backed by fake data — a fake dropdown would look finished and hide a real
  blocker; a labeled GUID field is honest about the current limitation. This is the top usability
  issue for a first customer demo and should be the first backend follow-up (see §2, "High").
- **POS search box searches name/SKU, not barcode**, and says so in the UI, for the reason in §2.
  The input is still built to be scanner-friendly (auto-submits the first result on Enter) so it
  will work correctly the moment barcode matching is added server-side — no frontend change needed.
- **The Reports page defaults its date picker to yesterday**, not today, and shows an explicit
  "generated overnight" explanation with a `not-found` empty state rather than a misleading blank
  "$0 in sales" for today.
- **Cash session state is tracked client-side in `localStorage`** after a successful open/close,
  because there's no way to ask the server for current state. This is called out in
  `lib/api/cashSessionsAndReports.ts` and in the Setup page.
- **The POS cart is a pure client-side Redux slice** (`features/cart`) and is not synced to the
  backend line-by-line as items are added. A `Sale` (draft) is only created, and items only added
  via `POST /api/v1/sales/items`, at the moment the cashier presses "Complete sale" — the saga then
  creates the sale, adds each cart line, completes it, and re-fetches the sale for the receipt, in
  that order. This was chosen over "sync every cart edit to the backend" for two reasons: (1) it
  keeps the cashier-facing cart interaction instant with zero network round-trips while building an
  order, which matters for the "POS must feel fast" requirement, and (2) it avoids leaving orphaned
  Draft sales server-side for carts the cashier abandons or clears. The backend remains fully
  authoritative for the persisted sale — the frontend never fabricates a sale number, total, or
  change amount; all of those come from the `SaleDto` returned after `Complete`.
- **Setup page banner on the POS app** (updated 2026-08-31, was "DEMO / DEVELOPMENT ACCESS —
  AUTHENTICATION NOT YET PROVIDED"): now states plainly that only Store/Register still need a
  manually-pasted GUID (no picker UI yet, though the CRUD API exists as of 2026-08-31) — the
  cashier identity is resolved automatically from the signed-in account now that `auth-service` is
  integrated (via `POST /api/v1/cashiers/ensure`, not a text field).

---

## 4. Recommended backend priority order for Version 2

1. Wire `Barcode` into the existing `SearchTerm` filter (or expose `GetByBarcodeAsync`) — smallest
   change, real POS-speed impact.
2. Add a `POST /reports/daily-sales/generate` (or equivalent) trigger so "today" is answerable.
3. ~~Category / Brand / Unit / Warehouse / Store / Register minimal CRUD~~ — **Store/Register done
   2026-08-31** (`services/pos-service`); Category/Brand/Unit/Warehouse were already covered by
   seed data (`SeedInitialData` migration) rather than CRUD, which remains the real gap for a
   non-technical operator who needs to add a *new* category/brand/unit beyond the 5/5/3/2 seeded.
4. Cash session GET (by id, and "current open for register") — removes the localStorage-as-source-
   of-truth workaround.
5. ~~Authentication~~ — **done 2026-08-31**, integrated into both frontend apps (see
   `AI-HANDOVER.md` §N). Next: RBAC-aware UI and tenant isolation (see §L/§N's "exact next
   command").
