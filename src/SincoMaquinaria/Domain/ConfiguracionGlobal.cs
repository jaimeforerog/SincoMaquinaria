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
        if (existing != null)
            throw new DomainException($"Tipo de medidor con código '{@event.Codigo}' ya existe");

        TiposMedidor.Add(new TipoMedidor
        {
            Codigo = @event.Codigo,
            Nombre = @event.Nombre,
            Unidad = @event.Unidad,
            Activo = true,
            CreadoPor = @event.UsuarioId,
            CreadoPorNombre = @event.UsuarioNombre,
            FechaCreacion = @event.FechaCreacion ?? DateTimeOffset.Now
        });
    }

    public void Apply(EstadoTipoMedidorCambiado @event)
    {
        var tipo = TiposMedidor.FirstOrDefault(t => t.Codigo == @event.Codigo)
            ?? throw new DomainException($"Tipo de medidor con código '{@event.Codigo}' no existe");
        tipo.Activo = @event.Activo;
    }

    public void Apply(GrupoMantenimientoCreado @event)
    {
        Id = SingletonId;
        var existing = GruposMantenimiento.FirstOrDefault(g => g.Codigo == @event.Codigo);
        if (existing != null)
            throw new DomainException($"Grupo de mantenimiento con código '{@event.Codigo}' ya existe");

        GruposMantenimiento.Add(new GrupoMantenimiento
        {
            Codigo = @event.Codigo,
            Nombre = @event.Nombre,
            Descripcion = @event.Descripcion,
            Activo = @event.Activo,
            CreadoPor = @event.UsuarioId,
            CreadoPorNombre = @event.UsuarioNombre,
            FechaCreacion = @event.FechaCreacion ?? DateTimeOffset.Now
        });
    }

    public void Apply(EstadoGrupoMantenimientoCambiado @event)
    {
        var grupo = GruposMantenimiento.FirstOrDefault(g => g.Codigo == @event.Codigo)
            ?? throw new DomainException($"Grupo de mantenimiento con código '{@event.Codigo}' no existe");
        grupo.Activo = @event.Activo;
    }

    public void Apply(TipoMedidorActualizado @event)
    {
        var tipo = TiposMedidor.FirstOrDefault(t => t.Codigo == @event.Codigo)
            ?? throw new DomainException($"Tipo de medidor con código '{@event.Codigo}' no existe");
        tipo.Nombre = @event.Nombre;
        tipo.Unidad = @event.Unidad;
    }

    public void Apply(GrupoMantenimientoActualizado @event)
    {
        var grupo = GruposMantenimiento.FirstOrDefault(g => g.Codigo == @event.Codigo)
            ?? throw new DomainException($"Grupo de mantenimiento con código '{@event.Codigo}' no existe");
        grupo.Nombre = @event.Nombre;
        grupo.Descripcion = @event.Descripcion;
    }

    public void Apply(TipoFallaCreado @event)
    {
        Id = SingletonId;
        var existing = TiposFalla.FirstOrDefault(t => t.Codigo == @event.Codigo);
        if (existing != null)
            throw new DomainException($"Tipo de falla con código '{@event.Codigo}' ya existe");

        TiposFalla.Add(new TipoFalla
        {
            Codigo = @event.Codigo,
            Descripcion = @event.Descripcion,
            Prioridad = @event.Prioridad,
            Activo = true,
            CreadoPor = @event.UsuarioId,
            CreadoPorNombre = @event.UsuarioNombre,
            FechaCreacion = @event.FechaCreacion ?? DateTimeOffset.Now
        });
    }

    public void Apply(CausaFallaCreada @event)
    {
        Id = SingletonId;
        var existing = CausasFalla.FirstOrDefault(c => c.Codigo == @event.Codigo);
        if (existing != null)
            throw new DomainException($"Causa de falla con código '{@event.Codigo}' ya existe");

        CausasFalla.Add(new CausaFalla
        {
            Codigo = @event.Codigo,
            Descripcion = @event.Descripcion,
            Activo = true,
            CreadoPor = @event.UsuarioId,
            CreadoPorNombre = @event.UsuarioNombre,
            FechaCreacion = @event.FechaCreacion ?? DateTimeOffset.Now
        });
    }

    public void Apply(CausaFallaActualizada @event)
    {
        var causa = CausasFalla.FirstOrDefault(c => c.Codigo == @event.Codigo)
            ?? throw new DomainException($"Causa de falla con código '{@event.Codigo}' no existe");
        causa.Descripcion = @event.Descripcion;
    }

    public void Apply(EstadoCausaFallaCambiado @event)
    {
        var causa = CausasFalla.FirstOrDefault(c => c.Codigo == @event.Codigo)
            ?? throw new DomainException($"Causa de falla con código '{@event.Codigo}' no existe");
        causa.Activo = @event.Activo;
    }
}

public class TipoMedidor
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public bool Activo { get; set; }
    // Auditoría
    public Guid? CreadoPor { get; set; }
    public string? CreadoPorNombre { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public Guid? ModificadoPor { get; set; }
    public string? ModificadoPorNombre { get; set; }
    public DateTimeOffset? FechaModificacion { get; set; }
}

public class GrupoMantenimiento
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    // Auditoría
    public Guid? CreadoPor { get; set; }
    public string? CreadoPorNombre { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public Guid? ModificadoPor { get; set; }
    public string? ModificadoPorNombre { get; set; }
    public DateTimeOffset? FechaModificacion { get; set; }
}

public class TipoFalla
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Prioridad { get; set; } = "Media";
    public bool Activo { get; set; }
    // Auditoría
    public Guid? CreadoPor { get; set; }
    public string? CreadoPorNombre { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public Guid? ModificadoPor { get; set; }
    public string? ModificadoPorNombre { get; set; }
    public DateTimeOffset? FechaModificacion { get; set; }
}

public class CausaFalla
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }
    // Auditoría
    public Guid? CreadoPor { get; set; }
    public string? CreadoPorNombre { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public Guid? ModificadoPor { get; set; }
    public string? ModificadoPorNombre { get; set; }
    public DateTimeOffset? FechaModificacion { get; set; }
}
