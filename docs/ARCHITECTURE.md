# Arquitectura del Sistema

## Event Sourcing

SincoMaquinaria usa **Event Sourcing** como patrón de persistencia principal.

### Conceptos Clave

```
                    ┌─────────────────────────────────────────┐
                    │              Event Store                │
                    │         (PostgreSQL + Marten)           │
                    │                                         │
  Comandos          │   ┌─────────┐  ┌─────────┐  ┌─────────┐│
  ─────────────────▶│   │ Evento1 │──│ Evento2 │──│ Evento3 ││
                    │   └─────────┘  └─────────┘  └─────────┘│
                    │                     │                   │
                    │              Apply  ▼                   │
                    │         ┌───────────────────┐           │
                    │         │   Snapshot        │           │
                    │         │   (Read Model)    │           │
                    │         └───────────────────┘           │
                    └─────────────────────────────────────────┘
                                      │
                                      ▼
                               Queries (Lectura)
```

### Eventos de Dominio

```csharp
// Los eventos son inmutables y describen hechos del pasado
public record OrdenDeTrabajoCreada(
    Guid OrdenId,
    string NumeroOrden,
    string EquipoId,
    string TipoMantenimiento,
    string Origen,
    DateTime FechaOrden,
    DateTimeOffset FechaCreacion
);

public record ActividadAgregada(
    Guid OrdenId,
    Guid ItemDetalleId,
    string Descripcion,
    int Frecuencia,
    string? TipoFallaId,
    string? CausaFallaId
);
```

### Agregados

Un agregado reconstruye su estado aplicando eventos:

```csharp
public class OrdenDeTrabajo
{
    public Guid Id { get; set; }
    public string Estado { get; set; } = "Inexistente";
    public List<DetalleOrden> Detalles { get; set; } = new();

    // Marten llama Apply() automáticamente
    public void Apply(OrdenDeTrabajoCreada @event)
    {
        Id = @event.OrdenId;
        Estado = "Borrador";
    }

    public void Apply(ActividadAgregada @event)
    {
        Detalles.Add(new DetalleOrden { ... });
    }
}
```

### Projections (Inline)

Las proyecciones se actualizan automáticamente al guardar eventos:

```csharp
services.AddMarten(opts =>
{
    opts.Projections.Snapshot<OrdenDeTrabajo>(SnapshotLifecycle.Inline);
    opts.Projections.Snapshot<Equipo>(SnapshotLifecycle.Inline);
});
```

---

## Flujo de una Operación

### Crear Orden de Trabajo

```
1. POST /ordenes { equipoId, tipo, fecha }
          │
          ▼
2. Endpoint valida datos
          │
          ▼
3. session.Events.StartStream<OrdenDeTrabajo>(
       new OrdenDeTrabajoCreada(...)
   );
          │
          ▼
4. session.SaveChangesAsync()
          │
          ▼
5. Marten guarda evento Y actualiza snapshot
```

---

## Estructura de Capas

```
┌─────────────────────────────────────────────────────────┐
│                      Endpoints                          │
│  OrdenesEndpoints, EquiposEndpoints, etc.              │
├─────────────────────────────────────────────────────────┤
│                      Services                           │
│  ExcelImportService, ExcelEquipoImportService          │
├─────────────────────────────────────────────────────────┤
│                       Domain                            │
│  Aggregates: OrdenDeTrabajo, Equipo, Empleado          │
│  Events: OrdenCreada, EquipoMigrado, etc.              │
├─────────────────────────────────────────────────────────┤
│                    Infrastructure                       │
│  Marten + PostgreSQL                                   │
└─────────────────────────────────────────────────────────┘
```

---

## Ventajas del Event Sourcing

| Ventaja | Descripción |
|---------|-------------|
| **Auditoría completa** | Cada cambio queda registrado |
| **Debugging** | Reconstruir estado en cualquier punto |
| **Evolución** | Agregar nuevas proyecciones sin migrar |
| **Temporal queries** | Consultar estado histórico |
