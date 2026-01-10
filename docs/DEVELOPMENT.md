# Guía de Desarrollo

## Configuración del Entorno

### Requisitos

- .NET 9.0 SDK
- PostgreSQL 14+
- Node.js 18+ (para frontend)
- IDE: Rider o Visual Studio 2022

### Base de Datos

La base de datos se crea automáticamente al iniciar la aplicación.

```bash
# Verificar PostgreSQL
psql -U postgres -c "SELECT version();"
```

---

## Desarrollo Backend

### Crear un Nuevo Endpoint

1. Crear archivo en `Endpoints/`:

```csharp
// Endpoints/MiNuevoEndpoints.cs
public static class MiNuevoEndpoints
{
    public static void MapMiNuevoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/mi-recurso", async (IDocumentSession session) =>
        {
            var items = await session.Query<MiAgregado>().ToListAsync();
            return Results.Ok(items);
        });
    }
}
```

2. Registrar en `Program.cs`:

```csharp
app.MapMiNuevoEndpoints();
```

### Crear un Nuevo Agregado

1. Definir eventos en `Domain/Events.cs`:

```csharp
public record MiAgregadoCreado(Guid Id, string Nombre);
```

2. Crear agregado en `Domain/`:

```csharp
public class MiAgregado
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }

    public void Apply(MiAgregadoCreado @event)
    {
        Id = @event.Id;
        Nombre = @event.Nombre;
    }
}
```

3. Registrar proyección en `Extensions/ServiceCollectionExtensions.cs`:

```csharp
opts.Projections.Snapshot<MiAgregado>(SnapshotLifecycle.Inline);
```

---

## Desarrollo Frontend

### Estructura

```
client-app/src/
├── pages/          # Componentes de página
├── layouts/        # Layouts compartidos
├── types.ts        # Tipos TypeScript
└── App.tsx         # Router principal
```

### Crear Nueva Página

1. Crear componente en `pages/`:

```tsx
// pages/MiPagina.tsx
export default function MiPagina() {
    const [data, setData] = useState([]);

    useEffect(() => {
        fetch('/mi-recurso')
            .then(r => r.json())
            .then(setData);
    }, []);

    return <div>{/* UI */}</div>;
}
```

2. Agregar ruta en `App.tsx`:

```tsx
<Route path="/mi-pagina" element={<MiPagina />} />
```

---

## Tests

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Tests específicos
dotnet test --filter "FullyQualifiedName~OrdenDeTrabajoTests"
```

### Crear Test de Integración

```csharp
public class MiTests : IntegrationContext
{
    public MiTests(IntegrationFixture fixture) : base(fixture) { }

    [Fact]
    public async Task MiTest_DebeHacerAlgo()
    {
        // Arrange
        var evento = new MiAgregadoCreado(Guid.NewGuid(), "Test");
        CurrentSession.Events.StartStream<MiAgregado>(evento.Id, evento);
        await SaveChangesAsync();

        // Act
        var result = await CurrentSession.LoadAsync<MiAgregado>(evento.Id);

        // Assert
        result.Should().NotBeNull();
    }
}
```

---

## Convenciones

### Naming

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Eventos | PasadoParticipativo | `OrdenCreada` |
| Agregados | Sustantivo | `OrdenDeTrabajo` |
| Endpoints | Verbo + Recurso | `CrearOrden` |
| Tests | Metodo_Escenario_Resultado | `Apply_OrdenCreada_CambiaEstado` |

### Commits

```
feat: agregar endpoint de historial
fix: corregir validación de fechas
test: agregar tests de importación
docs: actualizar README
```
