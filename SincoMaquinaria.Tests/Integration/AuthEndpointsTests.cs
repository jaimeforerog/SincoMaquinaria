using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Tests.Helpers;
using Xunit;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class AuthEndpointsTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200WithToken()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@test.com", "Admin123!");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<TestAuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.Email.Should().Be("admin@test.com");
        authResponse.Rol.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@test.com", "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturn401()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@test.com", "SomePassword");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest("", "Password123");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_AsAdmin_WithValidData_ShouldReturn201()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var registerRequest = new RegisterRequest(
            Email: "newuser@test.com",
            Password: "NewUser123!",
            Nombre: "Nuevo Usuario",
            Rol: "User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().ContainKey("id");
    }

    [Fact]
    public async Task Register_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange - crear usuario normal primero
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        // Use unique email AND name per test to avoid collisions
        var uniqueId = Guid.NewGuid().ToString("N");
        var normalUserEmail = $"test_registerauth_{uniqueId}@test.com";
        var normalUserName = $"TestUser RegisterAuth {uniqueId}";
        var userRegister = new RegisterRequest(normalUserEmail, "User123!", normalUserName, "User");
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", userRegister);
        registerResponse.EnsureSuccessStatusCode();

        // Wait for projection to complete
        await Task.Delay(1000);

        // Login como usuario normal
        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(normalUserEmail, "User123!"));
        loginResponse.EnsureSuccessStatusCode();
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        var newUserRequest = new RegisterRequest($"another_{uniqueId}@test.com", "Pass123!", $"Another {uniqueId}", "User");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", newUserRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var registerRequest = new RegisterRequest(
            Email: "admin@test.com", // Email ya existe
            Password: "SomePass123!",
            Nombre: "Duplicate User"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        var registerRequest = new RegisterRequest("test@test.com", "Pass123!", "Test User");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturn200WithUserData()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        user.Should().ContainKey("email");
        user.Should().ContainKey("nombre");
        user.Should().ContainKey("rol");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer invalid_token_12345");

        // Act
        var response = await _client.GetAsync("/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetAllUsers Tests

    [Fact]
    public async Task GetAllUsers_AsAdmin_ShouldReturn200WithUserList()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/auth/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        users.Should().NotBeNull();
        users!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAllUsers_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange - crear y loguear como usuario normal
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        var normalUserEmail = $"normaluser2_{Guid.NewGuid():N}@test.com";
        var userRegister = new RegisterRequest(normalUserEmail, "User123!", "Normal User 2", "User");
        await _client.PostAsJsonAsync("/auth/register", userRegister);

        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(normalUserEmail, "User123!"));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        // Act
        var response = await _client.GetAsync("/auth/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsers_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/auth/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidRefreshToken_ShouldReturn200WithNewTokens()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest("admin@test.com", "Admin123!"));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        var refreshRequest = new RefreshTokenRequest(authResponse!.RefreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTestAuthResponse = await response.Content.ReadFromJsonAsync<TestAuthResponse>();
        newTestAuthResponse.Should().NotBeNull();
        newTestAuthResponse!.Token.Should().NotBeNullOrEmpty();
        newTestAuthResponse.RefreshToken.Should().NotBeNullOrEmpty();
        newTestAuthResponse.Token.Should().NotBe(authResponse.Token); // Nuevo token
    }

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_ShouldReturn401()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest("invalid_refresh_token");

        // Act
        var response = await _client.PostAsJsonAsync("/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredRefreshToken_ShouldReturn401()
    {
        // Arrange - usar un refresh token que ya fue usado
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest("admin@test.com", "Admin123!"));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        // Usar el refresh token una vez
        await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshTokenRequest(authResponse!.RefreshToken));

        // Act - intentar usar el mismo refresh token de nuevo
        var response = await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshTokenRequest(authResponse.RefreshToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturn200()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.PostAsync("/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        result.Should().ContainKey("message");
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.PostAsync("/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_AfterLogout_TokenShouldBeInvalid()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest("admin@test.com", "Admin123!"));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        // Act - Logout
        await _client.PostAsync("/auth/logout", null);

        // Intentar usar el token después del logout
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse.Token}");
        var response = await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshTokenRequest(authResponse.RefreshToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region ActualizarUsuario Tests

    [Fact]
    public async Task ActualizarUsuario_AsAdmin_WithValidData_ShouldReturn200()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        // Crear usuario para actualizar
        var registerRequest = new RegisterRequest($"updatetest_{Guid.NewGuid():N}@test.com", "Test123!", "Update Test", "User");
        var createResponse = await _client.PostAsJsonAsync("/auth/register", registerRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var userId = createResult!["id"].ToString();

        var updateRequest = new ActualizarUsuarioRequest(
            Nombre: "Updated Name",
            Rol: "User",
            Activo: true
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/auth/users/{userId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActualizarUsuario_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange - crear usuario normal
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        var normalUserEmail = $"normaluser3_{Guid.NewGuid():N}@test.com";
        var userRegister = new RegisterRequest(normalUserEmail, "User123!", "Normal User 3", "User");
        var createResponse = await _client.PostAsJsonAsync("/auth/register", userRegister);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var userId = createResult!["id"].ToString();

        // Login como usuario normal
        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(normalUserEmail, "User123!"));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        var updateRequest = new ActualizarUsuarioRequest("Updated", "Admin", true);

        // Act
        var response = await _client.PutAsJsonAsync($"/auth/users/{userId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ActualizarUsuario_WithNonExistentId_ShouldReturn404()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var updateRequest = new ActualizarUsuarioRequest("Test", "User", true);

        // Act
        var response = await _client.PutAsJsonAsync($"/auth/users/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    private async Task<string> GetAdminToken()
    {
        _client.DefaultRequestHeaders.Clear();
        var loginRequest = new LoginRequest("admin@test.com", "Admin123!");
        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<TestAuthResponse>();
        return authResponse!.Token;
    }

    #endregion
}
