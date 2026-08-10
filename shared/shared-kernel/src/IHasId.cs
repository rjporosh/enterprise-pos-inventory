namespace SharedKernel;

public interface IHasId<TId> where TId : struct
{
    TId Id { get; }
}
