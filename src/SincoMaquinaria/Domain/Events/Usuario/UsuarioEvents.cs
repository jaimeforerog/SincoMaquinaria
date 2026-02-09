using SincoMaquinaria.Domain;

namespace SincoMaquinaria.Domain.Events.Usuario;

/// <summary>
/// Evento emitido cuando se crea un nuevo usuario en el sistema.
/// </summary>
public record UsuarioCreado(
    Guid Id,
    string Email,
    string PasswordHash,
    string Nombre,
    RolUsuario Rol,
    DateTime FechaCreacion
);

/// <summary>
/// Evento emitido cuando se actualizan los datos de un usuario.
/// </summary>
public record UsuarioActualizado(
    Guid Id,
    string Nombre,
    RolUsuario? Rol = null,
    bool? Activo = null,
    string? PasswordHash = null,
    Guid? ModificadoPor = null,
    string? ModificadoPorNombre = null,
    DateTimeOffset? FechaModificacion = null
);

/// <summary>
/// Evento emitido cuando se desactiva un usuario.
/// </summary>
public record UsuarioDesactivado(Guid Id);

// --- Refresh Tokens ---

/// <summary>
/// Evento emitido cuando se genera un refresh token para un usuario.
/// </summary>
public record RefreshTokenGenerado(
    Guid UsuarioId,
    string RefreshToken,
    DateTime Expiry,
    DateTimeOffset FechaCreacion
);

/// <summary>
/// Evento emitido cuando se revoca un refresh token.
/// </summary>
public record RefreshTokenRevocado(
    Guid UsuarioId,
    DateTimeOffset FechaRevocacion
);
