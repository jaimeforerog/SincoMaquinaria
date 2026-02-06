using Hangfire;
using SincoMaquinaria.Services.Jobs;

namespace SincoMaquinaria.Services;

public interface IBackgroundJobService
{
    string EnqueueImportarEquipos(Stream fileStream, string fileName);
    string EnqueueImportarEmpleados(Stream fileStream, string fileName);
    void ScheduleLimpiezaTokensExpirados();
}

public class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public BackgroundJobService(
        IBackgroundJobClient jobClient,
        IRecurringJobManager recurringJobManager)
    {
        _jobClient = jobClient;
        _recurringJobManager = recurringJobManager;
    }

    public string EnqueueImportarEquipos(Stream fileStream, string fileName)
    {
        // Guardar archivo temporalmente
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

        using (var fileStream2 = File.Create(tempPath))
        {
            fileStream.CopyTo(fileStream2);
        }

        // Encolar job
        var jobId = _jobClient.Enqueue<ImportacionJobHandler>(
            x => x.ImportarEquiposAsync(tempPath, null));

        return jobId;
    }

    public string EnqueueImportarEmpleados(Stream fileStream, string fileName)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

        using (var fileStream2 = File.Create(tempPath))
        {
            fileStream.CopyTo(fileStream2);
        }

        var jobId = _jobClient.Enqueue<ImportacionJobHandler>(
            x => x.ImportarEmpleadosAsync(tempPath, null));

        return jobId;
    }

    public void ScheduleLimpiezaTokensExpirados()
    {
        // Ejecutar diariamente a las 3 AM
        _recurringJobManager.AddOrUpdate<MantenimientoJobHandler>(
            "limpieza-tokens-expirados",
            x => x.LimpiarTokensExpiradosAsync(null),
            "0 3 * * *", // Cron: 3 AM diario
            new Hangfire.RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });
    }
}
