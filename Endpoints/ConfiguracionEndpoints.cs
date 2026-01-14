using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Infrastructure;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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
            .AddEndpointFilter<ValidationFilter<CrearTipoMedidorRequest>>();
        group.MapGet("/medidores", ListarTiposMedidor)
            .WithTags("Configuración - Medidores");
        group.MapPut("/medidores/{codigo}", ActualizarTipoMedidor)
            .WithTags("Configuración - Medidores")
            .AddEndpointFilter<ValidationFilter<ActualizarTipoMedidorRequest>>();
        group.MapPut("/medidores/{codigo}/estado", CambiarEstadoMedidor)
            .WithTags("Configuración - Medidores");

        // Grupos de Mantenimiento
        group.MapPost("/grupos", CrearGrupo)
            .WithTags("Configuración - Grupos")
            .AddEndpointFilter<ValidationFilter<CrearGrupoRequest>>();
        group.MapGet("/grupos", ListarGrupos)
            .WithTags("Configuración - Grupos");
        group.MapPut("/grupos/{codigo}", ActualizarGrupo)
            .WithTags("Configuración - Grupos")
            .AddEndpointFilter<ValidationFilter<ActualizarGrupoRequest>>();
        group.MapPut("/grupos/{codigo}/estado", CambiarEstadoGrupo)
            .WithTags("Configuración - Grupos");

        // Tipos de Falla
        group.MapPost("/fallas", CrearTipoFalla)
            .WithTags("Configuración - Fallas")
            .AddEndpointFilter<ValidationFilter<CrearTipoFallaRequest>>();
        group.MapGet("/fallas", ListarTiposFalla)
            .WithTags("Configuración - Fallas");

        // Causas de Falla
        group.MapPost("/causas-falla", CrearCausaFalla)
            .WithTags("Configuración - Causas Falla")
            .AddEndpointFilter<ValidationFilter<CrearCausaFallaRequest>>();
        group.MapGet("/causas-falla", ListarCausasFalla)
            .WithTags("Configuración - Causas Falla");
        group.MapPut("/causas-falla/{codigo}", ActualizarCausaFalla)
            .WithTags("Configuración - Causas Falla")
            .AddEndpointFilter<ValidationFilter<ActualizarCausaFallaRequest>>();
        group.MapPut("/causas-falla/{codigo}/estado", CambiarEstadoCausaFalla)
            .WithTags("Configuración - Causas Falla");

        return app;
    }

    // --- Tipos de Medidor ---

    private static async Task<IResult> CrearTipoMedidor(
        IDocumentSession session,
        HttpContext httpContext,
        [FromBody] CrearTipoMedidorRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
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
        return Results.Ok();
    }

    private static async Task<IResult> ListarTiposMedidor(IQuerySession session)
    {
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        return Results.Ok(config?.TiposMedidor ?? new List<TipoMedidor>());
    }

    private static async Task<IResult> ActualizarTipoMedidor(
        IDocumentSession session,
        HttpContext httpContext,
        string codigo, 
        [FromBody] ActualizarTipoMedidorRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new TipoMedidorActualizado(codigo, req.Nombre, req.Unidad, userId, userName));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoMedidor(
        IDocumentSession session,
        HttpContext httpContext,
        string codigo, 
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new EstadoTipoMedidorCambiado(codigo, req.Activo, userId, userName));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    // --- Grupos de Mantenimiento ---

    private static async Task<IResult> CrearGrupo(
        IDocumentSession session,
        HttpContext httpContext,
        [FromBody] CrearGrupoRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
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
        return Results.Ok();
    }

    private static async Task<IResult> ListarGrupos(IQuerySession session)
    {
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        return Results.Ok(config?.GruposMantenimiento ?? new List<GrupoMantenimiento>());
    }

    private static async Task<IResult> ActualizarGrupo(
        IDocumentSession session,
        HttpContext httpContext,
        string codigo, 
        [FromBody] ActualizarGrupoRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
        
        // Validación de unicidad
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config != null && config.GruposMantenimiento.Any(g => 
            g.Codigo != codigo && 
            g.Nombre.Trim().Equals(req.Nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"El grupo '{req.Nombre}' ya existe.");
        }

        session.Events.Append(ConfiguracionGlobal.SingletonId,
            new GrupoMantenimientoActualizado(codigo, req.Nombre, req.Descripcion, userId, userName));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoGrupo(
        IDocumentSession session,
        HttpContext httpContext,
        string codigo, 
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new EstadoGrupoMantenimientoCambiado(codigo, req.Activo, userId, userName));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    // --- Tipos de Falla ---

    private static async Task<IResult> CrearTipoFalla(
        IDocumentSession session,
        HttpContext httpContext,
        [FromBody] CrearTipoFallaRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
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
        return Results.Ok();
    }

    private static async Task<IResult> ListarTiposFalla(IQuerySession session)
    {
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        return Results.Ok(config?.TiposFalla ?? new List<TipoFalla>());
    }

    // --- Causas de Falla ---

    private static async Task<IResult> CrearCausaFalla(
        IDocumentSession session,
        HttpContext httpContext,
        [FromBody] CrearCausaFallaRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
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
        return Results.Ok();
    }

    private static async Task<IResult> ListarCausasFalla(IQuerySession session)
    {
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        return Results.Ok(config?.CausasFalla ?? new List<CausaFalla>());
    }

    private static async Task<IResult> ActualizarCausaFalla(
        IDocumentSession session,
        HttpContext httpContext,
        string codigo, 
        [FromBody] ActualizarCausaFallaRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
        
        // Validación de unicidad
        var config = await session.LoadAsync<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId);
        if (config != null && config.CausasFalla.Any(c => 
            c.Codigo != codigo && 
            c.Descripcion.Trim().Equals(req.Descripcion.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"La causa de falla '{req.Descripcion}' ya existe.");
        }

        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new CausaFallaActualizada(codigo, req.Descripcion, userId, userName));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoCausaFalla(
        IDocumentSession session, 
        HttpContext httpContext,
        string codigo, 
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = GetUserContext(httpContext);
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new EstadoCausaFallaCambiado(codigo, req.Activo, userId, userName));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    // Helper para extraer contexto del usuario desde JWT
    private static (Guid? UserId, string? UserName) GetUserContext(HttpContext context)
    {
        var sub = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                  ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var name = context.User.FindFirst(ClaimTypes.Name)?.Value 
                   ?? context.User.FindFirst("name")?.Value;

        Guid? uid = null;
        if (Guid.TryParse(sub, out var g)) uid = g;

        return (uid, name);
    }
}
