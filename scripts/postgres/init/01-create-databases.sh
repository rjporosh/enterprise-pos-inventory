#!/bin/bash
# Runs once, only against a brand-new (empty) postgres data volume, via the
# official postgres image's /docker-entrypoint-initdb.d convention.
#
# POSTGRES_DB (set in docker-compose.yml) creates exactly one database on
# first boot. This platform is 4 independently-owned microservice databases
# on one shared Postgres instance (dev/small-deployment topology) — this
# script creates the remaining three idempotently.
set -euo pipefail

EXTRA_DATABASES=("inventory_db" "auth_service" "notification_service")

for db in "${EXTRA_DATABASES[@]}"; do
  if [ "$db" = "$POSTGRES_DB" ]; then
    continue
  fi
  exists=$(psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -tAc "SELECT 1 FROM pg_database WHERE datname = '$db'")
  if [ "$exists" != "1" ]; then
    echo "Creating database: $db"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -c "CREATE DATABASE \"$db\";"
  else
    echo "Database already exists, skipping: $db"
  fi
done
