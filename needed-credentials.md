# Needed Credentials & Environment Setup

**Updated 2026-08-31** — the environment constraint described below (no `dotnet`/Docker) is
**resolved as of this session**: `.NET 10 SDK (10.0.400)` and Docker are both available, and were
used to build/test/migrate/run all five backend services for real. See `AI-HANDOVER.md` §L–§P for
what that unlocked.

## Local URLs (once the stack is running — `docker compose up -d` from the repo root)

| Service | URL |
|---|---|
| API Gateway (single public entry point) | `http://localhost:5010` |
| auth-service (direct, bypassing the gateway) | `http://localhost:5100` |
| notification-service (direct) | `http://localhost:5300` |
| inventory-service (direct) | `http://localhost:5002` |
| pos-service (direct) | `http://localhost:5001` |
| frontend/inventory | `http://localhost:3000` |
| frontend/pos | `http://localhost:3001` |
| Postgres | `localhost:5432` (postgres/postgres, 4 databases: `pos_db`, `inventory_db`, `auth_service`, `notification_service`) |
| Redis | `localhost:6379` |
| RabbitMQ (AMQP / management UI) | `localhost:5672` / `http://localhost:15672` (guest/guest) |
| Seq (structured logs, all 5 services) | `http://localhost:5341` |

## Demo credentials

A demo account was created and used for verification this session, then left in the dev database
(harmless — see `AI-HANDOVER.md` §N/§O):

- Email: `demo@enterprise-pos.test`
- Password: `P@ssw0rd123!`

No demo Store/Register exists persistently — they were created and cleaned up during verification.
See `GUIDE.md` for the exact commands to create your own.

## Env vars (frontend, both apps' `.env.example`)

- `NEXT_PUBLIC_INVENTORY_API_URL` — defaults to the gateway (`:5010`)
- `NEXT_PUBLIC_POS_API_URL` (pos app only) — defaults to the gateway
- `NEXT_PUBLIC_AUTH_API_URL` — defaults to the gateway

## Third-party credentials not yet needed by any code in the repo

Payment/card-terminal provider, SMS gateway, email provider (SMTP/API key) — `notification-service`
has channel abstractions for Email/SMS/Push (`src/NotificationService.Infrastructure/Channels/`)
but no provider credentials are configured in this repo. Also not yet needed: any payment provider
for the subscription/billing engine designed in `decisions/ADR-009-tenancy-and-licensing.md` (not
yet built — that ADR explicitly treats real payment integration as out of scope for its first
implementation pass).
