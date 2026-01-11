using FluentValidation;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Validators;

public class CrearEmpleadoRequestValidator : AbstractValidator<CrearEmpleadoRequest>
{
    private static readonly string[] EstadosValidos = { "Activo", "Inactivo" };
    private static readonly string[] CargosValidos = { "Conductor", "Operario", "Mecanico" };

    public CrearEmpleadoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del empleado es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Identificacion)
            .NotEmpty().WithMessage("La identificación es requerida")
            .MaximumLength(50).WithMessage("La identificación no puede exceder 50 caracteres");

        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("El cargo es requerido")
            .Must(c => CargosValidos.Contains(c))
            .WithMessage($"El cargo debe ser uno de: {string.Join(", ", CargosValidos)}");

        RuleFor(x => x.Especialidad)
            .MaximumLength(100).WithMessage("La especialidad no puede exceder 100 caracteres");

        RuleFor(x => x.ValorHora)
            .GreaterThanOrEqualTo(0).WithMessage("El valor por hora debe ser mayor o igual a cero");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("El estado es requerido")
            .Must(e => EstadosValidos.Contains(e))
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", EstadosValidos)}");
    }
}

public class ActualizarEmpleadoRequestValidator : AbstractValidator<ActualizarEmpleadoRequest>
{
    private static readonly string[] EstadosValidos = { "Activo", "Inactivo" };
    private static readonly string[] CargosValidos = { "Conductor", "Operario", "Mecanico" };

    public ActualizarEmpleadoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del empleado es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Identificacion)
            .NotEmpty().WithMessage("La identificación es requerida")
            .MaximumLength(50).WithMessage("La identificación no puede exceder 50 caracteres");

        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("El cargo es requerido")
            .Must(c => CargosValidos.Contains(c))
            .WithMessage($"El cargo debe ser uno de: {string.Join(", ", CargosValidos)}");

        RuleFor(x => x.Especialidad)
            .MaximumLength(100).WithMessage("La especialidad no puede exceder 100 caracteres");

        RuleFor(x => x.ValorHora)
            .GreaterThanOrEqualTo(0).WithMessage("El valor por hora debe ser mayor o igual a cero");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("El estado es requerido")
            .Must(e => EstadosValidos.Contains(e))
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", EstadosValidos)}");
    }
}
