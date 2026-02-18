using System;
using System.Collections.Generic;
using FluentAssertions;
using SincoMaquinaria.Endpoints;
using Xunit;

namespace SincoMaquinaria.Tests.DTOs;

public class AuditoriaDTOTests
{
    [Fact]
    public void AuditDataDto_Constructor_DebeEstablecerPropiedades()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var fechaAccion = DateTimeOffset.UtcNow;

        // Act
        var dto = new AuditDataDto
        {
            UsuarioId = usuarioId,
            UsuarioNombre = "Test User",
            FechaAccion = fechaAccion,
            Detalles = new Dictionary<string, string>
            {
                { "Placa", "ABC-123" },
                { "Descripcion", "Excavadora CAT" }
            }
        };

        // Assert
        dto.UsuarioId.Should().Be(usuarioId);
        dto.UsuarioNombre.Should().Be("Test User");
        dto.FechaAccion.Should().Be(fechaAccion);
        dto.Detalles.Should().HaveCount(2);
        dto.Detalles["Placa"].Should().Be("ABC-123");
    }

    [Fact]
    public void AuditDataDto_PropiedadesOpcionales_DebenSerNulables()
    {
        // Act
        var dto = new AuditDataDto();

        // Assert
        dto.UsuarioId.Should().BeNull();
        dto.UsuarioNombre.Should().BeNull();
        dto.FechaAccion.Should().BeNull();
        dto.Detalles.Should().NotBeNull();
        dto.Detalles.Should().BeEmpty();
    }

    [Fact]
    public void AuditDataDto_Detalles_DebePermitirMultiplesEntradas()
    {
        // Act
        var dto = new AuditDataDto
        {
            Detalles = new Dictionary<string, string>
            {
                { "Campo1", "Valor1" },
                { "Campo2", "Valor2" },
                { "Campo3", "Valor3" }
            }
        };

        // Assert
        dto.Detalles.Should().HaveCount(3);
        dto.Detalles.ContainsKey("Campo1").Should().BeTrue();
        dto.Detalles.ContainsKey("Campo2").Should().BeTrue();
        dto.Detalles.ContainsKey("Campo3").Should().BeTrue();
    }

    [Fact]
    public void AuditoriaEventoDto_Constructor_DebeEstablecerPropiedades()
    {
        // Arrange
        var id = Guid.NewGuid();
        var streamId = Guid.NewGuid();
        var fecha = DateTimeOffset.UtcNow;
        var auditData = new AuditDataDto
        {
            UsuarioId = Guid.NewGuid(),
            UsuarioNombre = "Admin User"
        };

        // Act
        var dto = new AuditoriaEventoDto
        {
            Id = id,
            StreamId = streamId,
            Tipo = "EquipoCreado",
            Modulo = "Equipos",
            Fecha = fecha,
            Version = 1,
            Datos = auditData
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.StreamId.Should().Be(streamId);
        dto.Tipo.Should().Be("EquipoCreado");
        dto.Modulo.Should().Be("Equipos");
        dto.Fecha.Should().Be(fecha);
        dto.Version.Should().Be(1);
        dto.Datos.Should().NotBeNull();
        dto.Datos!.UsuarioNombre.Should().Be("Admin User");
    }

    [Fact]
    public void AuditoriaEventoDto_ConDatosNulos_DebeCrearseCorrectamente()
    {
        // Act
        var dto = new AuditoriaEventoDto
        {
            Id = Guid.NewGuid(),
            StreamId = Guid.NewGuid(),
            Tipo = "ConfiguracionCambiada",
            Modulo = "Configuracion",
            Fecha = DateTimeOffset.UtcNow,
            Version = 1,
            Datos = null
        };

        // Assert
        dto.Tipo.Should().Be("ConfiguracionCambiada");
        dto.Datos.Should().BeNull();
    }

    [Fact]
    public void AuditoriaEventoDto_ConVersionIncremental_DebeMantenerVersion()
    {
        // Act
        var dto = new AuditoriaEventoDto
        {
            Id = Guid.NewGuid(),
            StreamId = Guid.NewGuid(),
            Tipo = "EquipoActualizado",
            Modulo = "Equipos",
            Fecha = DateTimeOffset.UtcNow,
            Version = 5
        };

        // Assert
        dto.Version.Should().Be(5);
    }

    [Fact]
    public void AuditoriaEventoDto_ConDiferentesModulos_DebeAlmacenarCorrectamente()
    {
        // Arrange & Act
        var equiposEvento = new AuditoriaEventoDto
        {
            Id = Guid.NewGuid(),
            StreamId = Guid.NewGuid(),
            Tipo = "EquipoCreado",
            Modulo = "Equipos",
            Fecha = DateTimeOffset.UtcNow,
            Version = 1
        };

        var ordenesEvento = new AuditoriaEventoDto
        {
            Id = Guid.NewGuid(),
            StreamId = Guid.NewGuid(),
            Tipo = "OrdenCreada",
            Modulo = "Ordenes",
            Fecha = DateTimeOffset.UtcNow,
            Version = 1
        };

        // Assert
        equiposEvento.Modulo.Should().Be("Equipos");
        ordenesEvento.Modulo.Should().Be("Ordenes");
    }

    [Fact]
    public void AuditDataDto_DetallesVacios_DebeInicializarse()
    {
        // Act
        var dto = new AuditDataDto
        {
            UsuarioId = Guid.NewGuid(),
            UsuarioNombre = "Test"
        };

        // Assert
        dto.Detalles.Should().NotBeNull();
        dto.Detalles.Should().BeEmpty();
    }
}
