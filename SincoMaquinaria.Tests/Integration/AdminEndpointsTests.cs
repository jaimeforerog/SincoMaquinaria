using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Tests.Helpers;
using Xunit;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class AdminEndpointsTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AdminEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region ListarLogs Tests

    [Fact]
    public async Task ListarLogs_AsAdmin_ShouldReturn200WithLogs()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/admin/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var logs = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        logs.Should().NotBeNull();
    }

    [Fact]
    public async Task ListarLogs_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange - crear usuario normal
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        var uniqueEmail = $"normaluser_logs_{Guid.NewGuid():N}@test.com";
        var userRegister = new RegisterRequest(uniqueEmail, "User123!", "Normal User", "User");
        await _client.PostAsJsonAsync("/auth/register", userRegister);

        // Login como usuario normal
        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(uniqueEmail, "User123!"));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        // Act
        var response = await _client.GetAsync("/admin/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListarLogs_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/admin/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListarLogs_ShouldReturnMax50Logs()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/admin/logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var logs = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        logs!.Count.Should().BeLessOrEqualTo(50);
    }

    #endregion

    #region DiagnosticosEventos Tests

    [Fact]
    public async Task DiagnosticosEventos_AsAdmin_ShouldReturn200WithDiagnostics()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/admin/diagnostics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var diagnostics = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        diagnostics.Should().NotBeNull();
        diagnostics!.Should().ContainKey("eventStore");
        diagnostics.Should().ContainKey("proyecciones");
        diagnostics.Should().ContainKey("diagnostico");
    }

    [Fact]
    public async Task DiagnosticosEventos_ShouldIncludeEventCounts()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/admin/diagnostics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("totalEventos");
        result.Should().Contain("eventosEquipo");
        result.Should().Contain("eventosRutina");
    }

    [Fact]
    public async Task DiagnosticosEventos_ShouldIncludeProjectionCounts()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/admin/diagnostics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("equipos");
        result.Should().Contain("rutinas");
        result.Should().Contain("ordenes");
        result.Should().Contain("empleados");
    }

    [Fact]
    public async Task DiagnosticosEventos_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        var uniqueEmail = $"normaluser_diag_{Guid.NewGuid():N}@test.com";
        var userRegister = new RegisterRequest(uniqueEmail, "User123!", "Normal User", "User");
        await _client.PostAsJsonAsync("/auth/register", userRegister);

        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(uniqueEmail, "User123!"));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        // Act
        var response = await _client.GetAsync("/admin/diagnostics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region ReconstruirProyecciones Tests

    [Fact]
    public async Task ReconstruirProyecciones_AsAdmin_ShouldReturn200()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.PostAsync("/admin/rebuild-projections", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result!.Should().ContainKey("message");
        result.Should().ContainKey("agreGadosReconstruidos");
        result.Should().ContainKey("totalStreams");
    }

    [Fact]
    public async Task ReconstruirProyecciones_ShouldReturnSuccessMessage()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.PostAsync("/admin/rebuild-projections", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("Proyecciones reconstruidas exitosamente");
    }

    [Fact]
    public async Task ReconstruirProyecciones_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        var uniqueEmail = $"normaluser_rebuild_{Guid.NewGuid():N}@test.com";
        var userRegister = new RegisterRequest(uniqueEmail, "User123!", "Normal User", "User");
        await _client.PostAsJsonAsync("/auth/register", userRegister);

        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(uniqueEmail, "User123!"));
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        // Act
        var response = await _client.PostAsync("/admin/rebuild-projections", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region VerificarDuplicados Tests

    [Fact]
    public async Task VerificarDuplicados_AsAdmin_ShouldReturn200()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/admin/check-duplicates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result!.Should().ContainKey("totalEquipos");
        result.Should().ContainKey("placasDuplicadas");
        result.Should().ContainKey("mensaje");
    }

    [Fact]
    public async Task VerificarDuplicados_ShouldReturnDuplicateInfo()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/admin/check-duplicates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("totalEquipos");
        result.Should().Contain("placasDuplicadas");
    }

    [Fact]
    public async Task VerificarDuplicados_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        // Use unique email AND name per test to avoid collisions
        var uniqueId = Guid.NewGuid().ToString("N");
        var testEmail = $"test_verificarduplicados_{uniqueId}@test.com";
        var testName = $"TestUser VerificarDuplicados {uniqueId}";
        var userRegister = new RegisterRequest(testEmail, "User123!", testName, "User");
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", userRegister);
        registerResponse.EnsureSuccessStatusCode();

        // Wait for projection to complete
        await Task.Delay(1000);

        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(testEmail, "User123!"));
        loginResponse.EnsureSuccessStatusCode();
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        // Act
        var response = await _client.GetAsync("/admin/check-duplicates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region CorregirDuplicados Tests

    [Fact]
    public async Task CorregirDuplicados_AsAdmin_ShouldReturn200()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.PostAsync("/admin/fix-duplicates", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        result.Should().NotBeNull();
        result!.Should().ContainKey("eliminados");
        result.Should().ContainKey("noEliminados");
        result.Should().ContainKey("mensaje");
    }

    [Fact]
    public async Task CorregirDuplicados_ShouldReturnDeletionDetails()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.PostAsync("/admin/fix-duplicates", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("eliminados");
        result.Should().Contain("detalleEliminados");
        result.Should().Contain("mensaje");
    }

    [Fact]
    public async Task CorregirDuplicados_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        // Use unique email AND name per test to avoid collisions
        var uniqueId = Guid.NewGuid().ToString("N");
        var testEmail = $"test_corregirduplicados_{uniqueId}@test.com";
        var testName = $"TestUser CorregirDuplicados {uniqueId}";
        var userRegister = new RegisterRequest(testEmail, "User123!", testName, "User");
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", userRegister);
        registerResponse.EnsureSuccessStatusCode();

        // Wait for projection to complete
        await Task.Delay(1000);

        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(testEmail, "User123!"));
        loginResponse.EnsureSuccessStatusCode();
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        // Act
        var response = await _client.PostAsync("/admin/fix-duplicates", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region ResetDatabase Tests

    [Fact]
    public async Task ResetDatabase_AsAdmin_ShouldReturn200()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.PostAsync("/admin/reset-db", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        result.Should().NotBeNull();
        result!.Should().ContainKey("message");
    }

    [Fact]
    public async Task ResetDatabase_ShouldPreserveAdminUser()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.PostAsync("/admin/reset-db", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("preservado");
    }

    [Fact]
    public async Task ResetDatabase_AfterReset_AdminShouldStillLogin()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act - Reset database
        await _client.PostAsync("/admin/reset-db", null);

        // Try to login after reset
        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest("admin@test.com", "Admin123!"));

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetDatabase_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange
        var adminToken = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {adminToken}");

        // Use unique email AND name per test to avoid collisions
        var uniqueId = Guid.NewGuid().ToString("N");
        var testEmail = $"test_resetdatabase_{uniqueId}@test.com";
        var testName = $"TestUser ResetDatabase {uniqueId}";
        var userRegister = new RegisterRequest(testEmail, "User123!", testName, "User");
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", userRegister);
        registerResponse.EnsureSuccessStatusCode();

        // Wait for projection to complete
        await Task.Delay(1000);

        _client.DefaultRequestHeaders.Clear();
        var loginResponse = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest(testEmail, "User123!"));
        loginResponse.EnsureSuccessStatusCode();
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<TestAuthResponse>();

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse!.Token}");

        // Act
        var response = await _client.PostAsync("/admin/reset-db", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Cross-Endpoint Security Tests

    [Fact]
    public async Task AllAdminEndpoints_WithExpiredToken_ShouldReturn401()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.expired";
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {expiredToken}");

        // Act & Assert
        var logsResponse = await _client.GetAsync("/admin/logs");
        logsResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var diagResponse = await _client.GetAsync("/admin/diagnostics");
        diagResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var checkResponse = await _client.GetAsync("/admin/check-duplicates");
        checkResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AllAdminEndpoints_WithoutToken_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act & Assert - GET endpoints
        (await _client.GetAsync("/admin/logs")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _client.GetAsync("/admin/diagnostics")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _client.GetAsync("/admin/check-duplicates")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // POST endpoints
        (await _client.PostAsync("/admin/reset-db", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _client.PostAsync("/admin/rebuild-projections", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _client.PostAsync("/admin/fix-duplicates", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
