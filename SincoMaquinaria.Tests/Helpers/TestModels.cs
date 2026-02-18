namespace SincoMaquinaria.Tests.Helpers;

public record TestAuthResponse(
    string Token,
    DateTime Expiration,
    string RefreshToken,
    DateTime RefreshExpiration,
    Guid Id,
    string Email,
    string Nombre,
    string Rol
);
