namespace SincoMaquinaria.DTOs.Requests;

public record CreateRutinaRequest(
    string Descripcion,
    string Grupo
);

public record UpdateRutinaRequest(
    string Descripcion,
    string Grupo
);

public record UpdateParteRequest(
    string Descripcion
);

public record AddParteRequest(
    string Descripcion
);

public record UpdateActividadRequest(
    string Descripcion,
    string Clase,
    int Frecuencia,
    string UnidadMedida,
    string NombreMedidor,
    int AlertaFaltando,
    int Frecuencia2,
    string UnidadMedida2,
    string NombreMedidor2,
    int AlertaFaltando2,
    string? Insumo,
    double Cantidad
);

public record AddActividadRequest(
    string Descripcion,
    string Clase,
    int Frecuencia,
    string UnidadMedida,
    string NombreMedidor,
    int AlertaFaltando,
    int Frecuencia2,
    string UnidadMedida2,
    string NombreMedidor2,
    int AlertaFaltando2,
    string? Insumo,
    double Cantidad
);
