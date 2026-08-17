# ADR-004: POS ↔ Inventory Communication Strategy

**Date:** 2026-08-10
**Status:** Accepted
**Decision Maker:** Principal Architect

## Context

POS and Inventory are separate services with separate databases. They need to communicate when:
- A sale is made (inventory → POS: stock confirmation, POS → inventory: stock deduction)
- A stock adjustment happens (inventory → POS: stock update notification)
- A purchase order is received (inventory → POS: availability update)

## Problem

What communication patterns should be used between the two services?

## Options Considered

### Option A: Direct HTTP calls from POS to Inventory
- **Pros:** Simple, synchronous response
- **Cons:** Tight coupling, failure cascades, no retry/backoff, not idempotent

### Option B: RabbitMQ Event-Driven (Selected)
- **Pros:** Loose coupling, async processing, resilient to failures, supports retry/circuit breaker, supports idempotency via Correlation ID
- **Cons:** More complex, requires message broker, eventual consistency

### Option C: Hybrid approach
- **Pros:** Best of both worlds
- **Cons:** More infrastructure to manage

## Decision

Use **RabbitMQ as the primary async communication layer** with explicit event contracts:

```
Events Published:
  InventoryService → pos-service
    - StockDeductedEvent
    - StockAdjustedEvent
    - StockUpdatedEvent

Events Consumed:
  PosService ← inventory-service
    - StockDeductedEvent (confirm stock for sale)
    - ProductUpdatedEvent (invalidate POS cache)

Synchronous (when needed):
  POS → Inventory via REST (read operations only)
```

**Guarantees:**
- Every event has a Correlation ID
- Events are idempotent (processed once per event ID)
- Dead-letter queue for failed events
- Retry with exponential backoff

## Consequences

- **Positive:** Decoupled, resilient, scalable
- **Negative:** Requires message broker, eventual consistency
- **Mitigation:** Critical operations (sale completion) wait for inventory confirmation before closing transaction
