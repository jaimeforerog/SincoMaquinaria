using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Validators;
using Xunit;

namespace SincoMaquinaria.Tests.Validators;

public class ActualizarEmpleadoValidatorTests
{
    private readonly ActualizarEmpleadoRequestValidator _validator = new();

    [Fact]
    public void ActualizarEmpleadoRequest_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest(
            "Juan Pérez",
            "123456789",
            "Conductor",
            "Transporte pesado",
            15000,
            "Activo"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ActualizarEmpleadoRequest_NombreVacio_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("", "123", "Operario", null, 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre" && e.ErrorMessage.Contains("requerido"));
    }

    [Fact]
    public void ActualizarEmpleadoRequest_NombreMuyLargo_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest(new string('A', 201), "123", "Mecanico", null, 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre" && e.ErrorMessage.Contains("200 caracteres"));
    }

    [Fact]
    public void ActualizarEmpleadoRequest_IdentificacionVacia_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "", "Conductor", null, 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Identificacion" && e.ErrorMessage.Contains("requerida"));
    }

    [Fact]
    public void ActualizarEmpleadoRequest_IdentificacionMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", new string('1', 51), "Operario", null, 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Identificacion" && e.ErrorMessage.Contains("50 caracteres"));
    }

    [Fact]
    public void ActualizarEmpleadoRequest_CargoVacio_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", "", null, 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cargo" && e.ErrorMessage.Contains("requerido"));
    }

    [Fact]
    public void ActualizarEmpleadoRequest_CargoInvalido_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", "CargoInvalido", null, 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cargo" && e.ErrorMessage.Contains("uno de"));
    }

    [Theory]
    [InlineData("Conductor")]
    [InlineData("Operario")]
    [InlineData("Mecanico")]
    public void ActualizarEmpleadoRequest_CargosValidos_DebeSerValido(string cargo)
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", cargo, null, 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ActualizarEmpleadoRequest_EspecialidadMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", "Mecanico", new string('X', 101), 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Especialidad" && e.ErrorMessage.Contains("100 caracteres"));
    }

    [Fact]
    public void ActualizarEmpleadoRequest_ValorHoraNegativo_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", "Conductor", null, -100, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ValorHora" && e.ErrorMessage.Contains("mayor o igual"));
    }

    [Fact]
    public void ActualizarEmpleadoRequest_ValorHoraCero_DebeSerValido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", "Operario", null, 0, "Activo");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ActualizarEmpleadoRequest_EstadoVacio_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", "Conductor", null, 0, "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Estado" && e.ErrorMessage.Contains("requerido"));
    }

    [Fact]
    public void ActualizarEmpleadoRequest_EstadoInvalido_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", "Mecanico", null, 0, "EstadoInvalido");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Estado" && e.ErrorMessage.Contains("uno de"));
    }

    [Theory]
    [InlineData("Activo")]
    [InlineData("Inactivo")]
    public void ActualizarEmpleadoRequest_EstadosValidos_DebeSerValido(string estado)
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Juan", "123", "Conductor", null, 0, estado);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
