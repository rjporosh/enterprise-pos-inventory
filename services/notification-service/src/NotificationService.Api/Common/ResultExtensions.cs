using SharedKernel;
using SharedWeb;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace NotificationService.Api.Common;

/// <summary>
/// Thin adapters so this service's endpoints keep their <c>result.ToApiResult(httpContext[, message])</c>
/// call shape while the actual envelope + status mapping now come from the shared
/// <see cref="SharedWeb.MinimalApiResultExtensions"/> (identical <see cref="SharedWeb.ApiResponse{T}"/>
/// shape as before this migration). Endpoints do not import <c>SharedWeb</c> directly, so there is no
/// overload ambiguity with the shared extension methods.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result, HttpContext httpContext, string? successMessage = null)
        => MinimalApiResultExtensions.ToApiResult(result, httpContext, successMessage: successMessage);

    public static IResult ToCreatedApiResult<T>(this Result<T> result, HttpContext httpContext, string location, string? successMessage = null)
        => MinimalApiResultExtensions.ToCreatedApiResult(result, httpContext, location, successMessage: successMessage);

    public static IResult ToApiResult(this Result result, HttpContext httpContext, string? successMessage = null)
        => MinimalApiResultExtensions.ToApiResult(result, httpContext, successMessage: successMessage);
}
