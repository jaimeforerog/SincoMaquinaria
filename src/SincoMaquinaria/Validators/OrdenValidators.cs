using FluentValidation;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Domain;

namespace SincoMaquinaria.Validators;

public class CrearOrdenRequestValidator : AbstractValidator<CrearOrdenRequest>
{
    private static readonly string[] TiposValidos = TipoMantenimientoConstants.Todos;
    private static readonly string[] OrigenesValidos = { "Interno", "Externo", "Manual", "Planificacion" };

    public CrearOrdenRequestValidator()
    {
        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("El número de orden es requerido")
            .MaximumLength(50).WithMessage("El número de orden no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.EquipoId)
            .NotEmpty().WithMessage("El ID del equipo es requerido");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo de mantenimiento es requerido")
            .Must(t => TiposValidos.Contains(t))
            .WithMessage($"El tipo debe ser uno de: {string.Join(", ", TiposValidos)}");

        RuleFor(x => x.Origen)
            .NotEmpty().WithMessage("El origen es requerido")
            .Must(o => OrigenesValidos.Contains(o))
            .WithMessage($"El origen debe ser uno de: {string.Join(", ", OrigenesValidos)}");

        RuleFor(x => x.FechaOrden)
            .GreaterThan(DateTime.MinValue)
            .When(x => x.FechaOrden.HasValue)
            .WithMessage("La fecha de orden no es válida");
    }
}

public class AgregarActividadRequestValidator : AbstractValidator<AgregarActividadRequest>
{
    public AgregarActividadRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(500).WithMessage("La descripción no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.FechaEstimada)
            .GreaterThan(DateTime.Now.AddMinutes(-5))
            .WithMessage("La fecha estimada debe ser futura");
    }
}

public class RegistrarAvanceRequestValidator : AbstractValidator<RegistrarAvanceRequest>
{
    public RegistrarAvanceRequestValidator()
    {
        RuleFor(x => x.DetalleId)
            .NotEmpty().WithMessage("El ID del detalle es requerido");

        RuleFor(x => x.Porcentaje)
            .InclusiveBetween(0, 100).WithMessage("El porcentaje debe estar entre {From} y {To}");

        RuleFor(x => x.NuevoEstado)
            .NotEmpty().WithMessage("El nuevo estado es requerido")
            .Must(e => e.IsValidEnum<EstadoDetalleOrden>())
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", EnumExtensions.GetEnumValues<EstadoDetalleOrden>())}");

        RuleFor(x => x.Observacion)
            .MaximumLength(1000).WithMessage("La observación no puede exceder {MaxLength} caracteres");
    }
}
