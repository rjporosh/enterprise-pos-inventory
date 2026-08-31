# ADR-009: Tenant Isolation and Subscription/Licensing Engine (Design — Not Yet Implemented)

**Date:** 2026-08-31
**Status:** Proposed (design only — see "Implementation status" below)
**Decision Maker:** Principal Architect

## Context

This platform is meant to be sold as a licensed SaaS product, per this project's own brief:

- A business signs up and gets a **3-day full-feature free trial**.
- After the trial, they must **subscribe** to a monthly plan (example prices: 1000 BDT, 2000 BDT —
  configurable, not hard-coded).
- Plans can be **POS-only**, **Inventory-only**, or **Combined**.
- Plans limit **how many distinct product types** a business can carry (example: the 1000 BDT plan
  allows up to 100 product types; more requires a higher tier).
- Once the trial or subscription lapses, the business **loses access** until they pay.

None of this exists today. A repo-wide search confirms zero Tenant/Subscription/Plan/Trial
entities anywhere, in any of the four services. `ITenantEntity`/`TenantId` exists as a bare marker
column on every `pos-service`/`inventory-service` entity (ADR-006) but is never populated or
filtered on. `auth-service` has no tenant concept at all — every registered user is a flat,
tenant-less `User`.

This ADR is a **design document**, written after auth/gateway/frontend integration was completed
in the same session, to hand off a concrete, unambiguous implementation plan rather than leave the
next phase to be re-derived from scratch. **Nothing described below has been built yet** — see
"Implementation status."

## Problem

1. How does a "tenant" (a business/account) come into existence, and how does every downstream
   request know which tenant it belongs to?
2. Where does the Plan/Subscription/Trial domain live?
3. How is a plan's product-count limit enforced, given `inventory-service` owns product data and
   any new billing service would not?
4. How does the frontend show trial-remaining/subscription-expired state, and block usage when
   expired?

## Decision

### 1. Tenant lives in `auth-service`, created automatically at registration

**Model: one tenant per registering account** (the "single owner signs up" case this brief
describes — not a multi-business-per-login model, and not multiple staff logins sharing one
tenant *yet*; that's a natural but separate future extension, not blocking this design).

Add to `auth-service` (mirroring its existing self-contained `Entity`/`AggregateRoot` style — it
does **not** reference `shared-kernel`, confirmed this session; do not add a cross-cutting
dependency just for this):

```
services/auth-service/src/AuthService.Domain/Entities/Tenant.cs
  - Id (Guid)
  - BusinessName (string)
  - OwnerUserId (Guid)
  - CreatedAtUtc (DateTimeOffset)
```

Add `TenantId` (Guid) to `User` (non-nullable — every user belongs to exactly one tenant under
this model). `RegisterHandler` (`services/auth-service/src/AuthService.Application/Features/Auth/
Register/RegisterHandler.cs`) creates a new `Tenant` in the same transaction as the new `User`,
using a new required `businessName` field on `RegisterRequest`/`RegisterCommand`.

Add a `tenant_id` claim in `JwtTokenService.GenerateAccessToken` (`services/auth-service/src/
AuthService.Infrastructure/Security/JwtTokenService.cs`, alongside the existing `sub`/`email`/
`first_name`/`last_name`/role claims). This is the mechanism every downstream service uses to know
"which tenant is this request for" — no shared database, no synchronous cross-service lookup
needed per request.

Add `TenantId` to `TokenPairResponse` and `UserDto` so the frontend can display it if needed.

New migration: `dotnet ef migrations add AddTenant --project services/auth-service/src/
AuthService.Infrastructure --startup-project services/auth-service/src/AuthService.Api`.

**Also fix while touching `RegisterHandler`**: new users are currently assigned the seeded
`Customer` role unconditionally (`Role.WellKnown` in `AuthService.Domain/Entities/Role.cs`) — a
bus-ticketing-template leftover. For this product, the registering user should get an `Owner` (or
equivalent) role reflecting that they administer their own tenant. Whether to rename the seeded
role or add a new one is a small follow-up decision, not blocking this ADR.

### 2. Billing/Subscription is a new bounded context: `services/billing-service`

Matches this repo's one-service-per-solution pattern (own `BillingService.sln`, Clean
Architecture Domain/Application/Infrastructure/Api layers, own Postgres database
`billing_service`). Deliberately **not** folded into `auth-service` — billing/subscription
lifecycle (trial expiry jobs, payment webhooks eventually, plan changes) is a distinct concern
from identity, per ADR-001's service-boundary philosophy.

