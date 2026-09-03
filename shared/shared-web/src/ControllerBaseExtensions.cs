using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using IResult = SharedKernel.IResult;

namespace SharedWeb;

/// <summary>
/// MVC-controller helpers that render a <see cref="SharedKernel.Result"/> /
/// <see cref="SharedKernel.Result{T}"/> into the platform contract. Applied per endpoint (not a
/// global filter) so health / OpenAPI / release endpoints stay untouched.
///
/// <para>
/// <b>Failure</b> always becomes the platform failure envelope (every error, mapped HTTP status).
/// <b>Success</b> is raw by default — the resource value, or 204 for a payload-less
/// <see cref="Result"/> — matching the pre-existing contract; pass <c>wrapSuccess: true</c> to
/// wrap it in <see cref="ApiResponse{T}"/> (done service-by-service in M1 C7, in lockstep with
/// the frontend clients).
/// </para>
/// </summary>
public static class ControllerBaseExtensions
{
    public static IActionResult ToApiResult<T>(
        this ControllerBase controller,
        Result<T> result,
        Func<Error, int>? statusOverride = null,
        bool wrapSuccess = false,
        string? successMessage = null)
    {
        if (!result.IsSuccess)
            return FailureResult(result, TraceId(controller), statusOverride);

        return wrapSuccess
            ? new OkObjectResult(ApiResponse<T>.Ok(result.Value!, TraceId(controller), successMessage ?? PlatformMessages.SuccessDefault))
            : new OkObjectResult(result.Value);
    }

    public static IActionResult ToApiResult(
        this ControllerBase controller,
        Result result,
        Func<Error, int>? statusOverride = null,
        bool wrapSuccess = false,
        string? successMessage = null)
    {
        if (!result.IsSuccess)
            return FailureResult(result, TraceId(controller), statusOverride);

        return wrapSuccess
            ? new OkObjectResult(ApiResponse<object?>.Ok(null, TraceId(controller), successMessage ?? PlatformMessages.SuccessDefault))
            : new NoContentResult();
    }

    /// <summary>A 400 failure envelope for a pre-handler check (e.g. route/body id mismatch).</summary>
    public static IActionResult ValidationEnvelope(this ControllerBase controller, string field, string code, string message)
    {
        var body = ResultEnvelopeMapper.Failure(
            Result.Failure(new Error(code, message, field)),
            TraceId(controller),
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
