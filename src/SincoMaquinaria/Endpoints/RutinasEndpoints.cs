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
            HttpContext httpContext,
            IFormFile? file) => await ImportarRutinas(importService, httpContext, file, maxFileUploadSizeMB))
            .DisableAntiforgery();

        group.MapGet("/", ListarRutinas);
        group.MapGet("/con-detalles", ListarRutinasConDetalles);
        group.MapGet("/{id:guid}", ObtenerRutina);
        group.MapPost("/", CrearRutina)
            .AddEndpointFilter<Infrastructure.ValidationFilter<CreateRutinaRequest>>();
        
        // Update endpoints
        group.MapPut("/{id:guid}", ActualizarRutina)
            .AddEndpointFilter<Infrastructure.ValidationFilter<UpdateRutinaRequest>>();
        
        group.MapPut("/{id:guid}/partes/{parteId:guid}", ActualizarParte)
            .AddEndpointFilter<Infrastructure.ValidationFilter<UpdateParteRequest>>();
            
        group.MapPost("/{id:guid}/partes", AgregarParte)
            .AddEndpointFilter<Infrastructure.ValidationFilter<AddParteRequest>>();
            
        group.MapDelete("/{id:guid}/partes/{parteId:guid}", EliminarParte);
        
        group.MapPut("/{id:guid}/partes/{parteId:guid}/actividades/{actividadId:guid}", ActualizarActividad)
            .AddEndpointFilter<Infrastructure.ValidationFilter<UpdateActividadRequest>>();
            
        group.MapPost("/{id:guid}/partes/{parteId:guid}/actividades", AgregarActividad)
             .AddEndpointFilter<Infrastructure.ValidationFilter<AddActividadRequest>>();
             
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
        var count = await importService.ImportarRutinas(stream, userId, userName);
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

    private static async Task<IResult> ListarRutinasConDetalles(
        IQuerySession session,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await session.Query<RutinaMantenimiento>()
            .ApplyOrdering(pagination)
            .ToPagedResponseAsync(pagination);

        return Results.Ok(result);
    }

    private static async Task<IResult> ObtenerRutina(IQuerySession session, Guid id)
    {
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        return rutina != null ? Results.Ok(rutina) : Results.NotFound();
    }

    private static async Task<IResult> CrearRutina(
        RutinasService service,
        HttpContext httpContext,
        CreateRutinaRequest request)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.CrearRutina(request, userId, userName);

        if (!result.IsSuccess)
            return Results.Conflict(result.Error);

        return Results.Created($"/rutinas/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> ActualizarRutina(
        RutinasService service,
        HttpContext httpContext,
        Guid id,
        UpdateRutinaRequest request)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var result = await service.ActualizarRutina(id, request, userId, userName);

        if (result.IsNotFound)
            return Results.NotFound(result.Error);

        if (!result.IsSuccess)
            return Results.Conflict(result.Error);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ActualizarParte(
        IDocumentSession session,
        HttpContext httpContext,
        Guid id,
        Guid parteId,
        UpdateParteRequest request)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        if (!rutina.Partes.Any(p => p.Id == parteId))
            return Results.NotFound("Parte no encontrada");

        session.Events.Append(id, new ParteActualizada(parteId, request.Descripcion, userId, userName));
        await session.SaveChangesAsync();

        return Results.Ok(new { Id = parteId, Descripcion = request.Descripcion });
    }

    private static async Task<IResult> AgregarParte(
        IDocumentSession session,
        HttpContext httpContext,
        Guid id,
        AddParteRequest request)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var nuevaParteId = Guid.NewGuid();
        session.Events.Append(id, new ParteAgregada(nuevaParteId, request.Descripcion, id, userId, userName));
        await session.SaveChangesAsync();

        return Results.Created($"/rutinas/{id}/partes/{nuevaParteId}",
            new { Id = nuevaParteId, Descripcion = request.Descripcion, Actividades = new List<object>() });
    }

    private static async Task<IResult> EliminarParte(
        IDocumentSession session,
        HttpContext httpContext,
        Guid id,
        Guid parteId)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        if (!rutina.Partes.Any(p => p.Id == parteId))
            return Results.NotFound("Parte no encontrada");

        session.Events.Append(id, new ParteEliminada(parteId, userId, userName));
        await session.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> ActualizarActividad(
        IDocumentSession session,
        HttpContext httpContext,
        Guid id,
        Guid parteId,
        Guid actividadId,
        UpdateActividadRequest request)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        if (parte == null)
            return Results.NotFound("Parte no encontrada");

        if (!parte.Actividades.Any(a => a.Id == actividadId))
            return Results.NotFound("Actividad no encontrada");

        session.Events.Append(id, new ActividadDeRutinaActualizada(actividadId,
            request.Descripcion, request.Clase, request.Frecuencia, request.UnidadMedida,
            request.NombreMedidor, request.AlertaFaltando, request.Frecuencia2, request.UnidadMedida2,
            request.NombreMedidor2, request.AlertaFaltando2, request.Insumo, request.Cantidad,
            userId, userName));
        await session.SaveChangesAsync();

        return Results.Ok(new { Id = actividadId, Descripcion = request.Descripcion });
    }

    private static async Task<IResult> AgregarActividad(
        IDocumentSession session,
        HttpContext httpContext,
        Guid id,
        Guid parteId,
        AddActividadRequest request)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        if (!rutina.Partes.Any(p => p.Id == parteId))
            return Results.NotFound("Parte no encontrada");

        var nuevaActividadId = Guid.NewGuid();
        session.Events.Append(id, new ActividadDeRutinaAgregada(nuevaActividadId, parteId,
            request.Descripcion, request.Clase, request.Frecuencia, request.UnidadMedida,
            request.NombreMedidor, request.AlertaFaltando, request.Frecuencia2, request.UnidadMedida2,
            request.NombreMedidor2, request.AlertaFaltando2, request.Insumo, request.Cantidad,
            userId, userName));
        await session.SaveChangesAsync();

        return Results.Created($"/rutinas/{id}/partes/{parteId}/actividades/{nuevaActividadId}",
            new { Id = nuevaActividadId, Descripcion = request.Descripcion });
    }

    private static async Task<IResult> EliminarActividad(
        IDocumentSession session,
        HttpContext httpContext,
        Guid id,
        Guid parteId,
        Guid actividadId)
    {
        var (userId, userName) = httpContext.GetUserContext();
        var rutina = await session.LoadAsync<RutinaMantenimiento>(id);
        if (rutina == null)
            return Results.NotFound("Rutina no encontrada");

        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        if (parte == null)
            return Results.NotFound("Parte no encontrada");

        if (!parte.Actividades.Any(a => a.Id == actividadId))
            return Results.NotFound("Actividad no encontrada");

        session.Events.Append(id, new ActividadDeRutinaEliminada(actividadId, parteId, userId, userName));
        await session.SaveChangesAsync();

        return Results.NoContent();
    }
}
