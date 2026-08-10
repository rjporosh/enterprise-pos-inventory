using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using FluentValidation;
using SharedKernel;

namespace PosService.API.Middleware;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Request.Headers[SharedKernel.Constants.CorrelationIdHeader].FirstOrDefault() ?? Guid.NewGuid().ToString();

        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail) = exception switch
        {
            ValidationException validationEx => ((int)HttpStatusCode.BadRequest, "Validation Failed", string.Join("; ", validationEx.Errors)),
            ArgumentNullException argEx => ((int)HttpStatusCode.BadRequest, "Invalid Request", argEx.Message),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource Not Found", "The requested resource was not found."),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized", "Authentication required."),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Invalid Operation", exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "Server Error", "An unexpected error occurred.")
        };

        logger.LogError(exception,
            "Unhandled exception: {CorrelationId} | Endpoint: {Endpoint} | Method: {Method} | Error: {Message}",
            correlationId,
            context.Request.Path,
            context.Request.Method,
            exception.Message);

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = title,
            status = statusCode,
            detail = env.IsDevelopment() ? detail : "An unexpected error occurred.",
            instance = context.Request.Path,
            traceId = context.TraceIdentifier,
            correlationId = correlationId,
            errors = exception is ValidationException ? detail : null,
            timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = statusCode;
        context.Response.Headers[SharedKernel.Constants.CorrelationIdHeader] = correlationId;
        await context.Response.WriteAsJsonAsync(problemDetails, JsonOptions);
    }
}
