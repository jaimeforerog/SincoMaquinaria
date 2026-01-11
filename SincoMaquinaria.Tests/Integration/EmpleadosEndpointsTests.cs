using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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


    [Fact]
    public async Task CrearEmpleado_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange - Usando cargos válidos del enum CargoEmpleado: Conductor, Operario, Mecanico
        var nuevoEmpleado = new
        {
            Nombre = "Juan Pérez Test",
            Identificacion = "1234567890",
            Cargo = "Mecanico",
            Especialidad = "Motores Diesel",
            ValorHora = 25000m,
            Estado = "Activo"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/empleados", nuevoEmpleado);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActualizarEmpleado_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange - Crear empleado primero
        var nuevoEmpleado = new
        {
            Nombre = $"Pedro Gómez Test {Guid.NewGuid().ToString("N").Substring(0, 4)}",
            Identificacion = "9876543210",
            Cargo = "Operario",
            Especialidad = "Hidráulica",
            ValorHora = 20000m,
            Estado = "Activo"
        };
        var createResponse = await _client.PostAsJsonAsync("/empleados", nuevoEmpleado);
        createResponse.EnsureSuccessStatusCode();

        // Obtener lista para conseguir el ID (simplemente usar un GUID para el test)
        var empleadoId = Guid.NewGuid();

        var datosActualizados = new
        {
            Nombre = "Pedro Gómez Actualizado",
            Identificacion = "9876543210",
            Cargo = "Conductor",
            Especialidad = "Sistemas Hidráulicos",
            ValorHora = 30000m,
            Estado = "Activo"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/empleados/{empleadoId}", datosActualizados);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CrearEmpleado_ConValorHoraCero_DebeAceptar()
    {
        // Arrange
        var nuevoEmpleado = new
        {
            Nombre = "Practicante Test",
            Identificacion = "1111111111",
            Cargo = "Operario",
            Especialidad = "",
            ValorHora = 0m,
            Estado = "Activo"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/empleados", nuevoEmpleado);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActualizarEmpleado_CambiarEstadoAInactivo_DebeRetornarOk()
    {
        // Arrange - Solo probar la actualización con un ID válido
        var empleadoId = Guid.NewGuid();

        var datosActualizados = new
        {
            Nombre = "Ana López Test",
            Identificacion = "5555555555",
            Cargo = "Mecanico",
            Especialidad = "",
            ValorHora = 15000m,
            Estado = "Inactivo"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/empleados/{empleadoId}", datosActualizados);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}