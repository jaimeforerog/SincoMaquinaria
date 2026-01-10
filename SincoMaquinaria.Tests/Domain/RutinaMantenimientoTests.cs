using Xunit;
using FluentAssertions;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using System;
using System.Linq;

namespace SincoMaquinaria.Tests.Domain;

public class RutinaMantenimientoTests
{
    [Fact]
    public void RutinaMantenimiento_Migrada_DebeEstablecerPropiedades()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var evento = new RutinaMigrada(rutinaId, "Mantenimiento 500 horas", "Preventivo");

        // Act
        rutina.Apply(evento);

        // Assert
        rutina.Id.Should().Be(rutinaId);
        rutina.Descripcion.Should().Be("Mantenimiento 500 horas");
        rutina.Grupo.Should().Be("Preventivo");
        rutina.Partes.Should().BeEmpty();
    }

    [Fact]
    public void RutinaMantenimiento_ParteAgregada_DebeAgregarParte()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();

        rutina.Apply(new RutinaMigrada(rutinaId, "Rutina Test", "Preventivo"));

        // Act
        var eventoParte = new ParteDeRutinaMigrada(parteId, "Motor", rutinaId);
        rutina.Apply(eventoParte);

        // Assert
        rutina.Partes.Should().HaveCount(1);
        var parte = rutina.Partes[0];
        parte.Id.Should().Be(parteId);
        parte.Descripcion.Should().Be("Motor");
        parte.Actividades.Should().BeEmpty();
    }

    [Fact]
    public void RutinaMantenimiento_ActividadAgregada_DebeAgregarActividadAParte()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        rutina.Apply(new RutinaMigrada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteDeRutinaMigrada(parteId, "Sistema Hidráulico", rutinaId));

        // Act
        var eventoActividad = new ActividadDeRutinaMigrada(
            actividadId,
            "Cambio de aceite hidráulico",
            "Lubricación",
            500, // Frecuencia
            "HRS",
            "Horómetro",
            50, // Alerta
            0, // Frecuencia2
            "",
            "",
            0,
            "Aceite Hidráulico 68",
            4.0, // Cantidad
            parteId
        );
        rutina.Apply(eventoActividad);

        // Assert
        var parte = rutina.Partes.First(p => p.Id == parteId);
        parte.Actividades.Should().HaveCount(1);

        var actividad = parte.Actividades[0];
        actividad.Id.Should().Be(actividadId);
        actividad.Descripcion.Should().Be("Cambio de aceite hidráulico");
        actividad.Clase.Should().Be("Lubricación");
        actividad.Frecuencia.Should().Be(500);
        actividad.UnidadMedida.Should().Be("HRS");
        actividad.AlertaFaltando.Should().Be(50);
        actividad.Insumo.Should().Be("Aceite Hidráulico 68");
        actividad.Cantidad.Should().Be(4.0);
    }

    [Fact]
    public void RutinaMantenimiento_MultiplesPartesYActividades_DebeOrganizarCorrectamente()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parte1Id = Guid.NewGuid();
        var parte2Id = Guid.NewGuid();
        var actividad1Id = Guid.NewGuid();
        var actividad2Id = Guid.NewGuid();

        // Act
        rutina.Apply(new RutinaMigrada(rutinaId, "Mantenimiento Completo", "Preventivo"));
        rutina.Apply(new ParteDeRutinaMigrada(parte1Id, "Motor", rutinaId));
        rutina.Apply(new ParteDeRutinaMigrada(parte2Id, "Transmisión", rutinaId));

        rutina.Apply(new ActividadDeRutinaMigrada(
            actividad1Id, "Cambio aceite motor", "Lubricación",
            250, "HRS", "Horómetro", 25, 0, "", "", 0, "Aceite 15W40", 6.0, parte1Id
        ));

        rutina.Apply(new ActividadDeRutinaMigrada(
            actividad2Id, "Inspección transmisión", "Inspección",
            500, "HRS", "Horómetro", 50, 0, "", "", 0, "", 0, parte2Id
        ));

        // Assert
        rutina.Partes.Should().HaveCount(2);

        var motor = rutina.Partes.First(p => p.Descripcion == "Motor");
        motor.Actividades.Should().HaveCount(1);
        motor.Actividades[0].Descripcion.Should().Be("Cambio aceite motor");

        var transmision = rutina.Partes.First(p => p.Descripcion == "Transmisión");
        transmision.Actividades.Should().HaveCount(1);
        transmision.Actividades[0].Descripcion.Should().Be("Inspección transmisión");
    }

    [Fact]
    public void RutinaMantenimiento_ActividadConFrecuenciaSecundaria_DebeGuardarAmbas()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        rutina.Apply(new RutinaMigrada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteDeRutinaMigrada(parteId, "Sistema", rutinaId));

        // Act - Actividad con dos frecuencias
        var eventoActividad = new ActividadDeRutinaMigrada(
            actividadId,
            "Mantenimiento dual",
            "General",
            500,  // Frecuencia primaria
            "HRS",
            "Horómetro",
            50,
            1000, // Frecuencia secundaria
            "KM",
            "Odómetro",
            100,
            "",
            0,
            parteId
        );
        rutina.Apply(eventoActividad);

        // Assert
        var actividad = rutina.Partes[0].Actividades[0];
        actividad.Frecuencia.Should().Be(500);
        actividad.UnidadMedida.Should().Be("HRS");
        actividad.Frecuencia2.Should().Be(1000);
        actividad.UnidadMedida2.Should().Be("KM");
        actividad.AlertaFaltando2.Should().Be(100);
    }
}