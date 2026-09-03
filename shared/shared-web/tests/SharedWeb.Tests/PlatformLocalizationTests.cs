using System.Globalization;
using FluentAssertions;
using SharedKernel;
using SharedWeb;
using SharedWeb.Localization;
using Xunit;

namespace SharedWeb.Tests;

[Collection("culture-sensitive")]
public class PlatformLocalizationTests
{
    private static T WithCulture<T>(string culture, Func<T> body)
    {
        var prev = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            return body();
        }
        finally
        {
            CultureInfo.CurrentUICulture = prev;
        }
    }

    [Fact]
    public void English_is_the_default()
    {
        WithCulture("en", () => PlatformMessages.ValidationFailure)
            .Should().Be("One or more validation errors occurred.");
    }

    [Fact]
    public void Bangla_resolves_from_the_satellite_resx()
    {
        WithCulture("bn", () => PlatformMessages.ValidationFailure)
            .Should().Be("এক বা একাধিক ভ্যালিডেশন ত্রুটি হয়েছে।");
        WithCulture("bn", () => PlatformMessages.UnexpectedError)
            .Should().Be("একটি অপ্রত্যাশিত ত্রুটি ঘটেছে।");
    }

    [Fact]
    public void Unknown_culture_falls_back_to_english()
    {
        WithCulture("fr", () => PlatformMessages.FailureDefault)
            .Should().Be("The request could not be completed.");
    }

    [Fact]
    public void Domain_error_message_is_localized_when_a_key_exists_else_kept()
    {
        var known = Result.Failure(new Error("NOT_FOUND", "Product 5 was not found."));
        var custom = Result.Failure(new Error("PRODUCT_SKU_EXISTS", "SKU 'ABC' already exists."));

        WithCulture("bn", () =>
        {
            ResultEnvelopeMapper.Failure(known, "t", 404).Errors[0].Message
                .Should().Be("অনুরোধ করা রিসোর্সটি পাওয়া যায়নি।");
            // no bn key for PRODUCT_SKU_EXISTS -> the handler's own text is preserved
            ResultEnvelopeMapper.Failure(custom, "t", 409).Errors[0].Message
                .Should().Be("SKU 'ABC' already exists.");
            return 0;
        });
    }

    [Fact]
    public void Provider_matches_supported_cultures_only()
    {
        PlatformLocalization.SupportedCultures.Should().BeEquivalentTo(new[] { "en", "bn" });
    }
}
