namespace SincoMaquinaria.Domain.Events.ConfiguracionGlobal;

// --- Tipos de Medidor ---

/// <summary>
/// Evento emitido cuando se crea un nuevo tipo de medidor (horómetro, odómetro, etc).
/// </summary>
public record TipoMedidorCreado(
    string Codigo,
    string Nombre,
    string Unidad,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaCreacion = null
);

/// <summary>
/// Evento emitido cuando se actualiza un tipo de medidor existente.
/// </summary>
public record TipoMedidorActualizado(
    string Codigo,
    string Nombre,
    string Unidad,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

/// <summary>
/// Evento emitido cuando se cambia el estado (activo/inactivo) de un tipo de medidor.
/// </summary>
public record EstadoTipoMedidorCambiado(
    string Codigo,
    bool Activo,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

// --- Grupos de Mantenimiento ---

/// <summary>
/// Evento emitido cuando se crea un nuevo grupo de mantenimiento.
/// </summary>
public record GrupoMantenimientoCreado(
    string Codigo,
    string Nombre,
    string Descripcion,
    bool Activo,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaCreacion = null
);

/// <summary>
/// Evento emitido cuando se actualiza un grupo de mantenimiento.
/// </summary>
public record GrupoMantenimientoActualizado(
    string Codigo,
    string Nombre,
    string Descripcion,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

/// <summary>
/// Evento emitido cuando se cambia el estado de un grupo de mantenimiento.
/// </summary>
public record EstadoGrupoMantenimientoCambiado(
    string Codigo,
    bool Activo,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

// --- Tipos de Falla ---

/// <summary>
/// Evento emitido cuando se crea un nuevo tipo de falla.
/// </summary>
public record TipoFallaCreado(
    string Codigo,
    string Descripcion,
    string Prioridad,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaCreacion = null
);

// --- Causas de Falla ---

/// <summary>
/// Evento emitido cuando se crea una nueva causa de falla.
/// </summary>
public record CausaFallaCreada(
    string Codigo,
    string Descripcion,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null,
    DateTimeOffset? FechaCreacion = null
);

/// <summary>
/// Evento emitido cuando se actualiza una causa de falla.
/// </summary>
public record CausaFallaActualizada(
    string Codigo,
    string Descripcion,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

/// <summary>
/// Evento emitido cuando se cambia el estado de una causa de falla.
/// </summary>
public record EstadoCausaFallaCambiado(
    string Codigo,
    bool Activo,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);
