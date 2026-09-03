using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SharedWeb;

/// <summary>
/// The one exception handler every service uses (registered via
/// <see cref="PlatformExceptionHandlingExtensions.AddPlatformExceptionHandling"/> +
/// <c>app.UseExceptionHandler()</c>). Expected failures → the platform failure envelope with a
/// mapped HTTP status; genuinely unhandled exceptions → a scrubbed RFC 7807 500 that never
/// leaks a stack trace, SQL, or connection string, plus one structured <c>Error</c>-level log
/// carrying endpoint / method / correlation id / root cause / possible solution.
/// </summary>
public sealed class PlatformExceptionHandler(
    IHostEnvironment environment,
    ILogger<PlatformExceptionHandler> logger,
    IEnumerable<IExceptionMapper> mappers) : IExceptionHandler
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IReadOnlyList<IExceptionMapper> _mappers = mappers.ToList();

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = context.Items.TryGetValue("CorrelationId", out var cid) && cid is string s && s.Length > 0
            ? s
            : context.TraceIdentifier;

        // Client went away — nothing to send, no error to log.
        if ((exception is OperationCanceledException or TaskCanceledException) && context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499;
            return true;
        }

        if (exception is ValidationException validation)
        {
            var items = validation.Errors
                .Select(e => ApiErrorItem.Of("VALIDATION_ERROR", e.ErrorMessage, e.PropertyName))
                .ToList();
            if (items.Count == 0)
                items.Add(new ApiErrorItem("VALIDATION_ERROR", PlatformMessages.ValidationFailure, null));
            await WriteEnvelope(context, correlationId, StatusCodes.Status400BadRequest, items, PlatformMessages.ValidationFailure);
            return true;
        }

        foreach (var mapper in _mappers)
        {
            if (mapper.TryMap(exception) is { } m)
            {
                await WriteEnvelope(context, correlationId, m.StatusCode,
                    new[] { new ApiErrorItem(m.Code, m.Message, m.Field) }, m.Message);
                return true;
            }
        }

        var builtIn = MapBuiltIn(exception);
        if (builtIn is { } b)
        {
            await WriteEnvelope(context, correlationId, b.StatusCode,
                new[] { new ApiErrorItem(b.Code, b.Message, null) }, b.Message);
            return true;
        }

        // Truly unhandled — log everything internally, tell the caller nothing sensitive.
        var (rootCause, possibleSolution) = Diagnose(exception);
        logger.LogError(exception,
            "Unhandled exception. CorrelationId={CorrelationId} Method={Method} Endpoint={Endpoint} " +
            "ExceptionType={ExceptionType} RootCause={RootCause} PossibleSolution={PossibleSolution}",
            correlationId, context.Request.Method, context.Request.Path.Value ?? "/",
            exception.GetType().FullName, rootCause, possibleSolution);

        var detail = environment.IsDevelopment()
            ? $"{exception.GetType().Name}: {exception.Message}"
            : PlatformMessages.UnexpectedError;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        SetCommonHeaders(context, correlationId);
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            type = ApiFailureResponse.Rfc7807Type,
            title = PlatformMessages.UnexpectedError,
            status = 500,
            detail,
            instance = context.Request.Path.Value,
            traceId = correlationId,
            correlationId,
            timestamp = DateTimeOffset.UtcNow,
        }, Json), cancellationToken);
        return true;
    }

    private static ExceptionMapping? MapBuiltIn(Exception exception) => exception switch
    {
        TimeoutException => new ExceptionMapping(StatusCodes.Status504GatewayTimeout, "GATEWAY_TIMEOUT",
            "A dependency did not respond in time. Please try again."),
        UnauthorizedAccessException => new ExceptionMapping(StatusCodes.Status403Forbidden, "FORBIDDEN",
            "You do not have permission to perform this action."),
        KeyNotFoundException => new ExceptionMapping(StatusCodes.Status404NotFound, "NOT_FOUND",
            "The requested resource was not found."),
        _ => null,
    };

    private async Task WriteEnvelope(HttpContext context, string correlationId, int status, IReadOnlyList<ApiErrorItem> errors, string? message)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        SetCommonHeaders(context, correlationId);
        var body = ApiFailureResponse.FromErrors(errors, correlationId, status, message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, Json));
    }

    private static void SetCommonHeaders(HttpContext context, string correlationId)
    {
        context.Response.Headers["X-Correlation-Id"] = correlationId;
        var idem = context.Request.Headers["Idempotency-Key"].ToString();
        if (!string.IsNullOrEmpty(idem))
            context.Response.Headers["Idempotency-Key"] = idem;
    }

    /// <summary>Best-effort operator hint for the runtime-error log — never shown to the caller.</summary>
    private static (string RootCause, string PossibleSolution) Diagnose(Exception exception)
    {
        var name = exception.GetType().Name;
        return name switch
        {
            "NpgsqlException" or "PostgresException" or "SocketException" =>
                ("Database dependency unavailable or query failed.",
                 "Verify the database container is running and Database:ConnectionString is correct; check logs/runtime-errors."),
            "BrokerUnreachableException" or "RabbitMQClientException" =>
                ("Message broker (RabbitMQ) unreachable.",
                 "Start the RabbitMQ container or unset RabbitMQ:Host; the API keeps serving without it."),
            "RedisConnectionException" =>
                ("Redis cache unreachable.",
                 "Start the Redis container or unset the Redis connection string; caching degrades gracefully."),
            "DbUpdateException" or "DbUpdateConcurrencyException" =>
                ("A database write failed (constraint violation or concurrency conflict).",
                 "Inspect the inner exception; this often means a unique-key or optimistic-concurrency conflict."),
            _ => ("Unclassified server error.",
                 "Inspect the exception and stack trace in logs/runtime-errors; add a domain exception + IExceptionMapper if this is an expected failure."),
        };
    }
}
