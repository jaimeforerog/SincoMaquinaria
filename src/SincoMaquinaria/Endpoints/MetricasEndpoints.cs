using Marten;
using SincoMaquinaria.Domain;
using static SincoMaquinaria.Domain.TipoMantenimientoConstants;

namespace SincoMaquinaria.Endpoints;

public static class MetricasEndpoints
{
    public static WebApplication MapMetricasEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/metricas")
            .WithTags("Métricas")
            .RequireAuthorization();

        group.MapGet("/mantenimiento", ObtenerMetricasMantenimiento);

        return app;
    }

    private static async Task<IResult> ObtenerMetricasMantenimiento(IQuerySession session)
    {
        // Fetch all non-deleted orders
        var ordenes = await session.Query<OrdenDeTrabajo>()
            .Where(o => o.Estado != EstadoOrdenDeTrabajo.Eliminada
                     && o.Estado != EstadoOrdenDeTrabajo.Inexistente)
            .ToListAsync();

        // Separate by type
        var correctivas = ordenes.Where(o => o.Tipo == Correctivo).ToList();
        var preventivas = ordenes.Where(o => o.Tipo == Preventivo).ToList();

        // --- MTTR: Mean Time To Repair ---
        // Only from completed corrective orders that have FechaCompletado
        var correctivasCompletadas = correctivas
            .Where(o => o.FechaCompletado.HasValue
                     && (o.Estado == EstadoOrdenDeTrabajo.EjecucionCompleta))
            .ToList();

        double? mttrHoras = null;
        if (correctivasCompletadas.Any())
        {
            mttrHoras = correctivasCompletadas
                .Average(o => (o.FechaCompletado!.Value - o.FechaCreacion).TotalHours);
        }

        // --- MTBF: Mean Time Between Failures ---
        // Group corrective orders by equipment, compute average gap between failures
        double? mtbfHoras = null;
        var correctivasPorEquipo = correctivas
            .GroupBy(o => o.EquipoId)
            .Where(g => g.Count() >= 2) // Need at least 2 failures to compute MTBF
            .ToList();

        if (correctivasPorEquipo.Any())
        {
            var allGaps = new List<double>();
            foreach (var grupo in correctivasPorEquipo)
            {
                var ordenadas = grupo.OrderBy(o => o.FechaCreacion).ToList();
                for (int i = 1; i < ordenadas.Count; i++)
                {
                    var gap = (ordenadas[i].FechaCreacion - ordenadas[i - 1].FechaCreacion).TotalHours;
                    allGaps.Add(gap);
                }
            }
            if (allGaps.Any())
                mtbfHoras = allGaps.Average();
        }

        // --- Disponibilidad ---
        double? disponibilidad = null;
        if (mtbfHoras.HasValue && mttrHoras.HasValue && (mtbfHoras.Value + mttrHoras.Value) > 0)
        {
            disponibilidad = mtbfHoras.Value / (mtbfHoras.Value + mttrHoras.Value);
        }

        // --- Órdenes por mes (últimos 6 meses) ---
        var sixMonthsAgo = DateTimeOffset.UtcNow.AddMonths(-6);
        var ordenesRecientes = ordenes
            .Where(o => o.FechaCreacion >= sixMonthsAgo)
            .ToList();

        var ordenesPorMes = ordenesRecientes
            .GroupBy(o => o.FechaCreacion.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Mes = g.Key,
                Preventivas = g.Count(o => o.Tipo == Preventivo),
                Correctivas = g.Count(o => o.Tipo == Correctivo)
            })
            .ToList();

        // --- MTTR por equipo (top 10) ---
        // Fetch equipment names for display
        var equipos = await session.Query<Equipo>().ToListAsync();
        var equipoDict = equipos.ToDictionary(e => e.Id.ToString(), e => e.Placa);

        var mttrPorEquipo = correctivasCompletadas
            .GroupBy(o => o.EquipoId)
            .Select(g => new
            {
                EquipoId = g.Key,
                Placa = equipoDict.GetValueOrDefault(g.Key, g.Key),
                MttrHoras = Math.Round(g.Average(o => (o.FechaCompletado!.Value - o.FechaCreacion).TotalHours), 1),
                TotalOrdenes = g.Count()
            })
            .OrderByDescending(x => x.MttrHoras)
            .Take(10)
            .ToList();

        return Results.Ok(new
        {
            MttrHoras = mttrHoras.HasValue ? Math.Round(mttrHoras.Value, 1) : (double?)null,
            MtbfHoras = mtbfHoras.HasValue ? Math.Round(mtbfHoras.Value, 1) : (double?)null,
            Disponibilidad = disponibilidad.HasValue ? Math.Round(disponibilidad.Value, 4) : (double?)null,
            TotalPreventivas = preventivas.Count,
            TotalCorrectivas = correctivas.Count,
            OrdenesPorMes = ordenesPorMes,
            MttrPorEquipo = mttrPorEquipo
        });
    }
}
