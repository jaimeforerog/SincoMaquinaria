using Hangfire.Server;
using Marten;

namespace SincoMaquinaria.Services.Jobs;

public class ImportacionJobHandler
{
    private readonly IDocumentStore _store;
    private readonly ILogger<ImportacionJobHandler> _logger;
    private readonly ExcelEquipoImportService _equipoImportService;
    private readonly ExcelEmpleadoImportService _empleadoImportService;

    public ImportacionJobHandler(
        IDocumentStore store,
        ILogger<ImportacionJobHandler> logger,
        ExcelEquipoImportService equipoImportService,
        ExcelEmpleadoImportService empleadoImportService)
    {
        _store = store;
        _logger = logger;
        _equipoImportService = equipoImportService;
        _empleadoImportService = empleadoImportService;
    }

    public async Task ImportarEquiposAsync(string filePath, PerformContext? context)
    {
        try
        {
            _logger.LogInformation("Iniciando importación de equipos desde {FilePath}", filePath);

            using var stream = File.OpenRead(filePath);
            var cantidadImportada = await _equipoImportService.ImportarEquipos(stream);

            _logger.LogInformation("Importación completada. {Count} equipos importados.", cantidadImportada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en importación de equipos: {Message}", ex.Message);
            throw;
        }
        finally
        {
            // Limpiar archivo temporal
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                var dir = Path.GetDirectoryName(filePath);
                if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir!).Any())
                {
                    Directory.Delete(dir!);
                }
            }
        }
    }

    public async Task ImportarEmpleadosAsync(string filePath, PerformContext? context)
    {
        try
        {
            _logger.LogInformation("Iniciando importación de empleados desde {FilePath}", filePath);

            using var stream = File.OpenRead(filePath);
            var cantidadImportada = await _empleadoImportService.ImportarEmpleados(stream);

            _logger.LogInformation("Importación completada. {Count} empleados importados.", cantidadImportada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en importación de empleados: {Message}", ex.Message);
            throw;
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                var dir = Path.GetDirectoryName(filePath);
                if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir!).Any())
                {
                    Directory.Delete(dir!);
                }
            }
        }
    }
}
