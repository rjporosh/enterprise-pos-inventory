namespace SharedWeb;

/// <summary>
/// One item in an API failure response's <c>errors</c> array.
/// <c>{ "code": "...", "field": "...", "message": "..." }</c> — <c>field</c> is null for
/// errors not tied to a single input (business-rule / not-found).
/// </summary>
public sealed record ApiErrorItem(string Code, string Message, string? Field);

/// <summary>
/// The success half of the platform's single API response contract (see
/// <c>docs/MASTER-SPEC-v3.0.md</c> §"API Response Standard"):
/// <code>{ "success": true, "message": "...", "data": {...}, "traceId": "...", "timestamp": "..." }</code>
/// Failures use <see cref="ApiFailureResponse"/>. RFC 7807 (application/problem+json) is used
/// additionally, only for genuinely unhandled exceptions — see <c>PlatformExceptionHandler</c>.
/// </summary>
public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    string TraceId,
    DateTimeOffset Timestamp)
{
    public static ApiResponse<T> Ok(T data, string traceId, string message) =>
        new(true, message, data, traceId, DateTimeOffset.UtcNow);
}

/// <summary>
/// The failure half of the response contract. Returns <b>every</b> error in one response
/// (never stops at the first). The <see cref="Type"/>/<see cref="Title"/>/<see cref="Detail"/>/
/// <see cref="Status"/> members are transitional RFC 7807 aliases so existing clients that read
/// <c>problem.detail ?? problem.title</c> keep working while the frontend migrates to
/// <c>message</c>/<c>errors</c>; they are removed in the final step of the response-envelope
/// migration (M1 C7).
/// </summary>
public sealed record ApiFailureResponse(
    bool Success,
    string Message,
    IReadOnlyList<ApiErrorItem> Errors,
    string TraceId,
    DateTimeOffset Timestamp,
    string Type,
    string Title,
    string? Detail,
    int Status)
{
    public const string Rfc7807Type = "https://tools.ietf.org/html/rfc7807";

    /// <summary>Build a failure envelope from an already-materialized error list.</summary>
    public static ApiFailureResponse FromErrors(
        IReadOnlyList<ApiErrorItem> errors,
        string traceId,
        int status,
        string? message = null)
    {
        var items = errors.Count > 0
            ? errors
            : new[] { new ApiErrorItem("ERROR", message ?? PlatformMessages.FailureDefault, null) };
        var msg = message ?? items[0].Message;
        return new ApiFailureResponse(
            Success: false,
            Message: msg,
            Errors: items,
            TraceId: traceId,
            Timestamp: DateTimeOffset.UtcNow,
            Type: Rfc7807Type,
            Title: msg,
            Detail: items[0].Message,
            Status: status);
    }
}
