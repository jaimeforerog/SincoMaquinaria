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
using SincoMaquinaria.Tests;
using Xunit;

namespace SincoMaquinaria.Tests.Services;

public class ExcelEquipoImportServiceTests : IntegrationContext
{
    private ExcelEquipoImportService _service = null!;

    public ExcelEquipoImportServiceTests(IntegrationFixture fixture) : base(fixture)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }
    
    private ExcelEquipoImportService Service => _service ??= new ExcelEquipoImportService(CurrentSession);

    private async Task SetupConfig()
    {
        // Check if stream already exists to avoid collision across tests in same fixture
        var state = await CurrentSession.Events.FetchStreamStateAsync(ConfiguracionGlobal.SingletonId);
        if (state != null)
        {
            return; // Already setup, skip
        }
        
        var events = new List<object>
        {
            new TipoMedidorCreado("HR", "Horómetro", "HR"),
            new TipoMedidorCreado("KM", "Odómetro", "KM"),
            new GrupoMantenimientoCreado("G1", "Excavadoras", "G1", true)
        };
        
        // Use StartStream with the SingletonId
        CurrentSession.Events.StartStream<ConfiguracionGlobal>(ConfiguracionGlobal.SingletonId, events);
        await SaveChangesAsync();
    }

    private async Task SetupRutinas()
    {
        var rutinaId = Guid.NewGuid();
        // Since Validation Logic queries the RutinaMantenimiento document,
        // we must ensure it is created via events if it is a Projection.
        // Use "Excavadoras" as group to match the test data

        var rutinaEvent = new RutinaMigrada(rutinaId, "Rutina Standard", "Excavadoras");

        CurrentSession.Events.StartStream<RutinaMantenimiento>(rutinaId, rutinaEvent);
        await SaveChangesAsync();
    }

    [Fact]
    public async Task ImportarEquipos_ValidData_ShouldImportSuccessfully()
    {
        // Arrange
        await SetupConfig();
        await SetupRutinas();
        
        var rows = new List<Dictionary<string, object>>
        {
            new() { 
                { "Placa", "ABC-123" }, 
                { "Descripcion", "Excavadora 1" }, 
                { "Grupo", "Excavadoras" }, 
                { "Rutina", "Rutina Standard" }, 
                { "Medidor 1", "HR" },
                { "Medidor inicial medidor 1", "0" },
                { "Fecha inicial medidor 1", "2023-01-01" },
                { "Fecha ultima OT", "2023-01-01" }
            }
        };
        
        using var stream = ExcelTestHelper.CreateExcelStream("Equipos", rows);

        // Act
        var result = await Service.ImportarEquipos(stream);
        await SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        var events = await CurrentSession.Events.QueryAllRawEvents().ToListAsync();
        events.Should().Contain(e => e.EventTypeName == "equipo_migrado");
    }

    [Fact]
    public async Task ImportarEquipos_MissingHeader_ShouldThrowException()
    {
        // Arrange - Create stream WITHOUT "Placa" header column
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "Nombre", "ABC-123" } } // Wrong header
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Equipos", rows);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => Service.ImportarEquipos(stream));
        ex.Message.Should().Contain("No se encontró la fila de encabezados");
    }

    [Fact]
    public async Task ImportarEquipos_MissingPlacaValue_ShouldSkipRow()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "Placa", "" }, { "Descripcion", "Test" } } // Empty Placa
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Equipos", rows);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => Service.ImportarEquipos(stream));
        ex.Message.Should().Contain("No se procesó ningún equipo");
    }

    [Fact]
    public async Task ImportarEquipos_InvalidMedidor_ShouldThrowException()
    {
        // Arrange
        await SetupConfig();
        var rows = new List<Dictionary<string, object>>
        {
            new() 
            { 
                { "Placa", "ABC-123" }, 
                { "Descripcion", "Desc" }, 
                { "Grupo", "Excavadoras" },
                { "Rutina", "Rutina Standard" },
                { "Medidor 1", "INVALIDO" },
                { "Medidor inicial medidor 1", "10" },
                { "Fecha inicial medidor 1", "2023-01-01" },
                { "Fecha ultima OT", "2023-01-01" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Equipos", rows);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => Service.ImportarEquipos(stream));
        ex.Message.Should().Contain("Medidor 1 'INVALIDO' no válido");
    }

    [Fact]
    public async Task ImportarEquipos_ExistingEquipo_ShouldUpdate()
    {
        // Arrange
        await SetupConfig();
        await SetupRutinas();

        // Simulate existing equipo by creating it first
        var rows = new List<Dictionary<string, object>>
        {
             new() { 
                { "Placa", "ABC-123" }, 
                { "Descripcion", "Old Desc" }, 
                { "Grupo", "Excavadoras" }, 
                { "Rutina", "Rutina Standard" }, 
                { "Medidor 1", "HR" },
                { "Medidor inicial medidor 1", "0" },
                { "Fecha inicial medidor 1", "2023-01-01" },
                { "Fecha ultima OT", "2023-01-01" }
             }
        };
        using var stream1 = ExcelTestHelper.CreateExcelStream("Equipos", rows);
        await Service.ImportarEquipos(stream1);
        await SaveChangesAsync(); // Ensure committed

        // Now update
         var rowsUpdate = new List<Dictionary<string, object>>
        {
             new() { 
                { "Placa", "ABC-123" }, 
                { "Descripcion", "New Desc" }, 
                { "Grupo", "Excavadoras" }, 
                { "Rutina", "Rutina Standard" }, 
                { "Medidor 1", "HR" },
                { "Medidor inicial medidor 1", "0" },
                { "Fecha inicial medidor 1", "2023-01-01" },
                { "Fecha ultima OT", "2023-01-01" }
             }
        };
        using var stream2 = ExcelTestHelper.CreateExcelStream("Equipos", rowsUpdate);

        // Act - USE NEW SESSION
        await using var session2 = _fixture.Store.LightweightSession();
        var service2 = new ExcelEquipoImportService(session2);
        
        var result = await service2.ImportarEquipos(stream2);
        await session2.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        // Verify events: Should see equipo_migrado AND equipo_actualizado
        var events = await session2.Events.QueryAllRawEvents().ToListAsync();
        events.Should().Contain(e => e.EventTypeName == "equipo_migrado");
        events.Should().Contain(e => e.EventTypeName == "equipo_actualizado");
        events.Count(e => e.EventTypeName == "equipo_actualizado").Should().Be(1);
    }

    [Fact]
    public async Task ImportarEquipos_WithInitialReadings_ShouldEmitMedicionRegistrada()
    {
        // Arrange - This test covers ProcesarLecturaInicial with valid date parsing
        await SetupConfig();
        await SetupRutinas();
        
        var rows = new List<Dictionary<string, object>>
        {
            new() 
            { 
                { "Placa", "XYZ-789" }, 
                { "Descripcion", "Retroexcavadora" }, 
                { "Grupo", "Excavadoras" }, 
                { "Rutina", "Rutina Standard" }, 
                { "Medidor 1", "HR" },
                { "Medidor inicial medidor 1", "150.5" },
                { "Fecha inicial medidor 1", "2025-12-01" },
                { "Fecha ultima OT", "2025-12-01" }
            }
        };
        
        using var stream = ExcelTestHelper.CreateExcelStream("Equipos", rows);

        // Act
        var result = await Service.ImportarEquipos(stream);
        await SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        // Verify MedicionRegistrada event was emitted
        var events = await CurrentSession.Events.QueryAllRawEvents().ToListAsync();
        events.Should().Contain(e => e.EventTypeName == "equipo_migrado");
        events.Should().Contain(e => e.EventTypeName == "medicion_registrada");
    }

    [Fact]
    public async Task ImportarEquipos_MedidorByName_ShouldResolveCorrectly()
    {
        // Arrange - This test covers lookup by meter type NAME (e.g. "Horómetro") instead of unit (e.g. "HR")
        await SetupConfig();
        await SetupRutinas();
        
        var rows = new List<Dictionary<string, object>>
        {
            new() 
            { 
                { "Placa", "DEF-456" }, 
                { "Descripcion", "Volqueta" }, 
                { "Grupo", "Excavadoras" }, 
                { "Rutina", "Rutina Standard" }, 
                { "Medidor 1", "Horómetro" }, // Name instead of code
                { "Medidor 2", "Odómetro" },    // Name instead of code
                { "Medidor inicial medidor 1", "0" },
                { "Fecha inicial medidor 1", "2023-01-01" },
                { "Medidor inicial medidor 2", "0" },
                { "Fecha inicial medidor 2", "2023-01-01" },
                { "Fecha ultima OT", "2023-01-01" }
            }
        };
        
        using var stream = ExcelTestHelper.CreateExcelStream("Equipos", rows);

        // Act
        var result = await Service.ImportarEquipos(stream);
        await SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        var events = await CurrentSession.Events.QueryAllRawEvents().ToListAsync();
        events.Should().Contain(e => e.EventTypeName == "equipo_migrado");
    }
}
