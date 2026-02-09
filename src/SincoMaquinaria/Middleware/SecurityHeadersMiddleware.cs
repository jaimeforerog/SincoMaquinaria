namespace SincoMaquinaria.Middleware;

/// <summary>
/// Middleware que agrega headers de seguridad a todas las respuestas HTTP.
/// Implementa las mejores prácticas de OWASP para headers de seguridad.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Content-Type-Options: Previene MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Protección contra clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-XSS-Protection: Protección contra XSS (legacy browsers)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy: Control de información de referencia
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy: Restricción de APIs del navegador
        context.Response.Headers.Append("Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

        // Content-Security-Policy: Protección robusta contra XSS
        // Más permisivo en desarrollo, estricto en producción
        var csp = _environment.IsDevelopment()
            ? "default-src 'self' 'unsafe-inline' 'unsafe-eval'; img-src 'self' data: https:; font-src 'self' data:;"
            : "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';";

        context.Response.Headers.Append("Content-Security-Policy", csp);

        // HSTS se maneja via app.UseHsts() en WebApplicationExtensions.cs
        // (Configurado en ServiceCollectionExtensions: Preload, IncludeSubDomains, MaxAge=365 days)

        // Remove server header para ocultar información del servidor
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        await _next(context);
    }
}
