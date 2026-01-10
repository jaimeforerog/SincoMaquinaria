using System;
using System.Collections.Generic;
using System.Linq;
using SincoMaquinaria.Domain.Events;

namespace SincoMaquinaria.Domain;

public class ConfiguracionGlobal
{
    // Singleton Id
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; set; }
    public List<TipoMedidor> TiposMedidor { get; set; } = new();
    public List<GrupoMantenimiento> GruposMantenimiento { get; set; } = new();
    public List<TipoFalla> TiposFalla { get; set; } = new();
    public List<CausaFalla> CausasFalla { get; set; } = new();

    // --- Apply Methods ---

    public void Apply(TipoMedidorCreado @event)
    {
        Id = SingletonId;
        var existing = TiposMedidor.FirstOrDefault(t => t.Codigo == @event.Codigo);
        if (existing == null)
        {
            TiposMedidor.Add(new TipoMedidor 
            { 
                Codigo = @event.Codigo, 
                Nombre = @event.Nombre, 
                Unidad = @event.Unidad, 
                Activo = true 
            });
        }
    }

    public void Apply(EstadoTipoMedidorCambiado @event)
    {
        var tipo = TiposMedidor.FirstOrDefault(t => t.Codigo == @event.Codigo);
        if (tipo != null)
        {
            tipo.Activo = @event.Activo;
        }
    }

    public void Apply(GrupoMantenimientoCreado @event)
    {
        Id = SingletonId;
        var existing = GruposMantenimiento.FirstOrDefault(g => g.Codigo == @event.Codigo);
        if (existing == null)
        {
            GruposMantenimiento.Add(new GrupoMantenimiento
            {
                Codigo = @event.Codigo,
                Nombre = @event.Nombre,
                Descripcion = @event.Descripcion,
                Activo = @event.Activo
            });
        }
    }

    public void Apply(EstadoGrupoMantenimientoCambiado @event)
    {
        var grupo = GruposMantenimiento.FirstOrDefault(g => g.Codigo == @event.Codigo);
        if (grupo != null)
        {
            grupo.Activo = @event.Activo;
        }
    }

    public void Apply(TipoMedidorActualizado @event)
    {
        var tipo = TiposMedidor.FirstOrDefault(t => t.Codigo == @event.Codigo);
        if (tipo != null)
        {
            tipo.Nombre = @event.Nombre;
            tipo.Unidad = @event.Unidad;
        }
    }

    public void Apply(GrupoMantenimientoActualizado @event)
    {
        var grupo = GruposMantenimiento.FirstOrDefault(g => g.Codigo == @event.Codigo);
        if (grupo != null)
        {
            grupo.Nombre = @event.Nombre;
            grupo.Descripcion = @event.Descripcion;
        }
    }

    public void Apply(TipoFallaCreado @event)
    {
        Id = SingletonId;
        var existing = TiposFalla.FirstOrDefault(t => t.Codigo == @event.Codigo);
        if (existing == null)
        {
            TiposFalla.Add(new TipoFalla 
            { 
                Codigo = @event.Codigo, 
                Descripcion = @event.Descripcion, 
                Prioridad = @event.Prioridad, 
                Activo = true 
            });
        }
    }

    public void Apply(CausaFallaCreada @event)
    {
        Id = SingletonId;
        var existing = CausasFalla.FirstOrDefault(c => c.Codigo == @event.Codigo);
        if (existing == null)
        {
            CausasFalla.Add(new CausaFalla 
            { 
                Codigo = @event.Codigo, 
                Descripcion = @event.Descripcion, 
                Activo = true 
            });
        }
    }

    public void Apply(CausaFallaActualizada @event)
    {
        var causa = CausasFalla.FirstOrDefault(c => c.Codigo == @event.Codigo);
        if (causa != null)
        {
            causa.Descripcion = @event.Descripcion;
        }
    }

    public void Apply(EstadoCausaFallaCambiado @event)
    {
        var causa = CausasFalla.FirstOrDefault(c => c.Codigo == @event.Codigo);
        if (causa != null)
        {
            causa.Activo = @event.Activo;
        }
    }
}

public class TipoMedidor
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class GrupoMantenimiento
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class TipoFalla
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Prioridad { get; set; } = "Media";
    public bool Activo { get; set; }
}

public class CausaFalla
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
