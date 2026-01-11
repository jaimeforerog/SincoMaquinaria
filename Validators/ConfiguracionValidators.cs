using FluentValidation;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Validators;

// Tipos de Medidor
public class CrearTipoMedidorRequestValidator : AbstractValidator<CrearTipoMedidorRequest>
{
    public CrearTipoMedidorRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del tipo de medidor es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Unidad)
            .NotEmpty().WithMessage("La unidad es requerida")
            .MaximumLength(50).WithMessage("La unidad no puede exceder 50 caracteres");
    }
}

public class ActualizarTipoMedidorRequestValidator : AbstractValidator<ActualizarTipoMedidorRequest>
{
    public ActualizarTipoMedidorRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del tipo de medidor es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Unidad)
            .NotEmpty().WithMessage("La unidad es requerida")
            .MaximumLength(50).WithMessage("La unidad no puede exceder 50 caracteres");
    }
}

// Grupos de Mantenimiento
public class CrearGrupoRequestValidator : AbstractValidator<CrearGrupoRequest>
{
    public CrearGrupoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del grupo es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");
    }
}

public class ActualizarGrupoRequestValidator : AbstractValidator<ActualizarGrupoRequest>
{
    public ActualizarGrupoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del grupo es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");
    }
}

// Tipos de Falla
public class CrearTipoFallaRequestValidator : AbstractValidator<CrearTipoFallaRequest>
{
    private static readonly string[] PrioridadesValidas = { "alta", "media", "baja" };

    public CrearTipoFallaRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción del tipo de falla es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres");

        RuleFor(x => x.Prioridad)
            .NotEmpty().WithMessage("La prioridad es requerida")
            .Must(p => PrioridadesValidas.Contains(p.ToLowerInvariant()))
            .WithMessage($"La prioridad debe ser una de: {string.Join(", ", PrioridadesValidas)}");
    }
}

// Causas de Falla
public class CrearCausaFallaRequestValidator : AbstractValidator<CrearCausaFallaRequest>
{
    public CrearCausaFallaRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción de la causa de falla es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres");
    }
}

public class ActualizarCausaFallaRequestValidator : AbstractValidator<ActualizarCausaFallaRequest>
{
    public ActualizarCausaFallaRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción de la causa de falla es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres");
    }
}
