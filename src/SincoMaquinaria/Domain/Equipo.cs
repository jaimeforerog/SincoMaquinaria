using System;
using SincoMaquinaria.Domain.Events;

namespace SincoMaquinaria.Domain;

public class Equipo
{
    public Guid Id { get; set; }
    
    // Identificador de negocio
    public string Placa { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string TipoMedidorId { get; set; } = string.Empty;
    public string TipoMedidorId2 { get; set; } = string.Empty;
    public string Grupo { get; set; } = string.Empty;
    public string Rutina { get; set; } = string.Empty;
    
    public string Estado { get; set; } = "Inactivo";

    // Auditoría
    public Guid? CreadoPor { get; set; }
    public string? CreadoPorNombre { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public Guid? ModificadoPor { get; set; }
    public string? ModificadoPorNombre { get; set; }
    public DateTimeOffset? FechaModificacion { get; set; }

    public void Apply(EquipoCreado @event)
    {
        Id = @event.Id;
        Placa = @event.Placa;
        Descripcion = @event.Descripcion;
        Marca = @event.Marca;
        Modelo = @event.Modelo;
        Serie = @event.Serie;
        Codigo = @event.Codigo;
        TipoMedidorId = @event.TipoMedidorId;
        TipoMedidorId2 = @event.TipoMedidorId2;
        Grupo = @event.Grupo;
        Rutina = @event.Rutina;
        Estado = "Activo";
        // Auditoría
        CreadoPor = @event.UsuarioId;
        CreadoPorNombre = @event.UsuarioNombre;
        FechaCreacion = @event.FechaCreacion ?? DateTimeOffset.Now;
    }

    public void Apply(EquipoMigrado @event)
    {
        Id = @event.Id;
        Placa = @event.Placa;
        Descripcion = @event.Descripcion;
        Marca = @event.Marca;
        Modelo = @event.Modelo;
        Serie = @event.Serie;
        Codigo = @event.Codigo;
        TipoMedidorId = @event.TipoMedidorId;
        TipoMedidorId2 = @event.TipoMedidorId2;
        Grupo = @event.Grupo;
        Rutina = @event.Rutina;
        Estado = "Activo"; // Asumimos activo al migrar
        // Auditoría
        CreadoPor = @event.UsuarioId;
        CreadoPorNombre = @event.UsuarioNombre;
        FechaCreacion = @event.FechaCreacion ?? DateTimeOffset.Now;
    }

    public void Apply(EquipoActualizado @event)
    {
        // No cambiamos Id ni Placa
        Descripcion = @event.Descripcion;
        Marca = @event.Marca;
        Modelo = @event.Modelo;
        Serie = @event.Serie;
        Codigo = @event.Codigo;
        TipoMedidorId = @event.TipoMedidorId;
        TipoMedidorId2 = @event.TipoMedidorId2;
        Grupo = @event.Grupo;
        Rutina = @event.Rutina;
        // Auditoría de modificación
        ModificadoPor = @event.UsuarioId;
        ModificadoPorNombre = @event.UsuarioNombre;
        FechaModificacion = @event.FechaModificacion ?? DateTimeOffset.UtcNow;
    }
}
