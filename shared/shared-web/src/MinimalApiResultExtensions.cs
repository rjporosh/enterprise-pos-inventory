using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace SharedWeb;

/// <summary>
/// Minimal-API equivalent of <see cref="ControllerBaseExtensions"/> — for auth-service and
/// notification-service, whose endpoints return <see cref="Microsoft.AspNetCore.Http.IResult"/>.
/// Same envelope, same status mapping. <paramref name="wrapSuccess"/> defaults to true here
/// because the services that use these already return an <see cref="ApiResponse{T}"/> envelope
/// on success (notification) or are migrated to it in the same pass (auth).
/// </summary>
public static class MinimalApiResultExtensions
{
    public static Microsoft.AspNetCore.Http.IResult ToApiResult<T>(
        this Result<T> result,
        HttpContext http,
        Func<Error, int>? statusOverride = null,
        bool wrapSuccess = true,
        string? successMessage = null)
    {
        if (!result.IsSuccess)
            return Failure(result, TraceId(http), statusOverride);

        return wrapSuccess
            ? Results.Ok(ApiResponse<T>.Ok(result.Value!, TraceId(http), successMessage ?? PlatformMessages.SuccessDefault))
            : Results.Ok(result.Value);
    }

    public static Microsoft.AspNetCore.Http.IResult ToApiResult(
        this Result result,
        HttpContext http,
        Func<Error, int>? statusOverride = null,
        bool wrapSuccess = true,
        string? successMessage = null)
    {
        if (!result.IsSuccess)
            return Failure(result, TraceId(http), statusOverride);

        return wrapSuccess
            ? Results.Ok(ApiResponse<object?>.Ok(null, TraceId(http), successMessage ?? PlatformMessages.SuccessDefault))
            : Results.NoContent();
    }

    public static Microsoft.AspNetCore.Http.IResult ToCreatedApiResult<T>(
        this Result<T> result,
        HttpContext http,
        string location,
        Func<Error, int>? statusOverride = null,
        string? successMessage = null)
    {
        if (!result.IsSuccess)
            return Failure(result, TraceId(http), statusOverride);

        return Results.Created(location, ApiResponse<T>.Ok(result.Value!, TraceId(http), successMessage ?? PlatformMessages.CreatedDefault));
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
