using Marten;
using Marten.Events;
using JasperFx.Events;
using SincoMaquinaria.Domain;
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

}
