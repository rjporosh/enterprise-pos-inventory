# Needed Credentials & Environment Setup

Created during the 2026-08-28 session. This file did not exist before.

## Critical: this session's sandbox could not build or run the .NET backend

The agent working in this session had `node`/`npm` only — **no `dotnet` SDK, no NuGet access**
(the sandbox's network allowlist is npm/GitHub-registry domains only). That means:

- `services/auth-service`, `services/notification-service`, `services/inventory-service`,
  `services/pos-service` could **not** be restored, built, migrated, or run this session.
- Any backend work items (license/subscription engine, AuthService/NotificationService
  integration verification, demo-data seeding via EF) could not be authored *and verified* here.
  Writing unverified C# against a combined ~350-file, two-service codebase and calling it "done"
  would violate this repo's own verification rule (`NEXT-AI-PROMPT.md` item 9/10), so it was not
  attempted blind.
- **Recommendation**: continue backend work in an environment with `dotnet` + Docker (Claude Code
  desktop/terminal, or a local dev machine) so `dotnet build`, `dotnet ef database update`, and
  `docker-compose up` can actually run and be verified, per `AI-HANDOVER.md` §H's existing
  recommendation.

## Local URLs (once services are running via `docker-compose.yml` + each service's own launch profile)

| Service | Default URL | Source |
|---|---|---|
| inventory-service | see `services/inventory-service/src/*.Api/Properties/launchSettings.json` | not modified this session |
| pos-service | see `services/pos-service/src/*.Api/Properties/launchSettings.json` | not modified this session |
| auth-service | see `services/auth-service/src/AuthService.Api/Properties/launchSettings.json` | not modified this session |
| notification-service | see `services/notification-service/src/NotificationService.Api/Properties/launchSettings.json` | not modified this session |
| frontend/inventory | http://localhost:3000 (Next.js default) | `frontend/inventory/.env.example` |
| frontend/pos | http://localhost:3000 or next free port | `frontend/pos/.env.example` |
| Postgres / Redis / RabbitMQ / Seq | per `docker-compose.yml` at repo root | not modified this session |

## Demo credentials

**None exist yet.** No seed data for `auth-service` users/roles, and no enterprise demo dataset for
products/stock/sales, was created this session (see root-cause above — seeding requires running
EF migrations against a live Postgres instance via `dotnet`, which this sandbox cannot do).

When seeding is done, record here:
- Demo tenant name/id
- Demo admin user email + password (placeholder, never a real secret)
- Demo cashier user email + password
- Demo store/register/warehouse IDs

## Env var placeholders (frontend, already documented in each app's `.env.example`)

- `NEXT_PUBLIC_INVENTORY_API_URL`
- `NEXT_PUBLIC_POS_API_URL`
- Not yet added (needed once auth is wired into the frontend): `NEXT_PUBLIC_AUTH_API_URL`,
  `NEXT_PUBLIC_NOTIFICATION_API_URL`

## Third-party credentials not yet needed by any code in the repo

Payment/card-terminal provider, SMS gateway, email provider (SMTP/API key) — `notification-service`
has channel abstractions for Email/SMS/Push (`src/NotificationService.Infrastructure/Channels/`)
but no provider credentials are configured in this repo. Fill in here once a provider is chosen.
