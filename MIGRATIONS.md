# Database migrations — exact commands (run from the repo root)

EF Core, PostgreSQL. One `dotnet-ef` install, then per-service `add` / `update` / `rollback`.
Every command below is copy-paste from the **repository root**.

```bash
# one-time
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools"          # (Windows: %USERPROFILE%\.dotnet\tools)

# the compose stack must be up so a Postgres is reachable
docker compose up -d postgres
```

## Per service — project paths

| Service | `--project` (Infrastructure) | `--startup-project` (API) |
|---|---|---|
| inventory | `services/inventory-service/src/InventoryService.Infrastructure` | `services/inventory-service/src/InventoryService.API` |
| pos | `services/pos-service/src/PosService.Infrastructure` | `services/pos-service/src/PosService.API` |
| auth | `services/auth-service/src/AuthService.Infrastructure` | `services/auth-service/src/AuthService.Api` |
| notification | `services/notification-service/src/NotificationService.Infrastructure` | `services/notification-service/src/NotificationService.Api` |
| billing *(M6, not yet built)* | `services/billing-service/src/BillingService.Infrastructure` | `services/billing-service/src/BillingService.Api` |

## Add a migration

```bash
dotnet ef migrations add <Name> \
  --project services/<svc>/src/<Svc>.Infrastructure \
  --startup-project services/<svc>/src/<Svc>.API
```
Example (inventory): `dotnet ef migrations add AddSupplier --project services/inventory-service/src/InventoryService.Infrastructure --startup-project services/inventory-service/src/InventoryService.API`

## Apply to the database

```bash
dotnet ef database update \
  --project services/<svc>/src/<Svc>.Infrastructure \
  --startup-project services/<svc>/src/<Svc>.API
```

## Roll back

```bash
# to a specific migration (everything after it is reverted)
dotnet ef database update <PreviousMigrationName> --project … --startup-project …
# undo the last-added migration that has NOT been applied
dotnet ef migrations remove --project … --startup-project …
```

## After any migration — rebuild the container

`dotnet ef` updates your local Postgres, not the Docker image. If you changed code too:
```bash
docker compose build <svc>-api && docker compose up -d <svc>-api
```

## Apply ALL services to a fresh database

```bash
for s in \
  "inventory-service/src/InventoryService.Infrastructure inventory-service/src/InventoryService.API" \
  "pos-service/src/PosService.Infrastructure pos-service/src/PosService.API" \
  "auth-service/src/AuthService.Infrastructure auth-service/src/AuthService.Api" \
  "notification-service/src/NotificationService.Infrastructure notification-service/src/NotificationService.Api"; do
  set -- $s
  dotnet ef database update --project "services/$1" --startup-project "services/$2"
done
```

---

## AI cheat-block (paste into an agent prompt — minimal tokens)

```
DB=EFCore/Postgres. From repo root. <svc>∈{inventory,pos,auth,notification,billing}.
Infra=services/<svc>/src/<Svc>.Infrastructure  Api=services/<svc>/src/<Svc>.API (auth/notification/billing use .Api)
add:    dotnet ef migrations add <N> --project <Infra> --startup-project <Api>
apply:  dotnet ef database update    --project <Infra> --startup-project <Api>
back:   dotnet ef database update <PrevN> --project <Infra> --startup-project <Api>
then:   docker compose build <svc>-api && docker compose up -d <svc>-api
verify: docker compose exec postgres psql -U postgres -d <svc>_service -c '\dt <schema>.*'
rule:   never hand-edit a migration's Designer.cs/ModelSnapshot; regenerate with --force if needed.
```
