using FluentValidation;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Validators;

public class CreateRutinaRequestValidator : AbstractValidator<CreateRutinaRequest>
{
    public CreateRutinaRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Grupo)
            .NotEmpty().WithMessage("El grupo es requerido")
            .MaximumLength(100).WithMessage("El grupo no puede exceder {MaxLength} caracteres");
    }
}

public class UpdateRutinaRequestValidator : AbstractValidator<UpdateRutinaRequest>
{
    public UpdateRutinaRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Grupo)
            .NotEmpty().WithMessage("El grupo es requerido")
            .MaximumLength(100).WithMessage("El grupo no puede exceder {MaxLength} caracteres");
    }
}

public class UpdateParteRequestValidator : AbstractValidator<UpdateParteRequest>
{
    public UpdateParteRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder {MaxLength} caracteres");
    }
}

public class AddParteRequestValidator : AbstractValidator<AddParteRequest>
{
    public AddParteRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder {MaxLength} caracteres");
    }
}

public class UpdateActividadRequestValidator : AbstractValidator<UpdateActividadRequest>
{
    public UpdateActividadRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(300).WithMessage("La descripción no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Clase)
            .NotEmpty().WithMessage("La clase de actividad es requerida")
            .MaximumLength(100).WithMessage("La clase no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Frecuencia)
            .GreaterThanOrEqualTo(0).WithMessage("La frecuencia debe ser mayor o igual a {ComparisonValue}");

        RuleFor(x => x.UnidadMedida)
            .NotEmpty().WithMessage("La unidad de medida es requerida")
            .MaximumLength(50).WithMessage("La unidad de medida no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.NombreMedidor)
            .NotEmpty().WithMessage("El nombre del medidor es requerido")
            .MaximumLength(100).WithMessage("El nombre del medidor no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.AlertaFaltando)
            .GreaterThanOrEqualTo(0).WithMessage("La alerta debe ser mayor o igual a {ComparisonValue}");

        RuleFor(x => x.Frecuencia2)
            .GreaterThanOrEqualTo(0).WithMessage("La frecuencia II debe ser mayor o igual a {ComparisonValue}");

        RuleFor(x => x.UnidadMedida2)
            .MaximumLength(50).WithMessage("La unidad de medida II no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.NombreMedidor2)
            .MaximumLength(100).WithMessage("El nombre del medidor II no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.AlertaFaltando2)
            .GreaterThanOrEqualTo(0).WithMessage("La alerta II debe ser mayor o igual a {ComparisonValue}");

        RuleFor(x => x.Cantidad)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad debe ser mayor o igual a {ComparisonValue}");
    }
}

public class AddActividadRequestValidator : AbstractValidator<AddActividadRequest>
{
    public AddActividadRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es requerida")
            .MaximumLength(300).WithMessage("La descripción no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Clase)
            .NotEmpty().WithMessage("La clase de actividad es requerida")
            .MaximumLength(100).WithMessage("La clase no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.Frecuencia)
            .GreaterThanOrEqualTo(0).WithMessage("La frecuencia debe ser mayor o igual a {ComparisonValue}");

        RuleFor(x => x.UnidadMedida)
            .NotEmpty().WithMessage("La unidad de medida es requerida")
            .MaximumLength(50).WithMessage("La unidad de medida no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.NombreMedidor)
            .NotEmpty().WithMessage("El nombre del medidor es requerido")
            .MaximumLength(100).WithMessage("El nombre del medidor no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.AlertaFaltando)
            .GreaterThanOrEqualTo(0).WithMessage("La alerta debe ser mayor o igual a {ComparisonValue}");

        RuleFor(x => x.Frecuencia2)
            .GreaterThanOrEqualTo(0).WithMessage("La frecuencia II debe ser mayor o igual a {ComparisonValue}");

        RuleFor(x => x.UnidadMedida2)
            .MaximumLength(50).WithMessage("La unidad de medida II no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.NombreMedidor2)
            .MaximumLength(100).WithMessage("El nombre del medidor II no puede exceder {MaxLength} caracteres");

        RuleFor(x => x.AlertaFaltando2)
            .GreaterThanOrEqualTo(0).WithMessage("La alerta II debe ser mayor o igual a {ComparisonValue}");

        RuleFor(x => x.Cantidad)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad debe ser mayor o igual a {ComparisonValue}");
    }
}
