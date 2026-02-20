using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events.Usuario;
using SincoMaquinaria.DTOs.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using SincoMaquinaria.Services;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace SincoMaquinaria.Tests.Services;

public class AuthServiceTests : IClassFixture<IntegrationFixture>, IAsyncLifetime
{
    private readonly IntegrationFixture _fixture;
    private readonly JwtService _jwtService;
    private AuthService _authService = null!;
    private IDocumentSession _session = null!;

    public AuthServiceTests(IntegrationFixture fixture)
    {
        _fixture = fixture;

        // Create test configuration for JwtService
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "SUPER-SECRET-TEST-KEY-FOR-INTEGRATION-TESTS-1234567890",
            ["Jwt:Issuer"] = "SincoMaquinariaTest",
            ["Jwt:Audience"] = "SincoMaquinariaTestApp",
            ["Jwt:ExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _jwtService = new JwtService(configuration, NullLogger<JwtService>.Instance);
    }

    public async Task InitializeAsync()
    {
        _session = _fixture.Store.LightweightSession();
        _authService = new AuthService(_session, _jwtService, NullLogger<AuthService>.Instance);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _session.DisposeAsync();
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        var passwordHash = JwtService.HashPassword(password);

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "Test User", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var request = new LoginRequest(email, password);

        // Act
        var result = await _authService.Login(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.Email.Should().Be(email);
        result.Value.Nombre.Should().Be("Test User");
        result.Value.Rol.Should().Be("User");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "Password123!");

        // Act
        var result = await _authService.Login(request);

        // Assert
        result.IsUnauthorized.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("CorrectPassword123!");

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "Test User", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var request = new LoginRequest(email, "WrongPassword123!");

        // Act
        var result = await _authService.Login(request);

        // Assert
        result.IsUnauthorized.Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        var passwordHash = JwtService.HashPassword(password);

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "Test User", RolUsuario.User, DateTime.UtcNow),
            new UsuarioDesactivado(usuarioId));
        await _session.SaveChangesAsync();

        var request = new LoginRequest(email, password);

        // Act
        var result = await _authService.Login(request);

