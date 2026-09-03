using AuthService.Domain.Exceptions;
using SharedWeb;

namespace AuthService.Api.Common;

/// <summary>
/// Maps auth-service's domain exceptions to HTTP status + a stable error code for
/// <see cref="SharedWeb.PlatformExceptionHandler"/>. Replicates the status mapping that the
/// (now deleted) <c>ExceptionHandlingMiddleware</c> did; the messages come straight from the
/// exception (developer-authored, safe to surface — <c>InvalidCredentialsException</c> is
/// deliberately generic to avoid user enumeration).
/// </summary>
public sealed class AuthExceptionMapper : IExceptionMapper
{
    public ExceptionMapping? TryMap(Exception exception) => exception switch
    {
        InvalidCredentialsException => new(StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", exception.Message),
        InvalidRefreshTokenException => new(StatusCodes.Status401Unauthorized, "INVALID_REFRESH_TOKEN", exception.Message),
        AccountLockedException => new(StatusCodes.Status423Locked, "ACCOUNT_LOCKED", exception.Message),
        AccountNotActiveException => new(StatusCodes.Status403Forbidden, "ACCOUNT_NOT_ACTIVE", exception.Message),
        UserAlreadyExistsException => new(StatusCodes.Status409Conflict, "USER_ALREADY_EXISTS", exception.Message),
        UserNotFoundException => new(StatusCodes.Status404NotFound, "USER_NOT_FOUND", exception.Message),
        RoleNotFoundException => new(StatusCodes.Status404NotFound, "ROLE_NOT_FOUND", exception.Message),
        ModuleNotFoundException => new(StatusCodes.Status404NotFound, "MODULE_NOT_FOUND", exception.Message),
        PermissionNotFoundException => new(StatusCodes.Status404NotFound, "PERMISSION_NOT_FOUND", exception.Message),
        OtpRateLimitExceededException => new(StatusCodes.Status429TooManyRequests, "OTP_RATE_LIMIT_EXCEEDED", exception.Message),
        DomainException => new(StatusCodes.Status400BadRequest, "BUSINESS_RULE_VIOLATION", exception.Message),
        _ => null,
    };
}
