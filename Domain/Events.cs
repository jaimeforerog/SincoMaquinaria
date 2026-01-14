using System;
using SincoMaquinaria.Domain;

namespace SincoMaquinaria.Domain.Events;

// --- Ciclo de Vida ---

public record OrdenDeTrabajoCreada(Guid OrdenId, string NumeroOrden, string EquipoId, string Origen, string TipoMantenimiento, DateTime FechaOrden, DateTimeOffset FechaCreacion, Guid? UsuarioId = null, string? UsuarioNombre = null);

public record OrdenProgramada(DateTime FechaProgramada, TimeSpan DuracionEstimada, Guid? UsuarioId = null, string? UsuarioNombre = null);

public record OrdenFinalizada(string EstadoFinal, string AprobadoPor, DateTime FechaAprobacion, Guid? UsuarioId = null, string? UsuarioNombre = null);

public record OrdenDeTrabajoEliminada(Guid OrdenId, Guid? UsuarioId = null, string? UsuarioNombre = null);

// --- Actividades ---

public record ActividadAgregada(Guid ItemDetalleId, string Descripcion, DateTime FechaEstimadaEjecucion, int Frecuencia = 0, string? TipoFallaId = null, string? CausaFallaId = null, Guid? UsuarioId = null, string? UsuarioNombre = null);

public record AvanceDeActividadRegistrado(Guid ItemDetalleId, decimal PorcentajeAvance, string Observacion, string NuevoEstado, Guid? UsuarioId = null, string? UsuarioNombre = null);

// --- Mediciones (Horómetros) ---

public record MedicionRegistrada(string TipoMedidor, decimal ValorMedicion, DateTime FechaLectura, decimal TrabajaAcumuladoCalculado);

// --- Configuración Global (Tipos de Medidor) ---

public record TipoMedidorCreado(string Codigo, string Nombre, string Unidad, Guid? UsuarioId = null, string? UsuarioNombre = null, DateTimeOffset? FechaCreacion = null);

public record EstadoTipoMedidorCambiado(string Codigo, bool Activo, Guid? UsuarioId = null, string? UsuarioNombre = null);

// --- Equipos ---

public record EquipoCreado(Guid Id, string Placa, string Descripcion, string Marca, string Modelo, string Serie, string Codigo, string TipoMedidorId, string TipoMedidorId2, string Grupo, string Rutina, Guid? UsuarioId = null, string? UsuarioNombre = null, DateTimeOffset? FechaCreacion = null);
public record EquipoMigrado(Guid Id, string Placa, string Descripcion, string Marca, string Modelo, string Serie, string Codigo, string TipoMedidorId, string TipoMedidorId2, string Grupo, string Rutina, Guid? UsuarioId = null, string? UsuarioNombre = null, DateTimeOffset? FechaCreacion = null);
public record EquipoActualizado(Guid Id, string Descripcion, string Marca, string Modelo, string Serie, string Codigo, string TipoMedidorId, string TipoMedidorId2, string Grupo, string Rutina, Guid? UsuarioId = null, string? UsuarioNombre = null);


public record GrupoMantenimientoCreado(string Codigo, string Nombre, string Descripcion, bool Activo, Guid? UsuarioId = null, string? UsuarioNombre = null, DateTimeOffset? FechaCreacion = null);
public record EstadoGrupoMantenimientoCambiado(string Codigo, bool Activo, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record GrupoMantenimientoActualizado(string Codigo, string Nombre, string Descripcion, Guid? UsuarioId = null, string? UsuarioNombre = null);

public record TipoMedidorActualizado(string Codigo, string Nombre, string Unidad, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record TipoFallaCreado(string Codigo, string Descripcion, string Prioridad, Guid? UsuarioId = null, string? UsuarioNombre = null, DateTimeOffset? FechaCreacion = null);

// --- Causas de Falla ---

public record CausaFallaCreada(string Codigo, string Descripcion, Guid? UsuarioId = null, string? UsuarioNombre = null, DateTimeOffset? FechaCreacion = null);
public record CausaFallaActualizada(string Codigo, string Descripcion, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record EstadoCausaFallaCambiado(string Codigo, bool Activo, Guid? UsuarioId = null, string? UsuarioNombre = null);

// --- Empleados ---

public record EmpleadoCreado(Guid Id, string Nombre, string Identificacion, string Cargo, string Especialidad, decimal ValorHora, string Estado, Guid? UsuarioId = null, string? UsuarioNombre = null, DateTimeOffset? FechaCreacion = null);
public record EmpleadoActualizado(Guid Id, string Nombre, string Identificacion, string Cargo, string Especialidad, decimal ValorHora, string Estado, Guid? UsuarioId = null, string? UsuarioNombre = null);

// --- Usuarios (Autenticación) ---

public record UsuarioCreado(Guid Id, string Email, string PasswordHash, string Nombre, RolUsuario Rol, DateTime FechaCreacion);
public record UsuarioActualizado(Guid Id, string Nombre, string? PasswordHash);
public record UsuarioDesactivado(Guid Id);
