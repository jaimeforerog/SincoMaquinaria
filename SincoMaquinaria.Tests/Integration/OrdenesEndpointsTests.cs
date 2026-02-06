using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Text.Json;

namespace SincoMaquinaria.Tests.Integration;

public class OrdenesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdenesEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }



    [Fact]
    public async Task CrearOrden_DebeRetornarCreated_ConOrdenValida()
    {
        // Arrange
        var nuevaOrden = new
        {
            Numero = $"OT-TEST-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
            EquipoId = Guid.NewGuid().ToString(),
            Origen = "Interno",
            Tipo = "Preventivo",
            FechaOrden = DateTime.Now
        };

        // Act
        var response = await _client.PostAsJsonAsync("/ordenes", nuevaOrden);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var contentString = await response.Content.ReadAsStringAsync();
        contentString.Should().NotBeNullOrEmpty();
        contentString.Should().Contain("id");
    }

    [Fact]
    public async Task GetOrden_DebeRetornarOrden_CuandoExiste()
    {
        // Arrange - Crear una orden primero
        var nuevaOrden = new
        {
            Numero = $"OT-TEST-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
            EquipoId = Guid.NewGuid().ToString(),
            Origen = "Interno",
            Tipo = "Correctivo"
        };
        var createResponse = await _client.PostAsJsonAsync("/ordenes", nuevaOrden);
        createResponse.EnsureSuccessStatusCode();

        // Extraer ID del Location header
        var location = createResponse.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
        {
            // Parse JSON to get ID
            var content = await createResponse.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var id = jsonDoc.RootElement.GetProperty("id").GetString();
            location = $"/ordenes/{id}";
        }

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentString = await response.Content.ReadAsStringAsync();
        contentString.Should().Contain(nuevaOrden.Numero);
    }

    [Fact]
    public async Task AgregarActividad_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange - Crear orden
        var nuevaOrden = new
        {
            Numero = $"OT-TEST-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
            EquipoId = Guid.NewGuid().ToString(),
            Origen = "Interno",
            Tipo = "Preventivo"
        };
        var createResponse = await _client.PostAsJsonAsync("/ordenes", nuevaOrden);
        var content = await createResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var ordenId = jsonDoc.RootElement.GetProperty("id").GetString();

        var nuevaActividad = new
        {
            Descripcion = "Cambio de aceite",
            FechaEstimada = DateTime.Now.AddDays(1)
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/ordenes/{ordenId}/actividades", nuevaActividad);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegistrarAvance_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange - Crear orden
        var nuevaOrden = new
        {
            Numero = $"OT-TEST-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
            EquipoId = Guid.NewGuid().ToString(),
            Origen = "Interno",
            Tipo = "Preventivo"
        };
        var createOrdenResponse = await _client.PostAsJsonAsync("/ordenes", nuevaOrden);
        var ordenContent = await createOrdenResponse.Content.ReadAsStringAsync();
        var ordenJsonDoc = JsonDocument.Parse(ordenContent);
        var ordenId = ordenJsonDoc.RootElement.GetProperty("id").GetString();

        // Agregar actividad
        var nuevaActividad = new
        {
            Descripcion = "Inspección general",
            FechaEstimada = DateTime.Now.AddDays(1)
        };
        var createActividadResponse = await _client.PostAsJsonAsync($"/ordenes/{ordenId}/actividades", nuevaActividad);
        var actividadContent = await createActividadResponse.Content.ReadAsStringAsync();
        var actividadJsonDoc = JsonDocument.Parse(actividadContent);
        var detalleId = actividadJsonDoc.RootElement.GetProperty("detalleId").GetString();

        var avance = new
        {
            DetalleId = detalleId,
            Porcentaje = 50,
            Observacion = "Trabajo en progreso",
            NuevoEstado = "EnProceso"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/ordenes/{ordenId}/avance", avance);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHistorial_DebeRetornarEventos_CuandoOrdenExiste()
    {
        // Arrange - Crear orden
        var nuevaOrden = new
        {
            Numero = $"OT-TEST-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
            EquipoId = Guid.NewGuid().ToString(),
            Origen = "Interno",
            Tipo = "Preventivo"
        };
        var createResponse = await _client.PostAsJsonAsync("/ordenes", nuevaOrden);
        var content = await createResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var ordenId = jsonDoc.RootElement.GetProperty("id").GetString();

        // Act
        var response = await _client.GetAsync($"/ordenes/{ordenId}/historial");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var historial = await response.Content.ReadFromJsonAsync<object[]>();
        historial.Should().NotBeNull();
        historial.Should().NotBeEmpty();
    }
}