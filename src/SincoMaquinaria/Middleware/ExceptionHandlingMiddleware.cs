using Marten;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SincoMaquinaria.Domain;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SincoMaquinaria.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IDocumentStore store)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Uncaught exception occurred.");
            await HandleExceptionAsync(context, ex, store);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, IDocumentStore store)
    {
        // 1. Guardar en DB
        string path = context.Request.Path;
        var errorLog = new ErrorLog(exception.Message, exception.StackTrace ?? string.Empty, path);

        try 
        {
            // Usamos una sesión ligera para guardar el error
            using var session = store.LightweightSession();
            session.Store(errorLog);
            await session.SaveChangesAsync();
        }
        catch(Exception dbEx)
        {
            // Si falla guardar en DB, lo logueamos en consola como fallback
             _logger.LogError(dbEx, "Failed to log error to database.");
        }

        // 2. Responder al cliente
        context.Response.ContentType = "application/json";

        if (exception is DomainException domainEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var response = new 
            {
                error = "Error de validación de negocio",
                detail = domainEx.Message,
                errors = domainEx.Errors,
                logId = errorLog.Id
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var result = JsonSerializer.Serialize(new
            {
                error = "Ocurrió un error interno.",
                logId = errorLog.Id
            });
            await context.Response.WriteAsync(result);
        }
    }
}
