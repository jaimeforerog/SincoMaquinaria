namespace SincoMaquinaria.Domain.Events.Equipo;

/// <summary>
/// Evento emitido cuando se crea un nuevo equipo/maquinaria.
/// </summary>
public record EquipoCreado(
    Guid Id,
    string Placa,
    string Descripcion,
    string Marca,
    string Modelo,
    string Serie,
    string Codigo,
    string TipoMedidorId,
    string TipoMedidorId2,
    string Grupo,
    string Rutina,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaCreacion = null
);

/// <summary>
/// Evento emitido cuando se migra un equipo desde un sistema legacy.
/// </summary>
public record EquipoMigrado(
    Guid Id,
    string Placa,
    string Descripcion,
    string Marca,
    string Modelo,
    string Serie,
    string Codigo,
    string TipoMedidorId,
    string TipoMedidorId2,
    string Grupo,
    string Rutina,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaCreacion = null
);

/// <summary>
/// Evento emitido cuando se actualizan los datos de un equipo existente.
/// </summary>
public record EquipoActualizado(
    Guid Id,
    string Descripcion,
    string Marca,
    string Modelo,
    string Serie,
    string Codigo,
    string TipoMedidorId,
    string TipoMedidorId2,
    string Grupo,
    string Rutina,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaModificacion = null
);
