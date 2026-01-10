using SincoMaquinaria.Extensions;
using SincoMaquinaria.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN DE SERVICIOS ---
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// --- PIPELINE HTTP ---
app.ConfigureMiddleware();

// --- CONFIGURACIÓN DE LÍMITES ---
var maxFileUploadSizeMB = builder.Configuration.GetValue<int>("Security:MaxFileUploadSizeMB", 10);

// --- ENDPOINTS (API) ---
app.MapOrdenesEndpoints();
app.MapEquiposEndpoints(maxFileUploadSizeMB);
app.MapEmpleadosEndpoints(maxFileUploadSizeMB);
app.MapRutinasEndpoints(maxFileUploadSizeMB);
app.MapConfiguracionEndpoints();
app.MapAdminEndpoints(builder.Configuration);

app.Run("http://localhost:5000");

public partial class Program { }
