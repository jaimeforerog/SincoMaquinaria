using Xunit;
using FluentAssertions;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using System;

namespace SincoMaquinaria.Tests.Domain;

public class EmpleadoTests
{
    [Fact]
    public void Empleado_Creado_DebeEstablecerPropiedadesCorrectamente()
    {
        // Arrange
        var empleado = new Empleado();
        var empleadoId = Guid.NewGuid();
        var evento = new EmpleadoCreado(
            empleadoId,
            "Juan Pérez",
            "1234567890",
            "Mecanico",
            "Motores Diesel",
            25000m,
            "Activo"
        );

        // Act
        empleado.Apply(evento);

        // Assert
        empleado.Id.Should().Be(empleadoId);
        empleado.Nombre.Should().Be("Juan Pérez");
        empleado.Identificacion.Should().Be("1234567890");
        empleado.Cargo.Should().Be(CargoEmpleado.Mecanico);
        empleado.Especialidad.Should().Be("Motores Diesel");
        empleado.ValorHora.Should().Be(25000m);
        empleado.Estado.Should().Be(EstadoEmpleado.Activo);
    }

    [Fact]
    public void Empleado_Actualizado_DebeModificarPropiedades()
    {
        // Arrange
        var empleado = new Empleado();
        var empleadoId = Guid.NewGuid();

        // Setup initial state
        empleado.Apply(new EmpleadoCreado(
            empleadoId, "Juan Pérez", "1234567890", "Mecanico",
            "Motores", 25000m, "Activo"
        ));

        // Act
        var eventoActualizado = new EmpleadoActualizado(
            empleadoId,
            "Juan Carlos Pérez",
            "1234567890",
            "Operario",
            "Sistemas Hidráulicos",
            35000m,
            "Activo"
        );
        empleado.Apply(eventoActualizado);

        // Assert
        empleado.Id.Should().Be(empleadoId);
        empleado.Nombre.Should().Be("Juan Carlos Pérez");
        empleado.Identificacion.Should().Be("1234567890");
        empleado.Cargo.Should().Be(CargoEmpleado.Operario);
        empleado.Especialidad.Should().Be("Sistemas Hidráulicos");
        empleado.ValorHora.Should().Be(35000m);
        empleado.Estado.Should().Be(EstadoEmpleado.Activo);
    }

    [Fact]
    public void Empleado_CreadoSinEspecialidad_DebeAceptarCampoVacio()
    {
        // Arrange
        var empleado = new Empleado();
        var empleadoId = Guid.NewGuid();
        var evento = new EmpleadoCreado(
            empleadoId,
            "Ana López",
            "9876543210",
            "Operario",
            "", // Sin especialidad
            15000m,
            "Activo"
        );

        // Act
        empleado.Apply(evento);

        // Assert
        empleado.Id.Should().Be(empleadoId);
        empleado.Nombre.Should().Be("Ana López");
        empleado.Especialidad.Should().BeEmpty();
        empleado.ValorHora.Should().Be(15000m);
    }

    [Fact]
    public void Empleado_ActualizadoConEstadoInactivo_DebeCambiarEstado()
    {
        // Arrange
        var empleado = new Empleado();
        var empleadoId = Guid.NewGuid();

        empleado.Apply(new EmpleadoCreado(
            empleadoId, "Pedro Gómez", "5555555555", "Conductor",
            "", 20000m, "Activo"
        ));

        // Act
        var eventoActualizado = new EmpleadoActualizado(
            empleadoId,
            "Pedro Gómez",
            "5555555555",
            "Conductor",
            "",
            20000m,
            "Inactivo"
        );
        empleado.Apply(eventoActualizado);

        // Assert
        empleado.Estado.Should().Be(EstadoEmpleado.Inactivo);
    }

    [Fact]
    public void Empleado_EstadoInicial_DebeSerActivo()
    {
        // Arrange & Act
        var empleado = new Empleado();

        // Assert
        empleado.Estado.Should().Be(EstadoEmpleado.Activo);
        empleado.Nombre.Should().BeEmpty();
        empleado.ValorHora.Should().Be(0m);
    }

    [Fact]
    public void Empleado_ConValorHoraCero_DebeAceptarValor()
    {
        // Arrange
        var empleado = new Empleado();
        var empleadoId = Guid.NewGuid();
        var evento = new EmpleadoCreado(
            empleadoId,
            "Practicante Sin Pago",
            "1111111111",
            "Operario",
            "",
            0m,
            "Activo"
        );

        // Act
        empleado.Apply(evento);

        // Assert
        empleado.ValorHora.Should().Be(0m);
        empleado.Cargo.Should().Be(CargoEmpleado.Operario);
    }
}