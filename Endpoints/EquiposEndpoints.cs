using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Infrastructure;
using SincoMaquinaria.Services;
using SincoMaquinaria.Extensions;

namespace SincoMaquinaria.Endpoints;

public static class EquiposEndpoints
{
    public static WebApplication MapEquiposEndpoints(this WebApplication app, int maxFileUploadSizeMB)
    {
        var group = app.MapGroup("/equipos")
            .WithTags("Equipos")
            .RequireAuthorization();

        group.MapPost("/importar", async (
            ExcelEquipoImportService importService,
            IFormFile? file) => await ImportarEquipos(importService, file, maxFileUploadSizeMB))
            .DisableAntiforgery();

        group.MapGet("/", ListarEquipos);
        group.MapGet("/{id:guid}", ObtenerEquipo);
        group.MapPut("/{id:guid}", ActualizarEquipo)
            .AddEndpointFilter<ValidationFilter<ActualizarEquipoRequest>>();

        return app;
    }

    private static async Task<IResult> ImportarEquipos(
        ExcelEquipoImportService importService,
        IFormFile? file,
        int maxFileUploadSizeMB)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded");

        // Validar tamaño del archivo
        var maxSizeBytes = maxFileUploadSizeMB * 1024 * 1024;
        if (file.Length > maxSizeBytes)
            return Results.BadRequest($"El archivo excede el tamaño máximo permitido de {maxFileUploadSizeMB} MB");

        // Validar extensión del archivo
        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
            return Results.BadRequest($"Tipo de archivo no permitido. Solo se aceptan archivos: {string.Join(", ", allowedExtensions)}");

        using var stream = file.OpenReadStream();
        try
        {
            var count = await importService.ImportarEquipos(stream);
            return Results.Ok(new { Message = $"Importación completada. {count} equipos creados/actualizados." });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> ListarEquipos(
        IQuerySession session,
        [AsParameters] PaginationRequest pagination)
    {
        var query = session.Query<Equipo>()
            .ApplyOrdering(pagination);

        var result = await query.ToPagedResponseAsync(pagination);
        return Results.Ok(result);
    }

    private static async Task<IResult> ObtenerEquipo(IQuerySession session, Guid id)
    {
        var equipo = await session.LoadAsync<Equipo>(id);
        return equipo is not null ? Results.Ok(equipo) : Results.NotFound();
    }

    private static async Task<IResult> ActualizarEquipo(
        IDocumentSession session, 
        Guid id, 
        [FromBody] ActualizarEquipoRequest req)
    {
        session.Events.Append(id, 
            new EquipoActualizado(id, req.Descripcion, req.Marca, req.Modelo, 
                req.Serie, req.Codigo, req.TipoMedidorId, req.TipoMedidorId2, 
                req.Grupo, req.Rutina));
        await session.SaveChangesAsync();
        return Results.Ok();
    }
}
