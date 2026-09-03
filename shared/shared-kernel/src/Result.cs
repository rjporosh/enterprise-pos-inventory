namespace SharedKernel;

public sealed class Result : IResult
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; } = Array.Empty<ValidationError>();

    private readonly IReadOnlyList<Error>? _errors;

    private Result(bool isSuccess, Error error, IReadOnlyList<ValidationError>? validationErrors = null, IReadOnlyList<Error>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? Array.Empty<ValidationError>();
        _errors = errors;
    }

    /// <summary>
    /// Every failure represented as a flat list, regardless of which factory produced it —
    /// the shape the API error envelope emits. Success yields an empty list.
    /// </summary>
    public IReadOnlyList<Error> Errors => ResultErrors.Flatten(IsSuccess, Error, ValidationErrors, _errors);

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result Failure(IEnumerable<Error> errors)
    {
        var list = ResultErrors.Materialize(errors);
        return new Result(false, list[0], errors: list);
    }

    public static Result ValidationFailure(IEnumerable<ValidationError> errors)
        => new(false, new Error("VALIDATION_ERROR"), errors.ToList());
}
