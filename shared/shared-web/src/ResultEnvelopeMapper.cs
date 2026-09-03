using Microsoft.AspNetCore.Http;
using SharedKernel;
using IResult = SharedKernel.IResult;

namespace SharedWeb;

/// <summary>
/// The one place that turns a <see cref="SharedKernel.Result"/> / <see cref="SharedKernel.Result{T}"/>
/// failure into an HTTP status code + the platform failure envelope. Used by both the MVC
/// (<see cref="ControllerBaseExtensions"/>) and Minimal-API (<see cref="MinimalApiResultExtensions"/>)
/// shims so every service produces an identical shape.
/// </summary>
public static class ResultEnvelopeMapper
{
    public const string Rfc7807Type = "https://tools.ietf.org/html/rfc7807";

    /// <summary>HTTP status for a failed result. Never call for a successful one.</summary>
    public static int StatusFor(IResult result, Func<Error, int>? statusOverride = null)
    {
        if (result.ValidationErrors.Count > 0)
            return StatusCodes.Status400BadRequest;

        var code = result.Error.Code;

        if (statusOverride is not null && !string.IsNullOrEmpty(code))
        {
            var overridden = statusOverride(result.Error);
            if (overridden > 0)
                return overridden;
        }

        return StatusForCode(code);
    }

    public static int StatusForCode(string? code)
    {
        if (string.IsNullOrEmpty(code))
            return StatusCodes.Status400BadRequest;

        switch (code)
        {
            case "NOT_FOUND": return StatusCodes.Status404NotFound;
            case "CONFLICT": return StatusCodes.Status409Conflict;
            case "INVALID_STATE": return StatusCodes.Status409Conflict;
            case "VALIDATION_ERROR": return StatusCodes.Status400BadRequest;
            case "UNAUTHORIZED": return StatusCodes.Status401Unauthorized;
            case "FORBIDDEN": return StatusCodes.Status403Forbidden;
            case "UNEXPECTED_ERROR": return StatusCodes.Status500InternalServerError;
            case "SUBSCRIPTION_INACTIVE": return StatusCodes.Status402PaymentRequired;
            case "MODULE_NOT_ENABLED": return StatusCodes.Status403Forbidden;
        }

        if (code.EndsWith("_NOT_FOUND", StringComparison.Ordinal)) return StatusCodes.Status404NotFound;
        if (code.EndsWith("_ALREADY_DELETED", StringComparison.Ordinal)) return StatusCodes.Status404NotFound;
        if (code.EndsWith("_DELETED", StringComparison.Ordinal)) return StatusCodes.Status404NotFound;
        if (code.EndsWith("_ALREADY_EXISTS", StringComparison.Ordinal)) return StatusCodes.Status409Conflict;
        if (code.EndsWith("_EXISTS", StringComparison.Ordinal)) return StatusCodes.Status409Conflict;
        if (code.EndsWith("_EXCEEDED", StringComparison.Ordinal)) return StatusCodes.Status409Conflict;

        return StatusCodes.Status400BadRequest;
    }

    /// <summary>Build the failure envelope for a failed result.</summary>
    public static ApiFailureResponse Failure(IResult result, string traceId, int statusCode)
    {
        var items = result.Errors.Count > 0
            ? result.Errors.Select(e => ApiErrorItem.Of(e.Code, e.Description ?? e.Code, e.Field)).ToList()
            : new List<ApiErrorItem> { ApiErrorItem.Of(result.Error.Code, result.Error.Description ?? PlatformMessages.FailureDefault, result.Error.Field) };

        var message = result.ValidationErrors.Count > 0 ? PlatformMessages.ValidationFailure : null;

        return ApiFailureResponse.FromErrors(items, traceId, statusCode, message);
    }
}
