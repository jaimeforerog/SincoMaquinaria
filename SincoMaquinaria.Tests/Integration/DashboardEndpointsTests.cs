using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events.Equipo;
using SincoMaquinaria.Domain.Events.OrdenDeTrabajo;
using Marten;
using Xunit;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Tests.Helpers;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class DashboardEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DashboardEndpointsTests(CustomWebApplicationFactory factory)
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
    public async Task ObtenerEstadisticas_ShouldReturnCounts()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Seed some data using Marten event sourcing
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Seed Equipment using event streams with unique placas
                var equipo1Id = Guid.NewGuid();
                var equipo2Id = Guid.NewGuid();
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

                session.Events.StartStream<Equipo>(equipo1Id,
                    new EquipoCreado(equipo1Id, $"EQ-TEST-{uniqueId}-01", "Equipo Test 1", "CAT", "320D", "", "", "", "", "", ""));

                session.Events.StartStream<Equipo>(equipo2Id,
                    new EquipoCreado(equipo2Id, $"EQ-TEST-{uniqueId}-02", "Equipo Test 2", "CAT", "330D", "", "", "", "", "", ""));

                // Seed Rutina using event streams
                var rutinaId = Guid.NewGuid();
                session.Events.StartStream<RutinaMantenimiento>(rutinaId,
                    new RutinaMigrada(rutinaId, "Rutina Test", "Grupo Test"));

                // Seed Order using event streams
                var ordenId = Guid.NewGuid();
                session.Events.StartStream<OrdenDeTrabajo>(ordenId,
                    new OrdenDeTrabajoCreada(ordenId, "OT-100", equipo1Id.ToString(), $"EQ-TEST-{uniqueId}-01", "Correctivo", DateTime.Now, DateTimeOffset.UtcNow));

                await session.SaveChangesAsync();
            }
        }

        // Authenticate
        var token = await GetAdminToken(client);
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await client.GetAsync("/dashboard/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<DashboardStats>();

        stats.Should().NotBeNull();
        stats.EquiposCount.Should().BeGreaterThanOrEqualTo(2);
        stats.RutinasCount.Should().BeGreaterThanOrEqualTo(1);
        stats.OrdenesActivasCount.Should().BeGreaterThanOrEqualTo(1);
    }

    // Helper record to match the anonymous type
    public record DashboardStats(int EquiposCount, int RutinasCount, int OrdenesActivasCount);
}
