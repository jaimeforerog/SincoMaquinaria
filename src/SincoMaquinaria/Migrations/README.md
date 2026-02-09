# Migraciones de Base de Datos

## Prevención de Placas Duplicadas

### Problema

Se identificó una condición de carrera (race condition) donde múltiples clics rápidos en el botón "Guardar" podían crear equipos duplicados con la misma placa.

**Causa raíz**: Dos solicitudes simultáneas pasaban la validación de existencia antes de que la primera solicitud completara la inserción.

### Solución Implementada

Se implementó una **defensa en dos capas**:

#### 1. Frontend - Prevención de Doble Clic (UX)
- **Archivo modificado**: `client-app/src/pages/EquipmentConfig.tsx`
- **Cambios**:
  - Estado `saving` para rastrear operaciones en curso
  - Botones deshabilitados durante el guardado
  - Indicador visual de progreso (spinner + texto "Creando...")
  - Prevención de múltiples clics simultáneos

#### 2. Backend - Índice Único en Base de Datos (Seguridad)
- **Archivo modificado**: `src/SincoMaquinaria/Extensions/ServiceCollectionExtensions.cs`
- **Cambios**:
  - Índice único en PostgreSQL: `idx_equipo_placa_unique`
  - Manejo de excepción PostgreSQL 23505 (violación de unicidad)
  - Mensaje de error claro al usuario

### Aplicar Migración a Base de Datos Existente

Si ya tienes datos en tu base de datos, sigue estos pasos:

#### Paso 1: Verificar Duplicados

```bash
# Conectar a PostgreSQL
psql -h localhost -U sincoadmin -d SincoMaquinaria

# Ejecutar consulta de verificación
SELECT
    data->>'Placa' as placa,
    COUNT(*) as cantidad
FROM public.mt_doc_equipo
GROUP BY data->>'Placa'
HAVING COUNT(*) > 1;
```

#### Paso 2A: Si NO hay duplicados

```bash
# Ejecutar script de migración directamente
psql -h localhost -U sincoadmin -d SincoMaquinaria -f add_unique_placa_index.sql
```

#### Paso 2B: Si HAY duplicados

1. **Revisar duplicados en detalle**:
```bash
psql -h localhost -U sincoadmin -d SincoMaquinaria -f cleanup_duplicate_placas.sql
```

2. **Decidir qué hacer**:
   - **Opción A**: Eliminar duplicados automáticamente (mantiene el más reciente)
     - Editar `cleanup_duplicate_placas.sql`
     - Descomentar el bloque DELETE
     - Ejecutar: `psql ... -f cleanup_duplicate_placas.sql`

   - **Opción B**: Eliminar manualmente desde la UI
     - Ir a Configuración → Equipos
     - Identificar y eliminar duplicados uno por uno

   - **Opción C**: Modificar placas duplicadas
     - Agregar sufijo a duplicados (ej: ABC123 → ABC123-2)

3. **Aplicar índice único**:
```bash
psql -h localhost -U sincoadmin -d SincoMaquinaria -f add_unique_placa_index.sql
```

### Verificación Post-Migración

1. **Verificar que el índice existe**:
```sql
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'mt_doc_equipo'
AND indexname = 'idx_equipo_placa_unique';
```

Resultado esperado:
```
indexname                | indexdef
-------------------------+--------------------------------------------------
idx_equipo_placa_unique  | CREATE UNIQUE INDEX ... ON mt_doc_equipo ...
```

2. **Probar creación de duplicado** (debe fallar):
```bash
# En la UI, intentar crear dos equipos con la misma placa
# Resultado esperado: "Ya existe un equipo con la placa ABC123"
```

### Migraciones para Ambientes

#### Desarrollo Local
```bash
cd src/SincoMaquinaria/Migrations
psql -h localhost -U sincoadmin -d SincoMaquinaria -f add_unique_placa_index.sql
```

#### Docker
```bash
# Copiar scripts al contenedor
docker cp add_unique_placa_index.sql sincomaquinaria-db:/tmp/

# Ejecutar en contenedor
docker exec -it sincomaquinaria-db psql -U sincoadmin -d SincoMaquinaria -f /tmp/add_unique_placa_index.sql
```

#### Azure PostgreSQL
```bash
# Usar conexión directa
psql "host=sincomaquinaria-db-server-1601.postgres.database.azure.com port=5432 dbname=SincoMaquinaria user=sincoadmin sslmode=require" -f add_unique_placa_index.sql
```

### Rollback (Si es necesario)

Para revertir el índice único:

```sql
DROP INDEX IF EXISTS idx_equipo_placa_unique;
```

**ADVERTENCIA**: Solo hacer rollback si es absolutamente necesario. Remover el índice permite volver a crear duplicados.

### Notas Técnicas

- **Performance**: El índice único es muy eficiente (O(log n)) y no afecta negativamente el rendimiento
- **Espacio en disco**: El índice consume espacio adicional mínimo (~1-2% del tamaño de la tabla)
- **Nuevas instalaciones**: Marten creará automáticamente el índice único al crear la base de datos
- **Case sensitivity**: El índice es case-sensitive. "ABC123" ≠ "abc123" son consideradas diferentes

### FAQ

**P: ¿Qué pasa si intento crear un equipo con placa duplicada?**
R: Recibirás un mensaje de error claro: "Ya existe un equipo con la placa {placa}"

**P: ¿Puedo modificar la placa de un equipo existente?**
R: Sí, siempre que la nueva placa no exista en otro equipo.

**P: ¿El índice afecta las importaciones de Excel?**
R: Sí, las importaciones también respetarán el índice único y reportarán errores para placas duplicadas.

**P: ¿Qué pasa si hay espacios en blanco en la placa?**
R: "ABC123" y "ABC123 " son consideradas diferentes. Recomendamos usar `.Trim()` en el frontend.

### Historial de Cambios

- **2026-02-06**: Implementación inicial de índice único para prevenir placas duplicadas
