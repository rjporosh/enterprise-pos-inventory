# ADR-003: PostgreSQL as Primary Database with Provider Abstraction

**Date:** 2026-08-10
**Status:** Accepted
**Decision Maker:** Principal Architect

## Context

The system must use PostgreSQL as the primary database but be designed to support future database providers (SQL Server, MySQL, Oracle) without rewriting business logic.

## Problem

How to implement database access so it's both PostgreSQL-first and provider-agnostic?

## Options Considered

### Option A: Direct Npgsql throughout
- **Pros:** PostgreSQL-specific features available
- **Cons:** Tightly coupled to PostgreSQL, hard to switch

### Option B: EF Core with IDbProviderFactory abstraction (Selected)
- **Pros:** Provider abstraction via IDbProviderFactory, business logic never touches database provider, configuration-driven
- **Cons:** Cannot use PostgreSQL-specific features without breaking abstraction

## Decision

Use **EF Core with IDbProviderFactory abstraction**:

```csharp
// Infrastructure/Persistence/IDbProviderFactory.cs
public interface IDbProviderFactory
{
    DbContextOptionsBuilder UseProvider(DbContextOptionsBuilder builder, string connectionString);
    string ProviderName { get; }
}

// Registration via configuration
services.AddDatabaseProvider(configuration, "Database:Provider");
```

- PostgreSQL is the primary and currently only supported provider
- Other providers throw `NotImplementedException` until implemented
- Connection string and provider name are configuration-driven
- DbContext options created via factory, injected into DbContext

## Consequences

- **Positive:** Can add SQL Server/MySQL/Oracle by implementing IDbProviderFactory without touching business logic
- **Negative:** Cannot use database-specific features in queries without violating abstraction
- **Mitigation:** Document database-specific features if needed; PostgreSQL is sufficient for MVP
