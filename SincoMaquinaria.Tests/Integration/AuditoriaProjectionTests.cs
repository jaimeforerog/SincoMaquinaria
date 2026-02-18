using System.Net.Http.Json;
using FluentAssertions;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Projections;
using SincoMaquinaria.Tests.Helpers;
using Marten;

namespace SincoMaquinaria.Tests.Integration;

public class AuditoriaProjectionTests : IClassFixture<IntegrationFixture>
{
    private readonly IntegrationFixture _fixture;

    public AuditoriaProjectionTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    // TODO: Fix this test once AuditoriaProjection is working
    /*
    [Fact]
    public async Task CrearEmpleado_DebeGenerarRegistroDeAuditoria()
    {
        // Arrange
        var client = _fixture.CreateClient();
        await _fixture.ResetDatabaseAsync();

        // 1. Login as Admin
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new
        {
            email = "admin@test.com",
            password = "AdminPassword123!"
        });
        loginResponse.EnsureSuccessStatusCode();
        var authData = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authData!.Token);

        // 2. Create Employee
        var nuevoEmpleado = new
        {
            Nombre = "Juan Perez Audit",
            Cargo = "Operario",
            Documento = "123456789",
            Telefono = "555-1234",
            Estado = "Activo",
            ValorHora = 15000
        };

        var response = await client.PostAsJsonAsync("/api/empleados", nuevoEmpleado);
        response.EnsureSuccessStatusCode();

        // Act
        // Allow some time for async projection (though Inline should be immediate, good practice for tests)
        await Task.Delay(500); 

        // Query Audit Log directly from Marten to verify projection
        using var session = _fixture.Store.QuerySession();
        var auditLogs = await session.Query<RegistroAuditoria>()
            .Where(x => x.Modulo == "Empleados" && x.UsuarioNombre == "Admin User")
            .ToListAsync();

        // Assert
        auditLogs.Should().NotBeEmpty();
        var log = auditLogs.First();
        log.TipoEvento.Should().Be("EmpleadoCreado");
        log.Modulo.Should().Be("Empleados");
        log.UsuarioNombre.Should().Be("Admin User");
        log.Detalles.Should().Contain("Juan Perez Audit");
    }
    */
}
