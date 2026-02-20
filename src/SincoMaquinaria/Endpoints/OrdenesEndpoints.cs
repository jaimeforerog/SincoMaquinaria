using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Infrastructure;
using SincoMaquinaria.Extensions;
using SincoMaquinaria.Services;

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
        OrdenesService service,
        DashboardNotifier notifier,
        HttpContext httpContext,
        [FromBody] CrearOrdenRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        var result = await service.CrearOrden(req, userId, userName);
        if (!result.IsSuccess)
            return Results.Conflict(result.Error);

        // Notificar al dashboard
        await notifier.NotificarOrdenCreada();

        return Results.Created($"/ordenes/{result.Value}", new { Id = result.Value });
    }

    private static async Task<IResult> ListarOrdenes(
        IQuerySession session,
        [AsParameters] PaginationRequest pagination)
    {
        var query = session.Query<OrdenDeTrabajo>()
            .Where(o => o.Estado != EstadoOrdenDeTrabajo.Eliminada)
            .ApplyOrdering(pagination);

        var result = await query.ToPagedResponseAsync(pagination);
        return Results.Ok(result);
    }

    private static async Task<IResult> ObtenerOrden(IQuerySession session, Guid ordenId)
    {
        var orden = await session.Events.AggregateStreamAsync<OrdenDeTrabajo>(ordenId);
        return orden != null ? Results.Ok(orden) : Results.NotFound();
    }

    private static async Task<IResult> AgregarActividad(
        OrdenesService service,
        Guid ordenId,
        HttpContext httpContext,
        [FromBody] AgregarActividadRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.AgregarActividad(ordenId, req, userId, userName);
        if (!result.IsSuccess)
            return Results.BadRequest(result.Error);
        return Results.Ok(new { DetalleId = result.Value });
    }

    private static async Task<IResult> RegistrarAvance(
        OrdenesService service,
        Guid ordenId,
        HttpContext httpContext,
        [FromBody] RegistrarAvanceRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.RegistrarAvance(ordenId, req, userId, userName);
        if (!result.IsSuccess)
            return Results.BadRequest(result.Error);
        return Results.Ok();
    }

    private static async Task<IResult> ObtenerHistorial(OrdenesService service, Guid ordenId)
    {
        var result = await service.ObtenerHistorial(ordenId);
        if (!result.IsSuccess)
            return Results.NotFound(result.Error);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> EliminarOrden(
        OrdenesService service,
        Guid ordenId,
        HttpContext httpContext)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.EliminarOrden(ordenId, userId, userName);
        if (result.IsNotFound)
            return Results.NotFound(result.Error);
        return Results.Ok();
    }

}
