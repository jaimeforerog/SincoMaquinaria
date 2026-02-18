using System;
using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using Xunit;

namespace SincoMaquinaria.Tests.DTOs;

public class AuthResponseTests
{
    [Fact]
    public void AuthResponse_Constructor_DebeEstablecerTodasLasPropiedades()
    {
        // Arrange
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var expiration = DateTime.UtcNow.AddHours(1);
        var refreshToken = "refresh-token-12345";
        var refreshExpiration = DateTime.UtcNow.AddDays(7);
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var nombre = "Test User";
        var rol = "Admin";

        // Act
        var response = new AuthResponse(
            token,
            expiration,
            refreshToken,
            refreshExpiration,
            id,
            email,
            nombre,
            rol
        );

        // Assert
        response.Token.Should().Be(token);
        response.Expiration.Should().Be(expiration);
        response.RefreshToken.Should().Be(refreshToken);
        response.RefreshExpiration.Should().Be(refreshExpiration);
        response.Id.Should().Be(id);
        response.Email.Should().Be(email);
        response.Nombre.Should().Be(nombre);
        response.Rol.Should().Be(rol);
    }

    [Fact]
    public void AuthResponse_Deconstruct_DebeExtraerTodasLasPropiedades()
    {
        // Arrange
        var response = new AuthResponse(
            "token-123",
            DateTime.UtcNow.AddHours(1),
            "refresh-123",
            DateTime.UtcNow.AddDays(7),
            Guid.NewGuid(),
            "user@test.com",
            "John Doe",
            "User"
        );

        // Act
        var (token, expiration, refreshToken, refreshExpiration, id, email, nombre, rol) = response;

        // Assert
        token.Should().Be("token-123");
        refreshToken.Should().Be("refresh-123");
        email.Should().Be("user@test.com");
        nombre.Should().Be("John Doe");
        rol.Should().Be("User");
        id.Should().NotBe(Guid.Empty);
        expiration.Should().BeAfter(DateTime.UtcNow);
        refreshExpiration.Should().BeAfter(expiration);
    }

    [Fact]
    public void AuthResponse_ConRolUser_DebeCrearseCorrectamente()
    {
        // Act
        var response = new AuthResponse(
            "token",
            DateTime.UtcNow.AddHours(1),
            "refresh",
            DateTime.UtcNow.AddDays(7),
            Guid.NewGuid(),
            "user@test.com",
            "Regular User",
            "User"
        );

        // Assert
        response.Rol.Should().Be("User");
    }

    [Fact]
    public void AuthResponse_ConRolAdmin_DebeCrearseCorrectamente()
    {
        // Act
        var response = new AuthResponse(
            "token",
            DateTime.UtcNow.AddHours(1),
            "refresh",
            DateTime.UtcNow.AddDays(7),
            Guid.NewGuid(),
            "admin@test.com",
            "Admin User",
            "Admin"
        );

        // Assert
        response.Rol.Should().Be("Admin");
    }

    [Fact]
    public void AuthResponse_TokenExpiration_DebeSerDespuesDeAhora()
    {
        // Arrange
        var futureExpiration = DateTime.UtcNow.AddHours(2);

        // Act
        var response = new AuthResponse(
            "token",
            futureExpiration,
            "refresh",
            DateTime.UtcNow.AddDays(7),
            Guid.NewGuid(),
            "test@test.com",
            "Test",
            "User"
        );

        // Assert
        response.Expiration.Should().BeAfter(DateTime.UtcNow);
        response.Expiration.Should().BeCloseTo(futureExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AuthResponse_RefreshTokenExpiration_DebeSerDespuesDeTokenExpiration()
    {
        // Arrange
        var tokenExpiration = DateTime.UtcNow.AddHours(1);
        var refreshExpiration = DateTime.UtcNow.AddDays(7);

        // Act
        var response = new AuthResponse(
            "token",
            tokenExpiration,
            "refresh",
            refreshExpiration,
            Guid.NewGuid(),
            "test@test.com",
            "Test",
            "User"
        );

        // Assert
        response.RefreshExpiration.Should().BeAfter(response.Expiration);
    }

    [Fact]
    public void AuthResponse_ConIdValido_DebeNoSerEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = new AuthResponse(
            "token",
            DateTime.UtcNow.AddHours(1),
            "refresh",
            DateTime.UtcNow.AddDays(7),
            userId,
            "test@test.com",
            "Test User",
            "User"
        );

        // Assert
        response.Id.Should().NotBe(Guid.Empty);
        response.Id.Should().Be(userId);
    }

    [Fact]
    public void AuthResponse_IgualdadPorValor_DebeCompararCorrectamente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expiration = DateTime.UtcNow.AddHours(1);
        var refreshExpiration = DateTime.UtcNow.AddDays(7);

        var response1 = new AuthResponse(
            "token",
            expiration,
            "refresh",
            refreshExpiration,
            id,
            "test@test.com",
            "Test User",
            "User"
        );

        var response2 = new AuthResponse(
            "token",
            expiration,
            "refresh",
            refreshExpiration,
            id,
            "test@test.com",
            "Test User",
            "User"
        );

        // Assert
        response1.Should().Be(response2);
    }

    [Fact]
    public void AuthResponse_ConDiferentesTokens_DebeSerDiferente()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expiration = DateTime.UtcNow.AddHours(1);
        var refreshExpiration = DateTime.UtcNow.AddDays(7);

        var response1 = new AuthResponse(
            "token-1",
            expiration,
            "refresh",
            refreshExpiration,
            id,
            "test@test.com",
            "Test User",
            "User"
        );

        var response2 = new AuthResponse(
            "token-2",
            expiration,
            "refresh",
            refreshExpiration,
            id,
            "test@test.com",
            "Test User",
            "User"
        );

        // Assert
        response1.Should().NotBe(response2);
    }
}
