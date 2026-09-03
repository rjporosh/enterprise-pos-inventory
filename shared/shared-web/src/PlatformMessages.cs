namespace SharedWeb;

/// <summary>
/// Default English strings for the response envelope. In M1 C6 these become
/// <c>IStringLocalizer</c> lookups (keys: <c>Response.Success</c>, <c>Response.Failure</c>,
/// <c>Response.ValidationFailure</c>) resolved against the request culture, with these values
/// as the <c>en</c> fallback — the constants stay as the guaranteed default.
/// </summary>
public static class PlatformMessages
{
    public const string SuccessDefault = "Request completed successfully.";
    public const string CreatedDefault = "Resource created successfully.";
    public const string FailureDefault = "The request could not be completed.";
    public const string ValidationFailure = "One or more validation errors occurred.";
    public const string UnexpectedError = "An unexpected error occurred.";
}
