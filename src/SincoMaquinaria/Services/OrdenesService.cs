using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Services;

public class OrdenesService
{
    private readonly IDocumentSession _session;
    private readonly ILogger<OrdenesService> _logger;

    public OrdenesService(IDocumentSession session, ILogger<OrdenesService> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<Result<Guid>> CrearOrden(CrearOrdenRequest req, Guid? userId, string? userName)
    {
        var ordenId = Guid.NewGuid();
        var events = new List<object>();

        var fechaOrden = req.FechaOrden ?? DateTime.Now;
        events.Add(new OrdenDeTrabajoCreada(ordenId, req.Numero, req.EquipoId,
            req.Origen, req.Tipo, fechaOrden, DateTimeOffset.Now, userId, userName));

        // Si viene RutinaId, cargamos actividades de la rutina
        if (req.RutinaId.HasValue)
        {
            var rutina = await _session.LoadAsync<RutinaMantenimiento>(req.RutinaId.Value);
            if (rutina != null)
            {
                foreach (var parte in rutina.Partes)
                {
                    foreach (var act in parte.Actividades)
                    {
                        if (ShouldIncludeActivity(act, req.FrecuenciaPreventiva))
                        {
                            events.Add(new ActividadAgregada(
                                Guid.NewGuid(),
                                $"{parte.Descripcion}: {act.Descripcion}",
                                DateTime.Now.AddHours(act.Frecuencia),
                                act.Frecuencia,
                                null, null,
                                userId, userName));
                        }
                    }
                }
            }
        }

        // Si es correctivo manual y viene actividad inicial
        if (!string.IsNullOrEmpty(req.ActividadInicial))
        {
            events.Add(new ActividadAgregada(Guid.NewGuid(), req.ActividadInicial,
                DateTime.Now.AddHours(1), 0, null, null, userId, userName));
        }

        _session.Events.StartStream<OrdenDeTrabajo>(ordenId, events);
        await _session.SaveChangesAsync();

        _logger.LogInformation("Orden {OrdenId} creada por usuario {UserId}", ordenId, userId);
        return Result<Guid>.Success(ordenId);
    }

    public async Task<Result<Guid>> AgregarActividad(Guid ordenId, AgregarActividadRequest req, Guid? userId, string? userName)
    {
        var detalleId = Guid.NewGuid();
        _session.Events.Append(ordenId,
            new ActividadAgregada(detalleId, req.Descripcion, req.FechaEstimada, 0,
                req.TipoFallaId, req.CausaFallaId, userId, userName));
        await _session.SaveChangesAsync();
        return Result<Guid>.Success(detalleId);
    }

    public async Task<Result<Unit>> RegistrarAvance(Guid ordenId, RegistrarAvanceRequest req, Guid? userId, string? userName)
    {
        _session.Events.Append(ordenId,
            new AvanceDeActividadRegistrado(req.DetalleId, req.Porcentaje,
                req.Observacion, req.NuevoEstado, userId, userName));
        await _session.SaveChangesAsync();
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> EliminarOrden(Guid ordenId, Guid? userId, string? userName)
    {
        var orden = await _session.LoadAsync<OrdenDeTrabajo>(ordenId);
        if (orden == null)
            return Result<Unit>.NotFound("Orden no encontrada");

        _session.Events.Append(ordenId, new OrdenDeTrabajoEliminada(ordenId, userId, userName));
        await _session.SaveChangesAsync();

        _logger.LogInformation("Orden {OrdenId} eliminada por usuario {UserId}", ordenId, userId);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<List<object>>> ObtenerHistorial(Guid ordenId)
    {
        var events = await _session.Events.FetchStreamAsync(ordenId);
        var historial = events.Select(e => new
        {
            Id = e.Id,
            Tipo = e.Data.GetType().Name,
            Fecha = e.Timestamp,
            Datos = e.Data,
            Descripcion = GetEventDescription(e.Data)
        }).OrderByDescending(h => h.Fecha).Cast<object>().ToList();

        return Result<List<object>>.Success(historial);
    }

    private static bool ShouldIncludeActivity(ActividadMantenimiento act, int? frecuenciaPreventiva)
    {
        if (!frecuenciaPreventiva.HasValue) return true;
        if (act.Frecuencia <= 0) return false;
        return frecuenciaPreventiva.Value % act.Frecuencia == 0;
    }

    private static string GetEventDescription(object eventData)
    {
        string desc = eventData switch
        {
            OrdenDeTrabajoCreada e => $"Orden creada. Equipo: {e.EquipoId}. Tipo: {e.TipoMantenimiento}",
            ActividadAgregada e => $"Se agregó actividad: {e.Descripcion}",
            AvanceDeActividadRegistrado e => $"Avance: {e.PorcentajeAvance}%. Estado: {e.NuevoEstado}. {e.Observacion}",
            OrdenFinalizada e => $"Orden finalizada en estado: {e.EstadoFinal}",
            _ => "Evento registrado"
        };

        var prop = eventData.GetType().GetProperty("UsuarioNombre");
        if (prop != null)
        {
            var val = prop.GetValue(eventData) as string;
            if (!string.IsNullOrEmpty(val))
            {
                desc += $" - Por: {val}";
            }
        }

        return desc;
    }
}
