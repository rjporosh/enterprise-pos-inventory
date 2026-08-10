namespace SharedKernel;

public interface IResult<out T> : IResult
{
    T? Value { get; }
}
