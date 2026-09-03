using NotificationService.Domain.Exceptions;
using SharedWeb;

namespace NotificationService.Api.Common;

/// <summary>
/// Maps notification-service's domain exceptions to HTTP status + a stable error code for
/// <see cref="SharedWeb.PlatformExceptionHandler"/>. Replicates the status mapping the (now
/// deleted) <c>ExceptionHandlingMiddleware</c> did. <c>TimeoutException</c> is handled by the
/// shared handler's built-ins (504).
/// </summary>
public sealed class NotificationExceptionMapper : IExceptionMapper
{
    public ExceptionMapping? TryMap(Exception exception) => exception switch
    {
        NotificationNotFoundException => new(StatusCodes.Status404NotFound, "NOTIFICATION_NOT_FOUND", exception.Message),
        TemplateNotFoundException => new(StatusCodes.Status404NotFound, "TEMPLATE_NOT_FOUND", exception.Message),
        TemplateAlreadyExistsException => new(StatusCodes.Status409Conflict, "TEMPLATE_ALREADY_EXISTS", exception.Message),
        InvalidNotificationStateException => new(StatusCodes.Status409Conflict, "INVALID_NOTIFICATION_STATE", exception.Message),
        TemplateRenderException => new(StatusCodes.Status422UnprocessableEntity, "TEMPLATE_RENDER_FAILED", exception.Message),
        DomainException => new(StatusCodes.Status400BadRequest, "BUSINESS_RULE_VIOLATION", exception.Message),
        _ => null,
    };
}
