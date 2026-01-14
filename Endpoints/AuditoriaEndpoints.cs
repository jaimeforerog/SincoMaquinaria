using Marten;
using Marten.Events;
using Marten.Pagination;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Domain;

namespace SincoMaquinaria.Endpoints;

public static class AuditoriaEndpoints
{
    // Mapeo de tipos de evento a módulos
    private static readonly Dictionary<string, string> EventoModuloMap = new()
    {
        // Órdenes de Trabajo
        ["OrdenDeTrabajoCreada"] = "Órdenes",
        ["orden_de_trabajo_creada"] = "Órdenes",
        ["OrdenDeTrabajoEliminada"] = "Órdenes",
        ["orden_de_trabajo_eliminada"] = "Órdenes",
        ["OrdenProgramada"] = "Órdenes",
        ["orden_programada"] = "Órdenes",
        ["OrdenFinalizada"] = "Órdenes",
        ["orden_finalizada"] = "Órdenes",
        ["ActividadAgregada"] = "Órdenes",
        ["actividad_agregada"] = "Órdenes",
        ["AvanceDeActividadRegistrado"] = "Órdenes",
        ["avance_de_actividad_registrado"] = "Órdenes",
        // Equipos
        ["EquipoMigrado"] = "Equipos",
        ["equipo_migrado"] = "Equipos",
        ["EquipoActualizado"] = "Equipos",
        ["equipo_actualizado"] = "Equipos",
        ["MedicionRegistrada"] = "Equipos",
        ["medicion_registrada"] = "Equipos",
        ["EquipoCreado"] = "Equipos",
        ["equipo_creado"] = "Equipos",
        // Empleados
        ["EmpleadoCreado"] = "Empleados",
        ["empleado_creado"] = "Empleados",
        ["EmpleadoActualizado"] = "Empleados",
        ["empleado_actualizado"] = "Empleados",
        // Configuración
        ["TipoMedidorCreado"] = "Configuración",
        ["tipo_medidor_creado"] = "Configuración",
        ["TipoMedidorActualizado"] = "Configuración",
        ["tipo_medidor_actualizado"] = "Configuración",
        ["EstadoTipoMedidorCambiado"] = "Configuración",
        ["estado_tipo_medidor_cambiado"] = "Configuración",
        ["GrupoMantenimientoCreado"] = "Configuración",
        ["grupo_mantenimiento_creado"] = "Configuración",
        ["GrupoMantenimientoActualizado"] = "Configuración",
        ["grupo_mantenimiento_actualizado"] = "Configuración",
        ["EstadoGrupoMantenimientoCambiado"] = "Configuración",
        ["estado_grupo_mantenimiento_cambiado"] = "Configuración",
        ["TipoFallaCreado"] = "Configuración",
        ["tipo_falla_creado"] = "Configuración",
        ["CausaFallaCreada"] = "Configuración",
        ["causa_falla_creada"] = "Configuración",
        ["CausaFallaActualizada"] = "Configuración",
        ["causa_falla_actualizada"] = "Configuración",
        ["EstadoCausaFallaCambiado"] = "Configuración",
        ["estado_causa_falla_cambiado"] = "Configuración",
        // Usuarios
        ["UsuarioCreado"] = "Usuarios",
        ["usuario_creado"] = "Usuarios",
        ["UsuarioActualizado"] = "Usuarios",
        ["usuario_actualizado"] = "Usuarios",
        ["UsuarioDesactivado"] = "Usuarios",
        ["usuario_desactivado"] = "Usuarios",
        // Rutinas de Mantenimiento
        ["RutinaCreada"] = "Rutinas",
        ["rutina_creada"] = "Rutinas",
        ["RutinaMigrada"] = "Rutinas",
        ["rutina_migrada"] = "Rutinas",
        ["ParteDeRutinaMigrada"] = "Rutinas",
        ["parte_de_rutina_migrada"] = "Rutinas",
        ["ActividadDeRutinaMigrada"] = "Rutinas",
        ["actividad_de_rutina_migrada"] = "Rutinas",
        ["RutinaActualizada"] = "Rutinas",
        ["rutina_actualizada"] = "Rutinas",
        ["ParteAgregada"] = "Rutinas",
        ["parte_agregada"] = "Rutinas",
        ["ParteActualizada"] = "Rutinas",
        ["parte_actualizada"] = "Rutinas",
        ["ParteEliminada"] = "Rutinas",
        ["parte_eliminada"] = "Rutinas",
        ["ActividadDeRutinaAgregada"] = "Rutinas",
        ["actividad_de_rutina_agregada"] = "Rutinas",
        ["ActividadDeRutinaActualizada"] = "Rutinas",
        ["actividad_de_rutina_actualizada"] = "Rutinas",
        ["ActividadDeRutinaEliminada"] = "Rutinas",
        ["actividad_de_rutina_eliminada"] = "Rutinas",
    };

    public static WebApplication MapAuditoriaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auditoria")
            .WithTags("Auditoría")
            .RequireAuthorization();

        group.MapGet("/", ListarEventos);
        group.MapGet("/modulos", ListarModulos);
        group.MapGet("/eventos", ListarEventosPorModulo);
        group.MapGet("/usuarios", ListarUsuarios);
        
