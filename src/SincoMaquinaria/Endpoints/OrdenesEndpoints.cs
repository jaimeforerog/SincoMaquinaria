using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Infrastructure;
using SincoMaquinaria.Extensions;
using SincoMaquinaria.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

namespace SincoMaquinaria.Endpoints;

public static class OrdenesEndpoints
{
    public static WebApplication MapOrdenesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/ordenes")
            .WithTags("Órdenes de Trabajo")
            .RequireAuthorization();

        group.MapPost("/", CrearOrden)
            .AddEndpointFilter<ValidationFilter<CrearOrdenRequest>>();

        group.MapGet("/", ListarOrdenes);

        group.MapGet("/{ordenId:guid}", ObtenerOrden);

        group.MapPost("/{ordenId:guid}/actividades", AgregarActividad)
            .AddEndpointFilter<ValidationFilter<AgregarActividadRequest>>();

        group.MapPost("/{ordenId:guid}/avance", RegistrarAvance)
            .AddEndpointFilter<ValidationFilter<RegistrarAvanceRequest>>();

        group.MapGet("/{ordenId:guid}/historial", ObtenerHistorial);

        group.MapDelete("/{ordenId:guid}", EliminarOrden);

        return app;
    }

    private static async Task<IResult> CrearOrden(
        IDocumentSession session, 
        DashboardNotifier notifier,
        HttpContext httpContext,
        [FromBody] CrearOrdenRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        var ordenId = Guid.NewGuid();
        var events = new List<object>();

        var fechaOrden = req.FechaOrden ?? DateTime.Now;
        events.Add(new OrdenDeTrabajoCreada(ordenId, req.Numero, req.EquipoId, 
            req.Origen, req.Tipo, fechaOrden, DateTimeOffset.Now, userId, userName));

        // Si viene RutinaId, cargamos actividades de la rutina
        if (req.RutinaId.HasValue)
        {
            var rutina = await session.LoadAsync<RutinaMantenimiento>(req.RutinaId.Value);
            if (rutina != null)
            {
                foreach (var parte in rutina.Partes)
                {
                    foreach (var act in parte.Actividades)
                    {
                        bool incluir = ShouldIncludeActivity(act, req.FrecuenciaPreventiva);
                        if (incluir)
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

        session.Events.StartStream<OrdenDeTrabajo>(ordenId, events);
        await session.SaveChangesAsync();
        
        // Notificar al dashboard
        await notifier.NotificarOrdenCreada();
        
        return Results.Created($"/ordenes/{ordenId}", new { Id = ordenId });
    }

    private static bool ShouldIncludeActivity(ActividadMantenimiento act, int? frecuenciaPreventiva)
    {
        if (!frecuenciaPreventiva.HasValue) return true;
        if (act.Frecuencia <= 0) return false;
        return frecuenciaPreventiva.Value % act.Frecuencia == 0;
    }

    private static async Task<IResult> ListarOrdenes(
        IQuerySession session,
        [AsParameters] PaginationRequest pagination)
    {
        // Optimized query: Database-side filtering and pagination
        var query = session.Query<OrdenDeTrabajo>()
            .Where(o => o.Estado != EstadoOrdenDeTrabajo.Eliminada)
            .ApplyOrdering(pagination); // Extension method handles ordering

        var result = await query.ToPagedResponseAsync(pagination); // Extension method handles pagination and total count efficiently
        return Results.Ok(result);
    }

    private static async Task<IResult> ObtenerOrden(IQuerySession session, Guid ordenId)
    {
        var orden = await session.Events.AggregateStreamAsync<OrdenDeTrabajo>(ordenId);
        return orden != null ? Results.Ok(orden) : Results.NotFound();
    }

    private static async Task<IResult> AgregarActividad(
        IDocumentSession session, 
        Guid ordenId, 
        HttpContext httpContext,
        [FromBody] AgregarActividadRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        var detalleId = Guid.NewGuid();
        session.Events.Append(ordenId, 
            new ActividadAgregada(detalleId, req.Descripcion, req.FechaEstimada, 0, 
                req.TipoFallaId, req.CausaFallaId, userId, userName));
        await session.SaveChangesAsync();
        return Results.Ok(new { DetalleId = detalleId });
    }

    private static async Task<IResult> RegistrarAvance(
        IDocumentSession session, 
        Guid ordenId, 
        HttpContext httpContext,
        [FromBody] RegistrarAvanceRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        session.Events.Append(ordenId, 
            new AvanceDeActividadRegistrado(req.DetalleId, req.Porcentaje, 
                req.Observacion, req.NuevoEstado, userId, userName));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> ObtenerHistorial(IQuerySession session, Guid ordenId)
    {
        var events = await session.Events.FetchStreamAsync(ordenId);
        var historial = events.Select(e => new 
        {
            Id = e.Id,
            Tipo = e.Data.GetType().Name,
            Fecha = e.Timestamp,
            Datos = e.Data,
            Descripcion = GetEventDescription(e.Data)
        }).OrderByDescending(h => h.Fecha).ToList();

        return Results.Ok(historial);
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

        // Try to extract User Name if present via reflection or dynamic to handle multiple record types generically
        // checking the "UsuarioNombre" property if it exists
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

    private static async Task<IResult> EliminarOrden(
        IDocumentSession session,
        Guid ordenId,
        HttpContext httpContext)
    {
        var (userId, userName) = httpContext.GetUserContext();

        var orden = await session.LoadAsync<OrdenDeTrabajo>(ordenId);
        if (orden == null)
            return Results.NotFound("Orden no encontrada");

        session.Events.Append(ordenId, new OrdenDeTrabajoEliminada(ordenId, userId, userName));
        await session.SaveChangesAsync();

        return Results.Ok();
    }

}
