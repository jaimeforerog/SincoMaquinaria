using System;
using System.Collections.Generic;
using System.Linq;
using SincoMaquinaria.Domain.Events;

using Marten.Events;

namespace SincoMaquinaria.Domain;

// Este es tu AGREGADO (La entidad principal)
// Marten usará esta clase para reconstruir el estado actual desde los eventos.
public class OrdenDeTrabajo
{
    // Identificador único (Requerido por Marten)
    public Guid Id { get; set; }

    // Propiedades de Estado (Proyección en Memoria)
    public string Numero { get; set; } = string.Empty;
    public string EquipoId { get; set; } = string.Empty;
    public EstadoOrdenDeTrabajo Estado { get; set; } = EstadoOrdenDeTrabajo.Inexistente;
    public string Tipo { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public DateTime FechaOrden { get; set; } // Fecha de la OT (Seleccionable)
    public DateTime? FechaProgramada { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }

    public List<DetalleOrden> Detalles { get; set; } = new();

    // Totales calculados
    public decimal PorcentajeAvanceGeneral => Detalles.Any() ? Detalles.Average(d => d.Avance) : 0;

    // --- Apply Methods (El corazón del Event Sourcing) ---
    // Marten llama a estos métodos automáticamente en orden cronológico.

    public void Apply(OrdenDeTrabajoCreada @event)
    {
        Id = @event.OrdenId;
        Numero = @event.NumeroOrden;
        EquipoId = @event.EquipoId;
        Estado = EstadoOrdenDeTrabajo.Borrador;
        // Mapeo de nuevas propiedades
        Tipo = @event.TipoMantenimiento;
        Origen = @event.Origen;
        FechaOrden = @event.FechaOrden;
        FechaCreacion = @event.FechaCreacion;
    }

    public void Apply(OrdenProgramada @event)
    {
        FechaProgramada = @event.FechaProgramada;
        Estado = EstadoOrdenDeTrabajo.Programada;
    }

    public void Apply(ActividadAgregada @event)
    {
        Detalles.Add(new DetalleOrden
        {
            Id = @event.ItemDetalleId,
            Descripcion = @event.Descripcion,
            Avance = 0,
            Estado = EstadoDetalleOrden.Pendiente,
            Frecuencia = @event.Frecuencia, // Mapping new property
            TipoFallaId = @event.TipoFallaId,
            CausaFallaId = @event.CausaFallaId
        });
    }

    public void Apply(AvanceDeActividadRegistrado @event)
    {
        var item = Detalles.FirstOrDefault(d => d.Id == @event.ItemDetalleId)
            ?? throw new DomainException($"Detalle con ID {@event.ItemDetalleId} no existe en la orden");

        item.Avance = @event.PorcentajeAvance;
        item.Observaciones = @event.Observacion;
        item.Estado = @event.NuevoEstado.ToEnum<EstadoDetalleOrden>();

        // Lógica de negocio derivada
        if (Detalles.All(d => d.Estado == EstadoDetalleOrden.Finalizado))
        {
            Estado = EstadoOrdenDeTrabajo.EjecucionCompleta;
        }
        else
        {
            Estado = EstadoOrdenDeTrabajo.EnEjecucion;
        }
    }

    public void Apply(OrdenFinalizada @event)
    {
        Estado = @event.EstadoFinal.ToEnum<EstadoOrdenDeTrabajo>();
    }

    public void Apply(OrdenDeTrabajoEliminada @event)
    {
        Estado = EstadoOrdenDeTrabajo.Eliminada;
    }
}

public class DetalleOrden
{
    public Guid Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Avance { get; set; }
    public EstadoDetalleOrden Estado { get; set; } = EstadoDetalleOrden.Pendiente;
    public string Observaciones { get; set; } = string.Empty;
    public int Frecuencia { get; set; } // New property
    public string? TipoFallaId { get; set; } // Failure type for corrective orders
    public string? CausaFallaId { get; set; } // Failure cause for corrective orders
}
