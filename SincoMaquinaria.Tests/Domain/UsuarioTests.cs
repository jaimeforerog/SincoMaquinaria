using System;
using FluentAssertions;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events.Usuario;
using Xunit;

namespace SincoMaquinaria.Tests.Domain;

public class UsuarioTests
{
    [Fact]
    public void Usuario_ConstructorVacio_DebeCrearInstanciaConValoresPorDefecto()
    {
        // Act
        var usuario = new Usuario();

        // Assert
        usuario.Should().NotBeNull();
        usuario.Id.Should().Be(Guid.Empty);
        usuario.Email.Should().BeEmpty();
        usuario.PasswordHash.Should().BeEmpty();
        usuario.Nombre.Should().BeEmpty();
        usuario.Rol.Should().Be(RolUsuario.User);
        usuario.Activo.Should().BeTrue();
        usuario.RefreshToken.Should().BeNull();
        usuario.RefreshTokenExpiry.Should().BeNull();
    }

    [Fact]
    public void Usuario_Apply_UsuarioCreado_DebeEstablecerPropiedades()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        var evento = new UsuarioCreado(
            usuarioId,
            "test@example.com",
            "hashed-password",
            "Test User",
            RolUsuario.Admin,
            DateTime.UtcNow
        );

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.Id.Should().Be(usuarioId);
        usuario.Email.Should().Be("test@example.com");
        usuario.PasswordHash.Should().Be("hashed-password");
        usuario.Nombre.Should().Be("Test User");
        usuario.Rol.Should().Be(RolUsuario.Admin);
        usuario.Activo.Should().BeTrue();
        usuario.FechaCreacion.Should().Be(evento.FechaCreacion);
    }

    [Fact]
    public void Usuario_Apply_UsuarioActualizado_ConTodosLosCampos_DebeActualizar()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        usuario.Apply(new UsuarioCreado(usuarioId, "old@test.com", "old-hash", "Old Name", RolUsuario.User, DateTime.UtcNow));

        var currentUserId = Guid.NewGuid();
        var evento = new UsuarioActualizado(
            usuarioId,
            "New Name",
            RolUsuario.Admin,
            false,
            "new-password-hash",
            currentUserId,
            "Admin User",
            DateTimeOffset.Now
        );

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.Nombre.Should().Be("New Name");
        usuario.Rol.Should().Be(RolUsuario.Admin);
        usuario.Activo.Should().BeFalse();
        usuario.PasswordHash.Should().Be("new-password-hash");
    }

    [Fact]
    public void Usuario_Apply_UsuarioActualizado_SinPasswordHash_NoDebeActualizarPassword()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        usuario.Apply(new UsuarioCreado(usuarioId, "test@test.com", "original-hash", "User", RolUsuario.User, DateTime.UtcNow));

        var evento = new UsuarioActualizado(
            usuarioId,
            "Updated Name",
            RolUsuario.User,
            true,
            null, // No password update
            Guid.NewGuid(),
            "Admin",
            DateTimeOffset.Now
        );

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.PasswordHash.Should().Be("original-hash");
        usuario.Nombre.Should().Be("Updated Name");
    }

    [Fact]
    public void Usuario_Apply_UsuarioActualizado_ConPasswordHashVacio_NoDebeActualizarPassword()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        usuario.Apply(new UsuarioCreado(usuarioId, "test@test.com", "original-hash", "User", RolUsuario.User, DateTime.UtcNow));

        var evento = new UsuarioActualizado(
            usuarioId,
            "Updated Name",
            RolUsuario.User,
            true,
            "", // Empty password
            Guid.NewGuid(),
            "Admin",
            DateTimeOffset.Now
        );

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.PasswordHash.Should().Be("original-hash");
    }

    [Fact]
    public void Usuario_Apply_UsuarioActualizado_SinRol_NoDebeActualizarRol()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        usuario.Apply(new UsuarioCreado(usuarioId, "test@test.com", "hash", "User", RolUsuario.Admin, DateTime.UtcNow));

        var evento = new UsuarioActualizado(
            usuarioId,
            "Updated Name",
            null, // No rol update
            true,
            null,
            Guid.NewGuid(),
            "Admin",
            DateTimeOffset.Now
        );

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.Rol.Should().Be(RolUsuario.Admin);
    }

    [Fact]
    public void Usuario_Apply_UsuarioActualizado_SinActivo_NoDebeActualizarActivo()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        usuario.Apply(new UsuarioCreado(usuarioId, "test@test.com", "hash", "User", RolUsuario.User, DateTime.UtcNow));
        usuario.Activo = false; // Set to false manually

        var evento = new UsuarioActualizado(
            usuarioId,
            "Updated Name",
            RolUsuario.User,
            null, // No activo update
            null,
            Guid.NewGuid(),
            "Admin",
            DateTimeOffset.Now
        );

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.Activo.Should().BeFalse();
    }

    [Fact]
    public void Usuario_Apply_UsuarioDesactivado_DebeDesactivarUsuario()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        usuario.Apply(new UsuarioCreado(usuarioId, "test@test.com", "hash", "User", RolUsuario.User, DateTime.UtcNow));

        var evento = new UsuarioDesactivado(usuarioId);

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.Activo.Should().BeFalse();
    }

    [Fact]
    public void Usuario_Apply_RefreshTokenGenerado_DebeEstablecerToken()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        usuario.Apply(new UsuarioCreado(usuarioId, "test@test.com", "hash", "User", RolUsuario.User, DateTime.UtcNow));

        var refreshToken = "refresh-token-12345";
        var expiry = DateTime.UtcNow.AddDays(7);
        var evento = new RefreshTokenGenerado(usuarioId, refreshToken, expiry, DateTimeOffset.Now);

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.RefreshToken.Should().Be(refreshToken);
        usuario.RefreshTokenExpiry.Should().Be(expiry);
    }

    [Fact]
    public void Usuario_Apply_RefreshTokenRevocado_DebeLimpiarToken()
    {
        // Arrange
        var usuario = new Usuario();
        var usuarioId = Guid.NewGuid();
        usuario.Apply(new UsuarioCreado(usuarioId, "test@test.com", "hash", "User", RolUsuario.User, DateTime.UtcNow));
        usuario.Apply(new RefreshTokenGenerado(usuarioId, "token", DateTime.UtcNow.AddDays(7), DateTimeOffset.Now));

        var evento = new RefreshTokenRevocado(usuarioId, DateTimeOffset.Now);

        // Act
        usuario.Apply(evento);

        // Assert
        usuario.RefreshToken.Should().BeNull();
        usuario.RefreshTokenExpiry.Should().BeNull();
    }

    [Fact]
    public void RolUsuario_Enum_DebeContenerAdminYUser()
    {
        // Assert
        var adminValue = RolUsuario.Admin;
        var userValue = RolUsuario.User;

        adminValue.Should().NotBe(userValue);
        Enum.IsDefined(typeof(RolUsuario), adminValue).Should().BeTrue();
        Enum.IsDefined(typeof(RolUsuario), userValue).Should().BeTrue();
    }
}
