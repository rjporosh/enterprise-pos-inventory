namespace SharedKernel;

public static class Guard
{
    public static T NotNull<T>(T? value, string parameterName)
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
        return value;
    }

    public static string NotNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
        return value;
    }

    public static T NotNegative<T>(T value, string parameterName) where T : struct, IComparable<T>
    {
        if (value.CompareTo(default(T)) < 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} cannot be negative.");
        return value;
    }

    public static void GreaterThan<T>(T value, T max, string parameterName) where T : struct, IComparable<T>
    {
        if (value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be less than or equal to {max}.");
    }
}
