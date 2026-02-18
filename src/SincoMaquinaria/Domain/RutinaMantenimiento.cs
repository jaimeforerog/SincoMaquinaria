using System;
using System.Collections.Generic;
using System.Linq;

namespace SincoMaquinaria.Domain;

// Eventos
public record RutinaMigrada(Guid RutinaId, string Descripcion, string Grupo, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record ParteDeRutinaMigrada(Guid ParteId, string Descripcion, Guid RutinaId);
public record ActividadDeRutinaMigrada(
    Guid ActividadId,
    string Descripcion, 
    string Clase,

    int Frecuencia, 
    string UnidadMedida,
    string NombreMedidor,    int AlertaFaltando,
    
    int Frecuencia2,
    string UnidadMedida2,
    string NombreMedidor2,    int AlertaFaltando2,

    string? Insumo,
    double Cantidad,
    Guid ParteId
);

public record RutinaCreada(Guid RutinaId, string Descripcion, string Grupo, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record RutinaActualizada(Guid RutinaId, string Descripcion, string Grupo, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record ParteAgregada(Guid ParteId, string Descripcion, Guid RutinaId, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record ParteActualizada(Guid ParteId, string Descripcion, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record ParteEliminada(Guid ParteId, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record ActividadDeRutinaAgregada(Guid ActividadId, Guid ParteId, string Descripcion, string Clase, int Frecuencia, string UnidadMedida, string NombreMedidor, int AlertaFaltando, int Frecuencia2, string UnidadMedida2, string NombreMedidor2, int AlertaFaltando2, string? Insumo, double Cantidad, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record ActividadDeRutinaActualizada(Guid ActividadId, string Descripcion, string Clase, int Frecuencia, string UnidadMedida, string NombreMedidor, int AlertaFaltando, int Frecuencia2, string UnidadMedida2, string NombreMedidor2, int AlertaFaltando2, string? Insumo, double Cantidad, Guid? UsuarioId = null, string? UsuarioNombre = null);
public record ActividadDeRutinaEliminada(Guid ActividadId, Guid ParteId, Guid? UsuarioId = null, string? UsuarioNombre = null);

public class RutinaMantenimiento
{
    public Guid Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Grupo { get; set; } = string.Empty;
    public List<ParteEquipo> Partes { get; set; } = new();

    // Marten require un constructor vacío o control de versiones
    public RutinaMantenimiento() { }

    public void Apply(RutinaMigrada @event)
    {
        Id = @event.RutinaId;
        Descripcion = @event.Descripcion;
        Grupo = @event.Grupo;
    }

    public void Apply(ParteDeRutinaMigrada @event)
    {
        Partes.Add(new ParteEquipo
        {
            Id = @event.ParteId,
            Descripcion = @event.Descripcion,
            Actividades = new List<ActividadMantenimiento>()
        });
    }

    public void Apply(ActividadDeRutinaMigrada @event)
    {
        var parte = Partes.FirstOrDefault(p => p.Id == @event.ParteId)
            ?? throw new DomainException($"Parte con ID {@event.ParteId} no existe en la rutina");

        parte.Actividades.Add(new ActividadMantenimiento
        {
            Id = @event.ActividadId,
            Descripcion = @event.Descripcion,
            Clase = @event.Clase,
            Frecuencia = @event.Frecuencia,
            UnidadMedida = @event.UnidadMedida,
            NombreMedidor = @event.NombreMedidor,
            AlertaFaltando = @event.AlertaFaltando,

            Frecuencia2 = @event.Frecuencia2,
            UnidadMedida2 = @event.UnidadMedida2,
            NombreMedidor2 = @event.NombreMedidor2,
            AlertaFaltando2 = @event.AlertaFaltando2,

            Insumo = @event.Insumo,
            Cantidad = @event.Cantidad
        });
    }

    public void Apply(RutinaCreada @event)
    {
        Id = @event.RutinaId;
        Descripcion = @event.Descripcion;
        Grupo = @event.Grupo;
    }

    public void Apply(RutinaActualizada @event)
    {
        Descripcion = @event.Descripcion;
        Grupo = @event.Grupo;
    }

    public void Apply(ParteAgregada @event)
    {
        Partes.Add(new ParteEquipo
        {
            Id = @event.ParteId,
            Descripcion = @event.Descripcion,
            Actividades = new List<ActividadMantenimiento>()
        });
    }

    public void Apply(ParteActualizada @event)
    {
        var parte = Partes.FirstOrDefault(p => p.Id == @event.ParteId)
            ?? throw new DomainException($"Parte con ID {@event.ParteId} no existe en la rutina");
        parte.Descripcion = @event.Descripcion;
    }

    public void Apply(ParteEliminada @event)
    {
        Partes.RemoveAll(p => p.Id == @event.ParteId);
    }

    public void Apply(ActividadDeRutinaAgregada @event)
    {
        var parte = Partes.FirstOrDefault(p => p.Id == @event.ParteId)
            ?? throw new DomainException($"Parte con ID {@event.ParteId} no existe en la rutina");

        parte.Actividades.Add(new ActividadMantenimiento
        {
            Id = @event.ActividadId,
            Descripcion = @event.Descripcion,
            Clase = @event.Clase,
            Frecuencia = @event.Frecuencia,
            UnidadMedida = @event.UnidadMedida,
            NombreMedidor = @event.NombreMedidor,
            AlertaFaltando = @event.AlertaFaltando,
            Frecuencia2 = @event.Frecuencia2,
            UnidadMedida2 = @event.UnidadMedida2,
            NombreMedidor2 = @event.NombreMedidor2,
            AlertaFaltando2 = @event.AlertaFaltando2,
            Insumo = @event.Insumo,
            Cantidad = @event.Cantidad
        });
    }

    public void Apply(ActividadDeRutinaActualizada @event)
    {
        foreach (var parte in Partes)
        {
            var actividad = parte.Actividades.FirstOrDefault(a => a.Id == @event.ActividadId);
            if (actividad == null) continue;

            actividad.Descripcion = @event.Descripcion;
            actividad.Clase = @event.Clase;
            actividad.Frecuencia = @event.Frecuencia;
            actividad.UnidadMedida = @event.UnidadMedida;
            actividad.NombreMedidor = @event.NombreMedidor;
            actividad.AlertaFaltando = @event.AlertaFaltando;
            actividad.Frecuencia2 = @event.Frecuencia2;
            actividad.UnidadMedida2 = @event.UnidadMedida2;
            actividad.NombreMedidor2 = @event.NombreMedidor2;
            actividad.AlertaFaltando2 = @event.AlertaFaltando2;
            actividad.Insumo = @event.Insumo;
            actividad.Cantidad = @event.Cantidad;
            return;
        }

        throw new DomainException($"Actividad con ID {@event.ActividadId} no existe en ninguna parte de la rutina");
    }

    public void Apply(ActividadDeRutinaEliminada @event)
    {
        var parte = Partes.FirstOrDefault(p => p.Id == @event.ParteId)
            ?? throw new DomainException($"Parte con ID {@event.ParteId} no existe en la rutina");
        parte.Actividades.RemoveAll(a => a.Id == @event.ActividadId);
    }
}

public class ParteEquipo
{
    public Guid Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public List<ActividadMantenimiento> Actividades { get; set; } = new();
}

public class ActividadMantenimiento
{
    public Guid Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Clase { get; set; } = string.Empty;
    public int Frecuencia { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public string NombreMedidor { get; set; } = string.Empty;    public int AlertaFaltando { get; set; }
    
    public int Frecuencia2 { get; set; }
    public string UnidadMedida2 { get; set; } = string.Empty;
    public string NombreMedidor2 { get; set; } = string.Empty;    public int AlertaFaltando2 { get; set; }

    public string? Insumo { get; set; }
    public double Cantidad { get; set; }
}
