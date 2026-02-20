using System;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SincoMaquinaria.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using SincoMaquinaria.Services;
using Xunit;
using System.IdentityModel.Tokens.Jwt;

namespace SincoMaquinaria.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public JwtServiceTests()
    {
        // Create test configuration for JwtService
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "SUPER-SECRET-TEST-KEY-FOR-INTEGRATION-TESTS-1234567890",
            ["Jwt:Issuer"] = "SincoMaquinariaTest",
            ["Jwt:Audience"] = "SincoMaquinariaTestApp",
            ["Jwt:ExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _jwtService = new JwtService(_configuration, NullLogger<JwtService>.Instance);
    }

    #region GenerateToken Tests

    [Fact]
    public void GenerateToken_WithValidUser_ShouldReturnToken()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = RolUsuario.User,
            Activo = true
        };

        // Act
        var (token, expiration) = _jwtService.GenerateToken(usuario);

        // Assert
        token.Should().NotBeNullOrEmpty();
        expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeCorrectClaims()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Nombre = "Admin User",
            Rol = RolUsuario.Admin,
            Activo = true
        };

        // Act
        var (token, _) = _jwtService.GenerateToken(usuario);

        // Assert - Decode token manually to verify claims
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == usuario.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == usuario.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == usuario.Nombre);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_ForAdminUser_ShouldIncludeAdminRole()
    {
        // Arrange
        var admin = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            Nombre = "Admin",
            Rol = RolUsuario.Admin,
            Activo = true
        };

        // Act
        var (token, _) = _jwtService.GenerateToken(admin);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_ForRegularUser_ShouldIncludeUserRole()
    {
        // Arrange
        var user = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            Nombre = "Regular User",
            Rol = RolUsuario.User,
            Activo = true
        };

        // Act
        var (token, _) = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public void GenerateToken_ShouldSetExpirationBasedOnConfiguration()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = RolUsuario.User,
            Activo = true
        };

        // Act
        var (_, expiration) = _jwtService.GenerateToken(usuario);

        // Assert - Default is 15 minutes in configuration
        var expectedExpiry = DateTime.UtcNow.AddMinutes(15);
        expiration.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region GenerateTokens Tests

    [Fact]
    public void GenerateTokens_WithValidUser_ShouldReturnAllTokens()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = RolUsuario.User,
            Activo = true
        };

        // Act
        var (token, tokenExpiry, refreshToken, refreshExpiry) = _jwtService.GenerateTokens(usuario);

        // Assert
        token.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
        tokenExpiry.Should().BeAfter(DateTime.UtcNow);
        refreshExpiry.Should().BeAfter(DateTime.UtcNow);
        refreshExpiry.Should().BeAfter(tokenExpiry); // Refresh token should expire after access token
    }

    [Fact]
    public void GenerateTokens_MultipleCalls_ShouldGenerateDifferentRefreshTokens()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = RolUsuario.User,
            Activo = true
        };

        // Act
        var (_, _, refreshToken1, _) = _jwtService.GenerateTokens(usuario);
        var (_, _, refreshToken2, _) = _jwtService.GenerateTokens(usuario);

        // Assert
        refreshToken1.Should().NotBe(refreshToken2);
    }

    [Fact]
    public void GenerateTokens_ShouldSetRefreshTokenExpiryTo7Days()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = RolUsuario.User,
            Activo = true
        };

        // Act
        var (_, _, _, refreshExpiry) = _jwtService.GenerateTokens(usuario);

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddDays(7);
        refreshExpiry.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateTokens_TokenShouldBeValidJwt()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Nombre = "Test User",
            Rol = RolUsuario.User,
            Activo = true
        };

        // Act
        var (token, _, _, _) = _jwtService.GenerateTokens(usuario);

        // Assert - Should be able to decode the JWT
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Should().NotBeNull();
        jwtToken.Claims.Should().NotBeEmpty();
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_MultipleCalls_ShouldReturnDifferentTokens()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert - Base64 strings should be decodable
        var act = () => Convert.FromBase64String(refreshToken);
        act.Should().NotThrow();
    }

    #endregion

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyHash()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hash = JwtService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2a$"); // BCrypt hash prefix
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "SamePassword123!";

        // Act
        var hash1 = JwtService.HashPassword(password);
        var hash2 = JwtService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // BCrypt uses random salt
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "";

        // Act
        var hash = JwtService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_WithLongPassword_ShouldReturnHash()
    {
        // Arrange
        var password = new string('a', 100);

        // Act
        var hash = JwtService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2a$");
    }

    [Fact]
    public void HashPassword_WithSpecialCharacters_ShouldReturnHash()
    {
        // Arrange
        var password = "P@ssw0rd!#$%^&*()";

        // Act
        var hash = JwtService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2a$");
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var hash = JwtService.HashPassword(password);

        // Act
        var result = JwtService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = JwtService.HashPassword(correctPassword);

        // Act
        var result = JwtService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SomePassword123!";
        var hash = JwtService.HashPassword(password);

        // Act
        var result = JwtService.VerifyPassword("", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldThrowException()
    {
        // Arrange
        var password = "Password123!";
        var invalidHash = "not-a-valid-bcrypt-hash";

        // Act & Assert - BCrypt throws SaltParseException for invalid hash format
        var act = () => JwtService.VerifyPassword(password, invalidHash);
        act.Should().Throw<BCrypt.Net.SaltParseException>();
    }

    [Fact]
    public void VerifyPassword_CaseSensitive_ShouldReturnFalse()
    {
        // Arrange
        var password = "Password123!";
        var hash = JwtService.HashPassword(password);

        // Act
        var result = JwtService.VerifyPassword("PASSWORD123!", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithWhitespace_ShouldBeTreatedAsPartOfPassword()
    {
        // Arrange
        var passwordWithSpace = "Pass word123!";
        var hash = JwtService.HashPassword(passwordWithSpace);

        // Act
        var resultWithSpace = JwtService.VerifyPassword(passwordWithSpace, hash);
        var resultWithoutSpace = JwtService.VerifyPassword("Password123!", hash);

        // Assert
        resultWithSpace.Should().BeTrue();
        resultWithoutSpace.Should().BeFalse();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void HashAndVerify_Integration_ShouldWorkCorrectly()
    {
        // Arrange
        var password = "IntegrationTest123!";

        // Act
        var hash = JwtService.HashPassword(password);
        var verifyCorrect = JwtService.VerifyPassword(password, hash);
        var verifyWrong = JwtService.VerifyPassword("WrongPassword", hash);

        // Assert
        verifyCorrect.Should().BeTrue();
        verifyWrong.Should().BeFalse();
    }

    [Fact]
    public void GenerateAndDecodeToken_Integration_ShouldWorkCorrectly()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = "integration@test.com",
            Nombre = "Integration Test",
            Rol = RolUsuario.Admin,
            Activo = true
        };

        // Act
        var (token, _) = _jwtService.GenerateToken(usuario);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == usuario.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == usuario.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    #endregion
}
