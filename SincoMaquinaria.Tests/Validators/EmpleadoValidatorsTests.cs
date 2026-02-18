using FluentValidation.TestHelper;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Validators;
using Xunit;

namespace SincoMaquinaria.Tests.Validators;

public class CrearEmpleadoRequestValidatorTests
{
    private readonly CrearEmpleadoRequestValidator _validator;

    public CrearEmpleadoRequestValidatorTests()
    {
        _validator = new CrearEmpleadoRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new CrearEmpleadoRequest(
            Nombre: "Juan Pérez",
            Identificacion: "1234567890",
            Cargo: "Conductor",
            Especialidad: "Manejo de maquinaria pesada",
            ValorHora: 15000,
            Estado: "Activo"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Conductor")]
    [InlineData("Operario")]
    [InlineData("Mecanico")]
    public void Validate_WithValidCargo_ShouldPass(string cargo)
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", cargo, null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Cargo);
    }

    [Theory]
    [InlineData("Activo")]
    [InlineData("Inactivo")]
    public void Validate_WithValidEstado_ShouldPass(string estado)
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", "Conductor", null, 10000, estado);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Estado);
    }

    [Fact]
    public void Validate_WithEmptyNombre_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CrearEmpleadoRequest("", "123", "Conductor", null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nombre)
            .WithErrorMessage("El nombre es requerido");
    }

    [Fact]
    public void Validate_WithNombreExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 201);
        var request = new CrearEmpleadoRequest(longName, "123", "Conductor", null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nombre)
            .WithErrorMessage("El nombre no puede exceder 200 caracteres");
    }

    [Fact]
    public void Validate_WithEmptyIdentificacion_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "", "Conductor", null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Identificacion)
            .WithErrorMessage("La identificación es requerida");
    }

    [Fact]
    public void Validate_WithIdentificacionExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longId = new string('1', 51);
        var request = new CrearEmpleadoRequest("Test", longId, "Conductor", null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Identificacion)
            .WithErrorMessage("La identificación no puede exceder 50 caracteres");
    }

    [Fact]
    public void Validate_WithEmptyCargo_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", "", null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cargo)
            .WithErrorMessage("El cargo es requerido");
    }

    [Theory]
    [InlineData("InvalidCargo")]
    [InlineData("Admin")]
    [InlineData("conductor")] // Case sensitive
    public void Validate_WithInvalidCargo_ShouldHaveValidationError(string cargo)
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", cargo, null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cargo)
            .WithErrorMessage("El cargo debe ser uno de: Conductor, Operario, Mecanico");
    }

    [Fact]
    public void Validate_WithEspecialidadExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longEsp = new string('A', 101);
        var request = new CrearEmpleadoRequest("Test", "123", "Conductor", longEsp, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Especialidad)
            .WithErrorMessage("La especialidad no puede exceder 100 caracteres");
    }

    [Fact]
    public void Validate_WithNullEspecialidad_ShouldPass()
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", "Conductor", null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Especialidad);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public void Validate_WithNegativeValorHora_ShouldHaveValidationError(decimal valorHora)
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", "Conductor", null, valorHora, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ValorHora)
            .WithErrorMessage("El valor por hora debe ser mayor o igual a 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10000)]
    [InlineData(50000.50)]
    public void Validate_WithValidValorHora_ShouldPass(decimal valorHora)
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", "Conductor", null, valorHora, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ValorHora);
    }

    [Fact]
    public void Validate_WithEmptyEstado_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", "Conductor", null, 10000, "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Estado)
            .WithErrorMessage("El estado es requerido");
    }

    [Theory]
    [InlineData("Pendiente")]
    [InlineData("activo")] // Case sensitive
    [InlineData("ACTIVO")]
    public void Validate_WithInvalidEstado_ShouldHaveValidationError(string estado)
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Test", "123", "Conductor", null, 10000, estado);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Estado)
            .WithErrorMessage("El estado debe ser uno de: Activo, Inactivo");
    }

    [Fact]
    public void Validate_WithBoundaryValues_ShouldPass()
    {
        // Arrange - Max length values
        var maxNombre = new string('A', 200);
        var maxId = new string('1', 50);
        var maxEsp = new string('B', 100);
        var request = new CrearEmpleadoRequest(maxNombre, maxId, "Conductor", maxEsp, 0, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var request = new CrearEmpleadoRequest("", "", "InvalidCargo", null, -10, "InvalidEstado");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nombre);
        result.ShouldHaveValidationErrorFor(x => x.Identificacion);
        result.ShouldHaveValidationErrorFor(x => x.Cargo);
        result.ShouldHaveValidationErrorFor(x => x.ValorHora);
        result.ShouldHaveValidationErrorFor(x => x.Estado);
    }
}

public class ActualizarEmpleadoRequestValidatorTests
{
    private readonly ActualizarEmpleadoRequestValidator _validator;

    public ActualizarEmpleadoRequestValidatorTests()
    {
        _validator = new ActualizarEmpleadoRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest(
            Nombre: "Juan Pérez Updated",
            Identificacion: "9876543210",
            Cargo: "Mecanico",
            Especialidad: "Reparación de motores",
            ValorHora: 20000,
            Estado: "Inactivo"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyNombre_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("", "123", "Conductor", null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nombre)
            .WithErrorMessage("El nombre es requerido");
    }

    [Theory]
    [InlineData("Conductor")]
    [InlineData("Operario")]
    [InlineData("Mecanico")]
    public void Validate_WithValidCargo_ShouldPass(string cargo)
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Test", "123", cargo, null, 10000, "Activo");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Cargo);
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var longNombre = new string('N', 201);
        var request = new ActualizarEmpleadoRequest(longNombre, "", "Invalid", null, -50, "BadEstado");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Nombre);
        result.ShouldHaveValidationErrorFor(x => x.Identificacion);
        result.ShouldHaveValidationErrorFor(x => x.Cargo);
        result.ShouldHaveValidationErrorFor(x => x.ValorHora);
        result.ShouldHaveValidationErrorFor(x => x.Estado);
    }
}
