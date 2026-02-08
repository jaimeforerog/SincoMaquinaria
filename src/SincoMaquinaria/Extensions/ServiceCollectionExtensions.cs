using System.Text;
using FluentValidation;
using Marten;
using Marten.Events.Projections;
using Weasel.Core;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Services;
using SincoMaquinaria.Infrastructure;
using SincoMaquinaria.Validators;
using System.Text.Json;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using Npgsql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Memory;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

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
            serializer.EnumStorage = EnumStorage.AsString;
            opts.Serializer(serializer);

            opts.Projections.Snapshot<OrdenDeTrabajo>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<ConfiguracionGlobal>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<Equipo>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<RutinaMantenimiento>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<Empleado>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<Usuario>(SnapshotLifecycle.Inline);
            opts.Projections.Snapshot<ErrorLog>(SnapshotLifecycle.Inline);

            // Índices únicos para prevenir duplicados
            opts.Schema.For<Equipo>().Index(x => x.Placa, x =>
            {
                x.IsUnique = true;
                x.Name = "idx_equipo_placa_unique";
            });

            // Proyecciones
            opts.Projections.Add(new SincoMaquinaria.Domain.Projections.AuditoriaProjection(), ProjectionLifecycle.Inline);
        });

        // Hangfire
        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                });
        });

        services.AddHangfireServer(options =>
        {
            options.ServerName = configuration["Hangfire:ServerName"] ?? "SincoMaquinaria";
            options.WorkerCount = configuration.GetValue<int>("Hangfire:WorkerCount", 5);
        });

        // Excel Services
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        services.AddScoped<ExcelImportService>();
        services.AddScoped<ExcelEquipoImportService>();
        services.AddScoped<ExcelEmpleadoImportService>();
        services.AddScoped<RutinasService>();
        services.AddScoped<EmpleadosService>();
        services.AddScoped<AuthService>();

        // Background Jobs
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<SincoMaquinaria.Services.Jobs.ImportacionJobHandler>();
        services.AddScoped<SincoMaquinaria.Services.Jobs.MantenimientoJobHandler>();

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

        // Response Compression (Brotli + Gzip)
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();

            // Comprimir todos los tipos MIME comunes
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/javascript",
                "application/json",
                "application/xml",
                "text/css",
                "text/html",
                "text/plain",
                "text/xml",
                "image/svg+xml"
            });
        });

        // Configurar niveles de compresión
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest; // Fastest para mejor performance CPU
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal; // Optimal para buen balance
        });

        // Health Checks
        services.AddHealthChecks()
            .AddCheck("PostgreSQL", () =>
            {
                try
                {
                    using var conn = new NpgsqlConnection(connectionString);
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT 1";
                    cmd.ExecuteScalar();
                    return HealthCheckResult.Healthy();
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("PostgreSQL no disponible.", ex);
                }
            });

        // Application Insights (Azure Monitoring)
        // Connection string configured via APPLICATIONINSIGHTS_CONNECTION_STRING environment variable
        services.AddApplicationInsightsTelemetry();

        // Swagger (configuración básica - compatible con todas las versiones)
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Caching - Redis si está habilitado, fallback a MemoryCache
        var cachingEnabled = configuration.GetValue<bool>("Caching:Enabled", false);
        if (cachingEnabled)
        {
            var redisConnection = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "SincoMaquinaria_";
                });
            }
            else
            {
                // Fallback a memoria si no hay Redis configurado
                services.AddMemoryCache();
            }
        }
        else
        {
            // Fallback a memoria para desarrollo
            services.AddMemoryCache();
        }

        // Registrar servicio de cache
        services.AddSingleton<ICacheService, CacheService>();

        // Rate Limiting
        if (!services.Any(x => x.ServiceType == typeof(IMemoryCache)))
        {
            services.AddMemoryCache();
        }
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
