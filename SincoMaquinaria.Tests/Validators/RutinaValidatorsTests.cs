using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Validators;
using Xunit;

namespace SincoMaquinaria.Tests.Validators;

public class RutinaValidatorsTests
{
    #region UpdateParteRequestValidator Tests

    [Fact]
    public void UpdateParteRequestValidator_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var validator = new UpdateParteRequestValidator();
        var request = new UpdateParteRequest("Motor principal");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateParteRequestValidator_DescripcionVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new UpdateParteRequestValidator();
        var request = new UpdateParteRequest("");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion" && e.ErrorMessage == "La descripción es requerida");
    }

    [Fact]
    public void UpdateParteRequestValidator_DescripcionMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var validator = new UpdateParteRequestValidator();
        var request = new UpdateParteRequest(new string('A', 201));

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
    }

    #endregion

    #region AddParteRequestValidator Tests

    [Fact]
    public void AddParteRequestValidator_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var validator = new AddParteRequestValidator();
        var request = new AddParteRequest("Sistema hidráulico");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AddParteRequestValidator_DescripcionVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new AddParteRequestValidator();
        var request = new AddParteRequest("");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion" && e.ErrorMessage == "La descripción es requerida");
    }

    [Fact]
    public void AddParteRequestValidator_DescripcionMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var validator = new AddParteRequestValidator();
        var request = new AddParteRequest(new string('A', 201));

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
    }

    #endregion

    #region UpdateActividadRequestValidator Tests

    [Fact]
    public void UpdateActividadRequestValidator_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var validator = new UpdateActividadRequestValidator();
        var request = new UpdateActividadRequest(
            "Cambio de aceite",
            "Mantenimiento",
            1000,
            "km",
            "Kilometraje",
            100,
            2000,
            "horas",
            "Horometro",
            50,
            "Aceite 15W40",
            4.5
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateActividadRequestValidator_DescripcionVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new UpdateActividadRequestValidator();
        var request = new UpdateActividadRequest(
            "",
            "Mantenimiento",
            1000,
            "km",
            "Kilometraje",
            100,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion" && e.ErrorMessage == "La descripción es requerida");
    }

    [Fact]
    public void UpdateActividadRequestValidator_ClaseVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new UpdateActividadRequestValidator();
        var request = new UpdateActividadRequest(
            "Cambio de aceite",
            "",
            1000,
            "km",
            "Kilometraje",
            100,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Clase" && e.ErrorMessage == "La clase de actividad es requerida");
    }

    [Fact]
    public void UpdateActividadRequestValidator_FrecuenciaNegativa_DebeSerInvalido()
    {
        // Arrange
        var validator = new UpdateActividadRequestValidator();
        var request = new UpdateActividadRequest(
            "Cambio de aceite",
            "Mantenimiento",
            -1,
            "km",
            "Kilometraje",
            100,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Frecuencia");
    }

    [Fact]
    public void UpdateActividadRequestValidator_UnidadMedidaVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new UpdateActividadRequestValidator();
        var request = new UpdateActividadRequest(
            "Cambio de aceite",
            "Mantenimiento",
            1000,
            "",
            "Kilometraje",
            100,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UnidadMedida" && e.ErrorMessage == "La unidad de medida es requerida");
    }

    [Fact]
    public void UpdateActividadRequestValidator_CantidadNegativa_DebeSerInvalido()
    {
        // Arrange
        var validator = new UpdateActividadRequestValidator();
        var request = new UpdateActividadRequest(
            "Cambio de aceite",
            "Mantenimiento",
            1000,
            "km",
            "Kilometraje",
            100,
            0,
            "",
            "",
            0,
            null,
            -1
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cantidad");
    }

    #endregion

    #region AddActividadRequestValidator Tests

    [Fact]
    public void AddActividadRequestValidator_ConDatosValidos_DebeSerValido()
    {
        // Arrange
        var validator = new AddActividadRequestValidator();
        var request = new AddActividadRequest(
            "Inspección de frenos",
            "Inspección",
            5000,
            "km",
            "Kilometraje",
            500,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AddActividadRequestValidator_DescripcionVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new AddActividadRequestValidator();
        var request = new AddActividadRequest(
            "",
            "Inspección",
            5000,
            "km",
            "Kilometraje",
            500,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion" && e.ErrorMessage == "La descripción es requerida");
    }

    [Fact]
    public void AddActividadRequestValidator_DescripcionMuyLarga_DebeSerInvalido()
    {
        // Arrange
        var validator = new AddActividadRequestValidator();
        var request = new AddActividadRequest(
            new string('A', 301),
            "Inspección",
            5000,
            "km",
            "Kilometraje",
            500,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descripcion");
    }

    [Fact]
    public void AddActividadRequestValidator_ClaseVacia_DebeSerInvalido()
    {
        // Arrange
        var validator = new AddActividadRequestValidator();
        var request = new AddActividadRequest(
            "Inspección de frenos",
            "",
            5000,
            "km",
            "Kilometraje",
            500,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Clase" && e.ErrorMessage == "La clase de actividad es requerida");
    }

    [Fact]
    public void AddActividadRequestValidator_NombreMedidorVacio_DebeSerInvalido()
    {
        // Arrange
        var validator = new AddActividadRequestValidator();
        var request = new AddActividadRequest(
            "Inspección de frenos",
            "Inspección",
            5000,
            "km",
            "",
            500,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NombreMedidor" && e.ErrorMessage == "El nombre del medidor es requerido");
    }

    [Fact]
    public void AddActividadRequestValidator_AlertaFaltandoNegativa_DebeSerInvalido()
    {
        // Arrange
        var validator = new AddActividadRequestValidator();
        var request = new AddActividadRequest(
            "Inspección de frenos",
            "Inspección",
            5000,
            "km",
            "Kilometraje",
            -1,
            0,
            "",
            "",
            0,
            null,
            0
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AlertaFaltando");
    }

    #endregion
}
