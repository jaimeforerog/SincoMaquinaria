using Marten;
using Marten.Events;
using Marten.Events.Projections;
using SincoMaquinaria.Domain.Events;
using System.Reflection;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SincoMaquinaria.Domain.Projections;

public class AuditoriaProjection : EventProjection
{
    public AuditoriaProjection()
    {
        // Explicitly map all events to the Create method if convention doesn't pick up IEvent
        // However, let's try convention first with Create(IEvent)
        // If that fails, we might need to be more specific or use Project(IEvent, ops)
    }

    // Convention-based method to create a document from an event
    /*
    public RegistroAuditoria? Create(IEvent @event)
    {
        var data = @event.Data;
        if (data == null) return null;

        var typeName = @event.EventTypeName;
        var modulo = EventoModuloMap.TryGetValue(typeName, out var mod) ? mod : "Otros";

        var (usuarioId, usuarioNombre, fechaAccion, detalles) = ExtractAuditData(data, @event.Timestamp);

        return new RegistroAuditoria
        {
            Id = @event.Id, // Use Event Id as the Document Id
            StreamId = @event.StreamId,
            TipoEvento = typeName,
            Modulo = modulo,
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            Fecha = fechaAccion,
            Version = @event.Version,
            Detalles = JsonSerializer.Serialize(detalles)
        };
    }
    */

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

    private (Guid? UsuarioId, string? UsuarioNombre, DateTimeOffset Fecha, Dictionary<string, string> Detalles) ExtractAuditData(object data, DateTimeOffset eventTimestamp)
    {
        var type = data.GetType();
        Guid? usuarioId = null;
        string? usuarioNombre = null;
        DateTimeOffset fecha = eventTimestamp;
        var detalles = new Dictionary<string, string>();

        // Extract properties via reflection
        var props = type.GetProperties();
        
        foreach (var prop in props)
        {
            if (prop.Name == "UsuarioId")
            {
                usuarioId = prop.GetValue(data) as Guid?;
            }
            else if (prop.Name == "UsuarioNombre")
            {
                usuarioNombre = prop.GetValue(data) as string;
            }
            // Ignore system/audit properties for the details blob
            else if (prop.Name != "FechaCreacion" && prop.Name != "Id") 
            {
                try 
                {
                    var val = prop.GetValue(data);
                    if (val != null)
                    {
                        detalles[prop.Name] = val.ToString() ?? "";
                    }
                }
                catch 
                {
                    // Ignore reflection errors
                }
            }
        }

        return (usuarioId, usuarioNombre, fecha, detalles);
    }
}
