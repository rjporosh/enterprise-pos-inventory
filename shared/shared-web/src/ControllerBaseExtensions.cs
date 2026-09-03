using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using IResult = SharedKernel.IResult;

namespace SharedWeb;

/// <summary>
/// MVC-controller helpers that render a <see cref="SharedKernel.Result"/> /
/// <see cref="SharedKernel.Result{T}"/> into the platform API envelope. Applied per endpoint
/// (not a global filter) so health / OpenAPI / release endpoints and any not-yet-migrated
/// success shapes are left untouched.
/// </summary>
public static class ControllerBaseExtensions
{
    public static IActionResult ToApiResult<T>(
        this ControllerBase controller,
        Result<T> result,
        Func<Error, int>? statusOverride = null,
        string? successMessage = null)
    {
        var traceId = TraceId(controller);
        if (result.IsSuccess)
            return new OkObjectResult(ApiResponse<T>.Ok(result.Value!, traceId, successMessage ?? PlatformMessages.SuccessDefault));

        return FailureResult(result, traceId, statusOverride);
    }

    public static IActionResult ToApiResult(
        this ControllerBase controller,
        Result result,
        Func<Error, int>? statusOverride = null,
        string? successMessage = null)
    {
        var traceId = TraceId(controller);
        if (result.IsSuccess)
            return new OkObjectResult(ApiResponse<object?>.Ok(null, traceId, successMessage ?? PlatformMessages.SuccessDefault));

        return FailureResult(result, traceId, statusOverride);
    }

    /// <summary>A 400 failure envelope for a pre-handler check (e.g. route/body id mismatch).</summary>
    public static IActionResult ValidationEnvelope(this ControllerBase controller, string field, string code, string message)
    {
        var traceId = TraceId(controller);
        var body = ResultEnvelopeMapper.Failure(
            Result.Failure(new Error(code, message, field)),
            traceId,
            StatusCodes.Status400BadRequest);
        return new ObjectResult(body) { StatusCode = StatusCodes.Status400BadRequest };
    }

    private static IActionResult FailureResult(IResult result, string traceId, Func<Error, int>? statusOverride)
    {
        var status = ResultEnvelopeMapper.StatusFor(result, statusOverride);
        var body = ResultEnvelopeMapper.Failure(result, traceId, status);
        return new ObjectResult(body) { StatusCode = status };
    }

    private static string TraceId(ControllerBase controller) =>
        controller.HttpContext.Items.TryGetValue("CorrelationId", out var cid) && cid is string s && !string.IsNullOrEmpty(s)
            ? s
            : controller.HttpContext.TraceIdentifier;
}
