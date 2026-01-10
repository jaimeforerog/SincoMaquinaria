using System;

namespace SincoMaquinaria.Domain;

public class ErrorLog
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty; // Endpoint donde ocurrió
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.Now;
    
    // Marten constructor
    public ErrorLog() { }

    public ErrorLog(string message, string stackTrace, string path)
    {
        Id = Guid.NewGuid();
        Message = message;
        StackTrace = stackTrace;
        Path = path;
        Fecha = DateTimeOffset.Now;
    }
}
