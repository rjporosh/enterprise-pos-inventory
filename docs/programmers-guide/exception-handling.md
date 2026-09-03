# Exception handling

One handler for all five services: `SharedWeb.PlatformExceptionHandler` (`IExceptionHandler`),
registered by `AddPlatformExceptionHandling()` + `app.UseExceptionHandler()`.

## What it does

| Exception | Result |
|---|---|
| `FluentValidation.ValidationException` | 400, **every** error as `{code:"VALIDATION_ERROR", field, message}` |
| a registered `IExceptionMapper` match | that mapper's `(status, code, message)` |
| `TimeoutException` | 504 `GATEWAY_TIMEOUT` |
| `UnauthorizedAccessException` | 403 `FORBIDDEN` |
| `KeyNotFoundException` | 404 `NOT_FOUND` |
| `OperationCanceledException` (client aborted) | 499, no body |
| anything else | **scrubbed 500** — generic `detail` (real text only in `Development`), never a stack trace / SQL / connection string; always `traceId` + `X-Correlation-Id`; one structured `Error` log with `RootCause`/`PossibleSolution` for `logs/runtime-errors/` |

`InvalidOperationException` / `ArgumentException` are **not** auto-mapped to 400 — they almost
always mean a real server bug. A genuine client-input rule belongs in a `Result.Failure` or a
domain exception + mapper (below).

## Adding a domain exception

1. Create it in your service's `Domain/Exceptions/` (extend the service's `DomainException` base
   so an unmapped one still lands at 400, not 500).
2. Add a line to your service's `IExceptionMapper`
   (`AuthService.Api/Common/AuthExceptionMapper.cs`,
   `NotificationService.Api/Common/NotificationExceptionMapper.cs` — `inventory`/`pos` have none
   yet; create `…/Common/<Service>ExceptionMapper.cs` and
   `builder.Services.AddExceptionMapper<…>()`):

```csharp
public ExceptionMapping? TryMap(Exception exception) => exception switch
{
    OrderAlreadyShippedException => new(StatusCodes.Status409Conflict, "ORDER_ALREADY_SHIPPED", exception.Message),
    // ...
    DomainException => new(StatusCodes.Status400BadRequest, "BUSINESS_RULE_VIOLATION", exception.Message),
    _ => null,
};
```

The exception's `Message` is surfaced to the caller — keep it user-safe (no internals). For a
security-sensitive path make it deliberately generic (see `InvalidCredentialsException`).

## Localizing the message

Add a resx key named exactly after the `code` — see [localization.md](localization.md). No handler
or mapper change.
