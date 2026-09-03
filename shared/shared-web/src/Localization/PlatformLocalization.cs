using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using SharedWeb.Localization;

namespace SharedWeb;

/// <summary>
/// Platform-wide request localization: English default, Bangla supported, extensible by adding a
/// culture code here + a <c>PlatformMessages.&lt;code&gt;.resx</c>. Wire with
/// <see cref="AddPlatformLocalization"/> (services) + <see cref="UsePlatformLocalization"/>
/// (pipeline, right after <c>UseExceptionHandler</c>).
/// </summary>
public static class PlatformLocalization
{
    public const string DefaultCulture = "en";

    /// <summary>Primary-subtag culture codes the platform serves. Add a code + a matching resx to extend.</summary>
    public static readonly IReadOnlyList<string> SupportedCultures = new[] { "en", "bn" };

    private static readonly ResourceManager Resources =
        new("SharedWeb.Resources.PlatformMessages", typeof(PlatformLocalization).Assembly);

    /// <summary>
    /// Localized cross-cutting string for the current request culture, English fallback, and the
    /// literal <paramref name="key"/> if no resource exists at all (so a missing key is visible,
    /// never a blank message).
    /// </summary>
    public static string Get(string key, string fallback)
    {
        try
        {
            return Resources.GetString(key, CultureInfo.CurrentUICulture) ?? fallback;
        }
        catch (MissingManifestResourceException)
        {
            return fallback;
        }
    }

    public static IServiceCollection AddPlatformLocalization(this IServiceCollection services)
    {
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(DefaultCulture);
            options.SetDefaultCulture(DefaultCulture);
            options.AddSupportedCultures(SupportedCultures.ToArray());
            options.AddSupportedUICultures(SupportedCultures.ToArray());
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.RequestCultureProviders.Insert(0, new PlatformRequestCultureProvider());
        });
        return services;
    }

    public static IApplicationBuilder UsePlatformLocalization(this IApplicationBuilder app)
        => app.UseRequestLocalization();
}
