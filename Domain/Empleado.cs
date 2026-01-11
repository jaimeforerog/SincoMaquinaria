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
    }

    public void Apply(EmpleadoActualizado @event)
    {
        Nombre = @event.Nombre;
        Identificacion = @event.Identificacion;
        Cargo = @event.Cargo.ToEnum<CargoEmpleado>();
        Especialidad = @event.Especialidad;
        ValorHora = @event.ValorHora;
        Estado = @event.Estado.ToEnum<EstadoEmpleado>();
    }
}
