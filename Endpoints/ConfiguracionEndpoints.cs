using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;

namespace SincoMaquinaria.Endpoints;

public static class ConfiguracionEndpoints
{
    public static WebApplication MapConfiguracionEndpoints(this WebApplication app)
    {
        // Tipos de Medidor
        app.MapPost("/configuracion/medidores", CrearTipoMedidor)
            .WithTags("Configuración - Medidores");
        app.MapGet("/configuracion/medidores", ListarTiposMedidor)
            .WithTags("Configuración - Medidores");
        app.MapPut("/configuracion/medidores/{codigo}", ActualizarTipoMedidor)
            .WithTags("Configuración - Medidores");
        app.MapPut("/configuracion/medidores/{codigo}/estado", CambiarEstadoMedidor)
            .WithTags("Configuración - Medidores");

        // Grupos de Mantenimiento
        app.MapPost("/configuracion/grupos", CrearGrupo)
            .WithTags("Configuración - Grupos");
        app.MapGet("/configuracion/grupos", ListarGrupos)
            .WithTags("Configuración - Grupos");
        app.MapPut("/configuracion/grupos/{codigo}", ActualizarGrupo)
            .WithTags("Configuración - Grupos");
        app.MapPut("/configuracion/grupos/{codigo}/estado", CambiarEstadoGrupo)
            .WithTags("Configuración - Grupos");

        // Tipos de Falla
        app.MapPost("/configuracion/fallas", CrearTipoFalla)
            .WithTags("Configuración - Fallas");
        app.MapGet("/configuracion/fallas", ListarTiposFalla)
            .WithTags("Configuración - Fallas");

        // Causas de Falla
        app.MapPost("/configuracion/causas-falla", CrearCausaFalla)
            .WithTags("Configuración - Causas Falla");
        app.MapGet("/configuracion/causas-falla", ListarCausasFalla)
            .WithTags("Configuración - Causas Falla");
        app.MapPut("/configuracion/causas-falla/{codigo}", ActualizarCausaFalla)
            .WithTags("Configuración - Causas Falla");
        app.MapPut("/configuracion/causas-falla/{codigo}/estado", CambiarEstadoCausaFalla)
            .WithTags("Configuración - Causas Falla");

        return app;
    }

    // --- Tipos de Medidor ---

    private static async Task<IResult> CrearTipoMedidor(
        IDocumentSession session, 
        [FromBody] CrearTipoMedidorRequest req)
    {
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
            new TipoMedidorCreado(nuevoCodigo, req.Nombre, req.Unidad.ToUpper()));
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
        string codigo, 
        [FromBody] ActualizarTipoMedidorRequest req)
    {
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new TipoMedidorActualizado(codigo, req.Nombre, req.Unidad));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoMedidor(
        IDocumentSession session, 
        string codigo, 
        [FromBody] CambiarEstadoRequest req)
    {
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new EstadoTipoMedidorCambiado(codigo, req.Activo));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    // --- Grupos de Mantenimiento ---

    private static async Task<IResult> CrearGrupo(
        IDocumentSession session, 
        [FromBody] CrearGrupoRequest req)
    {
        var configId = ConfiguracionGlobal.SingletonId;
        
        var config = await session.LoadAsync<ConfiguracionGlobal>(configId);
        if (config != null && config.GruposMantenimiento.Any(g => 
            g.Nombre.Equals(req.Nombre, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"El grupo '{req.Nombre}' ya existe.");
        }
        
        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        session.Events.Append(configId, 
            new GrupoMantenimientoCreado(nuevoCodigo, req.Nombre, req.Descripcion, true));
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
        string codigo, 
        [FromBody] ActualizarGrupoRequest req)
    {
        session.Events.Append(ConfiguracionGlobal.SingletonId,
            new GrupoMantenimientoActualizado(codigo, req.Nombre, req.Descripcion));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoGrupo(
        IDocumentSession session, 
        string codigo, 
        [FromBody] CambiarEstadoRequest req)
    {
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new EstadoGrupoMantenimientoCambiado(codigo, req.Activo));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    // --- Tipos de Falla ---

    private static async Task<IResult> CrearTipoFalla(
        IDocumentSession session, 
        [FromBody] CrearTipoFallaRequest req)
    {
        var configId = ConfiguracionGlobal.SingletonId;
        
        var config = await session.LoadAsync<ConfiguracionGlobal>(configId);
        if (config != null && config.TiposFalla.Any(f => 
            f.Descripcion.Equals(req.Descripcion, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"El tipo de falla '{req.Descripcion}' ya existe.");
        }
        
        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        session.Events.Append(configId, 
            new TipoFallaCreado(nuevoCodigo, req.Descripcion, req.Prioridad));
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
        [FromBody] CrearCausaFallaRequest req)
    {
        var configId = ConfiguracionGlobal.SingletonId;
        
        var config = await session.LoadAsync<ConfiguracionGlobal>(configId);
        if (config != null && config.CausasFalla.Any(c => 
            c.Descripcion.Equals(req.Descripcion, StringComparison.OrdinalIgnoreCase)))
        {
            return Results.Conflict($"La causa de falla '{req.Descripcion}' ya existe.");
        }
        
        var nuevoCodigo = Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);
        session.Events.Append(configId, 
            new CausaFallaCreada(nuevoCodigo, req.Descripcion));
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
        string codigo, 
        [FromBody] ActualizarCausaFallaRequest req)
    {
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new CausaFallaActualizada(codigo, req.Descripcion));
        await session.SaveChangesAsync();
        return Results.Ok();
    }

    private static async Task<IResult> CambiarEstadoCausaFalla(
        IDocumentSession session, 
        string codigo, 
        [FromBody] CambiarEstadoRequest req)
    {
        session.Events.Append(ConfiguracionGlobal.SingletonId, 
            new EstadoCausaFallaCambiado(codigo, req.Activo));
        await session.SaveChangesAsync();
        return Results.Ok();
    }
}
