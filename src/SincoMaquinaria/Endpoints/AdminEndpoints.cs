using Marten;
using Marten.Events;
using JasperFx.Events;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using System.Collections.Generic;
using System.Linq;

namespace SincoMaquinaria.Endpoints;

public static class AdminEndpoints
{
    public static WebApplication MapAdminEndpoints(this WebApplication app, IConfiguration configuration)
    {
        var enableAdminEndpoints = configuration.GetValue<bool>("Security:EnableAdminEndpoints", false);

        // Admin group requiere rol de Admin
        var adminGroup = app.MapGroup("/admin")
            .WithTags("Admin")
            .RequireAuthorization("Admin");

        if (enableAdminEndpoints)
        {
            adminGroup.MapGet("/logs", ListarLogs);
            adminGroup.MapPost("/reset-db", ResetDatabase);
            adminGroup.MapGet("/diagnostics", DiagnosticosEventos);
            adminGroup.MapPost("/rebuild-projections", ReconstruirProyecciones);
        }

        return app;
    }

    private static async Task<IResult> ListarLogs(IQuerySession session)
    {
        var logs = await session.Query<ErrorLog>()
            .OrderByDescending(x => x.Fecha)
            .Take(50)
            .ToListAsync();
        return Results.Ok(logs);
    }

    private static async Task<IResult> ResetDatabase(IDocumentStore store)
    {
        using var session = store.LightweightSession();
        // 1. Identificar el primer usuario creado (el admin original)
        var firstUser = await session.Query<Usuario>()
            .OrderBy(u => u.FechaCreacion)
            .FirstOrDefaultAsync();

        IReadOnlyList<IEvent> userEvents = new List<IEvent>();

        if (firstUser != null)
        {
            // 2. Extraer sus eventos
            userEvents = await session.Events.FetchStreamAsync(firstUser.Id);
        }

        // 3. Limpiar la base de datos
        await store.Advanced.Clean.CompletelyRemoveAllAsync();

        // 4. Re-insertar los eventos del usuario preservado
        if (userEvents.Any())
        {
            using var restoreSession = store.LightweightSession();
            restoreSession.Events.Append(firstUser!.Id, userEvents.Select(e => e.Data).ToArray());
            await restoreSession.SaveChangesAsync();
            return Results.Ok(new { Message = $"Base de datos reseteada. Usuario '{firstUser.Email}' preservado." });
        }

        return Results.Ok(new { Message = "Base de datos reseteada completamente (No se encontraron usuarios para preservar)." });
    }

    private static async Task<IResult> DiagnosticosEventos(IDocumentStore store)
    {
        using var session = store.QuerySession();

        // Contar eventos en el event store
        var totalEventos = await session.Events.QueryAllRawEvents().CountAsync();
        var equipoEventos = await session.Events.QueryRawEventDataOnly<EquipoCreado>().CountAsync();
        var rutinaEventos = await session.Events.QueryRawEventDataOnly<RutinaCreada>().CountAsync();

        // Contar documentos en las proyecciones
        var equiposDocs = await session.Query<Equipo>().CountAsync();
        var rutinasDocs = await session.Query<RutinaMantenimiento>().CountAsync();
        var ordenesDocs = await session.Query<OrdenDeTrabajo>().CountAsync();
        var empleadosDocs = await session.Query<Empleado>().CountAsync();

        return Results.Ok(new
        {
            EventStore = new
            {
                TotalEventos = totalEventos,
                EventosEquipo = equipoEventos,
                EventosRutina = rutinaEventos
            },
            Proyecciones = new
            {
                Equipos = equiposDocs,
                Rutinas = rutinasDocs,
                Ordenes = ordenesDocs,
                Empleados = empleadosDocs
            },
            Diagnostico = new
            {
                ProyeccionesDesactualizadas = equipoEventos > 0 && equiposDocs == 0,
                Mensaje = equipoEventos > 0 && equiposDocs == 0
                    ? "¡ALERTA! Hay eventos pero las proyecciones están vacías. Ejecuta /admin/rebuild-projections"
                    : "Proyecciones sincronizadas correctamente"
            }
        });
    }

    private static async Task<IResult> ReconstruirProyecciones(IDocumentStore store)
    {
        using var session = store.LightweightSession();

        // Obtener todos los streams de eventos
        var allStreams = await session.Events.QueryAllRawEvents()
            .Select(e => e.StreamId)
            .Distinct()
            .ToListAsync();

        var reconstruidos = 0;

        foreach (var streamId in allStreams)
        {
            // Obtener eventos del stream
            var events = await session.Events.FetchStreamAsync(streamId);

            if (events.Count == 0) continue;

            // Determinar el tipo de agregado basado en el primer evento
            var primerEvento = events.First().Data;

            // Reconstruir el agregado aplicando todos los eventos
            if (primerEvento is EquipoCreado || primerEvento is EquipoMigrado)
            {
                var equipo = await session.Events.AggregateStreamAsync<Equipo>(streamId);
                if (equipo != null)
                {
                    session.Store(equipo);
                    reconstruidos++;
                }
            }
            else if (primerEvento is RutinaMigrada)
            {
                var rutina = await session.Events.AggregateStreamAsync<RutinaMantenimiento>(streamId);
                if (rutina != null)
                {
                    session.Store(rutina);
                    reconstruidos++;
                }
            }
            else if (primerEvento is OrdenDeTrabajoCreada)
            {
                var orden = await session.Events.AggregateStreamAsync<OrdenDeTrabajo>(streamId);
                if (orden != null)
                {
                    session.Store(orden);
                    reconstruidos++;
                }
            }
            else if (primerEvento is EmpleadoCreado)
            {
                var empleado = await session.Events.AggregateStreamAsync<Empleado>(streamId);
                if (empleado != null)
                {
                    session.Store(empleado);
                    reconstruidos++;
                }
            }
            else if (primerEvento is UsuarioCreado)
            {
                var usuario = await session.Events.AggregateStreamAsync<Usuario>(streamId);
                if (usuario != null)
                {
                    session.Store(usuario);
                    reconstruidos++;
                }
            }
        }

        await session.SaveChangesAsync();

        return Results.Ok(new
        {
            Message = $"Proyecciones reconstruidas exitosamente. {reconstruidos} agregados procesados.",
            AgreGadosReconstruidos = reconstruidos,
            TotalStreams = allStreams.Count
        });
    }

}
