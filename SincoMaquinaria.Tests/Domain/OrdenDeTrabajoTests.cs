using Xunit;
using SincoMaquinaria.Domain;
using SincoMaquinaria.Domain.Events;
using SincoMaquinaria.Domain.Events.OrdenDeTrabajo;
using System;

namespace SincoMaquinaria.Tests.Domain;

public class OrdenDeTrabajoTests
{
    // Helper to create an order in Borrador state
    private static OrdenDeTrabajo CrearOrdenEnBorrador()
    {
        var orden = new OrdenDeTrabajo();
        orden.Apply(new OrdenDeTrabajoCreada(Guid.NewGuid(), "OT-001", "EQ-123", "Interno", "Preventivo", DateTime.Now, DateTimeOffset.UtcNow));
        return orden;
    }

    // Helper to create an order in Programada state
    private static OrdenDeTrabajo CrearOrdenProgramada()
    {
        var orden = CrearOrdenEnBorrador();
        orden.Apply(new OrdenProgramada(DateTime.UtcNow.AddDays(1), TimeSpan.FromHours(2)));
        return orden;
    }

    // Helper to create an order in EnEjecucion state
    private static (OrdenDeTrabajo orden, Guid detalleId) CrearOrdenEnEjecucion()
    {
        var orden = CrearOrdenEnBorrador();
        var detalleId = Guid.NewGuid();
        orden.Apply(new ActividadAgregada(detalleId, "Tarea setup", DateTime.UtcNow));
        orden.Apply(new OrdenProgramada(DateTime.UtcNow.AddDays(1), TimeSpan.FromHours(2)));
        orden.Apply(new AvanceDeActividadRegistrado(detalleId, 50, "En progreso", "EnProceso"));
        return (orden, detalleId);
    }

