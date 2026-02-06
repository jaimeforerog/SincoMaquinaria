using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Infrastructure;
using SincoMaquinaria.Extensions;
using SincoMaquinaria.Services;

namespace SincoMaquinaria.Endpoints;

public static class ConfiguracionEndpoints
{
    public static WebApplication MapConfiguracionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/configuracion")
            .RequireAuthorization();

        // Tipos de Medidor
        group.MapPost("/medidores", CrearTipoMedidor)
            .WithTags("Configuración - Medidores")
            .RequireAuthorization("Admin")
            .AddEndpointFilter<ValidationFilter<CrearTipoMedidorRequest>>();

        group.MapGet("/medidores", ListarTiposMedidor)
            .WithTags("Configuración - Medidores");

        group.MapPut("/medidores/{codigo}", ActualizarTipoMedidor)
            .WithTags("Configuración - Medidores")
            .RequireAuthorization("Admin")
            .AddEndpointFilter<ValidationFilter<ActualizarTipoMedidorRequest>>();

        group.MapPut("/medidores/{codigo}/estado", CambiarEstadoMedidor)
            .WithTags("Configuración - Medidores")
            .RequireAuthorization("Admin");

        // Grupos de Mantenimiento
        group.MapPost("/grupos", CrearGrupo)
            .WithTags("Configuración - Grupos")
            .RequireAuthorization("Admin")
            .AddEndpointFilter<ValidationFilter<CrearGrupoRequest>>();

        group.MapGet("/grupos", ListarGrupos)
            .WithTags("Configuración - Grupos");

        group.MapPut("/grupos/{codigo}", ActualizarGrupo)
            .WithTags("Configuración - Grupos")
            .RequireAuthorization("Admin")
            .AddEndpointFilter<ValidationFilter<ActualizarGrupoRequest>>();

        group.MapPut("/grupos/{codigo}/estado", CambiarEstadoGrupo)
            .WithTags("Configuración - Grupos")
            .RequireAuthorization("Admin");

        // Tipos de Falla
        group.MapPost("/fallas", CrearTipoFalla)
            .WithTags("Configuración - Fallas")
            .RequireAuthorization("Admin")
            .AddEndpointFilter<ValidationFilter<CrearTipoFallaRequest>>();
        group.MapGet("/fallas", ListarTiposFalla)
            .WithTags("Configuración - Fallas");

        // Causas de Falla
        group.MapPost("/causas-falla", CrearCausaFalla)
            .WithTags("Configuración - Causas Falla")
            .RequireAuthorization("Admin")
            .AddEndpointFilter<ValidationFilter<CrearCausaFallaRequest>>();
        group.MapGet("/causas-falla", ListarCausasFalla)
            .WithTags("Configuración - Causas Falla");
        group.MapPut("/causas-falla/{codigo}", ActualizarCausaFalla)
            .WithTags("Configuración - Causas Falla")
            .RequireAuthorization("Admin")
            .AddEndpointFilter<ValidationFilter<ActualizarCausaFallaRequest>>();
        group.MapPut("/causas-falla/{codigo}/estado", CambiarEstadoCausaFalla)
            .WithTags("Configuración - Causas Falla")
            .RequireAuthorization("Admin");

