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

        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Rutina", "Rutina Test 1" },
                { "Grupo", "Grupo A" },
                { "Parte", "Motor" },
                { "Actividad", "Cambio de Aceite" },
                { "Frecuencia", "100" },
                { "Frec UM", "Hr" },
                { "Alerta Faltando", "10" },
                { "Insumo", "1.5" },
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

        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Rutina", "Rutina Dup" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Frecuencia", "100" }, { "Frec UM", "Hr" }
            },
            new()
            {
                { "Rutina", "Rutina Dup" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Frecuencia", "100" }, { "Frec UM", "Hr" }
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
    public async Task ImportarRutinas_FloatingPointFrequency_ShouldParseAndRound()
    {
        // Arrange
        await SetupConfig();
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "Rutina", "Rutina Float" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Frecuencia", "100.0" }, { "Frec UM", "Hr" }, { "Alerta Faltando", "10.0" } },
            new() { { "Rutina", "Rutina Roundup" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Frecuencia", "50.6" }, { "Frec UM", "Hr" } }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        var result = await Service.ImportarRutinas(stream);

        // Assert
        result.Should().Be(2);

        var acts = await CurrentSession.Events.QueryRawEventDataOnly<ActividadDeRutinaMigrada>().ToListAsync();
        
        acts.Should().Contain(a => a.Frecuencia == 100); // 100.0 -> 100
        acts.Should().Contain(a => a.Frecuencia == 51);  // 50.6 -> 51
    }

    [Fact]
    public async Task ImportarRutinas_InvalidData_ShouldThrowValidationErrors()
    {
        // Arrange
        await SetupConfig();
        var rows = new List<Dictionary<string, object>>
        {
            // Missing required fields
            new() { { "Rutina", "Rutina Invalid" }, { "Parte", "" }, { "Actividad", "" }, { "Frecuencia", "" }, { "Frec UM", "" }, { "Alerta Faltando", "" }, { "Insumo", "" }, { "Cantidad", "" }, { "Grupo", "" } },
            // Invalid Frequency
            new() { { "Rutina", "Rutina Invalid 2" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Frecuencia", "-5" }, { "Frec UM", "Hr" }, { "Alerta Faltando", "" }, { "Insumo", "" }, { "Cantidad", "" }, { "Grupo", "" } },
             // Invalid Unit
            new() { { "Rutina", "Rutina Invalid 3" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Frecuencia", "100" }, { "Frec UM", "INVALID" }, { "Alerta Faltando", "" }, { "Insumo", "" }, { "Cantidad", "" }, { "Grupo", "" } }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        var ex = await Assert.ThrowsAsync<Exception>(() => Service.ImportarRutinas(stream));

        // Assert
        ex.Message.Should().Contain("Parte' es obligatorio");
        ex.Message.Should().Contain("Actividad' es obligatorio");
        ex.Message.Should().Contain("mayor a 0");
        ex.Message.Should().Contain("INVALID' no es válida"); // Adjust depending on actual error message in service
    }
}
