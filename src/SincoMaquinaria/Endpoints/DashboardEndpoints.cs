using Marten;
using SincoMaquinaria.Domain;
using Microsoft.AspNetCore.Http;

namespace SincoMaquinaria.Endpoints;

public static class DashboardEndpoints
{
    public static WebApplication MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        group.MapGet("/stats", ObtenerEstadisticas);

        return app;
    }

    private static async Task<IResult> ObtenerEstadisticas(IQuerySession session)
    {
        // Execute efficient Count queries directly against the DB
        var equiposCount = await session.Query<Equipo>().CountAsync();
        var rutinasCount = await session.Query<RutinaMantenimiento>().CountAsync();
        
        // Count active orders (not eliminated)
        var ordenesActivasCount = await session.Query<OrdenDeTrabajo>()
            .Where(o => o.Estado != EstadoOrdenDeTrabajo.Eliminada)
            .CountAsync();

        return Results.Ok(new 
        {
            EquiposCount = equiposCount,
            RutinasCount = rutinasCount,
            OrdenesActivasCount = ordenesActivasCount
        });
    }
}
