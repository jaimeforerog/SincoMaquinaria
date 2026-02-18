using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using Xunit;

namespace SincoMaquinaria.Tests.DTOs;

public class CrearEquipoRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetAllProperties()
    {
        // Arrange & Act
        var request = new CrearEquipoRequest(
            Placa: "ABC123",
            Descripcion: "Excavadora CAT 320D",
            Marca: "Caterpillar",
            Modelo: "320D",
            Serie: "CAT320D001",
            Codigo: "EQ001",
            TipoMedidorId: "Horometro",
            TipoMedidorId2: "Odometro",
            Grupo: "Excavadoras",
            Rutina: "Preventivo",
            LecturaInicial1: 1000m,
            FechaInicial1: new DateTime(2024, 1, 1),
            LecturaInicial2: 5000m,
            FechaInicial2: new DateTime(2024, 1, 1)
        );

        // Assert
        request.Placa.Should().Be("ABC123");
        request.Descripcion.Should().Be("Excavadora CAT 320D");
        request.Marca.Should().Be("Caterpillar");
        request.Modelo.Should().Be("320D");
        request.Serie.Should().Be("CAT320D001");
        request.Codigo.Should().Be("EQ001");
        request.TipoMedidorId.Should().Be("Horometro");
        request.TipoMedidorId2.Should().Be("Odometro");
        request.Grupo.Should().Be("Excavadoras");
        request.Rutina.Should().Be("Preventivo");
        request.LecturaInicial1.Should().Be(1000m);
        request.FechaInicial1.Should().Be(new DateTime(2024, 1, 1));
        request.LecturaInicial2.Should().Be(5000m);
        request.FechaInicial2.Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public void Constructor_WithNullOptionalFields_ShouldAllowNulls()
    {
        // Arrange & Act
        var request = new CrearEquipoRequest(
            "ABC123", "Test", "Marca", "Modelo", "Serie", "Codigo",
            "Med1", "Med2", "Grupo", "Rutina",
            null, null, null, null
        );

        // Assert
        request.LecturaInicial1.Should().BeNull();
        request.FechaInicial1.Should().BeNull();
        request.LecturaInicial2.Should().BeNull();
        request.FechaInicial2.Should().BeNull();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new CrearEquipoRequest(
            "ABC123", "Desc", "Marca", "Modelo", "Serie", "Codigo",
            "Med1", "Med2", "Grupo", "Rutina", 100m, null, null, null
        );
        var request2 = new CrearEquipoRequest(
            "ABC123", "Desc", "Marca", "Modelo", "Serie", "Codigo",
            "Med1", "Med2", "Grupo", "Rutina", 100m, null, null, null
        );

        // Assert
        request1.Should().Be(request2);
    }

    [Fact]
    public void Equality_WithDifferentPlaca_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new CrearEquipoRequest(
            "ABC123", "Desc", "", "", "", "", "", "", "", "", null, null, null, null
        );
        var request2 = new CrearEquipoRequest(
            "XYZ789", "Desc", "", "", "", "", "", "", "", "", null, null, null, null
        );

        // Assert
        request1.Should().NotBe(request2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000.5)]
    [InlineData(999999)]
    public void Constructor_WithVariousLecturaInicial_ShouldAcceptAllValues(decimal lectura)
    {
        // Arrange & Act
        var request = new CrearEquipoRequest(
            "ABC", "Desc", "", "", "", "", "", "", "", "",
            lectura, null, null, null
        );

        // Assert
        request.LecturaInicial1.Should().Be(lectura);
    }
}

public class ActualizarEquipoRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetAllProperties()
    {
        // Arrange & Act
        var request = new ActualizarEquipoRequest(
            Descripcion: "Retroexcavadora JCB",
            Marca: "JCB",
            Modelo: "3CX",
            Serie: "JCB3CX001",
            Codigo: "EQ002",
            TipoMedidorId: "Horometro",
            TipoMedidorId2: "Kilometraje",
            Grupo: "Retroexcavadoras",
            Rutina: "Mensual"
        );

        // Assert
        request.Descripcion.Should().Be("Retroexcavadora JCB");
        request.Marca.Should().Be("JCB");
        request.Modelo.Should().Be("3CX");
        request.Serie.Should().Be("JCB3CX001");
        request.Codigo.Should().Be("EQ002");
        request.TipoMedidorId.Should().Be("Horometro");
        request.TipoMedidorId2.Should().Be("Kilometraje");
        request.Grupo.Should().Be("Retroexcavadoras");
        request.Rutina.Should().Be("Mensual");
    }

    [Fact]
    public void Constructor_WithEmptyStrings_ShouldAllowEmptyStrings()
    {
        // Arrange & Act
        var request = new ActualizarEquipoRequest("Desc", "", "", "", "", "", "", "", "");

        // Assert
        request.Marca.Should().BeEmpty();
        request.Modelo.Should().BeEmpty();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new ActualizarEquipoRequest("Desc", "Marca", "", "", "", "", "", "", "");
        var request2 = new ActualizarEquipoRequest("Desc", "Marca", "", "", "", "", "", "", "");

        // Assert
        request1.Should().Be(request2);
    }

    [Fact]
    public void Equality_WithDifferentDescripcion_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new ActualizarEquipoRequest("Desc1", "", "", "", "", "", "", "", "");
        var request2 = new ActualizarEquipoRequest("Desc2", "", "", "", "", "", "", "", "");

        // Assert
        request1.Should().NotBe(request2);
    }

    [Fact]
    public void ToString_ShouldContainMainProperties()
    {
        // Arrange
        var request = new ActualizarEquipoRequest("Excavadora", "CAT", "320D", "", "", "", "", "", "");

        // Act
        var result = request.ToString();

        // Assert
        result.Should().Contain("Excavadora");
        result.Should().Contain("CAT");
        result.Should().Contain("320D");
    }
}
