namespace SincoMaquinaria.DTOs.Requests;

// Tipos de Medidor
public record CrearTipoMedidorRequest(string Nombre, string Unidad);
public record ActualizarTipoMedidorRequest(string Nombre, string Unidad);

// Grupos de Mantenimiento
public record CrearGrupoRequest(string Nombre, string Descripcion);
public record ActualizarGrupoRequest(string Nombre, string Descripcion);

// Tipos de Falla
public record CrearTipoFallaRequest(string Descripcion, string Prioridad);

// Causas de Falla
public record CrearCausaFallaRequest(string Descripcion);
public record ActualizarCausaFallaRequest(string Descripcion);

// Estado
public record CambiarEstadoRequest(bool Activo);
