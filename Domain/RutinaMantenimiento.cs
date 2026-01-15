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
    string NombreMedidor, // Nuevo
    int AlertaFaltando,
    
    int Frecuencia2,
    string UnidadMedida2,
    string NombreMedidor2, // Nuevo
    int AlertaFaltando2,

    string? Insumo,
    double Cantidad,
    Guid ParteId
);

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
        var parte = Partes.FirstOrDefault(p => p.Id == @event.ParteId);
        if (parte != null)
        {
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
    public string NombreMedidor { get; set; } = string.Empty; // Nuevo
    public int AlertaFaltando { get; set; }
    
    public int Frecuencia2 { get; set; }
    public string UnidadMedida2 { get; set; } = string.Empty;
    public string NombreMedidor2 { get; set; } = string.Empty; // Nuevo
    public int AlertaFaltando2 { get; set; }

    public string? Insumo { get; set; }
    public double Cantidad { get; set; }
}
