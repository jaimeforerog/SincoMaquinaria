using Microsoft.AspNetCore.Mvc;
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
        ConfiguracionService service,
        HttpContext httpContext,
        [FromBody] CrearTipoMedidorRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CrearTipoMedidor(req, userId, userName);
        return result.IsSuccess ? Results.Ok() : Results.Conflict(result.Error);
    }

    private static async Task<IResult> ListarTiposMedidor(ConfiguracionService service)
    {
        var result = await service.ListarTiposMedidor();
        if (!result.IsSuccess)
            return Results.Problem(result.Error);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ActualizarTipoMedidor(
        ConfiguracionService service,
        HttpContext httpContext,
        string codigo,
        [FromBody] ActualizarTipoMedidorRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.ActualizarTipoMedidor(codigo, req, userId, userName);
        if (result.IsNotFound) return Results.NotFound(result.Error);
        return result.IsSuccess ? Results.Ok() : Results.Conflict(result.Error);
    }

    private static async Task<IResult> CambiarEstadoMedidor(
        ConfiguracionService service,
        HttpContext httpContext,
        string codigo,
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CambiarEstadoMedidor(codigo, req.Activo, userId, userName);
        if (result.IsNotFound) return Results.NotFound(result.Error);
        return Results.Ok();
    }

    // --- Grupos de Mantenimiento ---

    private static async Task<IResult> CrearGrupo(
        ConfiguracionService service,
        HttpContext httpContext,
        [FromBody] CrearGrupoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CrearGrupo(req, userId, userName);
        return result.IsSuccess ? Results.Ok() : Results.Conflict(result.Error);
    }

    private static async Task<IResult> ListarGrupos(ConfiguracionService service)
    {
        var result = await service.ListarGrupos();
        if (!result.IsSuccess)
            return Results.Problem(result.Error);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ActualizarGrupo(
        ConfiguracionService service,
        HttpContext httpContext,
        string codigo,
        [FromBody] ActualizarGrupoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.ActualizarGrupo(codigo, req, userId, userName);
        if (result.IsNotFound) return Results.NotFound(result.Error);
        return result.IsSuccess ? Results.Ok() : Results.Conflict(result.Error);
    }

    private static async Task<IResult> CambiarEstadoGrupo(
        ConfiguracionService service,
        HttpContext httpContext,
        string codigo,
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CambiarEstadoGrupo(codigo, req.Activo, userId, userName);
        if (result.IsNotFound) return Results.NotFound(result.Error);
        return Results.Ok();
    }

    // --- Tipos de Falla ---

    private static async Task<IResult> CrearTipoFalla(
        ConfiguracionService service,
        HttpContext httpContext,
        [FromBody] CrearTipoFallaRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CrearTipoFalla(req, userId, userName);
        return result.IsSuccess ? Results.Ok() : Results.Conflict(result.Error);
    }

    private static async Task<IResult> ListarTiposFalla(ConfiguracionService service)
    {
        var result = await service.ListarTiposFalla();
        if (!result.IsSuccess)
            return Results.Problem(result.Error);
        return Results.Ok(result.Value);
    }

    // --- Causas de Falla ---

    private static async Task<IResult> CrearCausaFalla(
        ConfiguracionService service,
        HttpContext httpContext,
        [FromBody] CrearCausaFallaRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CrearCausaFalla(req, userId, userName);
        return result.IsSuccess ? Results.Ok() : Results.Conflict(result.Error);
    }

    private static async Task<IResult> ListarCausasFalla(ConfiguracionService service)
    {
        var result = await service.ListarCausasFalla();
        if (!result.IsSuccess)
            return Results.Problem(result.Error);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ActualizarCausaFalla(
        ConfiguracionService service,
        HttpContext httpContext,
        string codigo,
        [FromBody] ActualizarCausaFallaRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.ActualizarCausaFalla(codigo, req, userId, userName);
        if (result.IsNotFound) return Results.NotFound(result.Error);
        return result.IsSuccess ? Results.Ok() : Results.Conflict(result.Error);
    }

    private static async Task<IResult> CambiarEstadoCausaFalla(
        ConfiguracionService service,
        HttpContext httpContext,
        string codigo,
        [FromBody] CambiarEstadoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CambiarEstadoCausaFalla(codigo, req.Activo, userId, userName);
        if (result.IsNotFound) return Results.NotFound(result.Error);
        return Results.Ok();
    }

}
