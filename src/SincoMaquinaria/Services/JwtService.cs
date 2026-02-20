using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SincoMaquinaria.Domain;

namespace SincoMaquinaria.Services;

public class JwtService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly int _refreshTokenExpirationDays;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _logger = logger;
        var jwtSection = configuration.GetSection("Jwt");
        _key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        _issuer = jwtSection["Issuer"] ?? "SincoMaquinaria";
        _audience = jwtSection["Audience"] ?? "SincoMaquinariaApp";
        _expirationMinutes = jwtSection.GetValue<int>("ExpirationMinutes", 15);
        _refreshTokenExpirationDays = jwtSection.GetValue<int>("RefreshTokenExpirationDays", 7);
    }

    public (string Token, DateTime Expiration) GenerateToken(Usuario usuario)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiration = DateTime.UtcNow.AddMinutes(_expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public (string Token, DateTime Expiration, string RefreshToken, DateTime RefreshExpiry) GenerateTokens(Usuario usuario)
    {
        var (accessToken, accessExpiry) = GenerateToken(usuario);
        var refreshToken = GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

        return (accessToken, accessExpiry, refreshToken, refreshExpiry);
    }
}