        return app;
    }

    // --- Tipos de Medidor ---

    private static async Task<IResult> CrearTipoMedidor(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        [FromBody] CrearTipoMedidorRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var configId = ConfiguracionGlobal.SingletonId;

        // Validar si ya existe nombre
        var config = await session.LoadAsync<ConfiguracionGlobal>(configId);
        if (config != null && config.TiposMedidor.Any(t =>
            t.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"El tipo '{req.Nombre}' ya existe.");
        }

        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        session.Events.Append(configId,
            new TipoMedidorCreado(nuevoCodigo, req.Nombre, req.Unidad.ToUpper(), userId, userName, DateTimeOffset.Now));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:tiposmedidor");

        return Results.Ok();
    }

    private static async Task<IResult> ListarTiposMedidor(
        IQuerySession session,
        ICacheService cache)
    {
        const string cacheKey = "configuracion:tiposmedidor";

        // Intentar obtener de cache
        var cached = await cache.GetAsync<List<TipoMedidor>>(cacheKey);
        if (cached != null)
            return Results.Ok(cached);

        // Si no está en cache, consultar BD
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var tiposMedidor = config?.TiposMedidor ?? new List<TipoMedidor>();

        // Guardar en cache (1 hora)
        await cache.SetAsync(cacheKey, tiposMedidor, TimeSpan.FromHours(1));

        return Results.Ok(tiposMedidor);
    }

    private static async Task<IResult> ActualizarTipoMedidor(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        string codigo,
        [FromBody] ActualizarTipoMedidorRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null || !config.TiposMedidor.Any(t => t.Codigo == codigo))
            return Results.NotFound($"Tipo de medidor con código '{codigo}' no encontrado");

        session.Events.Append(ConfiguracionGlobal.SingletonId,
            new TipoMedidorActualizado(codigo, req.Nombre, req.Unidad, userId, userName));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:tiposmedidor");

        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoMedidor(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        string codigo,
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null || !config.TiposMedidor.Any(t => t.Codigo == codigo))
            return Results.NotFound($"Tipo de medidor con código '{codigo}' no encontrado");

        session.Events.Append(ConfiguracionGlobal.SingletonId,
            new EstadoTipoMedidorCambiado(codigo, req.Activo, userId, userName));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:tiposmedidor");

        return Results.Ok();
    }

    // --- Grupos de Mantenimiento ---

    private static async Task<IResult> CrearGrupo(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        [FromBody] CrearGrupoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var configId = ConfiguracionGlobal.SingletonId;

        var config = await session.LoadAsync<ConfiguracionGlobal>(configId);
        if (config != null && config.GruposMantenimiento.Any(g =>
            g.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"El grupo '{req.Nombre}' ya existe.");
        }

        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        session.Events.Append(configId,
            new GrupoMantenimientoCreado(nuevoCodigo, req.Nombre, req.Descripcion, true, userId, userName, DateTimeOffset.Now));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:grupos");

        return Results.Ok();
    }

    private static async Task<IResult> ListarGrupos(
        IQuerySession session,
        ICacheService cache)
    {
        const string cacheKey = "configuracion:grupos";

        // Intentar obtener de cache
        var cached = await cache.GetAsync<List<GrupoMantenimiento>>(cacheKey);
        if (cached != null)
            return Results.Ok(cached);

        // Si no está en cache, consultar BD
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var grupos = config?.GruposMantenimiento ?? new List<GrupoMantenimiento>();

        // Guardar en cache (1 hora)
        await cache.SetAsync(cacheKey, grupos, TimeSpan.FromHours(1));

        return Results.Ok(grupos);
    }

    private static async Task<IResult> ActualizarGrupo(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        string codigo,
        [FromBody] ActualizarGrupoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        // Validación de unicidad
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config != null && config.GruposMantenimiento.Any(g =>
            g.Codigo != codigo &&
            g.Nombre.Trim().Equals(req.Nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"El grupo '{req.Nombre}' ya existe.");
        }

        if (config == null || !config.GruposMantenimiento.Any(g => g.Codigo == codigo))
            return Results.NotFound($"Grupo de mantenimiento con código '{codigo}' no encontrado");

        session.Events.Append(ConfiguracionGlobal.SingletonId,
            new GrupoMantenimientoActualizado(codigo, req.Nombre, req.Descripcion, userId, userName));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:grupos");

        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoGrupo(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        string codigo,
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null || !config.GruposMantenimiento.Any(g => g.Codigo == codigo))
            return Results.NotFound($"Grupo de mantenimiento con código '{codigo}' no encontrado");

        session.Events.Append(ConfiguracionGlobal.SingletonId,
            new EstadoGrupoMantenimientoCambiado(codigo, req.Activo, userId, userName));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:grupos");

        return Results.Ok();
    }

    // --- Tipos de Falla ---

    private static async Task<IResult> CrearTipoFalla(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        [FromBody] CrearTipoFallaRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var configId = ConfiguracionGlobal.SingletonId;

        var config = await session.LoadAsync<ConfiguracionGlobal>(configId);
        if (config != null && config.TiposFalla.Any(f =>
            f.Descripcion.Equals(req.Descripcion, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"El tipo de falla '{req.Descripcion}' ya existe.");
        }

        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        session.Events.Append(configId,
            new TipoFallaCreado(nuevoCodigo, req.Descripcion, req.Prioridad, userId, userName, DateTimeOffset.Now));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:tiposfalla");

        return Results.Ok();
    }

    private static async Task<IResult> ListarTiposFalla(
        IQuerySession session,
        ICacheService cache)
    {
        const string cacheKey = "configuracion:tiposfalla";

        // Intentar obtener de cache
        var cached = await cache.GetAsync<List<TipoFalla>>(cacheKey);
        if (cached != null)
            return Results.Ok(cached);

        // Si no está en cache, consultar BD
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var tiposFalla = config?.TiposFalla ?? new List<TipoFalla>();

        // Guardar en cache (1 hora)
        await cache.SetAsync(cacheKey, tiposFalla, TimeSpan.FromHours(1));

        return Results.Ok(tiposFalla);
    }

    // --- Causas de Falla ---

    private static async Task<IResult> CrearCausaFalla(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        [FromBody] CrearCausaFallaRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var configId = ConfiguracionGlobal.SingletonId;

        var config = await session.LoadAsync<ConfiguracionGlobal>(configId);
        if (config != null && config.CausasFalla.Any(c =>
            c.Descripcion.Trim().Equals(req.Descripcion.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"La causa de falla '{req.Descripcion}' ya existe.");
        }

        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        session.Events.Append(configId,
            new CausaFallaCreada(nuevoCodigo, req.Descripcion, userId, userName, DateTimeOffset.Now));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:causasfalla");

        return Results.Ok();
    }

    private static async Task<IResult> ListarCausasFalla(
        IQuerySession session,
        ICacheService cache)
    {
        const string cacheKey = "configuracion:causasfalla";

        // Intentar obtener de cache
        var cached = await cache.GetAsync<List<CausaFalla>>(cacheKey);
        if (cached != null)
            return Results.Ok(cached);

        // Si no está en cache, consultar BD
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        var causasFalla = config?.CausasFalla ?? new List<CausaFalla>();

        // Guardar en cache (1 hora)
        await cache.SetAsync(cacheKey, causasFalla, TimeSpan.FromHours(1));

        return Results.Ok(causasFalla);
    }

    private static async Task<IResult> ActualizarCausaFalla(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        string codigo,
        [FromBody] ActualizarCausaFallaRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        // Validación de unicidad
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config != null && config.CausasFalla.Any(c =>
            c.Codigo != codigo &&
            c.Descripcion.Trim().Equals(req.Descripcion.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"La causa de falla '{req.Descripcion}' ya existe.");
        }

        if (config == null || !config.CausasFalla.Any(c => c.Codigo == codigo))
            return Results.NotFound($"Causa de falla con código '{codigo}' no encontrado");

        session.Events.Append(ConfiguracionGlobal.SingletonId,
            new CausaFallaActualizada(codigo, req.Descripcion, userId, userName));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:causasfalla");

        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoCausaFalla(
        IDocumentSession session,
        ICacheService cache,
        HttpContext httpContext,
        string codigo,
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config == null || !config.CausasFalla.Any(c => c.Codigo == codigo))
            return Results.NotFound($"Causa de falla con código '{codigo}' no encontrado");

        session.Events.Append(ConfiguracionGlobal.SingletonId,
            new EstadoCausaFallaCambiado(codigo, req.Activo, userId, userName));
        await session.SaveChangesAsync();

        // Invalidar cache
        await cache.RemoveAsync("configuracion:causasfalla");

        return Results.Ok();
    }

}
