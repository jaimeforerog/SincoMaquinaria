using Xunit;
using FluentAssertions;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
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
}