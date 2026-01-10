namespace SincoMaquinaria.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        // CORS
        app.UseCors("AllowedOrigins");

        // Swagger (solo en desarrollo)
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Global Exception Handler
        app.UseMiddleware<SincoMaquinaria.Middleware.ExceptionHandlingMiddleware>();

        return app;
    }
}
