# ADR-001: Service Boundaries

**Date:** 2026-08-10
**Status:** Accepted
**Decision Maker:** Principal Architect

## Context

The Enterprise POS & Inventory Management System requires two distinct, independently deployable backend services: a POS Service for checkout, sales, payments, and cash register operations, and an Inventory Service for products, stock management, purchase orders, and warehouse operations.

## Problem

Should the system be implemented as:
1. A single monolithic application with a shared database?
2. Two independently deployable services with separate databases?
3. A modular monolith with separate logical modules?

## Options Considered

### Option A: Monolithic Application
- **Pros:** Simpler deployment, single database, easier cross-cutting concerns
- **Cons:** Tightly coupled, harder to scale independently, long-term maintenance burden, violates microservices principles for large enterprise

### Option B: Two Independent Services (Selected)
- **Pros:** Independent deployment, independent scaling, clear service boundaries, aligns with enterprise architecture standards, each team can work independently
- **Cons:** Requires service-to-service communication, more complex deployment, duplicate infrastructure concerns

### Option C: Modular Monolith
- **Pros:** Good starting point, easier to split later
- **Cons:** Still has database coupling concerns, harder to enforce service boundaries strictly

## Decision

Implement as **two independently deployable backend services**:
- **POS Service** — Sales, payments, cash register, checkout
- **Inventory Service** — Products, stock, purchase orders, warehouse

Each service has its own:
- Database (`pos_db`, `inventory_db`)
- DbContext
- API endpoints
- Docker deployment

Communication between services uses:
- REST/HTTP for synchronous queries
- RabbitMQ events for asynchronous notifications (inventory → POS stock updates)

## Consequences

- **Positive:** Clear ownership, independent deployment, independent scaling
- **Negative:** Requires service-to-service communication infrastructure (RabbitMQ, Correlation IDs)
- **Mitigation:** Define explicit contracts and events in `shared/shared-kernel`
