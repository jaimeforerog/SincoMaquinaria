using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Tests.Integration;
using Xunit;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Tests.Helpers;

namespace SincoMaquinaria.Tests.Middleware;

/// <summary>
/// Integration tests for ExceptionHandlingMiddleware.
/// Note: The middleware is also tested indirectly through all other integration tests
/// that trigger validation errors and exceptions.
/// </summary>
[Collection("Integration")]
public class ExceptionHandlingMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ExceptionHandlingMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetAdminToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
        var loginRequest = new LoginRequest("admin@test.com", "Admin123!");
        var response = await client.PostAsJsonAsync("/auth/login", loginRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<TestAuthResponse>();
        return authResponse!.Token;
    }

    [Fact]
    public async Task Middleware_WhenValidRequest_ShouldReturn200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act - Call an endpoint that should succeed
        var response = await client.GetAsync("/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Middleware_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Call a non-existent endpoint
        var response = await client.GetAsync("/api/this-endpoint-does-not-exist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Middleware_ResponsesShouldHaveJsonContentType()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.GetAsync("/dashboard/stats");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task Middleware_WhenDomainExceptionThrown_ShouldReturn400WithErrorDetails()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Crear empleado con datos inválidos para provocar DomainException
        var request = new CrearEmpleadoRequest("", "", "", "", -1000, "Activo");

        // Act
        var response = await client.PostAsJsonAsync("/empleados", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error");
    }

    [Fact]
    public async Task Middleware_WhenAuthenticationFails_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Clear();
        // No token provided

        // Act
        var response = await client.GetAsync("/empleados");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
