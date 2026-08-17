using System.Text.Json;

namespace SharedKernel;

public readonly record struct Error(string Code, string? Description = null)
{
    public static readonly Error None = new(string.Empty);

    public static implicit operator Result(Error error) => Result.Failure(error);

    public override string ToString() => JsonSerializer.Serialize(new { Code, Description });
}
