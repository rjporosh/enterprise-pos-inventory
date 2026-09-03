using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace SharedWeb;

/// <summary>
/// Minimal-API equivalent of <see cref="ControllerBaseExtensions"/> — for auth-service and
/// notification-service, whose endpoints return <see cref="IResult"/>. Same envelope, same
/// status mapping (both go through <see cref="ResultEnvelopeMapper"/>).
/// </summary>
public static class MinimalApiResultExtensions
{
    public static Microsoft.AspNetCore.Http.IResult ToApiResult<T>(
        this Result<T> result,
        HttpContext http,
        Func<Error, int>? statusOverride = null,
        string? successMessage = null)
    {
        var traceId = TraceId(http);
        if (result.IsSuccess)
            return Results.Ok(ApiResponse<T>.Ok(result.Value!, traceId, successMessage ?? PlatformMessages.SuccessDefault));

        return Failure(result, traceId, statusOverride);
    }

    public static Microsoft.AspNetCore.Http.IResult ToCreatedApiResult<T>(
        this Result<T> result,
        HttpContext http,
        string location,
        Func<Error, int>? statusOverride = null,
        string? successMessage = null)
    {
        var traceId = TraceId(http);
        if (result.IsSuccess)
            return Results.Created(location, ApiResponse<T>.Ok(result.Value!, traceId, successMessage ?? PlatformMessages.CreatedDefault));

        return Failure(result, traceId, statusOverride);
    }

    public static Microsoft.AspNetCore.Http.IResult ToApiResult(
        this Result result,
        HttpContext http,
        Func<Error, int>? statusOverride = null,
        string? successMessage = null)
    {
        var traceId = TraceId(http);
        if (result.IsSuccess)
            return Results.Ok(ApiResponse<object?>.Ok(null, traceId, successMessage ?? PlatformMessages.SuccessDefault));

        return Failure(result, traceId, statusOverride);
    }

    private static Microsoft.AspNetCore.Http.IResult Failure(SharedKernel.IResult result, string traceId, Func<Error, int>? statusOverride)
    {
        var status = ResultEnvelopeMapper.StatusFor(result, statusOverride);
        return Results.Json(ResultEnvelopeMapper.Failure(result, traceId, status), statusCode: status);
    }

    private static string TraceId(HttpContext http) =>
        http.Items.TryGetValue("CorrelationId", out var cid) && cid is string s && !string.IsNullOrEmpty(s)
            ? s
            : http.TraceIdentifier;
}
