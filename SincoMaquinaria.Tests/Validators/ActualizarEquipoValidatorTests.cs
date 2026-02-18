using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Validators;
using Xunit;

namespace SincoMaquinaria.Tests.Validators;

public class ActualizarEquipoValidatorTests
{
    private readonly ActualizarEquipoRequestValidator _validator = new();

    [Fact]
    public void ActualizarEquipoRequest_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest(
            "Excavadora CAT 320D",
            "Caterpillar",
            "320D",
            "CAT123456",
            "EQ-001",
            "HR",
            "KM",
            "Excavadoras",
            "Mantenimiento 250h"
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ActualizarEquipoRequest_DescripcionVacia_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("", "", "", "", "", "", "", "", "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion" && e.ErrorMessage.Contains("requerida"));
    }

    [Fact]
    public void ActualizarEquipoRequest_DescripcionMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest(new string('A', 201), "", "", "", "", "", "", "", "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion" && e.ErrorMessage.Contains("200 caracteres"));
    }

    [Fact]
    public void ActualizarEquipoRequest_MarcaMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Equipo Test", new string('M', 101), "", "", "", "", "", "", "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Marca" && e.ErrorMessage.Contains("100 caracteres"));
    }

    [Fact]
    public void ActualizarEquipoRequest_ModeloMuyLargo_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Equipo Test", "", new string('X', 101), "", "", "", "", "", "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Modelo" && e.ErrorMessage.Contains("100 caracteres"));
    }

    [Fact]
    public void ActualizarEquipoRequest_SerieMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Equipo Test", "", "", new string('S', 101), "", "", "", "", "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Serie" && e.ErrorMessage.Contains("100 caracteres"));
    }

    [Fact]
    public void ActualizarEquipoRequest_CodigoMuyLargo_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Equipo Test", "", "", "", new string('C', 51), "", "", "", "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Codigo" && e.ErrorMessage.Contains("50 caracteres"));
    }

    [Fact]
    public void ActualizarEquipoRequest_GrupoMuyLargo_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Equipo Test", "", "", "", "", "", "", new string('G', 101), "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Grupo" && e.ErrorMessage.Contains("100 caracteres"));
    }

    [Fact]
    public void ActualizarEquipoRequest_RutinaMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Equipo Test", "", "", "", "", "", "", "", new string('R', 101));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rutina" && e.ErrorMessage.Contains("100 caracteres"));
    }

    [Fact]
    public void ActualizarEquipoRequest_CamposOpcionales_DebeSerValido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Equipo Minimo", "", "", "", "", "", "", "", "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ActualizarEquipoRequest_TodosCamposEnLimite_DebeSerValido()
    {
        // Arrange
        var request = new ActualizarEquipoRequest(
            new string('D', 200),
            new string('M', 100),
            new string('X', 100),
            new string('S', 100),
            new string('C', 50),
            "",
            "",
            new string('G', 100),
            new string('R', 100)
        );

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
