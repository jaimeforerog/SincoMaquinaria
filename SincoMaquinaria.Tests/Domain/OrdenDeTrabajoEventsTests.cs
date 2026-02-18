using System;
using FluentAssertions;
using SincoMaquinaria.Domain.Events.OrdenDeTrabajo;
using Xunit;

namespace SincoMaquinaria.Tests.Domain;

public class OrdenDeTrabajoEventsTests
{
    #region OrdenProgramada Tests

    [Fact]
    public void OrdenProgramada_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaProgramada = DateTime.UtcNow.AddDays(7);
        var duracion = TimeSpan.FromHours(4);
        var usuarioId = Guid.NewGuid();

        // Act
        var evento = new OrdenProgramada(
            fechaProgramada,
            duracion,
            usuarioId,
            "Programador Usuario"
        );

        // Assert
        evento.FechaProgramada.Should().Be(fechaProgramada);
        evento.DuracionEstimada.Should().Be(duracion);
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("Programador Usuario");
    }

    [Fact]
    public void OrdenProgramada_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaProgramada = DateTime.UtcNow.AddDays(3);
        var duracion = TimeSpan.FromHours(2);

        // Act
        var evento = new OrdenProgramada(fechaProgramada, duracion);

        // Assert
        evento.FechaProgramada.Should().Be(fechaProgramada);
        evento.DuracionEstimada.Should().Be(duracion);
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    [Fact]
    public void OrdenProgramada_Deconstruct_DebeExtraerPropiedades()
    {
        // Arrange
        var fechaProgramada = DateTime.UtcNow;
        var duracion = TimeSpan.FromHours(1);
        var usuarioId = Guid.NewGuid();
        var evento = new OrdenProgramada(fechaProgramada, duracion, usuarioId, "Test");

        // Act
        var (fecha, dur, userId, userName) = evento;

        // Assert
        fecha.Should().Be(fechaProgramada);
        dur.Should().Be(duracion);
        userId.Should().Be(usuarioId);
        userName.Should().Be("Test");
    }

    #endregion

    #region OrdenFinalizada Tests

    [Fact]
    public void OrdenFinalizada_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaAprobacion = DateTime.UtcNow;
        var usuarioId = Guid.NewGuid();

        // Act
        var evento = new OrdenFinalizada(
            "EjecucionCompleta",
            "Supervisor Jefe",
            fechaAprobacion,
            usuarioId,
            "Usuario Aprobador"
        );

        // Assert
        evento.EstadoFinal.Should().Be("EjecucionCompleta");
        evento.AprobadoPor.Should().Be("Supervisor Jefe");
        evento.FechaAprobacion.Should().Be(fechaAprobacion);
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("Usuario Aprobador");
    }

    [Fact]
    public void OrdenFinalizada_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange
        var fechaAprobacion = DateTime.UtcNow;

        // Act
        var evento = new OrdenFinalizada(
            "EjecucionCompleta",
            "Supervisor",
            fechaAprobacion
        );

        // Assert
        evento.EstadoFinal.Should().Be("EjecucionCompleta");
        evento.AprobadoPor.Should().Be("Supervisor");
        evento.FechaAprobacion.Should().Be(fechaAprobacion);
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    [Fact]
    public void OrdenFinalizada_Deconstruct_DebeExtraerPropiedades()
    {
        // Arrange
        var fechaAprobacion = DateTime.UtcNow;
        var usuarioId = Guid.NewGuid();
        var evento = new OrdenFinalizada("Completa", "Juan", fechaAprobacion, usuarioId, "Juan Pérez");

        // Act
        var (estado, aprobado, fecha, userId, userName) = evento;

        // Assert
        estado.Should().Be("Completa");
        aprobado.Should().Be("Juan");
        fecha.Should().Be(fechaAprobacion);
        userId.Should().Be(usuarioId);
        userName.Should().Be("Juan Pérez");
    }

    #endregion

    #region OrdenDeTrabajoEliminada Tests

    [Fact]
    public void OrdenDeTrabajoEliminada_ConDatosCompletos_DebeCrearseCorrectamente()
    {
        // Arrange
        var ordenId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        var evento = new OrdenDeTrabajoEliminada(
            ordenId,
            usuarioId,
            "Usuario Eliminador"
        );

        // Assert
        evento.OrdenId.Should().Be(ordenId);
        evento.UsuarioId.Should().Be(usuarioId);
        evento.UsuarioNombre.Should().Be("Usuario Eliminador");
    }

    [Fact]
    public void OrdenDeTrabajoEliminada_SinDatosOpcionales_DebeCrearseCorrectamente()
    {
        // Arrange
        var ordenId = Guid.NewGuid();

        // Act
        var evento = new OrdenDeTrabajoEliminada(ordenId);

        // Assert
        evento.OrdenId.Should().Be(ordenId);
        evento.UsuarioId.Should().BeNull();
        evento.UsuarioNombre.Should().BeNull();
    }

    [Fact]
    public void OrdenDeTrabajoEliminada_Deconstruct_DebeExtraerPropiedades()
    {
        // Arrange
        var ordenId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var evento = new OrdenDeTrabajoEliminada(ordenId, usuarioId, "Admin");

        // Act
        var (oId, userId, userName) = evento;

        // Assert
        oId.Should().Be(ordenId);
        userId.Should().Be(usuarioId);
        userName.Should().Be("Admin");
    }

    #endregion
}
