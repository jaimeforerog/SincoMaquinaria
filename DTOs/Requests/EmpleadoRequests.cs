namespace SincoMaquinaria.DTOs.Requests;

public record CrearEmpleadoRequest(
    string Nombre, 
    string Identificacion, 
    string Cargo, 
    string? Especialidad, 
    decimal ValorHora, 
    string Estado
);

public record ActualizarEmpleadoRequest(
    string Nombre, 
    string Identificacion, 
    string Cargo, 
    string? Especialidad, 
    decimal ValorHora, 
    string Estado
);
