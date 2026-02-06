using FluentValidation;

namespace SincoMaquinaria.Infrastructure;

/// <summary>
/// Filtro de endpoint que valida automáticamente los DTOs usando FluentValidation.
/// </summary>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        
        if (validator is null)
        {
            return await next(context);
        }

        // Buscar el argumento del tipo T
        var argument = context.Arguments.FirstOrDefault(a => a is T) as T;
        
        if (argument is null)
        {
            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(argument);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
