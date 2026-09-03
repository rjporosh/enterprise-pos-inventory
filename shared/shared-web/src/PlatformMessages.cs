namespace SharedWeb;

/// <summary>
/// Cross-cutting response strings, resolved for the current request culture from
/// <c>Resources/PlatformMessages[.&lt;culture&gt;].resx</c> with the English constant as the
/// guaranteed fallback. The middleware (<see cref="PlatformLocalization.UsePlatformLocalization"/>)
/// must have set the request culture; outside a request these return English.
/// </summary>
public static class PlatformMessages
{
    public const string SuccessDefaultEn = "Request completed successfully.";
    public const string CreatedDefaultEn = "Resource created successfully.";
    public const string FailureDefaultEn = "The request could not be completed.";
    public const string ValidationFailureEn = "One or more validation errors occurred.";
    public const string UnexpectedErrorEn = "An unexpected error occurred.";

    public static string SuccessDefault => PlatformLocalization.Get("Response.Success", SuccessDefaultEn);
    public static string CreatedDefault => PlatformLocalization.Get("Response.Created", CreatedDefaultEn);
    public static string FailureDefault => PlatformLocalization.Get("Response.Failure", FailureDefaultEn);
    public static string ValidationFailure => PlatformLocalization.Get("Response.ValidationFailure", ValidationFailureEn);
    public static string UnexpectedError => PlatformLocalization.Get("Error.Unexpected", UnexpectedErrorEn);

    /// <summary>
    /// Localized message for a domain error code (<c>NOT_FOUND</c>, <c>PRODUCT_SKU_EXISTS</c>, …) if
    /// a resx entry exists for it; otherwise <paramref name="fallback"/> — the handler's own
    /// (English) <c>Error.Description</c>. This is how domain messages get localized incrementally
    /// without touching handlers: add a key named exactly after the code.
    /// </summary>
    public static string ForCode(string code, string fallback)
        => string.IsNullOrEmpty(code) ? fallback : PlatformLocalization.Get(code, fallback);
}
