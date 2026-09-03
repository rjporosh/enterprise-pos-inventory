namespace SharedWeb;

/// <summary>
/// One item in an API failure response's <c>errors</c> array.
/// <c>{ "code": "...", "field": "...", "message": "..." }</c> — <c>field</c> is null for
/// errors not tied to a single input (business-rule / not-found).
/// </summary>
public sealed record ApiErrorItem(string Code, string Message, string? Field)
{
    /// <summary>
    /// Build an item, normalizing the field name so it lines up with a frontend form input:
    /// drops a leading <c>request.</c>/<c>command.</c>/<c>query.</c>/<c>dto.</c> segment that
    /// FluentValidation adds for wrapped-request commands, and camelCases the first letter.
    /// </summary>
    public static ApiErrorItem Of(string code, string message, string? field) =>
        new(code, message, NormalizeField(field));

    public static string? NormalizeField(string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return null;

        var value = field.Trim();
        foreach (var prefix in new[] { "request.", "command.", "query.", "dto.", "model." })
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = value[prefix.Length..];
                break;
            }
        }

        if (value.Length == 0)
            return null;

        return char.IsUpper(value[0]) ? char.ToLowerInvariant(value[0]) + value[1..] : value;
    }
}

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
