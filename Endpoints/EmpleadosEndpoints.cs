using Marten;
using Microsoft.AspNetCore.Mvc;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Services;

namespace SincoMaquinaria.Endpoints;

public static class EmpleadosEndpoints
{
    public static WebApplication MapEmpleadosEndpoints(this WebApplication app, int maxFileUploadSizeMB)
    {
        var group = app.MapGroup("/empleados")
            .WithTags("Empleados");

        group.MapPost("/importar", async (
            ExcelEmpleadoImportService importService,
            IFormFile? file) => await ImportarEmpleados(importService, file, maxFileUploadSizeMB))
            .DisableAntiforgery();

        group.MapPost("/", CrearEmpleado);
        group.MapGet("/", ListarEmpleados);
        group.MapPut("/{id:guid}", ActualizarEmpleado);

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

    private static async Task<IResult> ListarEmpleados(IQuerySession session)
    {
        var empleados = await session.Query<Empleado>().ToListAsync();
        return Results.Ok(empleados);
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
