using FluentAssertions;
using SincoMaquinaria.Domain;
using Xunit;

namespace SincoMaquinaria.Tests.Domain;

public class DomainGuardTests
{
    #region NotNullOrEmpty Tests

    [Fact]
    public void NotNullOrEmpty_WithValidString_ShouldNotThrow()
    {
        // Arrange
        var value = "Valid string";

        // Act
        var act = () => DomainGuard.NotNullOrEmpty(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotNullOrEmpty_WithNull_ShouldThrowDomainException()
    {
        // Arrange
        string? value = null;

        // Act
        var act = () => DomainGuard.NotNullOrEmpty(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName es requerido");
    }

    [Fact]
    public void NotNullOrEmpty_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        var act = () => DomainGuard.NotNullOrEmpty(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName es requerido");
    }

    [Fact]
    public void NotNullOrEmpty_WithWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        var value = "   ";

        // Act
        var act = () => DomainGuard.NotNullOrEmpty(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName es requerido");
    }

    [Fact]
    public void NotNullOrEmpty_WithTabsAndNewlines_ShouldThrowDomainException()
    {
        // Arrange
        var value = "\t\n\r";

        // Act
        var act = () => DomainGuard.NotNullOrEmpty(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName es requerido");
    }

    [Fact]
    public void NotNullOrEmpty_WithSingleCharacter_ShouldNotThrow()
    {
        // Arrange
        var value = "A";

        // Act
        var act = () => DomainGuard.NotNullOrEmpty(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region NotEmpty Tests

    [Fact]
    public void NotEmpty_WithValidGuid_ShouldNotThrow()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var act = () => DomainGuard.NotEmpty(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotEmpty_WithEmptyGuid_ShouldThrowDomainException()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var act = () => DomainGuard.NotEmpty(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName es requerido");
    }

    [Fact]
    public void NotEmpty_WithDefaultGuid_ShouldThrowDomainException()
    {
        // Arrange
        var value = default(Guid);

        // Act
        var act = () => DomainGuard.NotEmpty(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName es requerido");
    }

    #endregion

    #region Positive Tests

    [Fact]
    public void Positive_WithPositiveValue_ShouldNotThrow()
    {
        // Arrange
        var value = 10.5m;

        // Act
        var act = () => DomainGuard.Positive(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Positive_WithZero_ShouldNotThrow()
    {
        // Arrange
        var value = 0m;

        // Act
        var act = () => DomainGuard.Positive(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Positive_WithNegativeValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = -5.5m;

        // Act
        var act = () => DomainGuard.Positive(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName debe ser positivo");
    }

    [Fact]
    public void Positive_WithSmallNegativeValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = -0.01m;

        // Act
        var act = () => DomainGuard.Positive(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName debe ser positivo");
    }

    [Fact]
    public void Positive_WithLargePositiveValue_ShouldNotThrow()
    {
        // Arrange
        var value = 999999.99m;

        // Act
        var act = () => DomainGuard.Positive(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region InRange Tests

    [Fact]
    public void InRange_WithValueInRange_ShouldNotThrow()
    {
        // Arrange
        var value = 50;

        // Act
        var act = () => DomainGuard.InRange(value, 0, 100, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_WithValueAtMinimum_ShouldNotThrow()
    {
        // Arrange
        var value = 0;

        // Act
        var act = () => DomainGuard.InRange(value, 0, 100, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_WithValueAtMaximum_ShouldNotThrow()
    {
        // Arrange
        var value = 100;

        // Act
        var act = () => DomainGuard.InRange(value, 0, 100, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_WithValueBelowMinimum_ShouldThrowDomainException()
    {
        // Arrange
        var value = -1;

        // Act
        var act = () => DomainGuard.InRange(value, 0, 100, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName debe estar entre 0 y 100");
    }

    [Fact]
    public void InRange_WithValueAboveMaximum_ShouldThrowDomainException()
    {
        // Arrange
        var value = 101;

        // Act
        var act = () => DomainGuard.InRange(value, 0, 100, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName debe estar entre 0 y 100");
    }

    [Fact]
    public void InRange_WithNegativeRange_ShouldWork()
    {
        // Arrange
        var value = -5;

        // Act
        var act = () => DomainGuard.InRange(value, -10, -1, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_WithSingleValueRange_ShouldWork()
    {
        // Arrange
        var value = 42;

        // Act
        var act1 = () => DomainGuard.InRange(value, 42, 42, "FieldName");
        var act2 = () => DomainGuard.InRange(41, 42, 42, "FieldName");

        // Assert
        act1.Should().NotThrow();
        act2.Should().Throw<DomainException>();
    }

    #endregion

    #region ValidEnum Tests

    [Fact]
    public void ValidEnum_WithValidEnumValue_ShouldNotThrow()
    {
        // Arrange
        var value = "Admin";

        // Act
        var act = () => DomainGuard.ValidEnum<RolUsuario>(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidEnum_WithValidEnumValueLowerCase_ShouldNotThrow()
    {
        // Arrange
        var value = "admin";

        // Act
        var act = () => DomainGuard.ValidEnum<RolUsuario>(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidEnum_WithValidEnumValueUpperCase_ShouldNotThrow()
    {
        // Arrange
        var value = "ADMIN";

        // Act
        var act = () => DomainGuard.ValidEnum<RolUsuario>(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidEnum_WithValidEnumValueMixedCase_ShouldNotThrow()
    {
        // Arrange
        var value = "AdMiN";

        // Act
        var act = () => DomainGuard.ValidEnum<RolUsuario>(value, "FieldName");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidEnum_WithInvalidEnumValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = "InvalidRole";

        // Act
        var act = () => DomainGuard.ValidEnum<RolUsuario>(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName debe ser uno de: Admin, User");
    }

    [Fact]
    public void ValidEnum_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        var act = () => DomainGuard.ValidEnum<RolUsuario>(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName debe ser uno de: Admin, User");
    }

    [Fact]
    public void ValidEnum_WithSpecialCharacters_ShouldThrowDomainException()
    {
        // Arrange
        var value = "Admin@#$";

        // Act
        var act = () => DomainGuard.ValidEnum<RolUsuario>(value, "FieldName");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("FieldName debe ser uno de: Admin, User");
    }

    [Fact]
    public void ValidEnum_WithDifferentEnum_ShouldValidateCorrectly()
    {
        // Arrange
        var validValue = "Activo";
        var invalidValue = "Invalid";

        // Act
        var actValid = () => DomainGuard.ValidEnum<EstadoEmpleado>(validValue, "Estado");
        var actInvalid = () => DomainGuard.ValidEnum<EstadoEmpleado>(invalidValue, "Estado");

        // Assert
        actValid.Should().NotThrow();
        actInvalid.Should().Throw<DomainException>()
            .WithMessage("Estado debe ser uno de: Activo, Inactivo");
    }

    #endregion

    #region Edge Cases and Multiple Field Names

    [Fact]
    public void DomainGuard_ShouldIncludeFieldNameInErrorMessage()
    {
        // Arrange & Act & Assert
        var act1 = () => DomainGuard.NotNullOrEmpty(null, "Nombre");
        var act2 = () => DomainGuard.NotNullOrEmpty(null, "Descripción");

        act1.Should().Throw<DomainException>().WithMessage("Nombre es requerido");
        act2.Should().Throw<DomainException>().WithMessage("Descripción es requerido");
    }

    [Fact]
    public void DomainGuard_AllMethods_ShouldAcceptAnyFieldName()
    {
        // Arrange
        var fieldName = "CustomFieldName";

        // Act & Assert
        var act1 = () => DomainGuard.NotNullOrEmpty(null, fieldName);
        var act2 = () => DomainGuard.NotEmpty(Guid.Empty, fieldName);
        var act3 = () => DomainGuard.Positive(-1m, fieldName);
        var act4 = () => DomainGuard.InRange(200, 0, 100, fieldName);
        var act5 = () => DomainGuard.ValidEnum<RolUsuario>("Invalid", fieldName);

        act1.Should().Throw<DomainException>().WithMessage($"{fieldName} es requerido");
        act2.Should().Throw<DomainException>().WithMessage($"{fieldName} es requerido");
        act3.Should().Throw<DomainException>().WithMessage($"{fieldName} debe ser positivo");
        act4.Should().Throw<DomainException>().WithMessage($"{fieldName} debe estar entre 0 y 100");
        act5.Should().Throw<DomainException>().WithMessage($"{fieldName} debe ser uno de: Admin, User");
    }

    #endregion
}
