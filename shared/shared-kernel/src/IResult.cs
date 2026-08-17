namespace SharedKernel;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure => !IsSuccess;
    Error Error { get; }
    IReadOnlyList<ValidationError> ValidationErrors { get; }
}
