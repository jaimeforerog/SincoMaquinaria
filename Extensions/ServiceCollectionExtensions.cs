using System.Text;
using FluentValidation;
using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Services;
using SincoMaquinaria.Infrastructure;
using SincoMaquinaria.Validators;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;

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
        // Configure JSON options for enum serialization
        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        services.AddMarten(opts =>
        {
            opts.Connection(connectionString);

            // Use custom JSON serializer that handles enums as strings
            var serializer = new Marten.Services.SystemTextJsonSerializer(jsonOptions);
            opts.Serializer(serializer);

            opts.Projections.Snapshot<OrdenDeTrabajo>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<ConfiguracionGlobal>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<Equipo>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<RutinaMantenimiento>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<Empleado>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<Usuario>(SnapshotLifecycle.Inline);

            // Permitir que Marten cree las tablas automáticamente si no existen
            opts.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;

            // Proyecciones
            // opts.Projections.Add<SincoMaquinaria.Domain.Projections.AuditoriaProjection>(Marten.Events.Projections.ProjectionLifecycle.Inline);
        });

        // Excel Services
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        services.AddScoped<ExcelImportService>();
        services.AddScoped<ExcelEquipoImportService>();
        services.AddScoped<ExcelEmpleadoImportService>();

        // JWT Service
        services.AddScoped<JwtService>();

        // SignalR & Notifications
        services.AddSignalR();
        services.AddScoped<DashboardNotifier>();

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CrearOrdenRequestValidator>();

        // JWT Authentication
        var jwtKey = configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "SincoMaquinaria";
        var jwtAudience = configuration["Jwt:Audience"] ?? "SincoMaquinariaApp";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowedOrigins", corsBuilder =>
            {
                corsBuilder.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            });
        });

        // HSTS Configuration
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql");

        // Swagger (sin configuración avanzada por ahora)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Rate Limiting
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        // Configure Global JSON Options for API Responses
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        return services;
    }
}
