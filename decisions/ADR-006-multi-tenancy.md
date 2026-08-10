# ADR-006: Multi-Tenancy and Audit Strategy

**Date:** 2026-08-10
**Status:** Accepted
**Decision Maker:** Principal Architect

## Context

The system must be multi-tenant ready from day one, with audit trails on all entities, even though the MVP is single-tenant.

## Problem

How should tenant isolation and audit data be stored and managed?

## Options Considered

### Option A: Separate database per tenant
- **Pros:** Complete isolation, maximum security
- **Cons:** Complex provisioning, not suitable for multi-tenant SaaS at scale

### Option B: Shared database, tenant_id column (Selected)
- **Pros:** Simple, scalable, aligns with PostgreSQL row-level security for future multi-tenancy, single schema
- **Cons:** Requires discipline to always filter by TenantId

### Option C: Shared database with Row-Level Security (RLS)
- **Pros:** Database-enforced isolation
- **Cons:** Adds PostgreSQL-specific coupling, harder to test

## Decision

Use **shared database with tenant_id column** approach:

```csharp
// All entities implement ITenantEntity
public interface ITenantEntity { Guid? TenantId { get; set; } }

// BaseDbContext applies tenant filter
public void SetTenantId(Guid? tenantId) => _tenantId = tenantId;

// All queries automatically filter by tenant
// Query: var sales = await _context.Sales.ToListAsync(); // implicitly filtered
```

**Audit fields** (on all entities):
- `created_at` (NOT NULL, default NOW())
- `created_by` (nullable)
- `updated_at` (nullable)
- `updated_by` (nullable)
- `is_deleted` (soft delete, default false)
- `deleted_at` (nullable)
- `deleted_by` (nullable)

**Future multi-tenancy path:**
- Add PostgreSQL RLS policies
- Configure connection routing per tenant
- TenantId already in schema — no migration needed

## Consequences

- **Positive:** Ready for multi-tenancy, audit trail built-in, no schema changes needed
- **Negative:** Must ensure all queries filter by tenant (automated via query filters)
