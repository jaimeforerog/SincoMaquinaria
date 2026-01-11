using FluentValidation;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Validators;

public class CrearOrdenRequestValidator : AbstractValidator<CrearOrdenRequest>
{
    private static readonly string[] TiposValidos = { "Preventivo", "Correctivo" };
    private static readonly string[] OrigenesValidos = { "Interno", "Externo" };

    public CrearOrdenRequestValidator()
    {
        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("El número de orden es requerido")
            .MaximumLength(50).WithMessage("El número de orden no puede exceder 50 caracteres");

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
            .NotEmpty().WithMessage("La descripción de la actividad es requerida")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.FechaEstimada)
            .GreaterThan(DateTime.Now.AddMinutes(-5))
            .WithMessage("La fecha estimada debe ser futura");
    }
}

public class RegistrarAvanceRequestValidator : AbstractValidator<RegistrarAvanceRequest>
{
    private static readonly string[] EstadosValidos = { "Pendiente", "EnProgreso", "Completado", "Cancelado" };

    public RegistrarAvanceRequestValidator()
    {
        RuleFor(x => x.DetalleId)
            .NotEmpty().WithMessage("El ID del detalle es requerido");

        RuleFor(x => x.Porcentaje)
            .InclusiveBetween(0, 100).WithMessage("El porcentaje debe estar entre 0 y 100");

        RuleFor(x => x.NuevoEstado)
            .NotEmpty().WithMessage("El nuevo estado es requerido")
            .Must(e => EstadosValidos.Contains(e))
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", EstadosValidos)}");

        RuleFor(x => x.Observacion)
            .MaximumLength(1000).WithMessage("La observación no puede exceder 1000 caracteres");
    }
}
