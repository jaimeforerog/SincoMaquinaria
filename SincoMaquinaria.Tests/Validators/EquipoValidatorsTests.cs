using FluentValidation.TestHelper;
using SincoMaquinaria.DTOs.Requests;
using SincoMaquinaria.Validators;
using Xunit;

namespace SincoMaquinaria.Tests.Validators;

public class ActualizarEquipoRequestValidatorTests
{
    private readonly ActualizarEquipoRequestValidator _validator;

    public ActualizarEquipoRequestValidatorTests()
    {
        _validator = new ActualizarEquipoRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new ActualizarEquipoRequest(
            Descripcion: "Excavadora CAT 320D",
            Marca: "Caterpillar",
            Modelo: "320D",
            Serie: "CAT320D2024001",
            Codigo: "EQ001",
            TipoMedidorId: "Horometro",
            TipoMedidorId2: "Odometro",
            Grupo: "Excavadoras",
            Rutina: "Mantenimiento Preventivo"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyDescripcion_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("", "", "", "", "", "", "", "", "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Descripcion)
            .WithErrorMessage("La descripción es requerida");
    }

    [Fact]
    public void Validate_WithDescripcionExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longDesc = new string('A', 201);
        var request = new ActualizarEquipoRequest(longDesc, "", "", "", "", "", "", "", "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Descripcion)
            .WithErrorMessage("La descripción no puede exceder 200 caracteres");
    }

    [Fact]
    public void Validate_WithMarcaExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longMarca = new string('M', 101);
        var request = new ActualizarEquipoRequest("Test", longMarca, "", "", "", "", "", "", "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Marca)
            .WithErrorMessage("La marca no puede exceder 100 caracteres");
    }

    [Fact]
    public void Validate_WithModeloExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longModelo = new string('M', 101);
        var request = new ActualizarEquipoRequest("Test", "", longModelo, "", "", "", "", "", "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Modelo)
            .WithErrorMessage("El modelo no puede exceder 100 caracteres");
    }

    [Fact]
    public void Validate_WithSerieExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longSerie = new string('S', 101);
        var request = new ActualizarEquipoRequest("Test", "", "", longSerie, "", "", "", "", "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Serie)
            .WithErrorMessage("El número de serie no puede exceder 100 caracteres");
    }

    [Fact]
    public void Validate_WithCodigoExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longCodigo = new string('C', 51);
        var request = new ActualizarEquipoRequest("Test", "", "", "", longCodigo, "", "", "", "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Codigo)
            .WithErrorMessage("El código no puede exceder 50 caracteres");
    }

    [Fact]
    public void Validate_WithGrupoExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longGrupo = new string('G', 101);
        var request = new ActualizarEquipoRequest("Test", "", "", "", "", "", "", longGrupo, "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Grupo)
            .WithErrorMessage("El grupo no puede exceder 100 caracteres");
    }

    [Fact]
    public void Validate_WithRutinaExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longRutina = new string('R', 101);
        var request = new ActualizarEquipoRequest("Test", "", "", "", "", "", "", "", longRutina);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rutina)
            .WithErrorMessage("La rutina no puede exceder 100 caracteres");
    }

    [Fact]
    public void Validate_WithEmptyOptionalFields_ShouldPass()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Test Equipment", "", "", "", "", "", "", "", "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithBoundaryValues_ShouldPass()
    {
        // Arrange
        var maxDesc = new string('D', 200);
        var maxMarca = new string('M', 100);
        var maxModelo = new string('O', 100);
        var maxSerie = new string('S', 100);
        var maxCodigo = new string('C', 50);
        var maxGrupo = new string('G', 100);
        var maxRutina = new string('R', 100);

        var request = new ActualizarEquipoRequest(
            maxDesc, maxMarca, maxModelo, maxSerie, maxCodigo, "", "", maxGrupo, maxRutina);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var longDesc = new string('D', 201);
        var longMarca = new string('M', 101);
        var longModelo = new string('O', 101);

        var request = new ActualizarEquipoRequest(longDesc, longMarca, longModelo, "", "", "", "", "", "");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Descripcion);
        result.ShouldHaveValidationErrorFor(x => x.Marca);
        result.ShouldHaveValidationErrorFor(x => x.Modelo);
    }
}
