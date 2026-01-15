using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.Services;
using SincoMaquinaria.Tests.Helpers;
using Xunit;

namespace SincoMaquinaria.Tests.Services;

public class ExcelImportServiceTests : IntegrationContext
{
    private ExcelImportService _service = null!;

    public ExcelImportServiceTests(IntegrationFixture fixture) : base(fixture)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    private ExcelImportService Service => _service ??= new ExcelImportService(CurrentSession);

    private async Task SetupConfig()
    {
        var configId = ConfiguracionGlobal.SingletonId;
        // Check if exists
        var state = await CurrentSession.Events.FetchStreamStateAsync(configId);
        if (state == null)
        {
            var events = new List<object>
            {
                new TipoMedidorCreado("HR", "Horómetro", "HR"),
                new GrupoMantenimientoCreado("G1", "General", "General", true)
            };
            CurrentSession.Events.StartStream<ConfiguracionGlobal>(configId, events);
            await CurrentSession.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task ImportarRutinas_ValidData_ShouldImportSuccessfully()
    {
        // Arrange
        await SetupConfig();

        // Column order must match ExcelImportService expectations:
        // 1:Grupo, 2:Rutina, 3:Parte, 4:Actividad, 5:Clase, 6:Frecuencia, 7:FrecUM, 8:Alerta, 9:Frec2, 10:FrecUM2, 11:Alerta2, 12:Insumo, 13:Cantidad
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Grupo", "General" },
                { "Rutina", "Rutina Test 1" },
                { "Parte", "Motor" },
                { "Actividad", "Cambio de Aceite" },
                { "Clase", "" },
                { "Frecuencia", "100" },
                { "Frec UM", "HR" },
                { "Alerta", "10" },
                { "Frecuencia2", "" },
                { "Frec UM2", "" },
                { "Alerta2", "" },
                { "Insumo", "Aceite" },
                { "Cantidad", "2" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        var result = await Service.ImportarRutinas(stream);

        // Assert
        result.Should().Be(1);

        // Verify Events were persisted
        var rutinas = await CurrentSession.Events.QueryRawEventDataOnly<RutinaMigrada>().ToListAsync();
        rutinas.Should().ContainSingle(r => r.Descripcion == "Rutina Test 1");

        var partes = await CurrentSession.Events.QueryRawEventDataOnly<ParteDeRutinaMigrada>().ToListAsync();
        partes.Should().ContainSingle(p => p.Descripcion == "Motor");

        var actividades = await CurrentSession.Events.QueryRawEventDataOnly<ActividadDeRutinaMigrada>().ToListAsync();
        actividades.Should().ContainSingle(a => a.Descripcion == "Cambio de Aceite" && a.Frecuencia == 100 && a.UnidadMedida == "HR");
    }
    
    [Fact]
    public async Task ImportarRutinas_DuplicateInFile_ShouldSkipDuplicate()
    {
        // Arrange
        await SetupConfig();

        // Column order: 1:Grupo, 2:Rutina, 3:Parte, 4:Actividad, 5:Clase, 6:Frecuencia, 7:FrecUM, 8:Alerta, 9:Frec2, 10:FrecUM2, 11:Alerta2, 12:Insumo, 13:Cantidad
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Grupo", "General" }, { "Rutina", "Rutina Dup" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frecuencia", "100" }, { "Frec UM", "HR" }, { "Alerta", "" }, { "Frecuencia2", "" }, { "Frec UM2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" }
            },
            new()
            {
                { "Grupo", "General" }, { "Rutina", "Rutina Dup" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frecuencia", "100" }, { "Frec UM", "HR" }, { "Alerta", "" }, { "Frecuencia2", "" }, { "Frec UM2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        // ExcelImportService only counts distinct NEW routines
        var result = await Service.ImportarRutinas(stream);

        // Assert
        result.Should().Be(1, "should import only 1 routine despite 2 rows if they are identical/duplicates logic applies for routines");
    }

    [Fact]
    public async Task ImportarRutinas_IntegerFrequency_ShouldParseCorrectly()
    {
        // Arrange
        await SetupConfig();
        // Column order: 1:Grupo, 2:Rutina, 3:Parte, 4:Actividad, 5:Clase, 6:Frecuencia, 7:FrecUM, 8:Alerta, 9:Frec2, 10:FrecUM2, 11:Alerta2, 12:Insumo, 13:Cantidad
        // Note: ExcelImportService uses int.TryParse which only accepts integer strings
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "Grupo", "General" }, { "Rutina", "Rutina Int1" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frecuencia", "100" }, { "Frec UM", "HR" }, { "Alerta", "10" }, { "Frecuencia2", "" }, { "Frec UM2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" } },
            new() { { "Grupo", "General" }, { "Rutina", "Rutina Int2" }, { "Parte", "P1" }, { "Actividad", "A2" }, { "Clase", "" }, { "Frecuencia", "50" }, { "Frec UM", "HR" }, { "Alerta", "" }, { "Frecuencia2", "" }, { "Frec UM2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" } }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        var result = await Service.ImportarRutinas(stream);

        // Assert
        result.Should().Be(2);

        var acts = await CurrentSession.Events.QueryRawEventDataOnly<ActividadDeRutinaMigrada>().ToListAsync();

        acts.Should().Contain(a => a.Frecuencia == 100 && a.Descripcion == "A1");
        acts.Should().Contain(a => a.Frecuencia == 50 && a.Descripcion == "A2");
    }

    [Fact]
    public async Task ImportarRutinas_InvalidData_ShouldThrowValidationErrors()
    {
        // Arrange
        await SetupConfig();
        // Column order: 1:Grupo, 2:Rutina, 3:Parte, 4:Actividad, 5:Clase, 6:Frecuencia, 7:FrecUM, 8:Alerta, 9:Frec2, 10:FrecUM2, 11:Alerta2, 12:Insumo, 13:Cantidad
        var rows = new List<Dictionary<string, object>>
        {
            // Invalid Group (not existing)
            new() { { "Grupo", "GRUPO_INVALIDO" }, { "Rutina", "Rutina Invalid" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frecuencia", "100" }, { "Frec UM", "HR" }, { "Alerta", "" }, { "Frecuencia2", "" }, { "Frec UM2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" } },
            // Invalid Unit
            new() { { "Grupo", "General" }, { "Rutina", "Rutina Invalid 3" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frecuencia", "100" }, { "Frec UM", "INVALID" }, { "Alerta", "" }, { "Frecuencia2", "" }, { "Frec UM2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" } }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ImportarRutinas(stream));

        // Assert - Service validates Group and Unit existence
        ex.Message.Should().Contain("GRUPO_INVALIDO");
        ex.Message.Should().Contain("INVALID");
    }
}
