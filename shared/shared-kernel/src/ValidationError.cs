namespace SharedKernel;

public class ValidationError
{
    public string PropertyName { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public string? AttemptedValue { get; init; }

    public static ValidationError Create(string propertyName, string errorMessage, string? attemptedValue = null)
        => new() { PropertyName = propertyName, ErrorMessage = errorMessage, AttemptedValue = attemptedValue };
}
