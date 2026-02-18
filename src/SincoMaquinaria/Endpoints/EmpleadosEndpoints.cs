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

public static class EmpleadosEndpoints
{
    public static WebApplication MapEmpleadosEndpoints(this WebApplication app, int maxFileUploadSizeMB)
    {
        var group = app.MapGroup("/empleados")
            .WithTags("Empleados")
            .RequireAuthorization();

        group.MapPost("/importar", async (
            ExcelEmpleadoImportService importService,
            HttpContext httpContext,
            IFormFile? file) => await ImportarEmpleados(importService, httpContext, file, maxFileUploadSizeMB))
            .DisableAntiforgery();

        group.MapPost("/", CrearEmpleado)
            .AddEndpointFilter<ValidationFilter<CrearEmpleadoRequest>>();
        group.MapGet("/", ListarEmpleados);
        group.MapPut("/{id:guid}", ActualizarEmpleado)
            .AddEndpointFilter<ValidationFilter<ActualizarEmpleadoRequest>>();


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
        var worksheet = package.Workbook.Worksheets.Add("Empleados");

        // Encabezados
        var headers = new[]
        {
            "Nombres", 
            "Apellidos", 
            "No. Identificación", 
            "Cargo", 
            "Especialidad", 
            "Valor $ (Hr)"
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
        var fileName = $"plantillaEmpleados_{timestamp}.xlsx";

        return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }


    private static async Task<IResult> ImportarEmpleados(
        ExcelEmpleadoImportService importService,
        HttpContext httpContext,
        IFormFile? file,
        int maxFileUploadSizeMB)
    {
        var fileError = HttpContextExtensions.ValidateFileUpload(file, maxFileUploadSizeMB);
        if (fileError != null) return fileError;

        if (file == null)
            return Results.BadRequest(new { error = "No se proporcionó ningún archivo" });

        var (userId, userName) = httpContext.GetUserContext();

        using var stream = file.OpenReadStream();
        try
        {
            var count = await importService.ImportarEmpleados(stream, userId, userName);
            return Results.Ok(new { Message = $"Importación completada. {count} empleados creados." });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> CrearEmpleado(
        EmpleadosService service,
        HttpContext httpContext,
        [FromBody] CrearEmpleadoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        if (!userId.HasValue)
            return Results.Unauthorized();

        var result = await service.CrearEmpleado(req, userId.Value, userName);

        if (!result.IsSuccess)
        {
             return Results.Conflict(result.Error);
        }

        return Results.Ok(new { Id = result.Value });
    }

    private static async Task<IResult> ListarEmpleados(
        IQuerySession session,
        [AsParameters] PaginationRequest pagination)
    {
        // Obtener resultados paginados
        var pagedResult = await session.Query<Empleado>()
            .ApplyOrdering(pagination)
            .ToPagedResponseAsync(pagination);

        // Proyectar los datos
        var proyectados = pagedResult.Data.Select(e => new
        {
            e.Id,
            e.Nombre,
            e.Identificacion,
            Cargo = e.Cargo.ToString(),
            e.Especialidad,
            e.ValorHora,
            Estado = e.Estado.ToString()
        }).ToList();

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

    private static async Task<IResult> ActualizarEmpleado(
        EmpleadosService service,
        HttpContext httpContext,
        Guid id, 
        [FromBody] ActualizarEmpleadoRequest req)
    {
        var (userId, userName) = httpContext.GetUserContext();

        if (!userId.HasValue)
            return Results.Unauthorized();

        var result = await service.ActualizarEmpleado(id, req, userId.Value, userName);

        if (result.IsNotFound)
            return Results.NotFound(result.Error);

        if (!result.IsSuccess)
            return Results.Conflict(result.Error);

        return Results.Ok();
    }

}
