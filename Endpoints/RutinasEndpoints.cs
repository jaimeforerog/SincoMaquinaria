using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Services;

namespace SincoMaquinaria.Endpoints;

public static class RutinasEndpoints
{
    public static WebApplication MapRutinasEndpoints(this WebApplication app, int maxFileUploadSizeMB)
    {
        var group = app.MapGroup("/rutinas")
            .WithTags("Rutinas de Mantenimiento");

        group.MapPost("/importar", async (
            ExcelImportService importService,
            IFormFile? file) => await ImportarRutinas(importService, file, maxFileUploadSizeMB))
            .DisableAntiforgery();

        group.MapGet("/", ListarRutinas);
        group.MapGet("/{id:guid}", ObtenerRutina);

        return app;
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

    private static async Task<IResult> ListarRutinas(IQuerySession session)
    {
        var rutinas = await session.Query<RutinaMantenimiento>()
            .Select(r => new { r.Id, r.Descripcion })
            .ToListAsync();
        return Results.Ok(rutinas);
    }

    private static async Task<IResult> ObtenerRutina(IQuerySession session, Guid id)
    {
        var rutina = await session.Events.AggregateStreamAsync<RutinaMantenimiento>(id);
        return rutina != null ? Results.Ok(rutina) : Results.NotFound();
    }
}
