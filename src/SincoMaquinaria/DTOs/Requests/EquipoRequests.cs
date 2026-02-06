namespace SincoMaquinaria.DTOs.Requests;

public record CrearEquipoRequest(
    string Placa,
    string Descripcion, 
    string Marca, 
    string Modelo, 
    string Serie, 
    string Codigo, 
    string TipoMedidorId, 
    string TipoMedidorId2, 
    string Grupo, 
    string Rutina,
    decimal? LecturaInicial1,
    DateTime? FechaInicial1,
    decimal? LecturaInicial2,
    DateTime? FechaInicial2
);

public record ActualizarEquipoRequest(
    string Descripcion, 
    string Marca, 
    string Modelo, 
    string Serie, 
    string Codigo, 
    string TipoMedidorId, 
    string TipoMedidorId2, 
    string Grupo, 
    string Rutina
);
