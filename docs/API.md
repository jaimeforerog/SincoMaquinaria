# API Reference

## Base URL

```
http://localhost:5000
```

---

## Órdenes de Trabajo

### Listar Órdenes

```http
GET /ordenes
```

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numero": "OT-2026-001",
    "equipoId": "ABC-123",
    "estado": "Borrador",
    "tipo": "Preventivo",
    "fechaOrden": "2026-01-09"
  }
]
```

---

### Obtener Orden

```http
GET /ordenes/{id}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "numero": "OT-2026-001",
  "estado": "EnEjecucion",
  "detalles": [
    {
      "id": "...",
      "descripcion": "Cambio de aceite",
      "avance": 50,
      "estado": "EnProgreso"
    }
  ]
}
```

---

### Crear Orden

```http
POST /ordenes
Content-Type: application/json

{
  "equipoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tipoMantenimiento": "Preventivo",
  "origen": "Rutina",
  "fechaOrden": "2026-01-09"
}
```

---

### Agregar Actividad

```http
POST /ordenes/{id}/actividades
Content-Type: application/json

{
  "descripcion": "Cambio de filtro",
  "frecuencia": 500,
  "tipoFallaId": null,
  "causaFallaId": null
}
```

---

### Registrar Avance

```http
PUT /ordenes/{ordenId}/actividades/{actividadId}/avance
Content-Type: application/json

{
  "porcentaje": 75,
  "observacion": "En progreso"
}
```

---

### Historial de Eventos

```http
GET /ordenes/{id}/historial
```

**Response:**
```json
[
  {
    "tipo": "OrdenDeTrabajoCreada",
    "fecha": "2026-01-09T10:30:00Z",
    "data": { ... }
  },
  {
    "tipo": "ActividadAgregada",
    "fecha": "2026-01-09T10:35:00Z",
    "data": { ... }
  }
]
```

---

## Equipos

### Listar Equipos

```http
GET /equipos
```

### Importar Equipos

```http
POST /equipos/importar
Content-Type: multipart/form-data

file: <archivo.xlsx>
```

---

## Configuración

### Tipos de Medidor

```http
GET /configuracion/medidores
POST /configuracion/medidores
PUT /configuracion/medidores/{codigo}/estado
```

### Grupos de Mantenimiento

```http
GET /configuracion/grupos
POST /configuracion/grupos
```

### Tipos de Falla

```http
GET /configuracion/tipos-falla
POST /configuracion/tipos-falla
```

### Causas de Falla

```http
GET /configuracion/causas-falla
POST /configuracion/causas-falla
```

---

## Códigos de Error

| Código | Descripción |
|--------|-------------|
| 400 | Datos inválidos |
| 404 | Recurso no encontrado |
| 409 | Conflicto (duplicado) |
| 500 | Error interno |
