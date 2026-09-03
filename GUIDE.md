# Getting Started — Enterprise POS & Inventory

This is a practical, start-from-zero walkthrough: get the whole platform running locally, create
your first account, add a product, and complete a sale. Everything in this guide reflects **real,
currently-working functionality** — it does not describe anything planned or half-built. For what
isn't built yet, see [Known limitations](#known-limitations) at the end, and `docs/API-GAPS.md` /
`AI-HANDOVER.md` for the full detail.

## What this is

A microservice-based Point-of-Sale and Inventory platform: four backend services (`auth`,
`notification`, `inventory`, `pos`) behind a single API gateway, and two independent frontend
apps (a back-office **Inventory** app and a cashier-facing **POS** app). Built with .NET 10 and
Next.js 15/React 19.

## Prerequisites

- **Docker** and **Docker Compose** (this is the easiest way to run everything — see
  `docker-compose.yml` at the repo root).
- **Node.js 20+** and **npm**, to run the two frontend apps.
- (Optional, only if you want to run a backend service outside Docker) **.NET 10 SDK**.

## 1. Start the backend

From the repo root:

```bash
docker compose up -d
```

This brings up Postgres, Redis, RabbitMQ, Seq (structured logs), and all five backend containers:
`auth-api`, `notification-api`, `inventory-api`, `pos-api`, and `gateway-api` — the single public
entry point on **`http://localhost:5010`**.

Give it about 15–20 seconds on first run (Postgres needs to initialize four databases). Check
everything is healthy:

```bash
curl http://localhost:5010/health/services
```

You should see `"status":"Healthy"` for all four services. If a container isn't healthy, check its
logs: `docker logs <container-name>` (e.g. `docker logs pos-api`).

**First time only — apply database migrations.** `docker compose up` creates empty databases but
does not run EF Core migrations. With the .NET 10 SDK installed:

```bash
dotnet tool install --global dotnet-ef   # one-time
export PATH="$PATH:$HOME/.dotnet/tools"

dotnet ef database update --project services/inventory-service/src/InventoryService.Infrastructure --startup-project services/inventory-service/src/InventoryService.API
dotnet ef database update --project services/pos-service/src/PosService.Infrastructure --startup-project services/pos-service/src/PosService.API
dotnet ef database update --project services/auth-service/src/AuthService.Infrastructure --startup-project services/auth-service/src/AuthService.Api
dotnet ef database update --project services/notification-service/src/NotificationService.Infrastructure --startup-project services/notification-service/src/NotificationService.Api
```

