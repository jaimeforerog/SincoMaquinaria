using FluentValidation;
using SincoMaquinaria.DTOs.Common;

namespace SincoMaquinaria.Validators;

public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a {ComparisonValue}");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a {ComparisonValue}")
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página no puede exceder {ComparisonValue} elementos");

        RuleFor(x => x.OrderDirection)
            .Must(x => x != null && (x.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                      x.Equals("desc", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("La dirección de ordenamiento debe ser 'asc' o 'desc'");
    }
}