        return app;
    }

    private static async Task<IResult> ListarEventos(
        IQuerySession session,
        string? modulo = null,
        string? tipo = null,
        string? usuario = null,
        DateTimeOffset? fechaInicio = null,
        DateTimeOffset? fechaFin = null,
        int page = 1,
        int pageSize = 50)
    {
        // Query the Read Model directly (Efficient SQL query)
        var query = session.Query<SincoMaquinaria.Domain.Projections.RegistroAuditoria>()
            .OrderByDescending(e => e.Fecha);

        // Apply filters
        if (fechaInicio.HasValue)
        {
            query = (IOrderedQueryable<SincoMaquinaria.Domain.Projections.RegistroAuditoria>)query.Where(e => e.Fecha >= fechaInicio.Value);
        }
        
        if (fechaFin.HasValue)
        {
            var endOfDay = fechaFin.Value.Date.AddDays(1).AddTicks(-1);
            query = (IOrderedQueryable<SincoMaquinaria.Domain.Projections.RegistroAuditoria>)query.Where(e => e.Fecha <= new DateTimeOffset(endOfDay));
        }

        if (!string.IsNullOrEmpty(modulo))
        {
            query = (IOrderedQueryable<SincoMaquinaria.Domain.Projections.RegistroAuditoria>)query.Where(e => e.Modulo == modulo);
        }

        if (!string.IsNullOrEmpty(tipo))
        {
            query = (IOrderedQueryable<SincoMaquinaria.Domain.Projections.RegistroAuditoria>)query.Where(e => e.TipoEvento == tipo);
        }

        if (!string.IsNullOrEmpty(usuario))
        {
            // Case-insensitive search on indexed column
            query = (IOrderedQueryable<SincoMaquinaria.Domain.Projections.RegistroAuditoria>)query.Where(e => e.UsuarioNombre.Contains(usuario, StringComparison.OrdinalIgnoreCase));
        }

        // Paging using Marten's ToPagedListAsync
        var pagedResults = await query.ToPagedListAsync(page, pageSize);

        // Map to DTO
        var items = pagedResults.Select(e => new AuditoriaEventoDto
        {
            Id = e.Id,
            StreamId = e.StreamId,
            Tipo = e.TipoEvento,
            Modulo = e.Modulo,
            Fecha = e.Fecha,
            Version = e.Version,
            // Deserialize details on demand
            Datos = new AuditDataDto 
            { 
                UsuarioId = e.UsuarioId,
                UsuarioNombre = e.UsuarioNombre,
                FechaAccion = e.Fecha,
                Detalles = !string.IsNullOrEmpty(e.Detalles) 
                    ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(e.Detalles) ?? new()
                    : new()
            }
        }).ToList();

        return Results.Ok(new PagedResponse<AuditoriaEventoDto>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = (int)pagedResults.TotalItemCount
        });
    }

    private static IResult ListarModulos()
    {
        // Return distinct modules
        var modulos = EventoModuloMap.Values.Distinct().OrderBy(m => m).ToList();
        return Results.Ok(modulos);
    }

    private static IResult ListarEventosPorModulo(string? modulo = null)
    {
        IEnumerable<KeyValuePair<string, string>> eventos = EventoModuloMap;
        
        if (!string.IsNullOrEmpty(modulo))
        {
            eventos = eventos.Where(kv => kv.Value.Equals(modulo, StringComparison.OrdinalIgnoreCase));
        }

        var result = eventos.Select(kv => new { Tipo = kv.Key, Modulo = kv.Value }).ToList();
        return Results.Ok(result);
    }

    private static async Task<IResult> ListarUsuarios(IQuerySession session)
    {
        // Get all users from the users collection
        var usuarios = await session.Query<Usuario>()
            .Select(u => new { u.Id, u.Nombre })
            .ToListAsync();

        return Results.Ok(usuarios);
    }

    private static AuditDataDto? ExtractAuditData(object? data)
    {
        if (data == null) return null;

        var type = data.GetType();
        var result = new AuditDataDto();

        // Extract common audit fields using reflection
        var usuarioIdProp = type.GetProperty("UsuarioId");
        var usuarioNombreProp = type.GetProperty("UsuarioNombre");
        var fechaCreacionProp = type.GetProperty("FechaCreacion");

        if (usuarioIdProp != null)
        {
            var val = usuarioIdProp.GetValue(data);
            result.UsuarioId = val as Guid?;
        }

        if (usuarioNombreProp != null)
        {
            result.UsuarioNombre = usuarioNombreProp.GetValue(data) as string;
        }

        if (fechaCreacionProp != null)
        {
            var val = fechaCreacionProp.GetValue(data);
            if (val is DateTimeOffset dto) result.FechaAccion = dto;
            else if (val is DateTime dt) result.FechaAccion = new DateTimeOffset(dt);
        }

        // Get other relevant properties for context
        var props = type.GetProperties()
            .Where(p => !new[] { "UsuarioId", "UsuarioNombre", "FechaCreacion" }.Contains(p.Name))
            .Take(5)
            .ToDictionary(p => p.Name, p => p.GetValue(data)?.ToString() ?? "");

        result.Detalles = props;

        return result;
    }
}

public class AuditoriaEventoDto
{
    public Guid Id { get; set; }
    public Guid StreamId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
    public long Version { get; set; }
    public AuditDataDto? Datos { get; set; }
}

public class AuditDataDto
{
    public Guid? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public DateTimeOffset? FechaAccion { get; set; }
    public Dictionary<string, string> Detalles { get; set; } = new();
}

