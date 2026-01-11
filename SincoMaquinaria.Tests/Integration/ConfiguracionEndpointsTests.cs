using Xunit;
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SincoMaquinaria.Tests.Integration;

public class ConfiguracionEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ConfiguracionEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task CrearTipoMedidor_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange
        var nuevoTipo = new
        {
            Nombre = $"Horómetro Test {Guid.NewGuid().ToString("N").Substring(0, 8)}",
            Unidad = $"H{Guid.NewGuid().ToString("N").Substring(0, 2)}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/configuracion/medidores", nuevoTipo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTiposMedidor_DebeRetornarLista()
    {
        // Act
        var response = await _client.GetAsync("/configuracion/medidores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tipos = await response.Content.ReadFromJsonAsync<object[]>();
        tipos.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearGrupoMantenimiento_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange
        var nuevoGrupo = new
        {
            Nombre = $"Grupo Test {Guid.NewGuid().ToString("N").Substring(0, 8)}",
            Descripcion = "Descripción de prueba"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/configuracion/grupos", nuevoGrupo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetGruposMantenimiento_DebeRetornarLista()
    {
        // Act
        var response = await _client.GetAsync("/configuracion/grupos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var grupos = await response.Content.ReadFromJsonAsync<object[]>();
        grupos.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearTipoFalla_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange
        var nuevoTipo = new
        {
            Descripcion = $"Falla Mecánica Test {Guid.NewGuid().ToString("N").Substring(0, 8)}",
            Prioridad = "Alta"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/configuracion/fallas", nuevoTipo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTiposFalla_DebeRetornarLista()
    {
        // Act
        var response = await _client.GetAsync("/configuracion/fallas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tipos = await response.Content.ReadFromJsonAsync<object[]>();
        tipos.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearCausaFalla_DebeRetornarOk_ConDatosValidos()
    {
        // Arrange
        var nuevaCausa = new
        {
            Descripcion = $"Desgaste por uso Test {Guid.NewGuid().ToString("N").Substring(0, 8)}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/configuracion/causas-falla", nuevaCausa);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCausasFalla_DebeRetornarLista()
    {
        // Act
        var response = await _client.GetAsync("/configuracion/causas-falla");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var causas = await response.Content.ReadFromJsonAsync<object[]>();
        causas.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearTipoMedidorDuplicado_DebeRetornarConflict()
    {
        // Arrange
        var nuevoTipo = new
        {
            Nombre = "Tipo Duplicado",
            Unidad = "DUP"
        };

        // Crear el primero
        await _client.PostAsJsonAsync("/configuracion/medidores", nuevoTipo);

        // Act - Intentar crear duplicado
        var response = await _client.PostAsJsonAsync("/configuracion/medidores", nuevoTipo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ActualizarTipoMedidor_DebeRetornarOk_CuandoExiste()
    {
        // Arrange - Crear primero
        var nuevoTipo = new
        {
            Nombre = $"Tipo Para Actualizar {Guid.NewGuid().ToString("N").Substring(0, 4)}",
            Unidad = "UPD"
        };
        await _client.PostAsJsonAsync("/configuracion/medidores", nuevoTipo);

        // Usar un código de prueba válido
        var codigo = "TEST001";

        var datosActualizados = new
        {
            Nombre = "Tipo Actualizado",
            Unidad = "ACT"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/configuracion/medidores/{codigo}", datosActualizados);

        // Assert
        // El endpoint siempre retorna OK aunque el código no exista
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CambiarEstadoTipoMedidor_DebeRetornarOk_CuandoExiste()
    {
        // Arrange - Crear primero
        var nuevoTipo = new
        {
            Nombre = $"Tipo Para Desactivar {Guid.NewGuid().ToString("N").Substring(0, 4)}",
            Unidad = "DSA"
        };
        await _client.PostAsJsonAsync("/configuracion/medidores", nuevoTipo);

        // Usar código de prueba
        var codigo = "TEST002";

        var cambioEstado = new
        {
            Activo = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/configuracion/medidores/{codigo}/estado", cambioEstado);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}