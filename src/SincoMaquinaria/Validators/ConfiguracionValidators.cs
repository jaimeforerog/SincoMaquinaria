using FluentValidation;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Validators;

// Tipos de Medidor
public class CrearTipoMedidorRequestValidator : AbstractValidator<CrearTipoMedidorRequest>
{
    public CrearTipoMedidorRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Unidad)
            .NotEmpty().WithMessage("La unidad es requerida")
            .MaximumLength(50).WithMessage("La unidad no puede exceder {MaxLength} caracteres");
    }
}

public class ActualizarTipoMedidorRequestValidator : AbstractValidator<ActualizarTipoMedidorRequest>
{
    public ActualizarTipoMedidorRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Unidad)
            .NotEmpty().WithMessage("La unidad es requerida")
            .MaximumLength(50).WithMessage("La unidad no puede exceder {MaxLength} caracteres");
    }
}

// Grupos de Mantenimiento
public class CrearGrupoRequestValidator : AbstractValidator<CrearGrupoRequest>
{
    public CrearGrupoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder {MaxLength} caracteres");
    }
}

public class ActualizarGrupoRequestValidator : AbstractValidator<ActualizarGrupoRequest>
{
    public ActualizarGrupoRequestValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede exceder {MaxLength} caracteres");
    }
}

// Tipos de Falla
public class CrearTipoFallaRequestValidator : AbstractValidator<CrearTipoFallaRequest>
{
    private static readonly string[] PrioridadesValidas = { "alta", "media", "baja" };

    public CrearTipoFallaRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder {MaxLength} caracteres");

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
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder {MaxLength} caracteres");
    }
}

public class ActualizarCausaFallaRequestValidator : AbstractValidator<ActualizarCausaFallaRequest>
{
    public ActualizarCausaFallaRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder {MaxLength} caracteres");
    }
}
