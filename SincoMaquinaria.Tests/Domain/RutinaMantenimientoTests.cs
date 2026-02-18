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

    #region CRUD Operations Tests

    [Fact]
    public void RutinaMantenimiento_Creada_DebeEstablecerPropiedadesIniciales()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var evento = new RutinaCreada(rutinaId, "Nueva Rutina Preventiva", "Excavadoras", userId, "Admin User");
        rutina.Apply(evento);

        // Assert
        rutina.Id.Should().Be(rutinaId);
        rutina.Descripcion.Should().Be("Nueva Rutina Preventiva");
        rutina.Grupo.Should().Be("Excavadoras");
        rutina.Partes.Should().BeEmpty();
    }

    [Fact]
    public void RutinaMantenimiento_Actualizada_DebeModificarDescripcionYGrupo()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        rutina.Apply(new RutinaCreada(rutinaId, "Descripcion Inicial", "Grupo Inicial"));

        // Act
        var evento = new RutinaActualizada(rutinaId, "Descripcion Actualizada", "Grupo Actualizado");
        rutina.Apply(evento);

        // Assert
        rutina.Id.Should().Be(rutinaId);
        rutina.Descripcion.Should().Be("Descripcion Actualizada");
        rutina.Grupo.Should().Be("Grupo Actualizado");
    }

    [Fact]
    public void RutinaMantenimiento_ParteAgregadaConParteAgregada_DebeAgregarNuevaParte()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));

        // Act
        var evento = new ParteAgregada(parteId, "Sistema Eléctrico", rutinaId, userId, "Tech User");
        rutina.Apply(evento);

        // Assert
        rutina.Partes.Should().HaveCount(1);
        var parte = rutina.Partes[0];
        parte.Id.Should().Be(parteId);
        parte.Descripcion.Should().Be("Sistema Eléctrico");
        parte.Actividades.Should().BeEmpty();
    }

    [Fact]
    public void RutinaMantenimiento_ParteActualizada_DebeModificarDescripcion()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parteId, "Descripcion Inicial Parte", rutinaId));

        // Act
        var evento = new ParteActualizada(parteId, "Descripcion Actualizada Parte");
        rutina.Apply(evento);

        // Assert
        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        parte.Should().NotBeNull();
        parte!.Descripcion.Should().Be("Descripcion Actualizada Parte");
    }

    [Fact]
    public void RutinaMantenimiento_ParteActualizada_ParteNoExiste_DebeLanzarDomainException()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteIdExistente = Guid.NewGuid();
        var parteIdNoExistente = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parteIdExistente, "Parte Existente", rutinaId));

        // Act & Assert
        var evento = new ParteActualizada(parteIdNoExistente, "Descripcion Nueva");
        var act = () => rutina.Apply(evento);
        act.Should().Throw<DomainException>().Which.Message.Should().Contain(parteIdNoExistente.ToString());
    }

    [Fact]
    public void RutinaMantenimiento_ParteEliminada_DebeRemoverParte()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parte1Id = Guid.NewGuid();
        var parte2Id = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parte1Id, "Parte 1", rutinaId));
        rutina.Apply(new ParteAgregada(parte2Id, "Parte 2", rutinaId));

        // Act
        var evento = new ParteEliminada(parte1Id);
        rutina.Apply(evento);

        // Assert
        rutina.Partes.Should().HaveCount(1);
        rutina.Partes[0].Id.Should().Be(parte2Id);
        rutina.Partes[0].Descripcion.Should().Be("Parte 2");
    }

    [Fact]
    public void RutinaMantenimiento_ParteEliminada_ConActividades_DebeRemoverTodo()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parteId, "Parte con Actividad", rutinaId));
        rutina.Apply(new ActividadDeRutinaAgregada(
            actividadId, parteId, "Actividad Test", "Lubricación",
            500, "HRS", "Horómetro", 50, 0, "", "", 0, "", 0
        ));

        // Act
        var evento = new ParteEliminada(parteId);
        rutina.Apply(evento);

        // Assert
        rutina.Partes.Should().BeEmpty();
    }

    [Fact]
    public void RutinaMantenimiento_ActividadDeRutinaAgregada_DebeAgregarActividadAParte()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parteId, "Motor", rutinaId));

        // Act
        var evento = new ActividadDeRutinaAgregada(
            actividadId,
            parteId,
            "Cambio de filtro de aceite",
            "Filtración",
            250, // Frecuencia
            "HRS",
            "Horómetro",
            25,
            0,
            "",
            "",
            0,
            "Filtro de aceite",
            1.0
        );
        rutina.Apply(evento);

        // Assert
        var parte = rutina.Partes.FirstOrDefault(p => p.Id == parteId);
        parte.Should().NotBeNull();
        parte!.Actividades.Should().HaveCount(1);

        var actividad = parte.Actividades[0];
        actividad.Id.Should().Be(actividadId);
        actividad.Descripcion.Should().Be("Cambio de filtro de aceite");
        actividad.Clase.Should().Be("Filtración");
        actividad.Frecuencia.Should().Be(250);
        actividad.Insumo.Should().Be("Filtro de aceite");
        actividad.Cantidad.Should().Be(1.0);
    }

    [Fact]
    public void RutinaMantenimiento_ActividadDeRutinaAgregada_ParteNoExiste_DebeLanzarDomainException()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteIdNoExistente = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));

        // Act & Assert
        var evento = new ActividadDeRutinaAgregada(
            actividadId, parteIdNoExistente, "Actividad Test", "Clase",
            100, "HRS", "Horómetro", 10, 0, "", "", 0, "", 0
        );
        var act = () => rutina.Apply(evento);
        act.Should().Throw<DomainException>().Which.Message.Should().Contain(parteIdNoExistente.ToString());
    }

    [Fact]
    public void RutinaMantenimiento_ActividadDeRutinaActualizada_DebeModificarTodasPropiedades()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parteId, "Motor", rutinaId));
        rutina.Apply(new ActividadDeRutinaAgregada(
            actividadId, parteId, "Descripcion Original", "Clase Original",
            100, "HRS", "Horómetro", 10, 0, "", "", 0, "Insumo Original", 1.0
        ));

        // Act
        var evento = new ActividadDeRutinaActualizada(
            actividadId,
            "Descripcion Actualizada",
            "Clase Actualizada",
            200, // Nueva frecuencia
            "KM",
            "Odómetro",
            20,
            500, // Frecuencia secundaria
            "HRS",
            "Horómetro",
            50,
            "Insumo Actualizado",
            2.5
        );
        rutina.Apply(evento);

        // Assert
        var actividad = rutina.Partes[0].Actividades[0];
        actividad.Descripcion.Should().Be("Descripcion Actualizada");
        actividad.Clase.Should().Be("Clase Actualizada");
        actividad.Frecuencia.Should().Be(200);
        actividad.UnidadMedida.Should().Be("KM");
        actividad.NombreMedidor.Should().Be("Odómetro");
        actividad.AlertaFaltando.Should().Be(20);
        actividad.Frecuencia2.Should().Be(500);
        actividad.UnidadMedida2.Should().Be("HRS");
        actividad.NombreMedidor2.Should().Be("Horómetro");
        actividad.AlertaFaltando2.Should().Be(50);
        actividad.Insumo.Should().Be("Insumo Actualizado");
        actividad.Cantidad.Should().Be(2.5);
    }

    [Fact]
    public void RutinaMantenimiento_ActividadDeRutinaActualizada_ActividadNoExiste_DebeLanzarDomainException()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividadIdExistente = Guid.NewGuid();
        var actividadIdNoExistente = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parteId, "Motor", rutinaId));
        rutina.Apply(new ActividadDeRutinaAgregada(
            actividadIdExistente, parteId, "Actividad Original", "Clase",
            100, "HRS", "Horómetro", 10, 0, "", "", 0, "", 0
        ));

        // Act & Assert
        var evento = new ActividadDeRutinaActualizada(
            actividadIdNoExistente, "Descripcion Nueva", "Clase Nueva",
            200, "KM", "Odómetro", 20, 0, "", "", 0, "", 0
        );
        var act = () => rutina.Apply(evento);
        act.Should().Throw<DomainException>().Which.Message.Should().Contain(actividadIdNoExistente.ToString());
    }

    [Fact]
    public void RutinaMantenimiento_ActividadDeRutinaEliminada_DebeRemoverActividad()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var actividad1Id = Guid.NewGuid();
        var actividad2Id = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parteId, "Motor", rutinaId));
        rutina.Apply(new ActividadDeRutinaAgregada(
            actividad1Id, parteId, "Actividad 1", "Clase",
            100, "HRS", "Horómetro", 10, 0, "", "", 0, "", 0
        ));
        rutina.Apply(new ActividadDeRutinaAgregada(
            actividad2Id, parteId, "Actividad 2", "Clase",
            200, "HRS", "Horómetro", 20, 0, "", "", 0, "", 0
        ));

        // Act
        var evento = new ActividadDeRutinaEliminada(actividad1Id, parteId);
        rutina.Apply(evento);

        // Assert
        var parte = rutina.Partes[0];
        parte.Actividades.Should().HaveCount(1);
        parte.Actividades[0].Id.Should().Be(actividad2Id);
        parte.Actividades[0].Descripcion.Should().Be("Actividad 2");
    }

    [Fact]
    public void RutinaMantenimiento_ActividadDeRutinaEliminada_ParteNoExiste_DebeLanzarDomainException()
    {
        // Arrange
        var rutina = new RutinaMantenimiento();
        var rutinaId = Guid.NewGuid();
        var parteId = Guid.NewGuid();
        var parteIdNoExistente = Guid.NewGuid();
        var actividadId = Guid.NewGuid();

        rutina.Apply(new RutinaCreada(rutinaId, "Rutina Test", "Preventivo"));
        rutina.Apply(new ParteAgregada(parteId, "Motor", rutinaId));
        rutina.Apply(new ActividadDeRutinaAgregada(
            actividadId, parteId, "Actividad Test", "Clase",
            100, "HRS", "Horómetro", 10, 0, "", "", 0, "", 0
        ));

        // Act & Assert
        var evento = new ActividadDeRutinaEliminada(actividadId, parteIdNoExistente);
        var act = () => rutina.Apply(evento);
        act.Should().Throw<DomainException>().Which.Message.Should().Contain(parteIdNoExistente.ToString());
    }

    #endregion
}