**Domain:**

```
Plan
  - Id, Code (e.g. "combined-1000"), Name
  - MonthlyPriceBdt (decimal) -- NOT hard-coded into logic anywhere; read from this table
  - IncludesPos (bool), IncludesInventory (bool)
  - MaxProductTypes (int) -- the cap this plan allows

Subscription
  - Id, TenantId (Guid, from the JWT claim -- no FK to auth-service's DB, cross-service by design)
  - PlanId
  - Status (enum: Trial, Active, Expired, Cancelled)
  - TrialEndsAtUtc (DateTimeOffset?) -- set only for Status = Trial
  - CurrentPeriodEndsAtUtc (DateTimeOffset?) -- set once a real paid period starts
  - CreatedAtUtc
```

**Seed data** (configurable rows, not hard-coded branches — per the brief's explicit "prices must
not be hard-coded into business logic"):

| Code | Name | Price (BDT) | POS | Inventory | Max product types |
|---|---|---|---|---|---|
| `trial` | 3-Day Trial | 0 | true | true | unlimited (e.g. `int.MaxValue`) |
| `pos-only-1000` | POS Only | 1000 | true | false | 100 |
| `inventory-only-1000` | Inventory Only | 1000 | false | true | 100 |
| `combined-1000` | Combined | 1000 | true | true | 100 |
| `combined-2000` | Combined+ | 2000 | true | true | 250 |

(These exact tiers/limits beyond the "1000 BDT → 100 product types" the brief specified are a
reasonable placeholder, not a business decision this ADR is authorized to lock in — confirm real
pricing with the user before treating the 2000 BDT tier's 250-product number as final.)

**Trial provisioning**: `auth-service`'s `RegisterHandler`, after creating the `Tenant`, publishes
a `TenantRegisteredIntegrationEvent { TenantId, RegisteredAtUtc }` to RabbitMQ (same
`RabbitMqSaleEventPublisher`/`SaleEventsConsumer` pattern already proven between `pos-service` and
`inventory-service`, ADR-004). `billing-service` consumes it and creates a `Subscription` row with
`PlanId = trial`, `Status = Trial`, `TrialEndsAtUtc = RegisteredAtUtc + 3 days`. This keeps
`auth-service` and `billing-service` decoupled — `auth-service` never needs to know billing exists
synchronously, and registration never fails or blocks on billing being reachable.

**Trial/subscription expiry**: a Quartz background job in `billing-service` (matching the existing
`DailySalesReportJob`/Quartz patterns already in this codebase), running e.g. hourly, flips any
`Subscription` with `Status = Trial` and `TrialEndsAtUtc < now` (or `Status = Active` and
`CurrentPeriodEndsAtUtc < now`) to `Status = Expired`.

**Entitlements endpoint** — the one synchronous call other services make:

```
GET /api/v1/entitlements/{tenantId}
  -> { isActive: bool, status: "Trial"|"Active"|"Expired"|"Cancelled",
       trialEndsAtUtc, planCode, includesPos, includesInventory, maxProductTypes }
```

### 3. Enforcement happens in the owning service, not the gateway

Per the brief's own exit criteria ("Server/domain enforcement", "No partial writes on quota
failure"), each service enforces its own constraints rather than trusting a gateway-level check
that could be bypassed by calling a service directly:

- **`inventory-service`**: extract `tenant_id` from the validated JWT (middleware, applied once —
  see `CorrelationIdMiddleware` for the existing pattern of a small piece of shared middleware
  duplicated per service). Stamp it onto every created `Product`/`Stock`/etc. (`TenantId` already
  exists as a column via `ITenantEntity` — just needs to actually be set, and an EF Core global
  query filter added so cross-tenant reads are structurally impossible, not just conventionally
  avoided). Before `CreateProductHandler` inserts a new product, call `GET /api/v1/entitlements/
  {tenantId}` on `billing-service`; if `!isActive` or `!includesInventory`, return a stable error
  code (`SUBSCRIPTION_INACTIVE` / `MODULE_NOT_ENABLED`); if the tenant's current distinct product
  count (`COUNT(*) FROM products WHERE tenant_id = @t`) `>= maxProductTypes`, return
  `PLAN_LIMIT_PRODUCT_COUNT_EXCEEDED` (the exact error code the roadmap names) **before** any
  write — no partial writes.
- **`pos-service`**: same `tenant_id` middleware and stamping. Before `CompleteSale`, check
  `isActive` and `includesPos` the same way (a sale itself doesn't need a product-count check —
  that's enforced at the point a product is *added to the catalog*, not at the point it's sold).
- **Caching the entitlements check**: calling `billing-service` synchronously on every write is a
  real latency/coupling cost. A short-TTL cache (e.g. 60s, in Redis — already provisioned for both
  services) is the standard mitigation; not required for a first correctness-focused pass, but
  flagged so it isn't missed before real load.

### 4. Frontend

- A trial-remaining banner (e.g. "2 days left in your trial") and a blocked-access screen once
  `isActive` is false, shown by each app after checking its own service's pass-through of the
  entitlements state (simplest: each service's own `/me`-equivalent or a dedicated response header
  echoes `X-Subscription-Status`, so the frontend doesn't need a third API call per page load).
- An upgrade/subscribe page listing `GET /api/v1/plans` (from `billing-service`) with a "Subscribe"
  action. **No real payment gateway integration is in scope of this ADR** — the brief doesn't name
  a specific payment provider, and Phase 12/14 of the roadmap treats "Payment" and "Card
  terminal/payment integrations" as separate, later items. A "Subscribe" button that marks a
  `Subscription` active (an operator/admin-triggered or manual-invoice flow) is the appropriate
  stand-in until a real provider is chosen — document it plainly as a stub, the same way the
  existing POS "DEMO / DEVELOPMENT ACCESS" banners were used honestly rather than faked.

## Implementation status

**Nothing in this ADR has been built.** This is a design handoff, written 2026-08-31 in the same
session that completed auth/gateway/frontend integration (see `AI-HANDOVER.md` §L–§O), so that
implementing this is a matter of following a concrete plan rather than re-deriving one. The
research behind auth-service's exact `User`/JWT/`RegisterHandler` structure (file paths, line
numbers) that this design depends on was gathered this session and is accurate as of this commit.

## Consequences

- **Positive**: tenant isolation and billing are additive to every existing service — no existing
  endpoint's request/response shape changes, only new required behavior (tenant stamping,
  entitlement checks) gated behind a JWT claim that doesn't exist in any token issued before this
  is built (meaning old tokens simply have no tenant — a reason to ship this before real customers
  exist, not after).
- **Negative**: this touches four of five services (new claim in `auth-service`, new consumer
  wiring in `billing-service`, new middleware + enforcement in `pos-service`/`inventory-service`)
  and needs careful, real verification at each step (per this session's own experience: assumptions
  about how services connect — e.g. "cashierId = auth User.Id" — turned out wrong when actually
  tested; do not skip end-to-end verification for any part of this).
- **Mitigation**: build and verify one service at a time, in the order listed in "Decision" above
  (auth-service's Tenant/JWT claim first — everything else depends on it existing).
