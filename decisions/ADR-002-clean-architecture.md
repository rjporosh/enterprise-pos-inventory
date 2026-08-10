# ADR-002: Clean Architecture + Vertical Slice + CQRS

**Date:** 2026-08-10
**Status:** Accepted
**Decision Maker:** Principal Architect

## Context

Enterprise POS & Inventory requires a backend architecture that is testable, maintainable, and follows .NET enterprise best practices.

## Problem

Which architectural pattern should be used?

## Options Considered

### Option A: Traditional N-Tier
- **Pros:** Familiar, well understood
- **Cons:** Tight coupling between layers, controllers know too much, hard to test

### Option B: Clean Architecture (Selected)
- **Pros:** Domain at center, infrastructure independent, highly testable, follows industry best practices
- **Cons:** More files/structure, initial learning curve

### Option C: Minimal API with MediatR
- **Pros:** Lightweight, fast to develop
- **Cons:** Loses architectural boundaries, hard to grow enterprise-wide

## Decision

Use **Clean Architecture with Vertical Slice and CQRS**:

```
Domain  → Pure business logic, no dependencies on Infrastructure
       → Entities, Value Objects, Domain Events, Interfaces

Application → Use Cases, Commands, Queries, Validators, DTOs
            → MediatR for CQRS dispatch
            → Depends on Domain only

Infrastructure → EF Core, Repositories, Event Bus, External services
               → Depends on Application and Domain

API  → Controllers, Middleware, OpenAPI
     → Entry point, maps to Application handlers
```

**Vertical Slice:** Each feature lives in its own feature folder within Application, containing Command, Query, Handler, Validator, and DTO in a single cohesive unit.

## Consequences

- **Positive:** High testability, clear separation of concerns, scalable
- **Negative:** More upfront structure, requires discipline
- **Pattern:** Each new feature = one folder under `Features/` with {Command|Query}/{FeatureName}
