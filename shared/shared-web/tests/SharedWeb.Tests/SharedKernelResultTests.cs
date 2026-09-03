using FluentAssertions;
using SharedKernel;
using Xunit;

namespace SharedWeb.Tests;

public class SharedKernelResultTests
{
    [Fact]
    public void Error_is_still_constructible_the_old_ways()
    {
        var a = new Error("CODE");
        var b = new Error("CODE", "description");
        var c = new Error("CODE", "description", "field");

        a.Description.Should().BeNull();
        a.Field.Should().BeNull();
        b.Description.Should().Be("description");
        b.Field.Should().BeNull();
        c.Field.Should().Be("field");
    }

    [Fact]
    public void Success_result_has_no_errors()
    {
        Result.Success().Errors.Should().BeEmpty();
        Result<int>.Success(1).Errors.Should().BeEmpty();
    }

    [Fact]
    public void Single_failure_flattens_to_one_error()
    {
        var r = Result.Failure(new Error("X", "boom"));
        r.Errors.Should().ContainSingle();
        r.Errors[0].Code.Should().Be("X");
        r.Error.Code.Should().Be("X");
    }

    [Fact]
    public void Validation_failure_flattens_every_validation_error()
    {
        var r = Result<Guid>.ValidationFailure(new[]
        {
            ValidationError.Create("A", "a bad"),
            ValidationError.Create("B", "b bad"),
        });

        r.Errors.Should().HaveCount(2);
        r.Errors.Should().OnlyContain(e => e.Code == "VALIDATION_ERROR");
        r.Errors.Select(e => e.Field).Should().BeEquivalentTo(new[] { "A", "B" });
        r.Error.Code.Should().Be("VALIDATION_ERROR"); // unchanged — existing handler/test assertions still hold
    }

    [Fact]
    public void Multi_error_failure_is_preserved_and_first_error_is_exposed_as_Error()
    {
        var r = Result<string>.Failure(new[] { new Error("FIRST", "1"), new Error("SECOND", "2") });
        r.Errors.Should().HaveCount(2);
        r.Error.Code.Should().Be("FIRST");
    }

    [Fact]
    public void Empty_error_list_is_rejected()
    {
        var act = () => Result.Failure(Array.Empty<Error>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Generic_to_nongeneric_conversion_keeps_validation_errors()
    {
        Result<int> generic = Result<int>.ValidationFailure(new[] { ValidationError.Create("A", "bad") });
        Result nonGeneric = generic;
        nonGeneric.IsSuccess.Should().BeFalse();
        nonGeneric.ValidationErrors.Should().ContainSingle();
        nonGeneric.Errors.Should().ContainSingle();
    }
}
