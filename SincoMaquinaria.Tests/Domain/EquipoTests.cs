using Xunit;
using FluentAssertions;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.Domain.Events.Equipo;
using System;

namespace SincoMaquinaria.Tests.Domain;

public class EquipoTests
{
    [Fact]
    public void Equipo_Migrado_DebeEstablecerPropiedadesCorrectamente()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();
        var evento = new EquipoMigrado(
            equipoId,
            "EQ-001",
            "Excavadora",
            "Caterpillar",
            "320D",
            "SN123456",
            "COD-001",
            "MEDIDOR-01",
            "MEDIDOR-02",
            "GRUPO-01",
            "RUTINA-01"
        );

        // Act
        equipo.Apply(evento);

        // Assert
        equipo.Id.Should().Be(equipoId);
        equipo.Placa.Should().Be("EQ-001");
        equipo.Descripcion.Should().Be("Excavadora");
        equipo.Marca.Should().Be("Caterpillar");
        equipo.Modelo.Should().Be("320D");
        equipo.Serie.Should().Be("SN123456");
        equipo.Codigo.Should().Be("COD-001");
        equipo.TipoMedidorId.Should().Be("MEDIDOR-01");
        equipo.TipoMedidorId2.Should().Be("MEDIDOR-02");
        equipo.Grupo.Should().Be("GRUPO-01");
        equipo.Rutina.Should().Be("RUTINA-01");
        equipo.Estado.Should().Be("Activo");
    }

    [Fact]
    public void Equipo_Actualizado_DebeModificarPropiedades()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();

        // Setup initial state
        equipo.Apply(new EquipoMigrado(
            equipoId, "EQ-001", "Excavadora", "Caterpillar", "320D", "SN123456",
            "COD-001", "MEDIDOR-01", "MEDIDOR-02", "GRUPO-01", "RUTINA-01"
        ));

        // Act
        var eventoActualizado = new EquipoActualizado(
            equipoId,
            "Excavadora Hidráulica",
            "Caterpillar",
            "320D2",
            "SN789012",
            "COD-002",
            "MEDIDOR-01",
            "MEDIDOR-03",
            "GRUPO-02",
            "RUTINA-02"
        );
        equipo.Apply(eventoActualizado);

        // Assert
        equipo.Id.Should().Be(equipoId);
        equipo.Placa.Should().Be("EQ-001"); // No debe cambiar
        equipo.Descripcion.Should().Be("Excavadora Hidráulica");
        equipo.Modelo.Should().Be("320D2");
        equipo.Serie.Should().Be("SN789012");
        equipo.Codigo.Should().Be("COD-002");
        equipo.TipoMedidorId2.Should().Be("MEDIDOR-03");
        equipo.Grupo.Should().Be("GRUPO-02");
        equipo.Rutina.Should().Be("RUTINA-02");
    }

    [Fact]
    public void Equipo_EstadoInicial_DebeSerInactivo()
    {
        // Arrange & Act
        var equipo = new Equipo();

        // Assert
        equipo.Estado.Should().Be("Inactivo");
        equipo.Placa.Should().BeEmpty();
        equipo.Descripcion.Should().BeEmpty();
    }

    [Fact]
    public void Equipo_Migrado_ConDatosMinimos_DebeAceptarCamposVacios()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();
        var evento = new EquipoMigrado(
            equipoId,
            "EQ-002",
            "Grúa",
            "", // Marca vacía
            "", // Modelo vacío
            "", // Serie vacía
            "",
            "",
            "",
            "",
            ""
        );

        // Act
        equipo.Apply(evento);

        // Assert
        equipo.Id.Should().Be(equipoId);
        equipo.Placa.Should().Be("EQ-002");
        equipo.Descripcion.Should().Be("Grúa");
        equipo.Marca.Should().BeEmpty();
        equipo.Estado.Should().Be("Activo");
    }

    [Fact]
    public void Equipo_Creado_DebeEstablecerTodasLasPropiedades()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var fechaCreacion = DateTimeOffset.UtcNow;
        var evento = new EquipoCreado(
            equipoId,
            "EQ-003",
            "Retroexcavadora",
            "JCB",
            "3CX",
            "SN456789",
            "COD-003",
            "MEDIDOR-A",
            "MEDIDOR-B",
            "GRUPO-A",
            "RUTINA-A",
            usuarioId,
            "Juan Pérez",
            fechaCreacion
        );

        // Act
        equipo.Apply(evento);

        // Assert
        equipo.Id.Should().Be(equipoId);
        equipo.Placa.Should().Be("EQ-003");
        equipo.Descripcion.Should().Be("Retroexcavadora");
        equipo.Marca.Should().Be("JCB");
        equipo.Modelo.Should().Be("3CX");
        equipo.Serie.Should().Be("SN456789");
        equipo.Codigo.Should().Be("COD-003");
        equipo.TipoMedidorId.Should().Be("MEDIDOR-A");
        equipo.TipoMedidorId2.Should().Be("MEDIDOR-B");
        equipo.Grupo.Should().Be("GRUPO-A");
        equipo.Rutina.Should().Be("RUTINA-A");
        equipo.Estado.Should().Be("Activo");
        equipo.CreadoPor.Should().Be(usuarioId);
        equipo.CreadoPorNombre.Should().Be("Juan Pérez");
        equipo.FechaCreacion.Should().Be(fechaCreacion);
    }

    [Fact]
    public void Equipo_Creado_ConFechaNull_DebeUsarFechaActual()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var evento = new EquipoCreado(
            equipoId,
            "EQ-004",
            "Montacargas",
            "Toyota",
            "8FG25",
            "",
            "",
            "",
            "",
            "",
            "",
            usuarioId,
            "Admin",
            null // Fecha nula
        );

        var fechaAntes = DateTimeOffset.Now;

        // Act
        equipo.Apply(evento);

        var fechaDespues = DateTimeOffset.Now;

        // Assert
        equipo.Id.Should().Be(equipoId);
        equipo.Placa.Should().Be("EQ-004");
        equipo.Estado.Should().Be("Activo");
        equipo.CreadoPor.Should().Be(usuarioId);
        equipo.FechaCreacion.Should().BeOnOrAfter(fechaAntes);
        equipo.FechaCreacion.Should().BeOnOrBefore(fechaDespues);
    }

    [Fact]
    public void Equipo_Migrado_DebeGuardarAuditoria()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var fechaCreacion = DateTimeOffset.UtcNow.AddDays(-5);
        var evento = new EquipoMigrado(
            equipoId,
            "EQ-005",
            "Camión",
            "Volvo",
            "FH16",
            "",
            "",
            "",
            "",
            "",
            "",
            usuarioId,
            "Sistema Migración",
            fechaCreacion
        );

        // Act
        equipo.Apply(evento);

        // Assert
        equipo.CreadoPor.Should().Be(usuarioId);
        equipo.CreadoPorNombre.Should().Be("Sistema Migración");
        equipo.FechaCreacion.Should().Be(fechaCreacion);
    }

    [Fact]
    public void Equipo_Actualizado_DebeGuardarAuditoriaDeModificacion()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();
        var usuarioCreadorId = Guid.NewGuid();
        var usuarioModificadorId = Guid.NewGuid();

        // Setup initial state
        equipo.Apply(new EquipoCreado(
            equipoId, "EQ-006", "Bulldozer", "Komatsu", "D85", "", "", "", "", "", "",
            usuarioCreadorId, "Creador", DateTimeOffset.UtcNow.AddDays(-10)
        ));

        // Act
        var fechaModificacion = DateTimeOffset.UtcNow;
        var eventoActualizado = new EquipoActualizado(
            equipoId,
            "Bulldozer Grande",
            "Komatsu",
            "D85EX",
            "",
            "",
            "",
            "",
            "",
            "",
            usuarioModificadorId,
            "Modificador",
            fechaModificacion
        );
        equipo.Apply(eventoActualizado);

        // Assert
        equipo.Descripcion.Should().Be("Bulldozer Grande");
        equipo.Modelo.Should().Be("D85EX");
        equipo.ModificadoPor.Should().Be(usuarioModificadorId);
        equipo.ModificadoPorNombre.Should().Be("Modificador");
        equipo.FechaModificacion.Should().Be(fechaModificacion);
        // Los datos de creación no deben cambiar
        equipo.CreadoPor.Should().Be(usuarioCreadorId);
        equipo.CreadoPorNombre.Should().Be("Creador");
    }

    [Fact]
    public void Equipo_Actualizado_ConFechaNull_DebeUsarFechaActual()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();

        equipo.Apply(new EquipoCreado(
            equipoId, "EQ-007", "Tractor", "John Deere", "5075E", "", "", "", "", "", "",
            Guid.NewGuid(), "User", DateTimeOffset.UtcNow
        ));

        var fechaAntes = DateTimeOffset.UtcNow;

        // Act
        var eventoActualizado = new EquipoActualizado(
            equipoId,
            "Tractor Agrícola",
            "John Deere",
            "5075E",
            "",
            "",
            "",
            "",
            "",
            "",
            Guid.NewGuid(),
            "Admin",
            null // Fecha null
        );
        equipo.Apply(eventoActualizado);

        var fechaDespues = DateTimeOffset.UtcNow;

        // Assert
        equipo.Descripcion.Should().Be("Tractor Agrícola");
        equipo.FechaModificacion.Should().NotBeNull();
        equipo.FechaModificacion.Should().BeOnOrAfter(fechaAntes);
        equipo.FechaModificacion.Should().BeOnOrBefore(fechaDespues);
    }

    [Fact]
    public void Equipo_Actualizado_NoDebeCambiarPlaca()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoId = Guid.NewGuid();
        var placaOriginal = "EQ-ORIGINAL";

        equipo.Apply(new EquipoCreado(
            equipoId, placaOriginal, "Equipo Original", "Marca1", "Modelo1", "", "", "", "", "", "",
            Guid.NewGuid(), "User", DateTimeOffset.UtcNow
        ));

        // Act
        var eventoActualizado = new EquipoActualizado(
            equipoId,
            "Equipo Actualizado",
            "Marca2",
            "Modelo2",
            "",
            "",
            "",
            "",
            "",
            ""
        );
        equipo.Apply(eventoActualizado);

        // Assert
        equipo.Placa.Should().Be(placaOriginal); // La placa no debe cambiar
        equipo.Descripcion.Should().Be("Equipo Actualizado");
        equipo.Marca.Should().Be("Marca2");
    }

    [Fact]
    public void Equipo_Actualizado_NoDebeCambiarId()
    {
        // Arrange
        var equipo = new Equipo();
        var equipoIdOriginal = Guid.NewGuid();

        equipo.Apply(new EquipoCreado(
            equipoIdOriginal, "EQ-008", "Equipo Test", "Marca", "Modelo", "", "", "", "", "", "",
            Guid.NewGuid(), "User", DateTimeOffset.UtcNow
        ));

        // Act
        var eventoActualizado = new EquipoActualizado(
            equipoIdOriginal,
            "Equipo Test Actualizado",
            "Marca Nueva",
            "Modelo Nuevo",
            "",
            "",
            "",
            "",
            "",
            ""
        );
        equipo.Apply(eventoActualizado);

        // Assert
        equipo.Id.Should().Be(equipoIdOriginal); // El ID no debe cambiar
    }
}