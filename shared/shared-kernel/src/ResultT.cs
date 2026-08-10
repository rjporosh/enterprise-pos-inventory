namespace SharedKernel;

public sealed class Result<T> : IResult<T>
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public IReadOnlyList<ValidationError> ValidationErrors { get; } = Array.Empty<ValidationError>();
    public T? Value { get; }

    private Result(bool isSuccess, T? value, Error error, IReadOnlyList<ValidationError>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ValidationErrors = validationErrors ?? Array.Empty<ValidationError>();
    }

    public static Result<T> Success(T value) => new(true, value, Error.None);

    public static Result<T> Failure(Error error) => new(false, default, error);

    public static Result<T> ValidationFailure(IEnumerable<ValidationError> errors)
        => new(false, default, new Error("VALIDATION_ERROR"), errors.ToList());

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result(Result<T> result)
        => result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
}
