namespace SharedKernel;

/// <summary>
/// Shared helpers behind <c>Result.Errors</c> / <c>Result&lt;T&gt;.Errors</c> — a single place that
/// defines how the various failure representations (a single <see cref="Error"/>, a
/// <see cref="ValidationError"/> list from the FluentValidation pipeline, or an explicit
/// multi-<see cref="Error"/> list from a handler's post-validation business checks) all
/// flatten into the one <c>errors[]</c> shape the API envelope serializes.
/// </summary>
internal static class ResultErrors
{
    public static IReadOnlyList<Error> Flatten(
        bool isSuccess,
        Error error,
        IReadOnlyList<ValidationError> validationErrors,
        IReadOnlyList<Error>? explicitErrors)
    {
        if (isSuccess)
            return Array.Empty<Error>();

        if (explicitErrors is { Count: > 0 })
            return explicitErrors;

        if (validationErrors.Count > 0)
        {
            var mapped = new Error[validationErrors.Count];
            for (var i = 0; i < validationErrors.Count; i++)
            {
                var v = validationErrors[i];
                mapped[i] = new Error("VALIDATION_ERROR", v.ErrorMessage, v.PropertyName);
            }
            return mapped;
        }

        return string.IsNullOrEmpty(error.Code) ? Array.Empty<Error>() : new[] { error };
    }

    public static IReadOnlyList<Error> Materialize(IEnumerable<Error> errors)
    {
        var list = errors as IReadOnlyList<Error> ?? errors.ToList();
        if (list.Count == 0)
            throw new ArgumentException("At least one error is required for a failed result.", nameof(errors));
        return list;
    }
}
