using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Tests.Helpers;
using SincoMaquinaria.Tests.Integration;
using Xunit;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class EquiposEndpointsEdgeCasesTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EquiposEndpointsEdgeCasesTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminToken()
    {
        _client.DefaultRequestHeaders.Clear();
        var loginRequest = new LoginRequest("admin@test.com", "Admin123!");
        var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<TestAuthResponse>();
        return authResponse!.Token;
    }


    [Fact]
    public async Task GetEquipo_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync($"/equipos/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrearEquipo_WithEmptyGrupo_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new CrearEquipoRequest(
            "PLACA-001",
            "Descripción",
            "Marca",
            "Modelo",
            "Serie",
            "Código",
            "Medidor1",
            "Medidor2",
            "", // Grupo vacío
            "Rutina1",
            null,
            null,
            null,
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/equipos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearEquipo_WithEmptyRutina_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new CrearEquipoRequest(
            "PLACA-002",
            "Descripción",
            "Marca",
            "Modelo",
            "Serie",
            "Código",
            "Medidor1",
            "Medidor2",
            "Grupo1",
            "", // Rutina vacía
            null,
            null,
            null,
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/equipos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearEquipo_WithDuplicatePlaca_ShouldReturnConflict()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var placa = $"PLACA-{Guid.NewGuid():N}";
        var request = new CrearEquipoRequest(
            placa,
            "Descripción",
            "Marca",
            "Modelo",
            "Serie",
            "Código",
            "Medidor1",
            "Medidor2",
            "Grupo1",
            "Rutina1",
            null,
            null,
            null,
            null
        );

        // Crear primero
        await _client.PostAsJsonAsync("/equipos", request);

        // Act - Intentar crear duplicado
        var response = await _client.PostAsJsonAsync("/equipos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain(placa);
    }


    [Fact]
    public async Task ActualizarEquipo_WithEmptyGrupo_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Crear equipo primero
        var createRequest = new CrearEquipoRequest(
            $"PLACA-{Guid.NewGuid():N}",
            "Test Equipo",
            "Marca",
            "Modelo",
            "Serie",
            "Código",
            "Medidor1",
            "Medidor2",
            "Grupo1",
            "Rutina1",
            null,
            null,
            null,
            null
        );
        var createResponse = await _client.PostAsJsonAsync("/equipos", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var equipoId = createResult!["id"].ToString();

        // Act
        var updateRequest = new ActualizarEquipoRequest(
            "Descripción",
            "Marca",
            "Modelo",
            "Serie",
            "Código",
            null,
            null,
            "", // Grupo vacío
            "Rutina1"
        );
        var response = await _client.PutAsJsonAsync($"/equipos/{equipoId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActualizarEquipo_WithEmptyRutina_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Crear equipo primero
        var createRequest = new CrearEquipoRequest(
            $"PLACA-{Guid.NewGuid():N}",
            "Test Equipo",
            "Marca",
            "Modelo",
            "Serie",
            "Código",
            "Medidor1",
            "Medidor2",
            "Grupo1",
            "Rutina1",
            null,
            null,
            null,
            null
        );
        var createResponse = await _client.PostAsJsonAsync("/equipos", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var equipoId = createResult!["id"].ToString();

        // Act
        var updateRequest = new ActualizarEquipoRequest(
            "Descripción",
            "Marca",
            "Modelo",
            "Serie",
            "Código",
            null,
            null,
            "Grupo1",
            "" // Rutina vacía
        );
        var response = await _client.PutAsJsonAsync($"/equipos/{equipoId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEquipos_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/equipos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CrearEquipo_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();
        var request = new CrearEquipoRequest(
            "PLACA-003",
            "Descripción",
            "Marca",
            "Modelo",
            "Serie",
            "Código",
            "Medidor1",
            "Medidor2",
            "Grupo1",
            "Rutina1",
            null,
            null,
            null,
            null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/equipos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ImportarEquipos_WithoutFile_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/equipos/importar", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CrearEquipo_ThenGetById_ShouldReturnCreatedEquipo()
    {
        // Arrange
        var token = await GetAdminToken();
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var createRequest = new CrearEquipoRequest(
            $"PLACA-{Guid.NewGuid():N}",
            "Equipo de Prueba",
            "Caterpillar",
            "320D",
            "SERIE123",
            "COD123",
            "Medidor1",
            "Medidor2",
            "Grupo1",
            "Rutina1",
            null,
            null,
            null,
            null
        );

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/equipos", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createResult = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var equipoId = createResult!["id"].ToString();

        // Act - Get
        var getResponse = await _client.GetAsync($"/equipos/{equipoId}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var equipo = await getResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        equipo.Should().ContainKey("placa");
        equipo.Should().ContainKey("descripcion");
    }

    [Fact]
    public async Task DescargarPlantilla_ShouldReturnExcelFile()
    {
        // Arrange - No requiere autenticación (AllowAnonymous)
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/equipos/plantilla");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var content = await response.Content.ReadAsByteArrayAsync();
        content.Should().NotBeEmpty();
    }
}
