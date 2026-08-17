using System.Text.Json;

namespace SharedKernel;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(1, (current, obj) => HashCode.Combine(current, obj.GetHashCode()));
    }

    public static bool operator ==(ValueObject? first, ValueObject? second)
    {
        if (first is null && second is null) return true;
        if (first is null || second is null) return false;
        return first.Equals(second);
    }

    public static bool operator !=(ValueObject? first, ValueObject? second) => !(first == second);
}
