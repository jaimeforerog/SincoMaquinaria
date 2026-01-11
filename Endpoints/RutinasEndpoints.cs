using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.Services;
using SincoMaquinaria.Extensions;

namespace SincoMaquinaria.Endpoints;

public static class RutinasEndpoints
{
    public static WebApplication MapRutinasEndpoints(this WebApplication app, int maxFileUploadSizeMB)
    {
        var group = app.MapGroup("/rutinas")
            .WithTags("Rutinas de Mantenimiento")
            .RequireAuthorization();

        group.MapPost("/importar", async (
            ExcelImportService importService,
            IFormFile? file) => await ImportarRutinas(importService, file, maxFileUploadSizeMB))
            .DisableAntiforgery();

        group.MapGet("/", ListarRutinas);
        group.MapGet("/{id:guid}", ObtenerRutina);


        // Plantilla Excel para importación
        group.MapGet("/plantilla", DescargarPlantilla)
            .AllowAnonymous();

        return app;
    }

    private static IResult DescargarPlantilla()
    {
        // Configurar licencia EPPlus (Non-Commercial)
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Rutinas");

        // Encabezados
        var headers = new[]
        {
            "Grupo", 
            "Rutina", 
            "Parte", 
            "Actividad", 
            "Clase Actividad",
            "Frecuencia", 
            "Frec UM", 
            "Alerta Faltando", 
            "Frecuencia II", 
            "Frec UM II", 
            "Alerta Faltando II", 
            "Insumo", 
            "Cantidad"
        };

        // Escribir encabezados en la fila 1
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(68, 114, 196));
            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        // Ajustar ancho de columnas automáticamente
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // Generar el archivo
        var bytes = package.GetAsByteArray();
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"plantillaRutinas_{timestamp}.xlsx";

        return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }


    private static async Task<IResult> ImportarRutinas(
        ExcelImportService importService,
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
        var count = await importService.ImportarRutinas(stream);
        return Results.Ok(new { Message = $"Importación completada. {count} rutinas creadas." });
    }

    private static async Task<IResult> ListarRutinas(
        IQuerySession session,
        [AsParameters] PaginationRequest pagination)
    {
        // Obtener resultados paginados
        var pagedResult = await session.Query<RutinaMantenimiento>()
            .ApplyOrdering(pagination)
            .ToPagedResponseAsync(pagination);

        // Proyectar los datos
        var proyectados = pagedResult.Data
            .Select(r => new { r.Id, r.Descripcion })
            .ToList();

        // Crear nueva respuesta paginada con datos proyectados
        var resultado = new PagedResponse<object>
        {
            Data = proyectados,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount
        };

        return Results.Ok(resultado);
    }

    private static async Task<IResult> ObtenerRutina(IQuerySession session, Guid id)
    {
        var rutina = await session.Events.AggregateStreamAsync<RutinaMantenimiento>(id);
        return rutina != null ? Results.Ok(rutina) : Results.NotFound();
    }
}
