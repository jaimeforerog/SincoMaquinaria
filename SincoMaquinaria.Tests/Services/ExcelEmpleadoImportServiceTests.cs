using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Marten;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.Domain.Events.Empleado;
using SincoMaquinaria.Services;
using SincoMaquinaria.Tests.Helpers;
using SincoMaquinaria.Tests;
using Xunit;

namespace SincoMaquinaria.Tests.Services;

public class ExcelEmpleadoImportServiceTests : IntegrationContext
{
    private ExcelEmpleadoImportService _service = null!;

    public ExcelEmpleadoImportServiceTests(IntegrationFixture fixture) : base(fixture)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    // Helper to initialize service with current session (since CurrentSession is per-test and set in InitializeAsync which runs AFTER ctor)
    // Wait, InitializeAsync runs *before* test method but *after* constructor.
    // So we can instanciate service in the test method or use a helper property.
    // However, CurrentSession is null in Constructor. 
    // We can instantiate service inside the test or create a Lazy/Property.
    
    private ExcelEmpleadoImportService Service => _service ??= new ExcelEmpleadoImportService(CurrentSession);

    [Fact]
    public async Task ImportarEmpleados_ValidData_ShouldImportSuccessfully()
    {
        // Arrange
        // No pre-existing data
        
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombres", "Juan" },
                { "Apellidos", "Perez" },
                { "No. Identificación", "12345678" },
                { "Cargo", "Operario" },
                { "Valor $ (Hr)", "10.5" }
            }
        };
        
        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act
        var result = await Service.ImportarEmpleados(stream);

        // Assert
        result.Should().Be(1);
        
        // Verify in DB
        // Debug: Check count first
        var allEmpleados = await CurrentSession.Query<Empleado>().ToListAsync();
        allEmpleados.Should().NotBeEmpty("No employees found in DB after import");
        
        var empleado = allEmpleados.FirstOrDefault(e => e.Identificacion == "12345678");
        empleado.Should().NotBeNull($"Expected to find 12345678 but found: {string.Join(", ", allEmpleados.Select(e => e.Identificacion))}");
        
        empleado.Nombre.Should().Be("Juan Perez");
        empleado.Cargo.Should().Be(CargoEmpleado.Operario);
    }

    [Fact]
    public async Task ImportarEmpleados_MissingName_ShouldThrowException()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombres", "" }, // Missing Name
                { "No. Identificación", "12345" },
                { "Cargo", "Operario" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => Service.ImportarEmpleados(stream));
    }

    [Fact]
    public async Task ImportarEmpleados_MissingDocument_ShouldThrowException()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombres", "Juan" },
                { "No. Identificación", "" }, // Missing Doc
                { "Cargo", "Operario" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => Service.ImportarEmpleados(stream));
    }

    [Fact]
    public async Task ImportarEmpleados_InvalidCargo_ShouldThrowException()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombres", "Juan" },
                { "No. Identificación", "12345" },
                { "Cargo", "Presidente" } // Invalid Cargo
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => Service.ImportarEmpleados(stream));
        ex.Message.Should().Contain("El cargo 'Presidente' no es válido");
    }

    [Fact]
    public async Task ImportarEmpleados_DuplicateInFile_ShouldThrowException()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "Nombres", "Juan" }, { "No. Identificación", "12345" }, { "Cargo", "Operario" } },
            new() { { "Nombres", "Pedro" }, { "No. Identificación", "12345" }, { "Cargo", "Conductor" } } // Duplicate Doc
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => Service.ImportarEmpleados(stream));
        ex.Message.Should().Contain("duplicado en el archivo");
    }

    [Fact]
    public async Task ImportarEmpleados_DuplicateInSystem_ShouldThrowException()
    {
        // Arrange
        // Seed existing employee
        var empId = Guid.NewGuid();
        CurrentSession.Events.StartStream<Empleado>(empId, 
            new EmpleadoCreado(empId, "Old Juan", "99999", "Operario", "", 10, "Activo")
        );
        await SaveChangesAsync();

        var rows = new List<Dictionary<string, object>>
        {
            new() { { "Nombres", "New Juan" }, { "No. Identificación", "99999" }, { "Cargo", "Operario" } }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);
        
        // Ensure query sees the new data (should be automatic with same session, but Clean deletes all docs. Wait.
        // InitializeAsync runs before Test.
        // We seed data inside test.
        // ImportarEmpleados queries DB.
        // If ImportarEmpleados uses same CurrentSession, it should see changes if we SaveChangesAsync() first.
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => Service.ImportarEmpleados(stream));
        ex.Message.Should().Contain("ya existe en el sistema");
    }

    [Fact]
    public async Task ImportarEmpleados_MissingCargoColumn_ShouldDefaultToOperario()
    {
        // Arrange - File without "Cargo" column should default to "Operario"
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombres", "Juan" },
                { "Apellidos", "Default" },
                { "No. Identificación", "DEFAULT123" }
                // No Cargo column
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act
        var result = await Service.ImportarEmpleados(stream);

        // Assert
        result.Should().Be(1);
        var empleado = await CurrentSession.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == "DEFAULT123");
        empleado.Should().NotBeNull();
        empleado!.Cargo.Should().Be(CargoEmpleado.Operario);
    }

    [Fact]
    public async Task ImportarEmpleados_WithEspecialidad_ShouldImportCorrectly()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombres", "Pedro" },
                { "Apellidos", "Gonzalez" },
                { "No. Identificación", "ESP123" },
                { "Cargo", "Mecanico" },
                { "Especialidad", "Motores diesel" },
                { "Valor $ (Hr)", "25.5" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act
        var result = await Service.ImportarEmpleados(stream);

        // Assert
        result.Should().Be(1);
        var empleado = await CurrentSession.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == "ESP123");
        empleado.Should().NotBeNull();
        empleado!.Especialidad.Should().Be("Motores diesel");
        empleado.ValorHora.Should().Be(25.5m);
    }

    [Fact]
    public async Task ImportarEmpleados_AlternativeColumnNames_ShouldParse()
    {
        // Arrange - Using "Nombre" instead of "Nombres"/"Apellidos"
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombre", "Maria Lopez" },
                { "Documento", "ALT456" },
                { "Cargo", "Conductor" },
                { "Valor hora", "18" }
            }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act
        var result = await Service.ImportarEmpleados(stream);

        // Assert
        result.Should().Be(1);
        var empleado = await CurrentSession.Query<Empleado>()
            .FirstOrDefaultAsync(e => e.Identificacion == "ALT456");
        empleado.Should().NotBeNull();
        empleado!.Nombre.Should().Be("Maria Lopez");
        empleado.ValorHora.Should().Be(18m);
    }

    [Fact]
    public async Task ImportarEmpleados_MultipleEmployees_ShouldImportAll()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new() { { "Nombres", "Juan" }, { "No. Identificación", "MULTI1" }, { "Cargo", "Operario" } },
            new() { { "Nombres", "Pedro" }, { "No. Identificación", "MULTI2" }, { "Cargo", "Conductor" } },
            new() { { "Nombres", "Luis" }, { "No. Identificación", "MULTI3" }, { "Cargo", "Mecanico" } }
        };

        using var stream = ExcelTestHelper.CreateExcelStream("Empleados", rows);

        // Act
        var result = await Service.ImportarEmpleados(stream);

        // Assert
        result.Should().Be(3);
        var empleados = await CurrentSession.Query<Empleado>()
            .Where(e => e.Identificacion.StartsWith("MULTI"))
            .ToListAsync();
        empleados.Should().HaveCount(3);
    }
}
