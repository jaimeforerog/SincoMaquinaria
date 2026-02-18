using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SincoMaquinaria.Services;
using SincoMaquinaria.Services.Jobs;
using SincoMaquinaria.Tests.Helpers;
using Xunit;

namespace SincoMaquinaria.Tests.Services;

public class ImportacionJobHandlerTests : IntegrationContext
{
    private readonly Mock<ILogger<ImportacionJobHandler>> _mockLogger;

    public ImportacionJobHandlerTests(IntegrationFixture fixture) : base(fixture)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        _mockLogger = new Mock<ILogger<ImportacionJobHandler>>();
    }

    private ImportacionJobHandler CreateHandler()
    {
        // Create handler with fresh session - called from each test
        var equipoImportService = new ExcelEquipoImportService(CurrentSession);
        var empleadoImportService = new ExcelEmpleadoImportService(CurrentSession);

        return new ImportacionJobHandler(
            _fixture.Store,
            _mockLogger.Object,
            equipoImportService,
            empleadoImportService);
    }

    #region Helper Methods

    private string CreateTempExcelFile(string fileName, Stream excelStream)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-import-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, fileName);

        using (var fileStream = File.Create(filePath))
        {
            excelStream.CopyTo(fileStream);
        }

        return filePath;
    }

    private void CleanupIfExists(string filePath)
    {
        // Fallback cleanup in case test fails before handler runs
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            var dir = Path.GetDirectoryName(filePath);
            if (dir != null && Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                Directory.Delete(dir);
        }
        catch
        {
            // Ignore
        }
    }

    #endregion

    #region ImportarEquiposAsync Tests

    [Fact]
    public async Task ImportarEquiposAsync_ShouldDeleteTempFileAfterImport()
    {
        // Arrange
        var handler = CreateHandler();
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Placa", $"EQ-{Guid.NewGuid()}" },
                { "Descripcion", "Equipo Test" },
                { "Grupo de mtto", "EXCAVADORAS" },
                { "Rutina", "RUT-001" },
                { "Medidor 1", "HOROMETRO" },
                { "Medidor inicial medidor 1", "100" },
                { "Fecha inicial medidor 1", "01/01/2024" },
                { "Fecha ultima OT", "01/01/2024" }
            }
        };

        using var excelStream = ExcelTestHelper.CreateExcelStream("Equipos", rows);
        var tempFile = CreateTempExcelFile("equipos-cleanup.xlsx", excelStream);

        try
        {
            // Act - This will fail but file should still be cleaned up
            try
            {
                await handler.ImportarEquiposAsync(tempFile, null);
            }
            catch
            {
                // Expected to fail due to missing configuration, but that's OK
            }

            // Assert
            File.Exists(tempFile).Should().BeFalse("el archivo temporal debe ser eliminado");
        }
        finally
        {
            CleanupIfExists(tempFile);
        }
    }

    [Fact]
    public async Task ImportarEquiposAsync_WithEmptyDirectory_ShouldDeleteDirectory()
    {
        // Arrange
        var handler = CreateHandler();
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Placa", $"EQ-{Guid.NewGuid()}" },
                { "Descripcion", "Equipo Test" },
                { "Grupo de mtto", "EXCAVADORAS" },
                { "Rutina", "RUT-001" },
                { "Medidor 1", "HOROMETRO" },
                { "Medidor inicial medidor 1", "100" },
                { "Fecha inicial medidor 1", "01/01/2024" },
                { "Fecha ultima OT", "01/01/2024" }
            }
        };

        using var excelStream = ExcelTestHelper.CreateExcelStream("Equipos", rows);
        var tempFile = CreateTempExcelFile("equipos.xlsx", excelStream);
        var tempDir = Path.GetDirectoryName(tempFile)!;

        try
        {
            // Act
            try
            {
                await handler.ImportarEquiposAsync(tempFile, null);
            }
            catch
            {
                // Expected to fail, but that's OK
            }

            // Assert
            File.Exists(tempFile).Should().BeFalse("el archivo debe ser eliminado");
            Directory.Exists(tempDir).Should().BeFalse("el directorio vacío debe ser eliminado");
        }
        finally
        {
            CleanupIfExists(tempFile);
        }
    }

    [Fact]
    public async Task ImportarEquiposAsync_WithNonExistentFile_ShouldThrowException()
    {
        // Arrange
        var handler = CreateHandler();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"no-existe-{Guid.NewGuid()}.xlsx");

        // Act
        Func<Task> act = async () => await handler.ImportarEquiposAsync(nonExistentFile, null);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    #endregion

    #region ImportarEmpleadosAsync Tests

    [Fact]
    public async Task ImportarEmpleadosAsync_ShouldDeleteTempFileAfterImport()
    {
        // Arrange
        var handler = CreateHandler();
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombres", "Juan" },
                { "Apellidos", "Pérez" },
                { "No. Identificación", $"{Guid.NewGuid()}" },
                { "Cargo", "Operario" },
                { "Valor $ (Hr)", "10.5" }
            }
        };

        using var excelStream = ExcelTestHelper.CreateExcelStream("Empleados", rows);
        var tempFile = CreateTempExcelFile("empleados-cleanup.xlsx", excelStream);

        try
        {
            // Act
            try
            {
                await handler.ImportarEmpleadosAsync(tempFile, null);
            }
            catch
            {
                // May fail but file should still be cleaned up
            }

            // Assert
            File.Exists(tempFile).Should().BeFalse("el archivo temporal debe ser eliminado");
        }
        finally
        {
            CleanupIfExists(tempFile);
        }
    }

    [Fact]
    public async Task ImportarEmpleadosAsync_WithEmptyDirectory_ShouldDeleteDirectory()
    {
        // Arrange
        var handler = CreateHandler();
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Nombres", "María" },
                { "Apellidos", "González" },
                { "No. Identificación", $"{Guid.NewGuid()}" },
                { "Cargo", "Operario" }
            }
        };

        using var excelStream = ExcelTestHelper.CreateExcelStream("Empleados", rows);
        var tempFile = CreateTempExcelFile("empleados.xlsx", excelStream);
        var tempDir = Path.GetDirectoryName(tempFile)!;

        try
        {
            // Act
            try
            {
                await handler.ImportarEmpleadosAsync(tempFile, null);
            }
            catch
            {
                // May fail but cleanup should still happen
            }

            // Assert
            File.Exists(tempFile).Should().BeFalse("el archivo debe ser eliminado");
            Directory.Exists(tempDir).Should().BeFalse("el directorio vacío debe ser eliminado");
        }
        finally
        {
            CleanupIfExists(tempFile);
        }
    }

    [Fact]
    public async Task ImportarEmpleadosAsync_WithNonExistentFile_ShouldThrowException()
    {
        // Arrange
        var handler = CreateHandler();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"no-existe-empleados-{Guid.NewGuid()}.xlsx");

        // Act
        Func<Task> act = async () => await handler.ImportarEmpleadosAsync(nonExistentFile, null);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    #endregion
}
