using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Infrastructure;
using SincoMaquinaria.Services;
using SincoMaquinaria.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
            HttpContext httpContext,
            IFormFile? file, 
            DashboardNotifier notifier) => await ImportarEquipos(importService, httpContext, file, maxFileUploadSizeMB, notifier))
            .DisableAntiforgery();

        group.MapGet("/", ListarEquipos);
        group.MapPost("/", CrearEquipo);
        group.MapGet("/{id:guid}", ObtenerEquipo);
        group.MapPut("/{id:guid}", ActualizarEquipo)
            .AddEndpointFilter<ValidationFilter<ActualizarEquipoRequest>>();

        // Plantilla Excel para importación
        group.MapGet("/plantilla", DescargarPlantilla)
            .AllowAnonymous();

        return app;
    }

    private static IResult DescargarPlantilla()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Equipos");

        var headers = new[]
        {
            "Placa",
            "Descripcion",
            "Grupo de mtto",
            "Rutina",
            "Medidor 1",
            "Medidor inicial medidor 1",
            "Fecha inicial medidor 1",
            "Medidor 2",
            "Medidor inicial medidor 2",
            "Fecha inicial medidor 2",
            "Fecha ultima OT"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 114, 196));
            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var bytes = package.GetAsByteArray();
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"plantillaEquipos_{timestamp}.xlsx";

        return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private static async Task<IResult> ImportarEquipos(
        ExcelEquipoImportService importService,
        HttpContext httpContext,
        IFormFile? file,
        int maxFileUploadSizeMB,
        DashboardNotifier notifier)
    {
        var fileError = HttpContextExtensions.ValidateFileUpload(file, maxFileUploadSizeMB);
        if (fileError != null) return fileError;

        if (file == null)
            return Results.BadRequest(new { error = "No se proporcionó ningún archivo" });

        var (userId, userName) = httpContext.GetUserContext();

        using var stream = file.OpenReadStream();
        try
        {
            var count = await importService.ImportarEquipos(stream, userId, userName);
            await notifier.NotificarEquiposImportados();
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

    private static async Task<IResult> CrearEquipo(
        EquiposService service,
        HttpContext httpContext,
        [FromBody] CrearEquipoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CrearEquipo(req, userId, userName);

        if (result.IsConflict)
            return Results.Conflict(result.Error);
        if (!result.IsSuccess)
            return Results.BadRequest(result.Error);

        return Results.Created($"/equipos/{result.Value}", new { Id = result.Value });
    }

    private static async Task<IResult> ActualizarEquipo(
        EquiposService service,
        HttpContext httpContext,
        Guid id, 
        [FromBody] ActualizarEquipoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.ActualizarEquipo(id, req, userId, userName);

        if (!result.IsSuccess)
            return Results.BadRequest(result.Error);

        return Results.Ok();
    }

}
