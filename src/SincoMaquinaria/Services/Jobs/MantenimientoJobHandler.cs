using Hangfire.Server;
using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;

namespace SincoMaquinaria.Services.Jobs;

public class MantenimientoJobHandler
{
    private readonly IDocumentStore _store;
    private readonly ILogger<MantenimientoJobHandler> _logger;

    public MantenimientoJobHandler(
        IDocumentStore store,
        ILogger<MantenimientoJobHandler> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task LimpiarTokensExpiradosAsync(PerformContext? context)
    {
        using var session = _store.LightweightSession();

        _logger.LogInformation("Iniciando limpieza de tokens expirados...");

        // Buscar usuarios con refresh tokens expirados
        var usuariosConTokensExpirados = await session.Query<Usuario>()
            .Where(u => u.RefreshToken != null &&
                        u.RefreshTokenExpiry < DateTime.UtcNow)
            .ToListAsync();

        _logger.LogInformation("Encontrados {Count} tokens expirados", usuariosConTokensExpirados.Count);

        foreach (var usuario in usuariosConTokensExpirados)
        {
            session.Events.Append(usuario.Id, new RefreshTokenRevocado(
                usuario.Id, DateTimeOffset.UtcNow));
        }

        await session.SaveChangesAsync();

        _logger.LogInformation(
            "Limpieza de tokens expirados completada. {Count} tokens limpiados.",
            usuariosConTokensExpirados.Count);
    }
}
