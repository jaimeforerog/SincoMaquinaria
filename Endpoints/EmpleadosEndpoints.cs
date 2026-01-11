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
            IFormFile? file) => await ImportarEmpleados(importService, file, maxFileUploadSizeMB))
            .DisableAntiforgery();

        group.MapPost("/", CrearEmpleado)
            .AddEndpointFilter<ValidationFilter<CrearEmpleadoRequest>>();
        group.MapGet("/", ListarEmpleados);
        group.MapPut("/{id:guid}", ActualizarEmpleado)
            .AddEndpointFilter<ValidationFilter<ActualizarEmpleadoRequest>>();

        return app;
    }

    private static async Task<IResult> ImportarEmpleados(
        ExcelEmpleadoImportService importService,
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
            var count = await importService.ImportarEmpleados(stream);
            return Results.Ok(new { Message = $"Importación completada. {count} empleados creados." });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> CrearEmpleado(
        IDocumentSession session, 
        [FromBody] CrearEmpleadoRequest req)
    {
        var empleadoId = Guid.NewGuid();
        session.Events.StartStream<Empleado>(empleadoId, 
            new EmpleadoCreado(empleadoId, req.Nombre, req.Identificacion, 
                req.Cargo, req.Especialidad ?? "", req.ValorHora, req.Estado));
        await session.SaveChangesAsync();
        return Results.Ok();
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
        IDocumentSession session, 
        Guid id, 
        [FromBody] ActualizarEmpleadoRequest req)
    {
        session.Events.Append(id, 
            new EmpleadoActualizado(id, req.Nombre, req.Identificacion, 
                req.Cargo, req.Especialidad ?? "", req.ValorHora, req.Estado));
        await session.SaveChangesAsync();
        return Results.Ok();
    }
}
