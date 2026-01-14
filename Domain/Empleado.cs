using System;
using SincoMaquinaria.Domain.Events;

namespace SincoMaquinaria.Domain;

public class Empleado
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public CargoEmpleado Cargo { get; set; }
    public string Especialidad { get; set; } = string.Empty;
    public decimal ValorHora { get; set; }
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

    // Auditoría
    public Guid? CreadoPor { get; set; }
    public string? CreadoPorNombre { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public Guid? ModificadoPor { get; set; }
    public string? ModificadoPorNombre { get; set; }
    public DateTimeOffset? FechaModificacion { get; set; }

    public Empleado() { }

    public void Apply(EmpleadoCreado @event)
    {
        Id = @event.Id;
        Nombre = @event.Nombre;
        Identificacion = @event.Identificacion;
        Cargo = @event.Cargo.ToEnum<CargoEmpleado>();
        Especialidad = @event.Especialidad;
        ValorHora = @event.ValorHora;
        Estado = @event.Estado.ToEnum<EstadoEmpleado>();
        // Auditoría
        CreadoPor = @event.UsuarioId;
        CreadoPorNombre = @event.UsuarioNombre;
        FechaCreacion = @event.FechaCreacion ?? DateTimeOffset.Now;
    }

    public void Apply(EmpleadoActualizado @event)
    {
        Nombre = @event.Nombre;
        Identificacion = @event.Identificacion;
        Cargo = @event.Cargo.ToEnum<CargoEmpleado>();
        Especialidad = @event.Especialidad;
        ValorHora = @event.ValorHora;
        Estado = @event.Estado.ToEnum<EstadoEmpleado>();
        // Auditoría de modificación
        ModificadoPor = @event.UsuarioId;
        ModificadoPorNombre = @event.UsuarioNombre;
        FechaModificacion = DateTimeOffset.Now;
    }
}
