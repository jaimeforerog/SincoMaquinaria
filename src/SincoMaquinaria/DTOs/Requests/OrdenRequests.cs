namespace SincoMaquinaria.DTOs.Requests;

public record CrearOrdenRequest(
    string Numero, 
    string EquipoId, 
    string Origen, 
    string Tipo, 
    DateTime? FechaOrden = null, 
    Guid? RutinaId = null, 
    string? ActividadInicial = null,
    int? FrecuenciaPreventiva = null
);

public record AgregarActividadRequest(
    string Descripcion, 
    DateTime FechaEstimada, 
    string? TipoFallaId = null, 
    string? CausaFallaId = null
);

public record RegistrarAvanceRequest(
    Guid DetalleId, 
    decimal Porcentaje, 
    string Observacion, 
    string NuevoEstado
);
