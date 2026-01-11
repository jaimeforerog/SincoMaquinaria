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
        if (!string.IsNullOrEmpty(@event.PasswordHash))
            PasswordHash = @event.PasswordHash;
    }

    public void Apply(UsuarioDesactivado @event)
    {
        Activo = false;
    }
}

public enum RolUsuario
{
    Admin,
    User
}
