using FluentValidation;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Validators;

public class ActualizarEquipoRequestValidator : AbstractValidator<ActualizarEquipoRequest>
{
    public ActualizarEquipoRequestValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción del equipo es requerida")
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres");

        RuleFor(x => x.Marca)
            .MaximumLength(100).WithMessage("La marca no puede exceder 100 caracteres");

        RuleFor(x => x.Modelo)
            .MaximumLength(100).WithMessage("El modelo no puede exceder 100 caracteres");

        RuleFor(x => x.Serie)
            .MaximumLength(100).WithMessage("El número de serie no puede exceder 100 caracteres");

        RuleFor(x => x.Codigo)
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres");

        RuleFor(x => x.Grupo)
            .MaximumLength(100).WithMessage("El grupo no puede exceder 100 caracteres");

        RuleFor(x => x.Rutina)
            .MaximumLength(100).WithMessage("La rutina no puede exceder 100 caracteres");
    }
}