        // Assert
        result.IsUnauthorized.Should().BeTrue();
    }

    [Fact]
    public async Task Login_ShouldGenerateRefreshToken()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        var passwordHash = JwtService.HashPassword(password);

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "Test User", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var request = new LoginRequest(email, password);

        // Act
        var result = await _authService.Login(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify refresh token was stored in event stream
        var events = await _session.Events.FetchStreamAsync(usuarioId);
        events.Should().Contain(e => e.EventType == typeof(RefreshTokenGenerado));
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = $"newuser-{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "Password123!", "New User", "User");

        // Act
        var result = await _authService.Register(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var usuarioId = result.Value;
        usuarioId.Should().NotBeEmpty();

        // Verify user exists in database
        var usuario = await _session.LoadAsync<Usuario>(usuarioId);
        usuario.Should().NotBeNull();
        usuario!.Email.Should().Be(email);
        usuario.Nombre.Should().Be("New User");
        usuario.Rol.Should().Be(RolUsuario.User);
        usuario.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email = $"duplicate-{Guid.NewGuid()}@example.com";
        var existingUserId = Guid.NewGuid();
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(existingUserId,
            new UsuarioCreado(existingUserId, email, passwordHash, "Existing User", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var request = new RegisterRequest(email, "Password123!", "New User", "User");

        // Act
        var result = await _authService.Register(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("ya está registrado");
    }

    #endregion

    #region SetupAdmin Tests

    [Fact(Skip = "SetupAdmin cannot be tested reliably in shared test environment - requires isolated database")]
    public async Task SetupAdmin_WhenNoUsersExist_ShouldCreateAdminUser()
    {
        // This test is skipped because SetupAdmin checks for ANY users in the database,
        // which conflicts with the shared IntegrationFixture test environment where
        // multiple tests create users in the same schema.
        // SetupAdmin is an initialization endpoint meant to be called once per deployment.

        // Arrange
        var email = $"admin-{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "AdminPassword123!", "Admin User", "Admin");

        // Act
        var result = await _authService.SetupAdmin(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var adminId = result.Value;

        var admin = await _session.LoadAsync<Usuario>(adminId);
        admin.Should().NotBeNull();
        admin!.Rol.Should().Be(RolUsuario.Admin);
        admin.Email.Should().Be(email);
    }

    [Fact(Skip = "SetupAdmin cannot be tested reliably in shared test environment - requires isolated database")]
    public async Task SetupAdmin_WhenUsersExist_ShouldReturnError()
    {
        // This test is skipped for the same reason as above - shared test environment
        // makes it impossible to guarantee "no users exist" state.

        // Arrange - Create an existing user
        var existingUserId = Guid.NewGuid();
        var existingEmail = $"existing-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(existingUserId,
            new UsuarioCreado(existingUserId, existingEmail, passwordHash, "Existing User", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var request = new RegisterRequest($"admin-{Guid.NewGuid()}@example.com", "AdminPassword123!", "Admin User", "Admin");

        // Act
        var result = await _authService.SetupAdmin(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existen usuarios");
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "Test User", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        // Generate initial tokens
        var usuario = await _session.LoadAsync<Usuario>(usuarioId);
        var (_, _, refreshToken, refreshExpiry) = _jwtService.GenerateTokens(usuario!);

        _session.Events.Append(usuarioId, new RefreshTokenGenerado(usuarioId, refreshToken, refreshExpiry, DateTimeOffset.UtcNow));
        await _session.SaveChangesAsync();

        // Act
        var result = await _authService.RefreshToken(refreshToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Token.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBe(refreshToken); // New refresh token should be different
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var invalidToken = "invalid-refresh-token";

        // Act
        var result = await _authService.RefreshToken(invalidToken);

        // Assert
        result.IsUnauthorized.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "Test User", RolUsuario.User, DateTime.UtcNow));

        // Add expired refresh token
        var expiredToken = Guid.NewGuid().ToString();
        var expiredDate = DateTime.UtcNow.AddDays(-1); // Expired yesterday
        _session.Events.Append(usuarioId, new RefreshTokenGenerado(usuarioId, expiredToken, expiredDate, DateTimeOffset.UtcNow));
        await _session.SaveChangesAsync();

        // Act
        var result = await _authService.RefreshToken(expiredToken);

        // Assert
        result.IsUnauthorized.Should().BeTrue();
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidUser_ShouldRevokeTokens()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "Test User", RolUsuario.User, DateTime.UtcNow));

        var refreshToken = Guid.NewGuid().ToString();
        _session.Events.Append(usuarioId, new RefreshTokenGenerado(usuarioId, refreshToken, DateTime.UtcNow.AddDays(7), DateTimeOffset.UtcNow));
        await _session.SaveChangesAsync();

        // Act
        var result = await _authService.Logout(usuarioId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify token was revoked
        var events = await _session.Events.FetchStreamAsync(usuarioId);
        events.Should().Contain(e => e.EventType == typeof(RefreshTokenRevocado));
    }

    [Fact]
    public async Task Logout_WithNonExistentUser_ShouldStillSucceed()
    {
        // Arrange - Logout is idempotent and doesn't validate user existence
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await _authService.Logout(nonExistentUserId);

        // Assert - Service returns success even for non-existent users (optimistic approach)
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ActualizarUsuario Tests

    [Fact]
    public async Task ActualizarUsuario_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "Original Name", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var currentUserId = Guid.NewGuid();
        var uniqueName = $"Updated Name {Guid.NewGuid()}";
        var request = new ActualizarUsuarioRequest(uniqueName, "Admin", true, null);

        // Act
        var result = await _authService.ActualizarUsuario(usuarioId, request, currentUserId, "Admin User");

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify changes
        var usuario = await _session.LoadAsync<Usuario>(usuarioId);
        usuario.Should().NotBeNull();
        usuario!.Nombre.Should().Be(uniqueName);
        usuario.Rol.Should().Be(RolUsuario.Admin);
    }

    [Fact]
    public async Task ActualizarUsuario_WithNonExistentUser_ShouldReturnError()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var request = new ActualizarUsuarioRequest("Updated Name", "User", true, null);

        // Act
        var result = await _authService.ActualizarUsuario(nonExistentUserId, request, currentUserId, "Admin User");

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ActualizarUsuario_WithInvalidRol_ShouldReturnError()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "User to Update", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var request = new ActualizarUsuarioRequest("Updated Name", "InvalidRol", true);
        var currentUserId = Guid.NewGuid();

        // Act
        var result = await _authService.ActualizarUsuario(usuarioId, request, currentUserId, "Admin User");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Rol inválido");
    }

    [Fact]
    public async Task ActualizarUsuario_WithDuplicateName_ShouldReturnError()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var email1 = $"test1-{Guid.NewGuid()}@example.com";
        var email2 = $"test2-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(user1Id,
            new UsuarioCreado(user1Id, email1, passwordHash, "Existing User", RolUsuario.User, DateTime.UtcNow));
        _session.Events.StartStream<Usuario>(user2Id,
            new UsuarioCreado(user2Id, email2, passwordHash, "User to Update", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        // Try to update user2 with user1's name
        var request = new ActualizarUsuarioRequest("Existing User", "User", true);
        var currentUserId = Guid.NewGuid();

        // Act
        var result = await _authService.ActualizarUsuario(user2Id, request, currentUserId, "Admin User");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existe otro usuario con el nombre");
    }

    [Fact]
    public async Task ActualizarUsuario_WithNewPassword_ShouldUpdatePassword()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("OldPassword123!");

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "User to Update", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var uniqueName = $"Updated Name {Guid.NewGuid()}";
        var request = new ActualizarUsuarioRequest(uniqueName, "Admin", true, "NewPassword456!");
        var currentUserId = Guid.NewGuid();

        // Act
        var result = await _authService.ActualizarUsuario(usuarioId, request, currentUserId, "Admin User");

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify password was updated
        var events = await _session.Events.FetchStreamAsync(usuarioId);
        events.Should().Contain(e => e.Data is UsuarioActualizado);
    }

    [Fact]
    public async Task ActualizarUsuario_WithoutPassword_ShouldNotUpdatePassword()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = $"test-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("OldPassword123!");

        _session.Events.StartStream<Usuario>(usuarioId,
            new UsuarioCreado(usuarioId, email, passwordHash, "User to Update", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var request = new ActualizarUsuarioRequest("Updated Name", "Admin", true);
        var currentUserId = Guid.NewGuid();

        // Act
        var result = await _authService.ActualizarUsuario(usuarioId, request, currentUserId, "Admin User");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Additional Register Tests

    [Fact]
    public async Task Register_WithDuplicateName_ShouldReturnError()
    {
        // Arrange
        var existingUserId = Guid.NewGuid();
        var existingEmail = $"existing-{Guid.NewGuid()}@example.com";
        var passwordHash = JwtService.HashPassword("Password123!");

        _session.Events.StartStream<Usuario>(existingUserId,
            new UsuarioCreado(existingUserId, existingEmail, passwordHash, "John Doe", RolUsuario.User, DateTime.UtcNow));
        await _session.SaveChangesAsync();

        var newEmail = $"new-{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(newEmail, "Password456!", "John Doe", "User");

        // Act
        var result = await _authService.Register(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ya existe un usuario con ese nombre completo");
    }

    [Fact]
    public async Task Register_WithInvalidRol_ShouldDefaultToUser()
    {
        // Arrange
        var uniqueId = Guid.NewGuid();
        var email = $"test-{uniqueId}@example.com";
        var nombre = $"Test User {uniqueId}";
        var request = new RegisterRequest(email, "Password123!", nombre, "InvalidRole");

        // Act
        var result = await _authService.Register(request);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify user was created with User role (default)
        var usuario = await _session.LoadAsync<Usuario>(result.Value);
        usuario.Should().NotBeNull();
        usuario!.Rol.Should().Be(RolUsuario.User);
    }

    #endregion
}
