using System;
using System.Collections.Generic;
using FluentAssertions;
using SincoMaquinaria.Domain;
using Xunit;

namespace SincoMaquinaria.Tests.Domain;

public class EnumExtensionsTests
{
    [Fact]
    public void ToStringValue_ConEstadoOrdenDeTrabajo_DebeRetornarNombre()
    {
        // Act
        var result = EstadoOrdenDeTrabajo.Programada.ToStringValue();

        // Assert
        result.Should().Be("Programada");
    }

    [Fact]
    public void ToStringValue_ConCargoEmpleado_DebeRetornarNombre()
    {
        // Act
        var result = CargoEmpleado.Mecanico.ToStringValue();

        // Assert
        result.Should().Be("Mecanico");
    }

    [Fact]
    public void ToEnum_ConStringValido_DebeConvertirCorrectamente()
    {
        // Act
        var result = "Programada".ToEnum<EstadoOrdenDeTrabajo>();

        // Assert
        result.Should().Be(EstadoOrdenDeTrabajo.Programada);
    }

    [Fact]
    public void ToEnum_ConStringValidoIgnoreCase_DebeConvertirCorrectamente()
    {
        // Act
        var result = "programada".ToEnum<EstadoOrdenDeTrabajo>();

        // Assert
        result.Should().Be(EstadoOrdenDeTrabajo.Programada);
    }

    [Fact]
    public void ToEnum_ConStringValidoUpperCase_DebeConvertirCorrectamente()
    {
        // Act
        var result = "PREVENTIVO".ToEnum<TipoMantenimiento>();

        // Assert
        result.Should().Be(TipoMantenimiento.Preventivo);
    }

    [Fact]
    public void ToEnum_ConStringInvalido_DebeLanzarExcepcion()
    {
        // Act
        Action act = () => "InvalidValue".ToEnum<EstadoOrdenDeTrabajo>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no es un valor válido*");
    }

    [Fact]
    public void ToEnum_ConStringVacio_DebeLanzarExcepcion()
    {
        // Act
        Action act = () => "".ToEnum<CargoEmpleado>();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToEnum_ConCargoEmpleado_DebeConvertirCorrectamente()
    {
        // Act
        var result = "Conductor".ToEnum<CargoEmpleado>();

        // Assert
        result.Should().Be(CargoEmpleado.Conductor);
    }

    [Fact]
    public void IsValidEnum_ConStringValido_DebeRetornarTrue()
    {
        // Act
        var result = "Programada".IsValidEnum<EstadoOrdenDeTrabajo>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidEnum_ConStringValidoIgnoreCase_DebeRetornarTrue()
    {
        // Act
        var result = "programada".IsValidEnum<EstadoOrdenDeTrabajo>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidEnum_ConStringInvalido_DebeRetornarFalse()
    {
        // Act
        var result = "InvalidValue".IsValidEnum<EstadoOrdenDeTrabajo>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidEnum_ConStringVacio_DebeRetornarFalse()
    {
        // Act
        var result = "".IsValidEnum<CargoEmpleado>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidEnum_ConTipoMantenimiento_DebeValidarCorrectamente()
    {
        // Act
        var preventivo = "Preventivo".IsValidEnum<TipoMantenimiento>();
        var correctivo = "Correctivo".IsValidEnum<TipoMantenimiento>();
        var invalido = "Reparacion".IsValidEnum<TipoMantenimiento>();

        // Assert
        preventivo.Should().BeTrue();
        correctivo.Should().BeTrue();
        invalido.Should().BeFalse();
    }

    [Fact]
    public void GetEnumValues_ConEstadoOrdenDeTrabajo_DebeRetornarTodosLosValores()
    {
        // Act
        var result = EnumExtensions.GetEnumValues<EstadoOrdenDeTrabajo>();

        // Assert
        result.Should().HaveCount(6);
        result.Should().Contain("Inexistente");
        result.Should().Contain("Borrador");
        result.Should().Contain("Programada");
        result.Should().Contain("EnEjecucion");
        result.Should().Contain("EjecucionCompleta");
        result.Should().Contain("Eliminada");
    }

    [Fact]
    public void GetEnumValues_ConCargoEmpleado_DebeRetornarTodosLosValores()
    {
        // Act
        var result = EnumExtensions.GetEnumValues<CargoEmpleado>();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("Conductor");
        result.Should().Contain("Operario");
        result.Should().Contain("Mecanico");
    }

    [Fact]
    public void GetEnumValues_ConTipoMantenimiento_DebeRetornarTodosLosValores()
    {
        // Act
        var result = EnumExtensions.GetEnumValues<TipoMantenimiento>();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Preventivo");
        result.Should().Contain("Correctivo");
    }

    [Fact]
    public void GetEnumValues_ConEstadoDetalleOrden_DebeRetornarTodosLosValores()
    {
        // Act
        var result = EnumExtensions.GetEnumValues<EstadoDetalleOrden>();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("Pendiente");
        result.Should().Contain("EnProceso");
        result.Should().Contain("Finalizado");
    }

    [Fact]
    public void GetEnumValues_ConEstadoEmpleado_DebeRetornarTodosLosValores()
    {
        // Act
        var result = EnumExtensions.GetEnumValues<EstadoEmpleado>();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("Activo");
        result.Should().Contain("Inactivo");
    }

    [Fact]
    public void ToEnum_ConExcepcion_DebeMostrarValoresValidos()
    {
        // Act
        Action act = () => "Invalid".ToEnum<TipoMantenimiento>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Valores válidos: Preventivo, Correctivo*");
    }
}
