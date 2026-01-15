using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SincoMaquinaria.Domain;
using SincoMaquinaria.DTOs.Requests;
using Marten;
using Xunit;

namespace SincoMaquinaria.Tests.Integration;

[Collection("Integration")]
public class RutinaValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RutinaValidationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CrearRutina_Duplicada_DebeRetornarConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nombreRutina = "Rutina Duplicada Test";

        // 1. Crear la primera vez
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new RutinaMantenimiento { Id = Guid.NewGuid(), Descripcion = nombreRutina, Grupo = "General" });
                await session.SaveChangesAsync();
            }
        }

        // Act - Intentar crear la misma
        var request = new CreateRutinaRequest(nombreRutina, "General");
        var response = await client.PostAsJsonAsync("/rutinas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain($"Ya existe una rutina con el nombre '{nombreRutina}'");
    }

    [Fact]
    public async Task ActualizarRutina_Duplicada_DebeRetornarConflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nombreExistente = "Rutina A";
        var nombreAEditar = "Rutina B";
        Guid idAEditar = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                session.Store(new RutinaMantenimiento { Id = Guid.NewGuid(), Descripcion = nombreExistente, Grupo = "General" });
                session.Store(new RutinaMantenimiento { Id = idAEditar, Descripcion = nombreAEditar, Grupo = "General" });
                await session.SaveChangesAsync();
            }
        }

        // Act - Intentar cambiar nombre B a nombre A
        var request = new UpdateRutinaRequest(nombreExistente, "General");
        var response = await client.PutAsJsonAsync($"/rutinas/{idAEditar}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ImportarRutinas_Duplicada_DebeLanzarExcepcion()
    {
        // Arrange
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        var nombreDuplicado = "Rutina Existente";

        // Seed existing routine
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                // Ensure config exists
                var config = new ConfiguracionGlobal { Id = ConfiguracionGlobal.SingletonId };
                config.GruposMantenimiento.Add(new GrupoMantenimiento { Nombre = "General", Activo = true, Codigo = "GEN" });
                session.Store(config);
                session.Store(new RutinaMantenimiento { Id = Guid.NewGuid(), Descripcion = nombreDuplicado, Grupo = "General" });
                await session.SaveChangesAsync();
            }
        }

        // Create Excel with duplicate
        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Rutinas");
        worksheet.Cells[1, 1].Value = "Grupo";
        worksheet.Cells[1, 2].Value = "Rutina";
        
        // Row 2: Duplicate
        worksheet.Cells[2, 1].Value = "General"; // Ensure Group matches what Import checks (valid groups) - "General" is in default config? Checking...
        // Actually default config has "General"? Let's check ConfigureGlobal.CrearPorDefecto.
        // Assuming "General" might not be there, so let's better use "Mantenimiento General" or query DB.
        // Let's rely on what we just seeded: validGrupos comes from DB.
        
        worksheet.Cells[2, 2].Value = nombreDuplicado;

        var stream = new MemoryStream(package.GetAsByteArray());

        // Act
        using (var scope = _factory.Services.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
            using (var session = store.LightweightSession())
            {
                var service = new SincoMaquinaria.Services.ExcelImportService(session);
                
                // Assert
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                    await service.ImportarRutinas(stream));
                
                exception.Message.Should().Contain($"La Rutina '{nombreDuplicado}' ya existe");
            }
        }
    }
}
