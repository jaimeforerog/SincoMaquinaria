using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace SincoMaquinaria.Tests.Integration;

public class EmpleadosEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmpleadosEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Creates an employee via POST and retrieves its server-assigned ID from GET /empleados.
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

        var listResponse = await _client.GetAsync("/empleados");
        listResponse.EnsureSuccessStatusCode();

        var body = await listResponse.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(body)!;

        foreach (var item in json.GetProperty("data").EnumerateArray())
        {
            if (item.GetProperty("identificacion").GetString() == identificacion)
                return Guid.Parse(item.GetProperty("id").GetString()!);
        }

        throw new InvalidOperationException($"Empleado con identificacion '{identificacion}' no encontrado en lista");
    }

    [Fact]
    public async Task CrearEmpleado_DebeRetornarOk_ConDatosValidos()
    {
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
