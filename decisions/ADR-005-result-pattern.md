# ADR-005: Result Pattern and Error Handling Strategy

**Date:** 2026-08-10
**Status:** Accepted
**Decision Maker:** Principal Architect

## Context

The system needs a consistent way to represent success/failure outcomes across all use cases, without relying on exceptions for control flow.

## Problem

How should application-layer results be represented?

## Options Considered

### Option A: Exceptions for everything
- **Pros:** Built-in, familiar
- **Cons:** Performance overhead, exceptions for control flow is an anti-pattern

### Option B: Result Pattern (Selected)
- **Pros:** Explicit success/failure, type-safe, avoids exception overhead for expected failures, forces handling
- **Cons:** More code, requires understanding of pattern

## Decision

Use the **Result Pattern** from the shared kernel:

```csharp
// Non-generic
Result result = await Mediator.Send(new DoSomethingCommand());
if (result.IsFailure) { /* handle */ }

// Generic
Result<ProductDto> result = await Mediator.Send(new GetProductQuery(id));
if (result.IsSuccess) { var product = result.Value; }

// With validation errors
Result result = await Mediator.Send(command);
if (result.IsFailure && result.ValidationErrors.Any())
    // Return validation errors to client
```

**Rules:**
1. All Application layer methods return `Result` or `Result<T>`
2. Validation errors go through `ValidationBehavior` pipeline (FluentValidation)
3. Exceptions are reserved for truly exceptional/unexpected scenarios
4. Global exception handler converts unhandled exceptions to ProblemDetails
5. Business rule violations return `Result.Failure(error)`

## Consequences

- **Positive:** Explicit, testable, consistent
- **Negative:** Requires discipline; developers used to throwing exceptions need to adapt
