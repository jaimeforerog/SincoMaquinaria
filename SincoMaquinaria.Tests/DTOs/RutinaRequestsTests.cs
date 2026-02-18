using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using Xunit;

namespace SincoMaquinaria.Tests.DTOs;

public class CreateRutinaRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetAllProperties()
    {
        // Arrange & Act
        var request = new CreateRutinaRequest(
            Descripcion: "Mantenimiento Preventivo Mensual",
            Grupo: "Excavadoras"
        );

        // Assert
        request.Descripcion.Should().Be("Mantenimiento Preventivo Mensual");
        request.Grupo.Should().Be("Excavadoras");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new CreateRutinaRequest("Test", "Grupo1");
        var request2 = new CreateRutinaRequest("Test", "Grupo1");

        // Assert
        request1.Should().Be(request2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new CreateRutinaRequest("Test1", "Grupo1");
        var request2 = new CreateRutinaRequest("Test2", "Grupo1");

        // Assert
        request1.Should().NotBe(request2);
    }
}

public class UpdateRutinaRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetAllProperties()
    {
        // Arrange & Act
        var request = new UpdateRutinaRequest("Mantenimiento Correctivo", "Volquetas");

        // Assert
        request.Descripcion.Should().Be("Mantenimiento Correctivo");
        request.Grupo.Should().Be("Volquetas");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new UpdateRutinaRequest("Desc", "Grupo");
        var request2 = new UpdateRutinaRequest("Desc", "Grupo");

        // Assert
        request1.Should().Be(request2);
    }
}

public class AddParteRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetDescripcion()
    {
        // Arrange & Act
        var request = new AddParteRequest("Motor principal");

        // Assert
        request.Descripcion.Should().Be("Motor principal");
    }

    [Fact]
    public void Equality_WithSameDescripcion_ShouldBeEqual()
    {
        // Arrange
        var request1 = new AddParteRequest("Sistema hidráulico");
        var request2 = new AddParteRequest("Sistema hidráulico");

        // Assert
        request1.Should().Be(request2);
    }

    [Fact]
    public void Equality_WithDifferentDescripcion_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new AddParteRequest("Motor");
        var request2 = new AddParteRequest("Transmisión");

        // Assert
        request1.Should().NotBe(request2);
    }
}

public class UpdateParteRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetDescripcion()
    {
        // Arrange & Act
        var request = new UpdateParteRequest("Sistema eléctrico");

        // Assert
        request.Descripcion.Should().Be("Sistema eléctrico");
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldAllowEmpty()
    {
        // Arrange & Act
        var request = new UpdateParteRequest("");

        // Assert
        request.Descripcion.Should().BeEmpty();
    }
}

public class AddActividadRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetAllProperties()
    {
        // Arrange & Act
        var request = new AddActividadRequest(
            Descripcion: "Cambio de aceite",
            Clase: "Lubricación",
            Frecuencia: 250,
            UnidadMedida: "horas",
            NombreMedidor: "Horómetro",
            AlertaFaltando: 50,
            Frecuencia2: 6,
            UnidadMedida2: "meses",
            NombreMedidor2: "Calendario",
            AlertaFaltando2: 1,
            Insumo: "Aceite 15W40",
            Cantidad: 15.5
        );

        // Assert
        request.Descripcion.Should().Be("Cambio de aceite");
        request.Clase.Should().Be("Lubricación");
        request.Frecuencia.Should().Be(250);
        request.UnidadMedida.Should().Be("horas");
        request.NombreMedidor.Should().Be("Horómetro");
        request.AlertaFaltando.Should().Be(50);
        request.Frecuencia2.Should().Be(6);
        request.UnidadMedida2.Should().Be("meses");
        request.NombreMedidor2.Should().Be("Calendario");
        request.AlertaFaltando2.Should().Be(1);
        request.Insumo.Should().Be("Aceite 15W40");
        request.Cantidad.Should().Be(15.5);
    }

    [Fact]
    public void Constructor_WithNullInsumo_ShouldAllowNull()
    {
        // Arrange & Act
        var request = new AddActividadRequest(
            "Test", "Clase", 100, "horas", "Horómetro", 10,
            0, "", "", 0, null, 0
        );

        // Assert
        request.Insumo.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Constructor_WithVariousFrecuencias_ShouldAcceptAllValues(int frecuencia)
    {
        // Arrange & Act
        var request = new AddActividadRequest(
            "Test", "Clase", frecuencia, "horas", "Horómetro", 10,
            0, "", "", 0, null, 0
        );

        // Assert
        request.Frecuencia.Should().Be(frecuencia);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.5)]
    [InlineData(100.75)]
    public void Constructor_WithVariousCantidades_ShouldAcceptAllValues(double cantidad)
    {
        // Arrange & Act
        var request = new AddActividadRequest(
            "Test", "Clase", 100, "horas", "Horómetro", 10,
            0, "", "", 0, null, cantidad
        );

        // Assert
        request.Cantidad.Should().Be(cantidad);
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new AddActividadRequest(
            "Test", "Clase", 100, "horas", "Med", 10, 0, "", "", 0, null, 5.0
        );
        var request2 = new AddActividadRequest(
            "Test", "Clase", 100, "horas", "Med", 10, 0, "", "", 0, null, 5.0
        );

        // Assert
        request1.Should().Be(request2);
    }
}

public class UpdateActividadRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetAllProperties()
    {
        // Arrange & Act
        var request = new UpdateActividadRequest(
            Descripcion: "Inspección de frenos",
            Clase: "Seguridad",
            Frecuencia: 500,
            UnidadMedida: "km",
            NombreMedidor: "Odómetro",
            AlertaFaltando: 100,
            Frecuencia2: 3,
            UnidadMedida2: "meses",
            NombreMedidor2: "Calendario",
            AlertaFaltando2: 1,
            Insumo: "Líquido de frenos",
            Cantidad: 2.0
        );

        // Assert
        request.Descripcion.Should().Be("Inspección de frenos");
        request.Clase.Should().Be("Seguridad");
        request.Frecuencia.Should().Be(500);
        request.UnidadMedida.Should().Be("km");
        request.NombreMedidor.Should().Be("Odómetro");
        request.AlertaFaltando.Should().Be(100);
        request.Insumo.Should().Be("Líquido de frenos");
        request.Cantidad.Should().Be(2.0);
    }

    [Fact]
    public void Constructor_WithNullInsumo_ShouldAllowNull()
    {
        // Arrange & Act
        var request = new UpdateActividadRequest(
            "Test", "Clase", 100, "horas", "Med", 10, 0, "", "", 0, null, 0
        );

        // Assert
        request.Insumo.Should().BeNull();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new UpdateActividadRequest(
            "Test", "Clase", 100, "horas", "Med", 10, 0, "", "", 0, "Insumo", 5.0
        );
        var request2 = new UpdateActividadRequest(
            "Test", "Clase", 100, "horas", "Med", 10, 0, "", "", 0, "Insumo", 5.0
        );

        // Assert
        request1.Should().Be(request2);
    }

    [Fact]
    public void Equality_WithDifferentCantidad_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new UpdateActividadRequest(
            "Test", "Clase", 100, "horas", "Med", 10, 0, "", "", 0, null, 5.0
        );
        var request2 = new UpdateActividadRequest(
            "Test", "Clase", 100, "horas", "Med", 10, 0, "", "", 0, null, 10.0
        );

        // Assert
        request1.Should().NotBe(request2);
    }
}
