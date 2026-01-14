using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.DTOs.Common;
using SincoMaquinaria.DTOs.Requests;
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
        group.MapPost("/", CrearRutina);
        
        // Update endpoints
        group.MapPut("/{id:guid}", ActualizarRutina);
        group.MapPut("/{id:guid}/partes/{parteId:guid}", ActualizarParte);
        group.MapPost("/{id:guid}/partes", AgregarParte);
        group.MapDelete("/{id:guid}/partes/{parteId:guid}", EliminarParte);
        group.MapPut("/{id:guid}/partes/{parteId:guid}/actividades/{actividadId:guid}", ActualizarActividad);
        group.MapPost("/{id:guid}/partes/{parteId:guid}/actividades", AgregarActividad);
        group.MapDelete("/{id:guid}/partes/{parteId:guid}/actividades/{actividadId:guid}", EliminarActividad);

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
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        return rutina != null ? Results.Ok(rutina) : Results.NotFound();
    }

    private static async Task<IResult> CrearRutina(
        IDocumentSession session,
        CreateRutinaRequest request)
    {
        var nuevaRutina = new RutinaMantenimiento
        {
            Id = Guid.NewGuid(),
            Descripcion = request.Descripcion,
            Grupo = request.Grupo,
            Partes = new List<ParteEquipo>()
        };

        session.Store(nuevaRutina);
        await session.SaveChangesAsync();

        return Results.Created($"/rutinas/{nuevaRutina.Id}", nuevaRutina);
    }

    private static async Task<IResult> ActualizarRutina(
        IDocumentSession session,
        Guid id,
        UpdateRutinaRequest request)
    {
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound();

        rutina.Descripcion = request.Descripcion;
        rutina.Grupo = request.Grupo;

        session.Update(rutina);
        await session.SaveChangesAsync();

        return Results.Ok(rutina);
    }

    private static async Task<IResult> ActualizarParte(
        IDocumentSession session,
        Guid id,
        Guid parteId,
        UpdateParteRequest request)
    {
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        if (parte == null)
            return Results.NotFound("Parte no encontrada");

        parte.Descripcion = request.Descripcion;

        session.Update(rutina);
        await session.SaveChangesAsync();

        return Results.Ok(parte);
    }

    private static async Task<IResult> AgregarParte(
        IDocumentSession session,
        Guid id,
        AddParteRequest request)
    {
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var nuevaParte = new ParteEquipo
        {
            Id = Guid.NewGuid(),
            Descripcion = request.Descripcion,
            Actividades = new List<ActividadMantenimiento>()
        };

        rutina.Partes.Add(nuevaParte);

        session.Update(rutina);
        await session.SaveChangesAsync();

        return Results.Created($"/rutinas/{id}/partes/{nuevaParte.Id}", nuevaParte);
    }

    private static async Task<IResult> EliminarParte(
        IDocumentSession session,
        Guid id,
        Guid parteId)
    {
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        if (parte == null)
            return Results.NotFound("Parte no encontrada");

        rutina.Partes.Remove(parte);

        session.Update(rutina);
        await session.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> ActualizarActividad(
        IDocumentSession session,
        Guid id,
        Guid parteId,
        Guid actividadId,
        UpdateActividadRequest request)
    {
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        if (parte == null)
            return Results.NotFound("Parte no encontrada");

        var actividad = parte.Actividades.FirstOrDefault(a => a.Id == actividadId);
        if (actividad == null)
            return Results.NotFound("Actividad no encontrada");

        actividad.Descripcion = request.Descripcion;
        actividad.Clase = request.Clase;
        actividad.Frecuencia = request.Frecuencia;
        actividad.UnidadMedida = request.UnidadMedida;
        actividad.NombreMedidor = request.NombreMedidor;
        actividad.AlertaFaltando = request.AlertaFaltando;
        actividad.Frecuencia2 = request.Frecuencia2;
        actividad.UnidadMedida2 = request.UnidadMedida2;
        actividad.NombreMedidor2 = request.NombreMedidor2;
        actividad.AlertaFaltando2 = request.AlertaFaltando2;
        actividad.Insumo = request.Insumo;
        actividad.Cantidad = request.Cantidad;

        session.Update(rutina);
        await session.SaveChangesAsync();

        return Results.Ok(actividad);
    }

    private static async Task<IResult> AgregarActividad(
        IDocumentSession session,
        Guid id,
        Guid parteId,
        AddActividadRequest request)
    {
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        if (parte == null)
            return Results.NotFound("Parte no encontrada");

        var nuevaActividad = new ActividadMantenimiento
        {
            Id = Guid.NewGuid(),
            Descripcion = request.Descripcion,
            Clase = request.Clase,
            Frecuencia = request.Frecuencia,
            UnidadMedida = request.UnidadMedida,
            NombreMedidor = request.NombreMedidor,
            AlertaFaltando = request.AlertaFaltando,
            Frecuencia2 = request.Frecuencia2,
            UnidadMedida2 = request.UnidadMedida2,
            NombreMedidor2 = request.NombreMedidor2,
            AlertaFaltando2 = request.AlertaFaltando2,
            Insumo = request.Insumo,
            Cantidad = request.Cantidad
        };

        parte.Actividades.Add(nuevaActividad);

        session.Update(rutina);
        await session.SaveChangesAsync();

        return Results.Created($"/rutinas/{id}/partes/{parteId}/actividades/{nuevaActividad.Id}", nuevaActividad);
    }

    private static async Task<IResult> EliminarActividad(
        IDocumentSession session,
        Guid id,
        Guid parteId,
        Guid actividadId)
    {
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        if (parte == null)
            return Results.NotFound("Parte no encontrada");

        var actividad = parte.Actividades.FirstOrDefault(a => a.Id == actividadId);
        if (actividad == null)
            return Results.NotFound("Actividad no encontrada");

        parte.Actividades.Remove(actividad);

        session.Update(rutina);
        await session.SaveChangesAsync();

        return Results.NoContent();
    }
}
