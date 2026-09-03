using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace SharedWeb;

public static class PlatformApiBehavior
{
    /// <summary>
    /// Makes ASP.NET's automatic <c>[ApiController]</c> model-validation 400 return the platform
    /// failure envelope (all errors, <c>{code,field,message}</c>) instead of the framework's
    /// <c>ValidationProblemDetails</c> dictionary — so the caller sees one error shape whether a
    /// request fails at model binding, FluentValidation, or a business rule. Call right after
    /// <c>AddControllers()</c>.
    /// </summary>
    public static IServiceCollection ConfigurePlatformApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var traceId = context.HttpContext.Items.TryGetValue("CorrelationId", out var cid) && cid is string s && s.Length > 0
                    ? s
                    : context.HttpContext.TraceIdentifier;

                var errors = context.ModelState
                    .Where(kv => kv.Value is { Errors.Count: > 0 })
                    .SelectMany(kv => kv.Value!.Errors.Select(e => ApiErrorItem.Of(
                        "VALIDATION_ERROR",
                        string.IsNullOrWhiteSpace(e.ErrorMessage) ? "The value is invalid." : e.ErrorMessage,
                        string.IsNullOrEmpty(kv.Key) ? null : kv.Key)))
                    .ToList();

                var body = ApiFailureResponse.FromErrors(errors, traceId, StatusCodes.Status400BadRequest, PlatformMessages.ValidationFailure);
                return new BadRequestObjectResult(body);
            };
        });

        return services;
    }
}
