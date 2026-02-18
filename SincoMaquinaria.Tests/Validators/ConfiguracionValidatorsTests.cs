using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Validators;
using Xunit;

namespace SincoMaquinaria.Tests.Validators;

public class ConfiguracionValidatorsTests
{
    #region ActualizarTipoMedidorRequestValidator Tests

    [Fact]
    public void ActualizarTipoMedidorRequestValidator_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var validator = new ActualizarTipoMedidorRequestValidator();
        var request = new ActualizarTipoMedidorRequest("Horometro", "Horas");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ActualizarTipoMedidorRequestValidator_NombreVacio_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarTipoMedidorRequestValidator();
        var request = new ActualizarTipoMedidorRequest("", "Horas");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre" && e.ErrorMessage == "El nombre es requerido");
    }

    [Fact]
    public void ActualizarTipoMedidorRequestValidator_NombreMuyLargo_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarTipoMedidorRequestValidator();
        var request = new ActualizarTipoMedidorRequest(new string('A', 101), "Horas");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Fact]
    public void ActualizarTipoMedidorRequestValidator_UnidadVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarTipoMedidorRequestValidator();
        var request = new ActualizarTipoMedidorRequest("Horometro", "");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Unidad" && e.ErrorMessage == "La unidad es requerida");
    }

    [Fact]
    public void ActualizarTipoMedidorRequestValidator_UnidadMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarTipoMedidorRequestValidator();
        var request = new ActualizarTipoMedidorRequest("Horometro", new string('A', 51));

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Unidad");
    }

    #endregion

    #region ActualizarGrupoRequestValidator Tests

    [Fact]
    public void ActualizarGrupoRequestValidator_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var validator = new ActualizarGrupoRequestValidator();
        var request = new ActualizarGrupoRequest("Grupo Test", "Descripción del grupo");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ActualizarGrupoRequestValidator_NombreVacio_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarGrupoRequestValidator();
        var request = new ActualizarGrupoRequest("", "Descripción");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre" && e.ErrorMessage == "El nombre es requerido");
    }

    [Fact]
    public void ActualizarGrupoRequestValidator_NombreMuyLargo_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarGrupoRequestValidator();
        var request = new ActualizarGrupoRequest(new string('A', 101), "Descripción");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Fact]
    public void ActualizarGrupoRequestValidator_DescripcionMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarGrupoRequestValidator();
        var request = new ActualizarGrupoRequest("Grupo", new string('A', 501));

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
    }

    [Fact]
    public void ActualizarGrupoRequestValidator_DescripcionVacia_DebeSerValido()
    {
        // Arrange
        var validator = new ActualizarGrupoRequestValidator();
        var request = new ActualizarGrupoRequest("Grupo", "");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ActualizarCausaFallaRequestValidator Tests

    [Fact]
    public void ActualizarCausaFallaRequestValidator_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var validator = new ActualizarCausaFallaRequestValidator();
        var request = new ActualizarCausaFallaRequest("Falta de mantenimiento");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ActualizarCausaFallaRequestValidator_DescripcionVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarCausaFallaRequestValidator();
        var request = new ActualizarCausaFallaRequest("");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion" && e.ErrorMessage == "La descripción es requerida");
    }

    [Fact]
    public void ActualizarCausaFallaRequestValidator_DescripcionMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var validator = new ActualizarCausaFallaRequestValidator();
        var request = new ActualizarCausaFallaRequest(new string('A', 201));

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
    }

    #endregion
}
