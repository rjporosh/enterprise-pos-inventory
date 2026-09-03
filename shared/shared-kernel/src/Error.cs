using System.Text.Json;

namespace SharedKernel;

/// <summary>
/// A single structured error. <see cref="Code"/> is a stable machine-readable identifier
/// (e.g. <c>PRODUCT_NOT_FOUND</c>, <c>VALIDATION_ERROR</c>); <see cref="Description"/> is a
/// human-readable message; <see cref="Field"/> names the offending input for per-property
/// validation failures (null for business-rule / not-found errors). This shape serializes
/// directly into the platform's API error contract's <c>errors[]</c> items
/// (<c>{ code, field, message }</c>) — see <c>SharedWeb.ApiResponse</c>.
/// </summary>
public readonly record struct Error(string Code, string? Description = null, string? Field = null)
{
    public static readonly Error None = new(string.Empty);

    public static implicit operator Result(Error error) => Result.Failure(error);

    public override string ToString() => JsonSerializer.Serialize(new { Code, Description, Field });

    // Factories mirroring the codes the notification-service handlers already use, so those
    // handlers (and their tests) keep working once they move onto this shared type.
    public static Error Validation(string field, string message) => new("VALIDATION_ERROR", message, field);
    public static Error NotFound(string message) => new("NOT_FOUND", message);
    public static Error Conflict(string message) => new("CONFLICT", message);
    public static Error InvalidState(string message) => new("INVALID_STATE", message);
    public static Error Unexpected(string message) => new("UNEXPECTED_ERROR", message);
}
