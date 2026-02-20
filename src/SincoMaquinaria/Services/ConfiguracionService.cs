using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Services;

public class ConfiguracionService
{
    private readonly IDocumentSession _session;
    private readonly ICacheService _cache;
    private readonly ILogger<ConfiguracionService> _logger;

    public ConfiguracionService(IDocumentSession session, ICacheService cache, ILogger<ConfiguracionService> logger)
    {
        _session = session;
        _cache = cache;
        _logger = logger;
    }

    // --- Tipos de Medidor ---

    public async Task<Result<Unit>> CrearTipoMedidor(CrearTipoMedidorRequest req, Guid? userId, string? userName)
    {
        var configId = ConfiguracionGlobal.SingletonId;
        var config = await _session.LoadAsync<ConfiguracionGlobal>(configId);

        if (config != null && config.TiposMedidor.Any(t =>
            t.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Unit>.Failure($"El tipo '{req.Nombre}' ya existe.");
        }

        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        _session.Events.Append(configId,
            new TipoMedidorCreado(nuevoCodigo, req.Nombre, req.Unidad.ToUpper(), userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.TiposMedidor);
        _logger.LogInformation("Tipo medidor '{Nombre}' creado por usuario {UserId}", req.Nombre, userId);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<List<TipoMedidor>>> ListarTiposMedidor()
    {
        const string cacheKey = CacheKeys.TiposMedidor;
        var cached = await _cache.GetAsync<List<TipoMedidor>>(cacheKey);
        if (cached != null)
            return Result<List<TipoMedidor>>.Success(cached);

        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var tiposMedidor = config?.TiposMedidor ?? new List<TipoMedidor>();

        await _cache.SetAsync(cacheKey, tiposMedidor, TimeSpan.FromHours(1));
        return Result<List<TipoMedidor>>.Success(tiposMedidor);
    }

    public async Task<Result<Unit>> ActualizarTipoMedidor(string codigo, ActualizarTipoMedidorRequest req, Guid? userId, string? userName)
    {
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null || !config.TiposMedidor.Any(t => t.Codigo == codigo))
            return Result<Unit>.NotFound($"Tipo de medidor con código '{codigo}' no encontrado");

        _session.Events.Append(ConfiguracionGlobal.SingletonId,
            new TipoMedidorActualizado(codigo, req.Nombre, req.Unidad, userId, userName));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.TiposMedidor);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> CambiarEstadoMedidor(string codigo, bool activo, Guid? userId, string? userName)
    {
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null || !config.TiposMedidor.Any(t => t.Codigo == codigo))
            return Result<Unit>.NotFound($"Tipo de medidor con código '{codigo}' no encontrado");

        _session.Events.Append(ConfiguracionGlobal.SingletonId,
            new EstadoTipoMedidorCambiado(codigo, activo, userId, userName));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.TiposMedidor);
        return Result<Unit>.Success(Unit.Value);
    }

    // --- Grupos de Mantenimiento ---

