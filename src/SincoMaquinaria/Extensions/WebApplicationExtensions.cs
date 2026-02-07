using AspNetCoreRateLimit;
using Hangfire;

namespace SincoMaquinaria.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // Response Compression (debe estar primero para comprimir todo)
        app.UseResponseCompression();

        // HTTPS Redirection (forzar HTTPS en producción)
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();

            // HSTS (HTTP Strict Transport Security)
            app.UseHsts();
        }

        // Security Headers
        app.UseMiddleware<SincoMaquinaria.Middleware.SecurityHeadersMiddleware>();

        // Rate Limiting (debe estar antes de CORS y Authentication)
        app.UseIpRateLimiting();

        // CORS
        app.UseCors("AllowedOrigins");

        // Static files para SPA (siempre en Docker, check si wwwroot existe)
        var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        if (Directory.Exists(wwwrootPath))
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        // Swagger (solo en desarrollo)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SincoMaquinaria API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "SincoMaquinaria API Documentation";
                options.DisplayRequestDuration();
            });
        }

        // Hangfire Dashboard
        var dashboardEnabled = app.Configuration.GetValue<bool>("Hangfire:DashboardEnabled", true);
        if (dashboardEnabled)
        {
            var dashboardPath = app.Configuration["Hangfire:DashboardPath"] ?? "/hangfire";
            app.UseHangfireDashboard(dashboardPath, new DashboardOptions
            {
                Authorization = new[]
                {
                    new SincoMaquinaria.Infrastructure.HangfireAuthorizationFilter()
                },
                DashboardTitle = "SincoMaquinaria - Background Jobs"
            });
        }

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Global Exception Handler
        app.UseMiddleware<SincoMaquinaria.Middleware.ExceptionHandlingMiddleware>();

        // Fallback para SPA routing (si wwwroot existe)
        if (Directory.Exists(wwwrootPath))
        {
            app.MapFallbackToFile("index.html");
        }

        return app;
    }
}
