# SincoMaquinaria

Sistema de Gestión de Mantenimiento de Maquinaria basado en Event Sourcing.

## 🚀 Quick Start

### Prerrequisitos

- .NET 9.0 SDK
- PostgreSQL 14+
- Node.js 18+

### Instalación

```bash
# 1. Clonar repositorio
git clone <repo-url>
cd SincoMaquinaria

# 2. Configurar base de datos
# Editar appsettings.json con tu conexión PostgreSQL

# 3. Iniciar backend
dotnet run

# 4. Iniciar frontend (en otra terminal)
cd client-app
npm install
npm run dev
```

### URLs

| Servicio | URL |
|----------|-----|
| API Backend | http://localhost:5000 |
| Frontend (Vite) | http://localhost:5173 |
| Swagger | http://localhost:5000/swagger |

---

## 📁 Estructura del Proyecto

```
SincoMaquinaria/
├── Domain/                    # Agregados y eventos
│   ├── OrdenDeTrabajo.cs     # Órdenes de trabajo
│   ├── Equipo.cs             # Equipos/Maquinaria
│   ├── Empleado.cs           # Empleados
│   ├── RutinaMantenimiento.cs# Rutinas de mantenimiento
│   ├── ConfiguracionGlobal.cs# Configuración singleton
│   └── Events.cs             # Eventos de dominio
├── Endpoints/                 # Minimal API endpoints
├── Services/                  # Servicios de importación Excel
├── Extensions/               # Configuración de servicios
├── Middleware/               # Exception handling
├── client-app/               # React + Vite frontend
└── SincoMaquinaria.Tests/    # Tests de integración
```

---

## 🏗️ Arquitectura

### Event Sourcing con Marten

El proyecto usa **Marten** como Event Store sobre PostgreSQL:

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   API       │───▶│  Agregados  │───▶│   Marten    │
│  Endpoints  │    │  (Domain)   │    │ PostgreSQL  │
└─────────────┘    └─────────────┘    └─────────────┘
                          │
                     Apply Events
                          │
                   ┌──────▼──────┐
                   │ Projections │
                   │  (Inline)   │
                   └─────────────┘
```

### Agregados Principales

| Agregado | Descripción |
|----------|-------------|
| `OrdenDeTrabajo` | Órdenes de trabajo con actividades |
| `Equipo` | Equipos/maquinaria con mediciones |
| `Empleado` | Personal de mantenimiento |
| `RutinaMantenimiento` | Plantillas de mantenimiento |
| `ConfiguracionGlobal` | Tipos de medidor, grupos, etc. |

---

## 🔌 API Endpoints

### Órdenes de Trabajo

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/ordenes` | Listar órdenes |
| GET | `/ordenes/{id}` | Obtener orden |
| POST | `/ordenes` | Crear orden |
| POST | `/ordenes/{id}/actividades` | Agregar actividad |
| PUT | `/ordenes/{id}/actividades/{actId}/avance` | Registrar avance |
| GET | `/ordenes/{id}/historial` | Historial de eventos |

### Equipos

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/equipos` | Listar equipos |
| PUT | `/equipos/{id}` | Actualizar equipo |
| POST | `/equipos/importar` | Importar desde Excel |

### Configuración

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/configuracion/medidores` | Tipos de medidor |
| GET | `/configuracion/grupos` | Grupos de mantenimiento |
| GET | `/configuracion/tipos-falla` | Tipos de falla |
| GET | `/configuracion/causas-falla` | Causas de falla |

---

## 🧪 Tests

```bash
# Ejecutar todos los tests
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

**Cobertura actual**: 80.63% (76 tests)

---

## ⚙️ Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "host=localhost;database=SincoMaquinaria;password=postgres;username=postgres"
  },
  "Security": {
    "MaxFileUploadSizeMB": 10,
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

---

## 📊 Stack Tecnológico

| Capa | Tecnología |
|------|------------|
| Backend | .NET 9, Minimal APIs |
| Event Store | Marten + PostgreSQL |
| Frontend | React 18, TypeScript, Vite |
| Tests | xUnit, FluentAssertions |

---

## 📄 Licencia

Proyecto propietario - Sinco S.A.
