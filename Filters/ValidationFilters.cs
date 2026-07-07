using System.ComponentModel.DataAnnotations;

namespace ClinicIntakeApi.Filters;

public class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        T? dto = context.Arguments.OfType<T>().FirstOrDefault();

        if (dto is null)
        {
            return await next(context);
        }

        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            dto,
            new ValidationContext(dto),
            validationResults,
            true
        );

        if (!isValid)
        {
            return Results.ValidationProblem(
                validationResults.ToDictionary(
                    r => r.MemberNames.First(),
                    r => new[] { r.ErrorMessage! }
                )
            );
        }

        return await next(context);
    }
}
