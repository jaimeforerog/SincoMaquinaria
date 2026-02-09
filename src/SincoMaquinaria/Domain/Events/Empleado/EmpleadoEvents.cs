namespace SincoMaquinaria.Domain.Events.Empleado;

/// <summary>
/// Evento emitido cuando se crea un nuevo empleado.
/// </summary>
public record EmpleadoCreado(
    Guid Id,
    string Nombre,
    string Identificacion,
    string Cargo,
    string Especialidad,
    decimal ValorHora,
    string Estado,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaCreacion = null
);

/// <summary>
/// Evento emitido cuando se actualizan los datos de un empleado.
/// </summary>
public record EmpleadoActualizado(
    Guid Id,
    string Nombre,
    string Identificacion,
    string Cargo,
    string Especialidad,
    decimal ValorHora,
    string Estado,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaModificacion = null
);
