using Serilog;
using SincoMaquinaria.Extensions;
using SincoMaquinaria.Endpoints;

// --- SERILOG CONFIGURATION ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/sinco-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Iniciando SincoMaquinaria API...");
    
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // --- CONFIGURACIÓN DE SERVICIOS ---
    builder.Services.AddApplicationServices(builder.Configuration);

    var app = builder.Build();

    // --- PIPELINE HTTP ---
    app.ConfigureMiddleware();
    app.UseSerilogRequestLogging();

    // --- HEALTH CHECK ---
    app.MapHealthChecks("/health");

    // --- CONFIGURACIÓN DE LÍMITES ---
    var maxFileUploadSizeMB = builder.Configuration.GetValue<int>("Security:MaxFileUploadSizeMB", 10);

    // --- ENDPOINTS (API) ---
    // Auth endpoints (sin protección - público)
    app.MapAuthEndpoints();
    
    // Endpoints protegidos (la autorización se configura en cada grupo)
    app.MapOrdenesEndpoints();
    app.MapEquiposEndpoints(maxFileUploadSizeMB);
    app.MapEmpleadosEndpoints(maxFileUploadSizeMB);
    app.MapRutinasEndpoints(maxFileUploadSizeMB);
    app.MapConfiguracionEndpoints();
    app.MapAdminEndpoints(builder.Configuration);

    // --- SIGNALR HUBS ---
    app.MapHub<SincoMaquinaria.Infrastructure.Hubs.DashboardHub>("/hubs/dashboard");

    // Configuración de URLs y puerto (configurable para Docker y producción)
    var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

    if (string.IsNullOrEmpty(urls))
    {
        // Desarrollo: HTTP en 5000 y HTTPS en 5001
        // Producción: usar variables de entorno
        urls = app.Environment.IsDevelopment()
            ? "http://localhost:5000;https://localhost:5001"
            : "http://0.0.0.0:5000";
    }

    Log.Information("API iniciada en {Urls}", urls);
    app.Run(urls);
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