The inventory migration also seeds a small set of reference data (units, categories, brands, a
default warehouse) — see [Seeded reference data](#seeded-reference-data) below.

## 2. Start the two frontend apps

```bash
cd frontend/inventory && npm install && cp .env.example .env.local && npm run dev
```

By default this runs on **`http://localhost:3000`**. In a second terminal:

```bash
cd frontend/pos && npm install && cp .env.example .env.local && npm run dev -- -p 3001
```

This runs on **`http://localhost:3001`**. Both `.env.local` files already point at the gateway
(`:5010`) by default — no editing needed for a local Docker setup.

## 3. Create your first account

Open **`http://localhost:3000`** (or `:3001`) — you'll land on a sign-in page. There's no sign-up
page yet (see [Known limitations](#known-limitations)), so create your first account directly
against the API:

```bash
curl -X POST http://localhost:5010/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"owner@yourshop.test","password":"YourP@ssw0rd1","firstName":"Your","lastName":"Name"}'
```

Now sign in with that email/password on either app's login page. You stay signed in across both
apps and across page reloads (the session is remembered in your browser) until you sign out.

## 4. Inventory — add your first product

Go to `http://localhost:3000/products` → **New product**.

Every product needs a Category, Brand, and Unit. There's no picker for these yet — paste one of
the seeded GUIDs below into the corresponding field (or see
[Seeded reference data](#seeded-reference-data) to add more via the API):

| Field | Example value to paste |
|---|---|
| Category | `20000000-0000-0000-0000-000000000001` (All) |
| Brand | `30000000-0000-0000-0000-000000000001` (Generic) |
| Unit | `10000000-0000-0000-0000-000000000001` (Piece) |

Fill in name, SKU, cost/selling price, and save. The product now appears in your catalog and in
POS's product search at checkout.

**Receiving stock**: go to **Stock → Stock In**, pick your product, enter a quantity and the
warehouse GUID `40000000-0000-0000-0000-000000000001` (the seeded "Main Warehouse"), and submit.
Your product now has stock on hand.

## 5. POS — set up a terminal and make a sale

POS needs a **Store** and a **Register** to exist before you can open a cash session. There's no
picker UI for these yet either — create them once via the API (you only need to do this once per
store/register, not per sale):

```bash
STORE_ID=$(curl -s -X POST http://localhost:5010/api/v1/stores \
  -H "Content-Type: application/json" \
  -d '{"name":"Main Store","code":"MAIN","currency":"BDT"}' | tr -d '"')

REGISTER_ID=$(curl -s -X POST http://localhost:5010/api/v1/registers \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"Register 1\",\"code\":\"REG-1\",\"storeId\":\"$STORE_ID\"}" | tr -d '"')

echo "Store: $STORE_ID"
echo "Register: $REGISTER_ID"
```

Now, in the POS app (`http://localhost:3001/setup`):

1. Paste the Store ID and Register ID you just created.
2. The **Cashier** field is filled in automatically from your signed-in account — nothing to type.
3. Click **Save terminal identity**.
4. Enter an opening cash amount and click **Open session**.

You're now ready to sell. Go to the terminal (`http://localhost:3001`), search for the product you
created in step 4 (by name or SKU), add it to the cart, and complete the sale. A receipt is shown
and can be printed directly from the browser (58mm/80mm thermal paper profiles are supported —
pick one on the Setup page).

At the end of the day, go back to Setup and **Close cash session**.

## Seeded reference data

The inventory database ships with a small set of starter reference data (from the
`SeedInitialData` migration) so you can create a product immediately without building a
Category/Brand/Unit picker UI first:

| Type | Name | Id |
|---|---|---|
| Unit | Piece | `10000000-0000-0000-0000-000000000001` |
| Unit | Kilogram | `10000000-0000-0000-0000-000000000002` |
| Unit | Liter | `10000000-0000-0000-0000-000000000003` |
| Unit | Box | `10000000-0000-0000-0000-000000000004` |
| Unit | Meter | `10000000-0000-0000-0000-000000000005` |
| Category | All | `20000000-0000-0000-0000-000000000001` |
| Category | Grocery | `20000000-0000-0000-0000-000000000002` |
| Category | Electronics | `20000000-0000-0000-0000-000000000003` |
| Category | Clothing | `20000000-0000-0000-0000-000000000004` |
| Category | Beverages | `20000000-0000-0000-0000-000000000005` |
| Brand | Generic | `30000000-0000-0000-0000-000000000001` |
| Brand | TechPro | `30000000-0000-0000-0000-000000000002` |
| Brand | StyleWear | `30000000-0000-0000-0000-000000000003` |
| Warehouse | Main Warehouse (default) | `40000000-0000-0000-0000-000000000001` |
| Warehouse | Branch Warehouse | `40000000-0000-0000-0000-000000000002` |

There's no CRUD API to add more of these yet — they only exist as fixed rows from the seed
migration (`docs/API-GAPS.md` tracks this as a real, open gap).

## Where to look when something isn't working

| What | Where |
|---|---|
| Is everything up? | `curl http://localhost:5010/health/services` |
| Structured logs (all 5 services) | Seq at `http://localhost:5341` (no login by default locally) |
| Live API docs for one service | Each service's own Scalar UI in Development mode, e.g. `http://localhost:5002/scalar` (inventory), `:5001/scalar` (pos), `:5100/scalar` (auth), `:5300/scalar` (notification) |
| Metrics (Prometheus format) | `http://localhost:5010/metrics` (gateway), or each service's own `/metrics` |
| RabbitMQ management UI | `http://localhost:15672` (guest/guest) |
| A specific request's trail across logs | Every response carries an `X-Correlation-Id` header — search for that value in Seq |
| Why did an API call fail? | Every failure is one shape: `{ "success": false, "message": "…", "errors": [ { "code", "field", "message" } ], "traceId", "timestamp" }` — **all** errors, not just the first. Unexpected 500s are scrubbed (no stack traces). See `docs/programmers-guide/api-response-contract.md`. |
| Bengali API messages | add `?lang=bn` or send `Accept-Language: bn` — e.g. `curl "http://localhost:5010/api/v1/products?lang=bn" -X POST -d '{}' -H 'Content-Type: application/json'`. English is the default/fallback. |

## Known limitations

Read honestly, not glossed over — this is a real, working platform, but it is not commercially
complete yet:

- **No self-service sign-up, no forgot-password/reset-password UI.** Accounts are created via a
  direct API call (step 3 above) or by a developer. The backend endpoints for forgot/reset
  password exist (`auth-service`) but no page calls them yet.
- **No picker UI for Category/Brand/Unit/Warehouse/Store/Register** — every one of these is a
  paste-a-GUID field or a direct API call, as shown above. The Store/Register/Category/Brand/Unit
  *data* is real and the APIs for Store/Register exist; only the frontend dropdown UI is missing.
- **No subscription, trial, or billing of any kind exists yet.** Every account has unlimited,
  permanent access to every feature. If you're evaluating this for a licensed/paid product, that
  entire layer (3-day trial, monthly plans, product-count limits) is still to be built — see
  `AI-HANDOVER.md` for the design notes and exact next steps.
- **No multi-tenant data isolation.** Every signed-in user currently sees the same shared product
  catalog, stores, and sales data — there's no concept yet of "your business's data" vs. "another
  business's data." This is a prerequisite for the licensing/subscription work above.
- **No role-based UI.** Every signed-in user sees the same screens regardless of role — there's no
  "cashier can't see cost price" or "only an owner can void a sale" enforcement in the UI yet
  (though `auth-service` has a full roles/permissions model ready to be wired in).
- **Barcode scanning isn't wired up.** Barcode *label generation/printing* works (Inventory →
  product → "Print barcode label"), but the POS search box matches by name/SKU only, not barcode
  — see `docs/API-GAPS.md`.
- **No returns/refunds, no purchase orders, no expense tracking.** Only the "sell what's in stock"
  flow exists today.
- **The frontend UI is English only.** Backend API messages localize to Bangla (`?lang=bn`), but
  the two Next.js apps have no language switcher yet — that's milestone M4.
- **No offline POS.** A network blip during checkout fails the sale; there is no local queue/sync
  yet — milestone M10.

## For developers continuing this project

Read, in this order: `AI-HANDOVER.md` (what's real, what was verified this session, exact next
commands), `docs/ROADMAP-v3.0.md` (the full phased plan and status), `docs/API-GAPS.md` (backend
contract vs. what the docs claim), `docs/AI-CODING-RULES.md` (conventions to follow).
