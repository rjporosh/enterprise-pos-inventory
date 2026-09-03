namespace SharedKernel;

public interface IResult
{
    bool IsSuccess { get; }
    bool IsFailure => !IsSuccess;
    Error Error { get; }
    IReadOnlyList<ValidationError> ValidationErrors { get; }

    /// <summary>Every failure flattened into the one <c>errors[]</c> shape the API envelope emits; empty on success.</summary>
    IReadOnlyList<Error> Errors { get; }
}
