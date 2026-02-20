using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.Domain.Events.ConfiguracionGlobal;
using Microsoft.Extensions.Logging.Abstractions;
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

    private ExcelImportService Service => _service ??= new ExcelImportService(CurrentSession, NullLogger<ExcelImportService>.Instance);

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
                { "Frec UM", "HR" },
                { "Frecuencia", "100" },
                { "Alerta", "10" },
                { "Frec UM2", "" },
                { "Frecuencia2", "" },
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
                { "Grupo", "General" }, { "Rutina", "Rutina Dup" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frec UM", "HR" }, { "Frecuencia", "100" }, { "Alerta", "" }, { "Frec UM2", "" }, { "Frecuencia2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" }
            },
            new()
            {
                { "Grupo", "General" }, { "Rutina", "Rutina Dup" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frec UM", "HR" }, { "Frecuencia", "100" }, { "Alerta", "" }, { "Frec UM2", "" }, { "Frecuencia2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" }
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
            new() { { "Grupo", "General" }, { "Rutina", "Rutina Int1" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frec UM", "HR" }, { "Frecuencia", "100" }, { "Alerta", "10" }, { "Frec UM2", "" }, { "Frecuencia2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" } },
            new() { { "Grupo", "General" }, { "Rutina", "Rutina Int2" }, { "Parte", "P1" }, { "Actividad", "A2" }, { "Clase", "" }, { "Frec UM", "HR" }, { "Frecuencia", "50" }, { "Alerta", "" }, { "Frec UM2", "" }, { "Frecuencia2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" } }
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
            new() { { "Grupo", "GRUPO_INVALIDO" }, { "Rutina", "Rutina Invalid" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frec UM", "HR" }, { "Frecuencia", "100" }, { "Alerta", "" }, { "Frec UM2", "" }, { "Frecuencia2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" } },
            // Invalid Unit
            new() { { "Grupo", "General" }, { "Rutina", "Rutina Invalid 3" }, { "Parte", "P1" }, { "Actividad", "A1" }, { "Clase", "" }, { "Frec UM", "INVALID" }, { "Frecuencia", "100" }, { "Alerta", "" }, { "Frec UM2", "" }, { "Frecuencia2", "" }, { "Alerta2", "" }, { "Insumo", "" }, { "Cantidad", "" } }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Service.ImportarRutinas(stream));

        // Assert - Service validates Group and Unit existence
        ex.Message.Should().Contain("GRUPO_INVALIDO");
        ex.Message.Should().Contain("INVALID");
    }

    [Fact]
    public async Task ImportarRutinas_FullRow_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        await SetupConfig();

        // User requested order:
        // Grupo | Rutina | Parte | Actividad | Clase Actividad | Frec UM | Frecuencia | Alerta Faltando | Frec UM II | Frecuencia II | Alerta Faltando II | Insumo | Cantidad
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Grupo", "General" },
                { "Rutina", "Rutina Completa" },
                { "Parte", "Parte A" },
                { "Actividad", "Actividad Full" },
                { "Clase", "Mecánica" }, // Clase Actividad
                { "Frec UM", "HR" },
                { "Frecuencia", "500" },
                { "Alerta", "50" },      // Alerta Faltando
                { "Frec UM II", "HR" },
                { "Frecuencia2", "1000" }, // Frecuencia II
                { "Alerta2", "100" },      // Alerta Faltando II
                { "Insumo", "Filtro" },
                { "Cantidad", "1" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        var result = await Service.ImportarRutinas(stream);

        // Assert
        result.Should().Be(1);

        var actividades = await CurrentSession.Events.QueryRawEventDataOnly<ActividadDeRutinaMigrada>().ToListAsync();
        var act = actividades.Should().ContainSingle(a => a.Descripcion == "Actividad Full").Subject;

        act.Clase.Should().Be("Mecánica");
        act.UnidadMedida.Should().Be("HR");
        act.Frecuencia.Should().Be(500);
        act.AlertaFaltando.Should().Be(50);
        act.UnidadMedida2.Should().Be("HR"); // Frec UM II
        act.Frecuencia2.Should().Be(1000);   // Frecuencia2
        act.AlertaFaltando2.Should().Be(100);        // Alerta2
        act.Insumo.Should().Be("Filtro");
        act.Cantidad.Should().Be(1);
    }

    [Fact]
    public async Task ImportarRutinas_WithHeaderInRow2_ShouldSkipHeaderAndSucceed()
    {
        // Arrange
        await SetupConfig();

        // Simulate a file where Row 1 is Title (skipped by StartRow=2 naturally?) 
        // OR simply Row 2 contains the Header strings which cause validation errors if processed as data.
        // User says: "Fila 2: El Grupo de Mantenimiento 'Grupo' no existe..."
        // This means the code read "Grupo" from the cell.
        
        var rows = new List<Dictionary<string, object>>
        {
            // Row 2 (effectively, if we treat this list as the rows starting at 2)
            new()
            {
                { "Grupo", "Grupo" }, // Header name as value
                { "Rutina", "Rutina" },
                { "Parte", "Parte" },
                { "Actividad", "Actividad" },
                { "Clase", "Clase Actividad" },
                { "Frec UM", "Frec UM" }, // Invokes "Unit 'Frec UM' not found"
                { "Frecuencia", "Frecuencia" },
                { "Alerta", "Alerta Faltando" },
                { "Frec UM II", "Frec UM II" }, // Invokes "Unit 'Frec UM II' not found"
                { "Frecuencia2", "Frecuencia II" },
                { "Alerta2", "Alerta Faltando II" },
                { "Insumo", "Insumo" },
                { "Cantidad", "Cantidad" }
            },
            // Row 3 - Valid Data
            new()
            {
                { "Grupo", "General" },
                { "Rutina", "Rutina Real" },
                { "Parte", "Parte A" },
                { "Actividad", "Actividad Real" },
                { "Clase", "" },
                { "Frec UM", "HR" },
                { "Frecuencia", "100" },
                { "Alerta", "" },
                { "Frec UM II", "" },
                { "Frecuencia2", "" },
                { "Alerta2", "" },
                { "Insumo", "" },
                { "Cantidad", "" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Rutinas", rows);

        // Act
        // Current behavior: Should throw InvalidOperationException
        // Desired behavior: Should skip the header row and import 1 routine.
        
        // Let's assert what currently happens to confirm repro (failure) or just fix it.
        // I'll assume I want to Fix it, so I assert Success(1). If it fails, I know I reproduced it.
        var result = await Service.ImportarRutinas(stream);

        // Assert
        result.Should().Be(1);
    }
}
