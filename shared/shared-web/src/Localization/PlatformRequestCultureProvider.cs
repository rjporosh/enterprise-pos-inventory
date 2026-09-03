using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace SharedWeb.Localization;

/// <summary>
/// Resolves the request culture in this order: <c>?lang=</c> query parameter, then the
/// <c>Accept-Language</c> header, then a <c>locale</c>/<c>culture</c> claim on the authenticated
/// user, then the default (<c>en</c>). Only cultures in <see cref="PlatformLocalization.SupportedCultures"/>
/// are honored; anything else falls through to the next source.
/// </summary>
public sealed class PlatformRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var culture =
            FromQuery(httpContext) ??
            FromHeader(httpContext) ??
            FromClaim(httpContext) ??
            PlatformLocalization.DefaultCulture;

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, culture));
    }

    private static string? FromQuery(HttpContext ctx) =>
        ctx.Request.Query.TryGetValue("lang", out var lang) ? Match(lang.ToString()) : null;

    private static string? FromHeader(HttpContext ctx)
    {
        var header = ctx.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(header))
            return null;

        foreach (var segment in header.Split(','))
        {
            var candidate = segment.Split(';')[0].Trim();
            var primary = candidate.Split('-')[0];
            var matched = Match(primary);
            if (matched is not null)
                return matched;
        }

        return null;
    }

    private static string? FromClaim(HttpContext ctx)
    {
        var claim = ctx.User?.FindFirst("locale")?.Value ?? ctx.User?.FindFirst("culture")?.Value;
        return claim is null ? null : Match(claim);
    }

    private static string? Match(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var primary = value.Trim().Split('-')[0].ToLowerInvariant();
        return PlatformLocalization.SupportedCultures.Contains(primary) ? primary : null;
    }
}
