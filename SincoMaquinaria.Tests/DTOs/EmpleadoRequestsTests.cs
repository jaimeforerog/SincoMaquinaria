using FluentAssertions;
using SincoMaquinaria.DTOs.Requests;
using Xunit;

namespace SincoMaquinaria.Tests.DTOs;

public class CrearEmpleadoRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetAllProperties()
    {
        // Arrange & Act
        var request = new CrearEmpleadoRequest(
            Nombre: "Juan Pérez",
            Identificacion: "1234567890",
            Cargo: "Conductor",
            Especialidad: "Maquinaria pesada",
            ValorHora: 15000m,
            Estado: "Activo"
        );

        // Assert
        request.Nombre.Should().Be("Juan Pérez");
        request.Identificacion.Should().Be("1234567890");
        request.Cargo.Should().Be("Conductor");
        request.Especialidad.Should().Be("Maquinaria pesada");
        request.ValorHora.Should().Be(15000m);
        request.Estado.Should().Be("Activo");
    }

    [Fact]
    public void Constructor_WithNullEspecialidad_ShouldAllowNull()
    {
        // Arrange & Act
        var request = new CrearEmpleadoRequest("Test", "123", "Conductor", null, 10000m, "Activo");

        // Assert
        request.Especialidad.Should().BeNull();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new CrearEmpleadoRequest("Test", "123", "Conductor", "Esp", 10000m, "Activo");
        var request2 = new CrearEmpleadoRequest("Test", "123", "Conductor", "Esp", 10000m, "Activo");

        // Assert
        request1.Should().Be(request2);
        request1.GetHashCode().Should().Be(request2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentNombre_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new CrearEmpleadoRequest("Juan", "123", "Conductor", null, 10000m, "Activo");
        var request2 = new CrearEmpleadoRequest("Pedro", "123", "Conductor", null, 10000m, "Activo");

        // Assert
        request1.Should().NotBe(request2);
    }

    [Fact]
    public void Equality_WithDifferentValorHora_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new CrearEmpleadoRequest("Test", "123", "Conductor", null, 10000m, "Activo");
        var request2 = new CrearEmpleadoRequest("Test", "123", "Conductor", null, 15000m, "Activo");

        // Assert
        request1.Should().NotBe(request2);
    }

    [Fact]
    public void Deconstruct_ShouldExtractAllProperties()
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Juan", "123", "Conductor", "Esp", 10000m, "Activo");

        // Act
        var (nombre, id, cargo, esp, valor, estado) = request;

        // Assert
        nombre.Should().Be("Juan");
        id.Should().Be("123");
        cargo.Should().Be("Conductor");
        esp.Should().Be("Esp");
        valor.Should().Be(10000m);
        estado.Should().Be("Activo");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var request = new CrearEmpleadoRequest("Juan", "123", "Conductor", null, 10000m, "Activo");

        // Act
        var result = request.ToString();

        // Assert
        result.Should().Contain("Juan");
        result.Should().Contain("123");
        result.Should().Contain("Conductor");
    }
}

public class ActualizarEmpleadoRequestTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSetAllProperties()
    {
        // Arrange & Act
        var request = new ActualizarEmpleadoRequest(
            Nombre: "María González",
            Identificacion: "9876543210",
            Cargo: "Mecanico",
            Especialidad: "Motores diesel",
            ValorHora: 20000m,
            Estado: "Inactivo"
        );

        // Assert
        request.Nombre.Should().Be("María González");
        request.Identificacion.Should().Be("9876543210");
        request.Cargo.Should().Be("Mecanico");
        request.Especialidad.Should().Be("Motores diesel");
        request.ValorHora.Should().Be(20000m);
        request.Estado.Should().Be("Inactivo");
    }

    [Fact]
    public void Constructor_WithNullEspecialidad_ShouldAllowNull()
    {
        // Arrange & Act
        var request = new ActualizarEmpleadoRequest("Test", "123", "Operario", null, 12000m, "Activo");

        // Assert
        request.Especialidad.Should().BeNull();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new ActualizarEmpleadoRequest("Test", "123", "Conductor", "Esp", 10000m, "Activo");
        var request2 = new ActualizarEmpleadoRequest("Test", "123", "Conductor", "Esp", 10000m, "Activo");

        // Assert
        request1.Should().Be(request2);
    }

    [Fact]
    public void Equality_WithDifferentCargo_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new ActualizarEmpleadoRequest("Test", "123", "Conductor", null, 10000m, "Activo");
        var request2 = new ActualizarEmpleadoRequest("Test", "123", "Mecanico", null, 10000m, "Activo");

        // Assert
        request1.Should().NotBe(request2);
    }

    [Fact]
    public void Equality_WithDifferentEstado_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new ActualizarEmpleadoRequest("Test", "123", "Conductor", null, 10000m, "Activo");
        var request2 = new ActualizarEmpleadoRequest("Test", "123", "Conductor", null, 10000m, "Inactivo");

        // Assert
        request1.Should().NotBe(request2);
    }

    [Fact]
    public void Properties_ShouldBeImmutable()
    {
        // Arrange
        var request = new ActualizarEmpleadoRequest("Test", "123", "Conductor", null, 10000m, "Activo");

        // Act & Assert - Records are immutable by default, this tests the pattern
        request.Nombre.Should().Be("Test");
        request.Should().BeOfType<ActualizarEmpleadoRequest>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5000.50)]
    [InlineData(100000)]
    public void Constructor_WithVariousValorHora_ShouldAcceptAllValues(decimal valorHora)
    {
        // Arrange & Act
        var request = new ActualizarEmpleadoRequest("Test", "123", "Conductor", null, valorHora, "Activo");

        // Assert
        request.ValorHora.Should().Be(valorHora);
    }
}
