namespace SharedKernel;

public sealed class Result<T> : IResult<T>
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; } = Array.Empty<ValidationError>();
    public T? Value { get; }

    private readonly IReadOnlyList<Error>? _errors;

    private Result(bool isSuccess, T? value, Error error, IReadOnlyList<ValidationError>? validationErrors = null, IReadOnlyList<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ValidationErrors = validationErrors ?? Array.Empty<ValidationError>();
        _errors = errors;
    }

    /// <summary>
    /// Every failure represented as a flat list, regardless of which factory produced it —
    /// the shape the API error envelope emits. Success yields an empty list.
    /// </summary>
    public IReadOnlyList<Error> Errors => ResultErrors.Flatten(IsSuccess, Error, ValidationErrors, _errors);

    public static Result<T> Success(T value) => new(true, value, Error.None);

    public static Result<T> Failure(Error error) => new(false, default, error);

    public static Result<T> Failure(IEnumerable<Error> errors)
    {
        var list = ResultErrors.Materialize(errors);
        return new Result<T>(false, default, list[0], errors: list);
    }

    public static Result<T> ValidationFailure(IEnumerable<ValidationError> errors)
        => new(false, default, new Error("VALIDATION_ERROR"), errors.ToList());

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result(Result<T> result)
    {
        if (result.IsSuccess)
            return Result.Success();
        if (result.ValidationErrors.Count > 0)
            return Result.ValidationFailure(result.ValidationErrors);
        return result._errors is { Count: > 0 }
            ? Result.Failure(result._errors)
            : Result.Failure(result.Error);
    }
}
