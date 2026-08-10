namespace SharedKernel;

public static class Constants
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    public const string AcceptLanguageHeader = "Accept-Language";
    public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;
    public const int DefaultPageNumber = 1;
}
