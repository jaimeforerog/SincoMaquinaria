namespace SincoMaquinaria.Domain.Projections;

/// <summary>
/// Read model for audit log entries. This is a flat, indexed table
/// optimized for efficient querying of audit history.
/// </summary>
public class RegistroAuditoria
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The stream (aggregate) this event belongs to
    /// </summary>
    public Guid StreamId { get; set; }
    
    /// <summary>
    /// Type of the event (e.g., "EquipoCreado", "OrdenFinalizada")
    /// </summary>
    public string TipoEvento { get; set; } = string.Empty;
    
    /// <summary>
    /// Functional module (e.g., "Equipos", "Órdenes", "Configuración")
    /// </summary>
    public string Modulo { get; set; } = string.Empty;
    
    /// <summary>
    /// User who performed the action
    /// </summary>
    public Guid? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    
    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTimeOffset Fecha { get; set; }
    
    /// <summary>
    /// Event version in the stream
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// JSON string with additional event details (for display purposes)
    /// </summary>
    public string? Detalles { get; set; }
}
