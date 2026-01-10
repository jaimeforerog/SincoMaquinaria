namespace SincoMaquinaria.DTOs.Requests;

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
