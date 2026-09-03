# Troubleshooting

## A service container crash-loops with exit 139 / `CultureNotFoundException`

The alpine base image runs in globalization-invariant mode. The service's Dockerfile `base` stage
needs:
```dockerfile
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
```
All five service Dockerfiles have this as of M1 C6. If you add a new service, copy it.

## A 404 / 400 looks like a routing bug right after a backend change

The Docker image is stale. `dotnet build` succeeding does **not** update the running container:
```bash
docker compose build <service> && docker compose up -d <service>
```
Do this after every backend code change before testing through the gateway. (This exact mistake
has been made repeatedly — see `AI-HANDOVER.md` §O.)

## `GET /api/v1/products` throws NullReferenceException once real data exists

Historic bug (fixed): `GetPagedAsync` must `.Include()` `Category`/`Brand`/`Unit` — the DTO
mapping dereferences those navigations.

## Validation error response has no per-field detail

Fixed in M1 C3. If you see `{title:"VALIDATION_ERROR", detail:null}` you are hitting a service
whose controllers were not migrated to `this.ToApiResult(result)` — migrate them.

## `dotnet ef` can't find the DbContext / wrong database

See [../../MIGRATIONS.md](../../MIGRATIONS.md) — always run from the repo root with both
`--project` and `--startup-project`.

## Auth integration tests: `Admin_ListPermissions` and `SecurityQuestions_ConfigureAndVerify` fail

Pre-existing (fail identically before M1). `Admin_ListPermissions` expects a fresh "Customer" user
to list admin permissions (needs RBAC seed or the test relaxed); `SecurityQuestions` posts a
random question id (needs a seeded question). Tracked in `AI-HANDOVER.md`; not caused by the
cross-cutting work.

## Where are the logs?

- Structured logs, all services: Seq at `http://localhost:5341` (search by `X-Correlation-Id`).
- Runtime-error files: `logs/runtime-errors/` (per service; full cross-service file logging is
  milestone M3).
- A request's trail: every response carries `X-Correlation-Id` — search that value in Seq.
