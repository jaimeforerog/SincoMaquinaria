using Xunit;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.Domain.Events.OrdenDeTrabajo;
using System;

namespace SincoMaquinaria.Tests.Domain;

public class OrdenDeTrabajoTests
{
    [Fact]
    public void OrdenDeTrabajo_Creada_DebeTenerEstadoBorrador()
    {
        // Arrange
        var orden = new OrdenDeTrabajo();
        // Ctor: Guid OrdenId, string NumeroOrden, string EquipoId, string Origen, string TipoMantenimiento, DateTime FechaOrden, DateTimeOffset FechaCreacion
        var evento = new OrdenDeTrabajoCreada(Guid.NewGuid(), "OT-001", "EQ-123", "Interno", "Preventivo", DateTime.Now, DateTimeOffset.UtcNow);

        // Act
        orden.Apply(evento);

        // Assert
        Assert.Equal(EstadoOrdenDeTrabajo.Borrador, orden.Estado);
        Assert.Equal("OT-001", orden.Numero);
        Assert.Equal("EQ-123", orden.EquipoId);
        Assert.Equal("Preventivo", orden.Tipo);
        Assert.Equal("Interno", orden.Origen);
    }

    [Fact]
    public void OrdenDeTrabajo_ActividadAgregada_DebeAgregarDetalle()
    {
        // Arrange
        var orden = new OrdenDeTrabajo();
        var detalleId = Guid.NewGuid();
        // Ctor: Guid ItemDetalleId, string Descripcion, DateTime FechaEstimadaEjecucion
        var evento = new ActividadAgregada(detalleId, "Cambio de aceite", DateTime.UtcNow.AddDays(1));

        // Act
        orden.Apply(evento);

        // Assert
        Assert.Single(orden.Detalles);
        var detalle = orden.Detalles[0];
        Assert.Equal(detalleId, detalle.Id);
        Assert.Equal("Cambio de aceite", detalle.Descripcion);
        Assert.Equal(EstadoDetalleOrden.Pendiente, detalle.Estado);
        Assert.Equal(0, detalle.Avance);
    }

    [Fact]
    public void OrdenDeTrabajo_AvanceRegistrado_DebeActualizarDetalleYEstado()
    {
        // Arrange
        var orden = new OrdenDeTrabajo();
        var detalleId = Guid.NewGuid();
        
        // Setup initial state
        orden.Apply(new ActividadAgregada(detalleId, "Tarea 1", DateTime.UtcNow));
        
        // Act
        // Ctor: Guid ItemDetalleId, decimal PorcentajeAvance, string Observacion, string NuevoEstado
        var eventoAvance = new AvanceDeActividadRegistrado(detalleId, 100, "Todo ok", "Finalizado");
        orden.Apply(eventoAvance);

        // Assert
        var detalle = orden.Detalles[0];
        Assert.Equal(100, detalle.Avance);
        Assert.Equal(EstadoDetalleOrden.Finalizado, detalle.Estado);
        Assert.Equal(EstadoOrdenDeTrabajo.EjecucionCompleta, orden.Estado);
    }

    [Fact]
    public void OrdenDeTrabajo_Programada_DebeCambiarEstado()
    {
        // Arrange
        var orden = new OrdenDeTrabajo();
        var fecha = DateTime.UtcNow.AddDays(1);
        // Ctor: DateTime FechaProgramada, TimeSpan DuracionEstimada
        var evento = new OrdenProgramada(fecha, TimeSpan.FromHours(2));

        // Act
        orden.Apply(evento);

        // Assert
        Assert.Equal(EstadoOrdenDeTrabajo.Programada, orden.Estado);
        Assert.Equal(fecha, orden.FechaProgramada);
    }

    [Fact]
    public void OrdenDeTrabajo_ActividadAgregadaConFallaInfo_DebeGuardarTipoYCausa()
    {
        // Arrange
        var orden = new OrdenDeTrabajo();
        var detalleId = Guid.NewGuid();
        var tipoFallaId = "TIPO001";
        var causaFallaId = "CAUSA001";
        
        // Act
        var evento = new ActividadAgregada(
            detalleId, 
            "Reparación de motor", 
            DateTime.UtcNow.AddDays(1),
            0,
            tipoFallaId,
            causaFallaId
        );
        orden.Apply(evento);

        // Assert
        Assert.Single(orden.Detalles);
        var detalle = orden.Detalles[0];
        Assert.Equal(tipoFallaId, detalle.TipoFallaId);
        Assert.Equal(causaFallaId, detalle.CausaFallaId);
    }

    [Fact]
    public void OrdenDeTrabajo_ActividadSinFallaInfo_DebePermitirNulos()
    {
        // Arrange
        var orden = new OrdenDeTrabajo();
        var detalleId = Guid.NewGuid();
        
        // Act
        var evento = new ActividadAgregada(
            detalleId, 
            "Cambio de filtro", 
            DateTime.UtcNow.AddDays(1)
        );
        orden.Apply(evento);

        // Assert
        Assert.Single(orden.Detalles);
        var detalle = orden.Detalles[0];
        Assert.Null(detalle.TipoFallaId);
        Assert.Null(detalle.CausaFallaId);
    }
}
