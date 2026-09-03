using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using SharedWeb;
using Xunit;

namespace SharedWeb.Tests;

public class PlatformExceptionHandlerTests
{
    private sealed class FakeEnv : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "test";
        public string ContentRootPath { get; set; } = "/";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    private sealed class ThrowMapper(Type match, ExceptionMapping mapping) : IExceptionMapper
    {
        public ExceptionMapping? TryMap(Exception exception) => match.IsInstanceOfType(exception) ? mapping : null;
    }

    private static async Task<(int status, JsonElement body, string contentType)> Handle(
        Exception ex, IHostEnvironment? env = null, params IExceptionMapper[] mappers)
    {
        var handler = new PlatformExceptionHandler(env ?? new FakeEnv(), NullLogger<PlatformExceptionHandler>.Instance, mappers);
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path = "/api/v1/things";
        var stream = new MemoryStream();
        ctx.Response.Body = stream;

        var handled = await handler.TryHandleAsync(ctx, ex, CancellationToken.None);
        handled.Should().BeTrue();

        stream.Position = 0;
        var text = await new StreamReader(stream).ReadToEndAsync();
        using var doc = JsonDocument.Parse(text);
        return (ctx.Response.StatusCode, doc.RootElement.Clone(), ctx.Response.ContentType ?? "");
    }

    [Fact]
    public async Task Validation_exception_becomes_400_with_every_error()
    {
        var ex = new ValidationException(new[]
        {
            new ValidationFailure("Email", "Email is required."),
            new ValidationFailure("Password", "Password is too short."),
        });

        var (status, body, contentType) = await Handle(ex);

        status.Should().Be(400);
        contentType.Should().StartWith("application/json");
        body.GetProperty("success").GetBoolean().Should().BeFalse();
        body.GetProperty("errors").GetArrayLength().Should().Be(2);
        body.GetProperty("errors")[0].GetProperty("field").GetString().Should().Be("Email");
        body.GetProperty("errors")[1].GetProperty("code").GetString().Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task Registered_mapper_wins()
    {
        var mapper = new ThrowMapper(typeof(InvalidOperationException),
            new ExceptionMapping(423, "ACCOUNT_LOCKED", "This account is locked."));

        var (status, body, _) = await Handle(new InvalidOperationException("internal detail"), null, mapper);

        status.Should().Be(423);
        body.GetProperty("errors")[0].GetProperty("code").GetString().Should().Be("ACCOUNT_LOCKED");
        body.GetProperty("message").GetString().Should().Be("This account is locked.");
    }

    [Fact]
    public async Task Unhandled_exception_is_a_scrubbed_500_in_production()
    {
        var (status, body, contentType) = await Handle(new InvalidOperationException("connection string=secret;password=hunter2"));

        status.Should().Be(500);
        contentType.Should().StartWith("application/problem+json");
        body.GetProperty("detail").GetString().Should().Be("An unexpected error occurred.");
        JsonSerializer.Serialize(body).Should().NotContain("hunter2");
        body.GetProperty("traceId").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Unhandled_exception_shows_detail_only_in_development()
    {
        var (status, body, _) = await Handle(
            new InvalidOperationException("boom"), new FakeEnv { EnvironmentName = "Development" });

        status.Should().Be(500);
        body.GetProperty("detail").GetString().Should().Contain("boom");
    }

    [Fact]
    public async Task Timeout_maps_to_504()
    {
        var (status, _, _) = await Handle(new TimeoutException("db timed out"));
        status.Should().Be(504);
    }
}
