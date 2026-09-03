namespace SharedWeb;

/// <summary>
/// Lets a service teach <see cref="PlatformExceptionHandler"/> how to render its own domain
/// exception types (e.g. auth-service's <c>InvalidCredentialsException</c>) — register one or
/// more implementations in DI. The handler tries every mapper in registration order and uses
/// the first non-null result; anything unmapped becomes a scrubbed 500.
/// </summary>
public interface IExceptionMapper
{
    ExceptionMapping? TryMap(Exception exception);
}

/// <param name="StatusCode">HTTP status to return.</param>
/// <param name="Code">Stable machine-readable error code for the <c>errors[]</c> item.</param>
/// <param name="Message">
/// Human-readable message. Must be safe to expose — a developer-authored domain-exception
/// message, never a raw infrastructure message.
/// </param>
/// <param name="Field">Optional offending input name.</param>
public readonly record struct ExceptionMapping(int StatusCode, string Code, string Message, string? Field = null);
