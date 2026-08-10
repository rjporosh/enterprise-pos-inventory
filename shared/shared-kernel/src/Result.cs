namespace SharedKernel;

public sealed class Result : IResult
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; } = Array.Empty<ValidationError>();

    private Result(bool isSuccess, Error error, IReadOnlyList<ValidationError>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? Array.Empty<ValidationError>();
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result ValidationFailure(IEnumerable<ValidationError> errors)
        => new(false, new Error("VALIDATION_ERROR"), errors.ToList());
}
