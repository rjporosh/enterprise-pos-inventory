using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using SharedKernel;
using SharedWeb;
using Xunit;

namespace SharedWeb.Tests;

public class ResultEnvelopeMapperTests
{
    // The JSON options every service configures (camelCase, omit nulls) — the snapshots below
    // are what a client actually receives on the wire.
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [Fact]
    public void Success_envelope_has_the_standard_shape()
    {
        var body = ApiResponse<object>.Ok(new { id = 7 }, "trace-1", PlatformMessages.SuccessDefault);

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(body, Json));
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("message").GetString().Should().Be("Request completed successfully.");
        root.GetProperty("data").GetProperty("id").GetInt32().Should().Be(7);
        root.GetProperty("traceId").GetString().Should().Be("trace-1");
        root.TryGetProperty("timestamp", out _).Should().BeTrue();
        root.TryGetProperty("errors", out _).Should().BeFalse("success responses carry no errors array");
    }

    [Fact]
    public void Validation_failure_returns_every_error_not_just_the_first()
    {
        var result = Result<Guid>.ValidationFailure(new[]
        {
            ValidationError.Create("Name", "Name is required."),
            ValidationError.Create("Price", "Price must be greater than 0."),
        });

        var status = ResultEnvelopeMapper.StatusFor(result);
        var body = ResultEnvelopeMapper.Failure(result, "trace-2", status);

        status.Should().Be(400);
        body.Success.Should().BeFalse();
        body.Errors.Should().HaveCount(2);
        body.Errors.Select(e => e.Field).Should().BeEquivalentTo(new[] { "Name", "Price" });
        body.Errors.Should().OnlyContain(e => e.Code == "VALIDATION_ERROR");
        body.Message.Should().Be(PlatformMessages.ValidationFailure);
        // transitional RFC7807 aliases so current clients keep resolving problem.detail/title
        body.Detail.Should().Be("Name is required.");
        body.Title.Should().Be(PlatformMessages.ValidationFailure);
        body.Status.Should().Be(400);
    }

    [Theory]
    [InlineData("PRODUCT_NOT_FOUND", 404)]
    [InlineData("NOT_FOUND", 404)]
    [InlineData("SALE_NOT_FOUND", 404)]
    [InlineData("PRODUCT_ALREADY_DELETED", 404)]
    [InlineData("STOCK_DELETED", 404)]
    [InlineData("PRODUCT_SKU_EXISTS", 409)]
    [InlineData("CONFLICT", 409)]
    [InlineData("PLAN_LIMIT_PRODUCT_COUNT_EXCEEDED", 409)]
    [InlineData("SUBSCRIPTION_INACTIVE", 402)]
    [InlineData("MODULE_NOT_ENABLED", 403)]
    [InlineData("SOME_BUSINESS_RULE", 400)]
    public void Error_code_maps_to_the_expected_status(string code, int expected)
    {
        var result = Result.Failure(new Error(code, "message"));
        ResultEnvelopeMapper.StatusFor(result).Should().Be(expected);
    }

    [Fact]
    public void Status_override_wins_when_it_returns_a_positive_code()
    {
        var result = Result.Failure(new Error("PRODUCT_NOT_FOUND", "gone"));
        ResultEnvelopeMapper.StatusFor(result, _ => 410).Should().Be(410);
    }

    [Fact]
    public void Single_domain_error_produces_one_error_item_with_null_field()
    {
        var result = Result.Failure(new Error("SALE_NOT_FOUND", "Sale 5 was not found."));
        var body = ResultEnvelopeMapper.Failure(result, "trace-3", 404);

        body.Errors.Should().ContainSingle();
        body.Errors[0].Code.Should().Be("SALE_NOT_FOUND");
        body.Errors[0].Message.Should().Be("Sale 5 was not found.");
        body.Errors[0].Field.Should().BeNull();

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(body, Json));
        doc.RootElement.GetProperty("errors")[0].TryGetProperty("field", out _)
            .Should().BeFalse("a null field is omitted from the wire");
    }

    [Fact]
    public void Multi_error_business_failure_keeps_all_errors()
    {
        var result = Result.Failure(new[]
        {
            new Error("RECIPIENT_OPTED_OUT", "The recipient has opted out."),
            new Error("TEMPLATE_NOT_FOUND", "No template for channel Email."),
        });

        result.Errors.Should().HaveCount(2);
        var body = ResultEnvelopeMapper.Failure(result, "trace-4", ResultEnvelopeMapper.StatusFor(result));
        body.Errors.Select(e => e.Code).Should().BeEquivalentTo(new[] { "RECIPIENT_OPTED_OUT", "TEMPLATE_NOT_FOUND" });
    }
}
