using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;

namespace SincoMaquinaria.Tests.Integration;

public class EquiposEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EquiposEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task GetEquipos_DebeRetornarLista()
    {
        // Act
        var response = await _client.GetAsync("/equipos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var equipos = await response.Content.ReadFromJsonAsync<object[]>();
        equipos.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEquipo_DebeRetornarNotFound_CuandoNoExiste()
    {
        // Arrange
        var equipoId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/equipos/{equipoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActualizarEquipo_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange
        var equipoId = Guid.NewGuid();
        var datosActualizados = new
        {
            Descripcion = "Excavadora Actualizada",
            Marca = "Caterpillar",
            Modelo = "320D",
            Serie = "SN123456",
            Codigo = "EQ-001",
            TipoMedidorId = "MED001",
            TipoMedidorId2 = "MED002",
            Grupo = "GRUPO001",
            Rutina = "RUT001"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/equipos/{equipoId}", datosActualizados);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}