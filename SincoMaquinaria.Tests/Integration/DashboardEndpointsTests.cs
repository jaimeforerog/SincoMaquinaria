using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SincoMaquinaria.Domain;
using Marten;
using Xunit;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class DashboardEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DashboardEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ObtenerEstadisticas_ShouldReturnCounts()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed some data using Marten
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Seed Equipment
                session.Store(new Equipo { Id = Guid.NewGuid(), Placa = "EQ-TEST-01", Estado = "Activo" });
                session.Store(new Equipo { Id = Guid.NewGuid(), Placa = "EQ-TEST-02", Estado = "EnMantenimiento" });

                // Seed Rutina
                session.Store(new RutinaMantenimiento { Id = Guid.NewGuid(), Descripcion = "Rutina Test" });

                // Seed Order
                session.Store(new OrdenDeTrabajo { Id = Guid.NewGuid(), Numero = "OT-100", Estado = EstadoOrdenDeTrabajo.EnEjecucion });
                
                await session.SaveChangesAsync();
            }
        }

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
