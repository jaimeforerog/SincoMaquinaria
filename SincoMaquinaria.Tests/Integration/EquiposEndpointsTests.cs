using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Tests.Helpers;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class EquiposEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EquiposEndpointsTests(CustomWebApplicationFactory factory)
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

    // Local class to deserialize paged response
    private class PagedResult
    {
        public List<object>? Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

    [Fact]
    public async Task GetEquipos_DebeRetornarLista()
    {
        // Arrange
        var token = await GetAdminToken(_client);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await _client.GetAsync("/equipos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEquipo_DebeRetornarNotFound_CuandoNoExiste()
    {
        // Arrange
        var token = await GetAdminToken(_client);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

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
        var token = await GetAdminToken(_client);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

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