# Result pattern

There is **one** `Result` type: `SharedKernel.Result` / `SharedKernel.Result<T>`
(`shared/shared-kernel/src/`). `auth` and `notification` used to have their own — deleted in M1.

## Rules

1. Every Application-layer command/query handler returns `Result` or `Result<T>` — never `void`,
   never a bare DTO, never `throw` for an **expected** failure (not-found, conflict, business rule).
2. `throw` is reserved for the genuinely unexpected (a dependency is down, a bug). The shared
   exception handler turns those into a scrubbed 500. See [exception-handling.md](exception-handling.md).
3. Validation failures return **every** error, never just the first — the `ValidationBehavior`
   pipeline already guarantees this; your handler must not short-circuit.

## Writing a failure

```csharp
// single error — pick a stable, SCREAMING_SNAKE code; suffix drives the HTTP status
return Result<ProductDto>.Failure(new Error("PRODUCT_NOT_FOUND", $"Product {id} was not found."));

// or a factory for the common shapes
return Result.Failure(Error.NotFound($"Product {id} was not found."));   // -> 404
return Result.Failure(Error.Conflict("That SKU is already in use."));    // -> 409

// several business-rule failures at once (all surfaced to the caller)
return Result<SendResultDto>.Failure(new[]
{
    new Error("RECIPIENT_OPTED_OUT", "The recipient has opted out of email."),
    new Error("TEMPLATE_NOT_FOUND", "No 'welcome' template for channel Email."),
});
```

`Error(string Code, string? Description = null, string? Field = null)` — set `Field` for a
per-input validation error so the frontend can highlight the exact form field.

## HTTP status from the code

`ResultEnvelopeMapper.StatusForCode` maps `Error.Code` → status: an exact table
(`NOT_FOUND`→404, `CONFLICT`/`INVALID_STATE`→409, `UNAUTHORIZED`→401, `FORBIDDEN`→403,
`SUBSCRIPTION_INACTIVE`→402, …) plus suffix conventions: `*_NOT_FOUND`→404, `*_EXISTS`/
`*_ALREADY_EXISTS`→409, `*_DELETED`/`*_ALREADY_DELETED`→404, `*_EXCEEDED`→409, everything else 400.
Name your codes to fit a convention and you never touch the controller.

## Returning it from an endpoint

**MVC controller** (`inventory`, `pos`):
```csharp
var result = await mediator.Send(command, ct);
return this.ToApiResult(result);                 // failure -> envelope; success -> raw value / 204
// bespoke status for one code:
return this.ToApiResult(result, statusOverride: e => e.Code == "X" ? 410 : 0);
```

**Minimal API** (`auth`, `notification`):
```csharp
return result.ToApiResult(httpContext);                 // or .ToCreatedApiResult(httpContext, location)
```

Never write `return Problem(...)` or hand-build an error body — that path is gone.
