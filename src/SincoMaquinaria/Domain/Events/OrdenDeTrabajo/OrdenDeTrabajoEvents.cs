namespace SincoMaquinaria.Domain.Events.OrdenDeTrabajo;

// --- Ciclo de Vida de la Orden de Trabajo ---

/// <summary>
/// Evento emitido cuando se crea una nueva orden de trabajo.
/// </summary>
public record OrdenDeTrabajoCreada(
    Guid OrdenId,
    string NumeroOrden,
    string EquipoId,
    string Origen,
    string TipoMantenimiento,
    DateTime FechaOrden,
    DateTimeOffset FechaCreacion,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

/// <summary>
/// Evento emitido cuando se programa una orden de trabajo para una fecha específica.
/// </summary>
public record OrdenProgramada(
    DateTime FechaProgramada,
    TimeSpan DuracionEstimada,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

/// <summary>
/// Evento emitido cuando se finaliza una orden de trabajo y se aprueba.
/// </summary>
public record OrdenFinalizada(
    string EstadoFinal,
    string AprobadoPor,
    DateTime FechaAprobacion,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

/// <summary>
/// Evento emitido cuando se elimina una orden de trabajo.
/// </summary>
public record OrdenDeTrabajoEliminada(
    Guid OrdenId,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

// --- Actividades de la Orden ---

/// <summary>
/// Evento emitido cuando se agrega una actividad a una orden de trabajo.
/// </summary>
public record ActividadAgregada(
    Guid ItemDetalleId,
    string Descripcion,
    DateTime FechaEstimadaEjecucion,
    int Frecuencia = 0,
    string? TipoFallaId = null,
    string? CausaFallaId = null,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

/// <summary>
/// Evento emitido cuando se registra el avance de una actividad.
/// </summary>
public record AvanceDeActividadRegistrado(
    Guid ItemDetalleId,
    decimal PorcentajeAvance,
    string Observacion,
    string NuevoEstado,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);

// --- Mediciones (Horómetros) ---

/// <summary>
/// Evento emitido cuando se registra una medición de horómetro o contador.
/// </summary>
public record MedicionRegistrada(
    string TipoMedidor,
    decimal ValorMedicion,
    DateTime FechaLectura,
    decimal TrabajaAcumuladoCalculado,
    Guid? UsuarioId = null,
    string? UsuarioNombre = null
);