    [Fact]
    public void OrdenDeTrabajo_Creada_DebeTenerEstadoBorrador()
    {
        // Arrange
        var orden = new OrdenDeTrabajo();
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
        // Arrange - Order must be in Borrador to add activities
        var orden = CrearOrdenEnBorrador();
        var detalleId = Guid.NewGuid();
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
        // Arrange - Need Programada state to register progress
        var orden = CrearOrdenEnBorrador();
        var detalleId = Guid.NewGuid();
        orden.Apply(new ActividadAgregada(detalleId, "Tarea 1", DateTime.UtcNow));
        orden.Apply(new OrdenProgramada(DateTime.UtcNow.AddDays(1), TimeSpan.FromHours(2)));

        // Act
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
        // Arrange - Order must be in Borrador to be programmed
        var orden = CrearOrdenEnBorrador();
        var fecha = DateTime.UtcNow.AddDays(1);
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
        // Arrange - Order must be in Borrador to add activities
        var orden = CrearOrdenEnBorrador();
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
        // Arrange - Order must be in Borrador to add activities
        var orden = CrearOrdenEnBorrador();
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

    [Fact]
    public void OrdenDeTrabajo_OrdenFinalizada_DebeCambiarEstadoAEjecucionCompleta()
    {
        // Arrange - Order must be in EnEjecucion or EjecucionCompleta to be finalized
        var (orden, _) = CrearOrdenEnEjecucion();

        // Act
        var evento = new OrdenFinalizada("EjecucionCompleta", "Admin User", DateTime.Now, Guid.NewGuid(), "Admin");
        orden.Apply(evento);

        // Assert
        Assert.Equal(EstadoOrdenDeTrabajo.EjecucionCompleta, orden.Estado);
    }

    [Fact]
    public void OrdenDeTrabajo_OrdenFinalizada_ConEstadoEliminada_DebeCambiarEstado()
    {
        // Arrange - Order must be in EnEjecucion or EjecucionCompleta to be finalized
        var (orden, _) = CrearOrdenEnEjecucion();

        // Act
        var evento = new OrdenFinalizada("Eliminada", "Admin User", DateTime.Now, Guid.NewGuid(), "Admin");
        orden.Apply(evento);

        // Assert
        Assert.Equal(EstadoOrdenDeTrabajo.Eliminada, orden.Estado);
    }

    [Fact]
    public void OrdenDeTrabajo_OrdenDeTrabajoEliminada_DebeCambiarEstadoAEliminada()
    {
        // Arrange
        var orden = CrearOrdenEnBorrador();
        var ordenId = orden.Id;

        // Act
        var evento = new OrdenDeTrabajoEliminada(ordenId);
        orden.Apply(evento);

        // Assert
        Assert.Equal(EstadoOrdenDeTrabajo.Eliminada, orden.Estado);
    }

    [Fact]
    public void OrdenDeTrabajo_AvanceRegistrado_DetalleNoExiste_DebeLanzarDomainException()
    {
        // Arrange - Need Programada state to register progress
        var orden = CrearOrdenEnBorrador();
        var detalleIdExistente = Guid.NewGuid();
        var detalleIdNoExistente = Guid.NewGuid();

        orden.Apply(new ActividadAgregada(detalleIdExistente, "Tarea existente", DateTime.UtcNow));
        orden.Apply(new OrdenProgramada(DateTime.UtcNow.AddDays(1), TimeSpan.FromHours(2)));

        // Act & Assert
        var eventoAvance = new AvanceDeActividadRegistrado(detalleIdNoExistente, 50, "Test", "EnProgreso");
        var ex = Assert.Throws<DomainException>(() => orden.Apply(eventoAvance));
        Assert.Contains(detalleIdNoExistente.ToString(), ex.Message);
    }

    [Fact]
    public void OrdenDeTrabajo_MultipleDetalles_AvanceParcial_DebeEstarEnEjecucion()
    {
        // Arrange - Need Programada state to register progress
        var orden = CrearOrdenEnBorrador();
        var detalle1Id = Guid.NewGuid();
        var detalle2Id = Guid.NewGuid();
        var detalle3Id = Guid.NewGuid();

        orden.Apply(new ActividadAgregada(detalle1Id, "Tarea 1", DateTime.UtcNow));
        orden.Apply(new ActividadAgregada(detalle2Id, "Tarea 2", DateTime.UtcNow));
        orden.Apply(new ActividadAgregada(detalle3Id, "Tarea 3", DateTime.UtcNow));
        orden.Apply(new OrdenProgramada(DateTime.UtcNow.AddDays(1), TimeSpan.FromHours(2)));

        // Act - Finalizar solo la primera tarea
        orden.Apply(new AvanceDeActividadRegistrado(detalle1Id, 100, "Completado", "Finalizado"));

        // Assert
        Assert.Equal(EstadoOrdenDeTrabajo.EnEjecucion, orden.Estado);
        Assert.Equal(EstadoDetalleOrden.Finalizado, orden.Detalles[0].Estado);
        Assert.Equal(EstadoDetalleOrden.Pendiente, orden.Detalles[1].Estado);
        Assert.Equal(EstadoDetalleOrden.Pendiente, orden.Detalles[2].Estado);
    }

    [Fact]
    public void OrdenDeTrabajo_MultipleDetalles_TodosFinalizados_DebeEstarCompleta()
    {
        // Arrange - Need Programada state to register progress
        var orden = CrearOrdenEnBorrador();
        var detalle1Id = Guid.NewGuid();
        var detalle2Id = Guid.NewGuid();

        orden.Apply(new ActividadAgregada(detalle1Id, "Tarea 1", DateTime.UtcNow));
        orden.Apply(new ActividadAgregada(detalle2Id, "Tarea 2", DateTime.UtcNow));
        orden.Apply(new OrdenProgramada(DateTime.UtcNow.AddDays(1), TimeSpan.FromHours(2)));

        // Act - Finalizar ambas tareas
        orden.Apply(new AvanceDeActividadRegistrado(detalle1Id, 100, "Completado", "Finalizado"));
        orden.Apply(new AvanceDeActividadRegistrado(detalle2Id, 100, "Completado", "Finalizado"));

        // Assert
        Assert.Equal(EstadoOrdenDeTrabajo.EjecucionCompleta, orden.Estado);
        Assert.All(orden.Detalles, d => Assert.Equal(EstadoDetalleOrden.Finalizado, d.Estado));
    }

    [Fact]
    public void OrdenDeTrabajo_PorcentajeAvanceGeneral_SinDetalles_DebeSer0()
    {
        // Arrange
        var orden = CrearOrdenEnBorrador();

        // Assert
        Assert.Equal(0, orden.PorcentajeAvanceGeneral);
    }

    [Fact]
    public void OrdenDeTrabajo_PorcentajeAvanceGeneral_ConDetalles_DebeCalcularPromedio()
    {
        // Arrange - Need Programada state to register progress
        var orden = CrearOrdenEnBorrador();
        var detalle1Id = Guid.NewGuid();
        var detalle2Id = Guid.NewGuid();
        var detalle3Id = Guid.NewGuid();

        orden.Apply(new ActividadAgregada(detalle1Id, "Tarea 1", DateTime.UtcNow));
        orden.Apply(new ActividadAgregada(detalle2Id, "Tarea 2", DateTime.UtcNow));
        orden.Apply(new ActividadAgregada(detalle3Id, "Tarea 3", DateTime.UtcNow));
        orden.Apply(new OrdenProgramada(DateTime.UtcNow.AddDays(1), TimeSpan.FromHours(2)));

        // Act
        orden.Apply(new AvanceDeActividadRegistrado(detalle1Id, 100, "Completado", "Finalizado"));
        orden.Apply(new AvanceDeActividadRegistrado(detalle2Id, 50, "En progreso", "EnProceso"));
        // detalle3 queda en 0

        // Assert
        // Promedio: (100 + 50 + 0) / 3 = 50
        Assert.Equal(50, orden.PorcentajeAvanceGeneral);
    }

}