    public async Task<Result<Unit>> CrearGrupo(CrearGrupoRequest req, Guid? userId, string? userName)
    {
        var configId = ConfiguracionGlobal.SingletonId;
        var config = await _session.LoadAsync<ConfiguracionGlobal>(configId);

        if (config != null && config.GruposMantenimiento.Any(g =>
            g.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Unit>.Failure($"El grupo '{req.Nombre}' ya existe.");
        }

        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        _session.Events.Append(configId,
            new GrupoMantenimientoCreado(nuevoCodigo, req.Nombre, req.Descripcion, true, userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.Grupos);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<List<GrupoMantenimiento>>> ListarGrupos()
    {
        const string cacheKey = CacheKeys.Grupos;
        var cached = await _cache.GetAsync<List<GrupoMantenimiento>>(cacheKey);
        if (cached != null)
            return Result<List<GrupoMantenimiento>>.Success(cached);

        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var grupos = config?.GruposMantenimiento ?? new List<GrupoMantenimiento>();

        await _cache.SetAsync(cacheKey, grupos, TimeSpan.FromHours(1));
        return Result<List<GrupoMantenimiento>>.Success(grupos);
    }

    public async Task<Result<Unit>> ActualizarGrupo(string codigo, ActualizarGrupoRequest req, Guid? userId, string? userName)
    {
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);

        if (config != null && config.GruposMantenimiento.Any(g =>
            g.Codigo != codigo &&
            g.Nombre.Trim().Equals(req.Nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Unit>.Failure($"El grupo '{req.Nombre}' ya existe.");
        }

        if (config == null || !config.GruposMantenimiento.Any(g => g.Codigo == codigo))
            return Result<Unit>.NotFound($"Grupo de mantenimiento con código '{codigo}' no encontrado");

        _session.Events.Append(ConfiguracionGlobal.SingletonId,
            new GrupoMantenimientoActualizado(codigo, req.Nombre, req.Descripcion, userId, userName));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.Grupos);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> CambiarEstadoGrupo(string codigo, bool activo, Guid? userId, string? userName)
    {
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null || !config.GruposMantenimiento.Any(g => g.Codigo == codigo))
            return Result<Unit>.NotFound($"Grupo de mantenimiento con código '{codigo}' no encontrado");

        _session.Events.Append(ConfiguracionGlobal.SingletonId,
            new EstadoGrupoMantenimientoCambiado(codigo, activo, userId, userName));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.Grupos);
        return Result<Unit>.Success(Unit.Value);
    }

    // --- Tipos de Falla ---

    public async Task<Result<Unit>> CrearTipoFalla(CrearTipoFallaRequest req, Guid? userId, string? userName)
    {
        var configId = ConfiguracionGlobal.SingletonId;
        var config = await _session.LoadAsync<ConfiguracionGlobal>(configId);

        if (config != null && config.TiposFalla.Any(f =>
            f.Descripcion.Equals(req.Descripcion, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Unit>.Failure($"El tipo de falla '{req.Descripcion}' ya existe.");
        }

        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        _session.Events.Append(configId,
            new TipoFallaCreado(nuevoCodigo, req.Descripcion, req.Prioridad, userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.TiposFalla);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<List<TipoFalla>>> ListarTiposFalla()
    {
        const string cacheKey = CacheKeys.TiposFalla;
        var cached = await _cache.GetAsync<List<TipoFalla>>(cacheKey);
        if (cached != null)
            return Result<List<TipoFalla>>.Success(cached);

        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var tiposFalla = config?.TiposFalla ?? new List<TipoFalla>();

        await _cache.SetAsync(cacheKey, tiposFalla, TimeSpan.FromHours(1));
        return Result<List<TipoFalla>>.Success(tiposFalla);
    }

    // --- Causas de Falla ---

    public async Task<Result<Unit>> CrearCausaFalla(CrearCausaFallaRequest req, Guid? userId, string? userName)
    {
        var configId = ConfiguracionGlobal.SingletonId;
        var config = await _session.LoadAsync<ConfiguracionGlobal>(configId);

        if (config != null && config.CausasFalla.Any(c =>
            c.Descripcion.Trim().Equals(req.Descripcion.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Unit>.Failure($"La causa de falla '{req.Descripcion}' ya existe.");
        }

        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        _session.Events.Append(configId,
            new CausaFallaCreada(nuevoCodigo, req.Descripcion, userId, userName, DateTimeOffset.Now));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.CausasFalla);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<List<CausaFalla>>> ListarCausasFalla()
    {
        const string cacheKey = CacheKeys.CausasFalla;
        var cached = await _cache.GetAsync<List<CausaFalla>>(cacheKey);
        if (cached != null)
            return Result<List<CausaFalla>>.Success(cached);

        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var causasFalla = config?.CausasFalla ?? new List<CausaFalla>();

        await _cache.SetAsync(cacheKey, causasFalla, TimeSpan.FromHours(1));
        return Result<List<CausaFalla>>.Success(causasFalla);
    }

    public async Task<Result<Unit>> ActualizarCausaFalla(string codigo, ActualizarCausaFallaRequest req, Guid? userId, string? userName)
    {
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);

        if (config != null && config.CausasFalla.Any(c =>
            c.Codigo != codigo &&
            c.Descripcion.Trim().Equals(req.Descripcion.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Unit>.Failure($"La causa de falla '{req.Descripcion}' ya existe.");
        }

        if (config == null || !config.CausasFalla.Any(c => c.Codigo == codigo))
            return Result<Unit>.NotFound($"Causa de falla con código '{codigo}' no encontrado");

        _session.Events.Append(ConfiguracionGlobal.SingletonId,
            new CausaFallaActualizada(codigo, req.Descripcion, userId, userName));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.CausasFalla);
        return Result<Unit>.Success(Unit.Value);
    }

    public async Task<Result<Unit>> CambiarEstadoCausaFalla(string codigo, bool activo, Guid? userId, string? userName)
    {
        var config = await _session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null || !config.CausasFalla.Any(c => c.Codigo == codigo))
            return Result<Unit>.NotFound($"Causa de falla con código '{codigo}' no encontrado");

        _session.Events.Append(ConfiguracionGlobal.SingletonId,
            new EstadoCausaFallaCambiado(codigo, activo, userId, userName));
        await _session.SaveChangesAsync();

        await _cache.RemoveAsync(CacheKeys.CausasFalla);
        return Result<Unit>.Success(Unit.Value);
    }
}
