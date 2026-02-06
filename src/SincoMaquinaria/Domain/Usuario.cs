using SincoMaquinaria.Domain.Events;

namespace SincoMaquinaria.Domain;

public class Usuario
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; } = RolUsuario.User;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    // Refresh Token properties
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public Usuario() { }

    public void Apply(UsuarioCreado @event)
    {
        Id = @event.Id;
        Email = @event.Email;
        PasswordHash = @event.PasswordHash;
        Nombre = @event.Nombre;
        Rol = @event.Rol;
        Activo = true;
        FechaCreacion = @event.FechaCreacion;
    }

    public void Apply(UsuarioActualizado @event)
    {
        Nombre = @event.Nombre;
        if (@event.Rol.HasValue) Rol = @event.Rol.Value;
        if (@event.Activo.HasValue) Activo = @event.Activo.Value;
        if (!string.IsNullOrEmpty(@event.PasswordHash))
            PasswordHash = @event.PasswordHash;
    }

    public void Apply(UsuarioDesactivado @event)
    {
        Activo = false;
    }

    public void Apply(RefreshTokenGenerado @event)
    {
        RefreshToken = @event.RefreshToken;
        RefreshTokenExpiry = @event.Expiry;
    }

    public void Apply(RefreshTokenRevocado @event)
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
    }
}

public enum RolUsuario
{
    Admin,
    User
}
