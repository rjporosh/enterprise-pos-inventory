using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Linq;

namespace SharedInfrastructure.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var validatorList = validators as IReadOnlyList<IValidator<TRequest>> ?? validators.ToList();
        if (validatorList.Count == 0)
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct)));
        var errors = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (errors.Count == 0)
            return await next();

        foreach (var error in errors)
        {
            logger.LogWarning("Validation failure for {RequestType}: {PropertyName} - {ErrorMessage}", typeof(TRequest).Name, error.PropertyName, error.ErrorMessage);
        }

        var validationErrors = errors.Select(e => SharedKernel.ValidationError.Create(e.PropertyName, e.ErrorMessage, e.AttemptedValue?.ToString())).ToList();

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = typeof(Result<>).MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                .GetMethod(nameof(Result<object>.ValidationFailure), new[] { typeof(IEnumerable<SharedKernel.ValidationError>) });
            if (failureMethod is not null)
            {
                var result = failureMethod.Invoke(null, new object[] { validationErrors });
                if (result is not null) return (TResponse)result;
            }
        }

        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)SharedKernel.Result.ValidationFailure(validationErrors);

        throw new ValidationException(errors);
    }
}
