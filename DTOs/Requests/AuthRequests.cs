namespace SincoMaquinaria.DTOs.Requests;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string Email, string Password, string Nombre, string? Rol = "User");

public record AuthResponse(
    string Token, 
    DateTime Expiration, 
    string Email, 
    string Nombre, 
    string Rol
);
