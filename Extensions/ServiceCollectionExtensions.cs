using Marten;
using Marten.Events.Projections;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Services;
using SincoMaquinaria.Infrastructure;

namespace SincoMaquinaria.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var allowedOrigins = configuration.GetSection("Security:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };

        // Asegurar que la base de datos existe
        DatabaseSetup.EnsureDatabaseExists(connectionString);

        // Marten (Event Sourcing)
        services.AddMarten(opts =>
        {
            opts.Connection(connectionString);
            opts.Projections.Snapshot<OrdenDeTrabajo>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<ConfiguracionGlobal>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<Equipo>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<RutinaMantenimiento>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<Empleado>(SnapshotLifecycle.Inline);
        });

        // Excel Services
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        services.AddScoped<ExcelImportService>();
        services.AddScoped<ExcelEquipoImportService>();
        services.AddScoped<ExcelEmpleadoImportService>();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowedOrigins", corsBuilder =>
            {
                corsBuilder.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
