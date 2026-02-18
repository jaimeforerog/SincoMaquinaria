using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Tests.Helpers;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class EmpleadosEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmpleadosEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
        var loginRequest = new LoginRequest("admin@test.com", "Admin123!");
        var response = await client.PostAsJsonAsync("/auth/login", loginRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<TestAuthResponse>();
        return authResponse!.Token;
    }

    /// <summary>
    /// Creates an employee via POST and retrieves its server-assigned ID from the response.
    /// </summary>
    private async Task<Guid> CrearEmpleadoYObtenerIdAsync(string nombre, string identificacion,
        string cargo = "Mecanico", string especialidad = "Motores Diesel", decimal valorHora = 25000m)
    {
        var nuevoEmpleado = new
        {
            Nombre = nombre,
            Identificacion = identificacion,
            Cargo = cargo,
            Especialidad = especialidad,
            ValorHora = valorHora,
            Estado = "Activo"
        };

        var createResponse = await _client.PostAsJsonAsync("/empleados", nuevoEmpleado);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Parse the response to get the created ID
        var responseBody = await createResponse.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

        return Guid.Parse(json.GetProperty("id").GetString()!);
    }

    [Fact]
    public async Task CrearEmpleado_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange
        var token = await GetAdminToken(_client);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var uid = Guid.NewGuid().ToString("N")[..8];
        var nuevoEmpleado = new
        {
            Nombre = $"Juan Pérez Test {uid}",
            Identificacion = $"test-create-{uid}",
            Cargo = "Mecanico",
            Especialidad = "Motores Diesel",
            ValorHora = 25000m,
            Estado = "Activo"
        };

        var response = await _client.PostAsJsonAsync("/empleados", nuevoEmpleado);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActualizarEmpleado_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange
        var token = await GetAdminToken(_client);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var uid = Guid.NewGuid().ToString("N")[..8];
        var identificacion = $"test-upd-{uid}";

        var empleadoId = await CrearEmpleadoYObtenerIdAsync(
            nombre: $"Pedro Gómez Test {uid}",
            identificacion: identificacion,
            cargo: "Operario",
            especialidad: "Hidráulica",
            valorHora: 20000m);

        var datosActualizados = new
        {
            Nombre = "Pedro Gómez Actualizado",
            Identificacion = identificacion,
            Cargo = "Conductor",
            Especialidad = "Sistemas Hidráulicos",
            ValorHora = 30000m,
            Estado = "Activo"
        };

        var response = await _client.PutAsJsonAsync($"/empleados/{empleadoId}", datosActualizados);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CrearEmpleado_ConValorHoraCero_DebeAceptar()
    {
        // Arrange
        var token = await GetAdminToken(_client);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var uid = Guid.NewGuid().ToString("N")[..8];
        var nuevoEmpleado = new
        {
            Nombre = $"Practicante Test {uid}",
            Identificacion = $"test-zero-{uid}",
            Cargo = "Operario",
            Especialidad = "",
            ValorHora = 0m,
            Estado = "Activo"
        };

        var response = await _client.PostAsJsonAsync("/empleados", nuevoEmpleado);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActualizarEmpleado_CambiarEstadoAInactivo_DebeRetornarOk()
    {
        // Arrange
        var token = await GetAdminToken(_client);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var uid = Guid.NewGuid().ToString("N")[..8];
        var identificacion = $"test-inact-{uid}";

        var empleadoId = await CrearEmpleadoYObtenerIdAsync(
            nombre: $"Ana López Test {uid}",
            identificacion: identificacion,
            cargo: "Mecanico",
            especialidad: "Equipos Pesados",
            valorHora: 15000m);

        var datosActualizados = new
        {
            Nombre = "Ana López Inactiva",
            Identificacion = identificacion,
            Cargo = "Mecanico",
            Especialidad = "Equipos Pesados",
            ValorHora = 15000m,
            Estado = "Inactivo"
        };

        var response = await _client.PutAsJsonAsync($"/empleados/{empleadoId}", datosActualizados);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
