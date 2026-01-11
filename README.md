# SincoMaquinaria

Sistema de Gestión de Mantenimiento de Maquinaria basado en Event Sourcing con autenticación JWT y arquitectura moderna.

## 🚀 Quick Start

### Prerrequisitos

- .NET 9.0 SDK
- PostgreSQL 14+
- Node.js 20+
- Docker & Docker Compose (opcional)

### Instalación - Desarrollo Local

```bash
# 1. Clonar repositorio
git clone <repo-url>
cd SincoMaquinaria

# 2. Configurar variables de entorno
cp .env.example .env

# 3. Generar JWT Key (Linux/Mac)
openssl rand -base64 64
# Windows PowerShell:
# [Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))

# 4. Editar .env con tu JWT key y credenciales de base de datos

# 5. Iniciar backend
dotnet run

# 6. Crear primer usuario admin (solo primera vez)
curl -X POST http://localhost:5000/auth/setup \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@sinco.com","password":"Admin123!","nombre":"Administrador"}'

# 7. Iniciar frontend (en otra terminal)
cd client-app
npm install
npm run dev
```

### Instalación - Docker (Recomendado)

```bash
# 1. Configurar variables de entorno
cp .env.example .env
# Editar .env con tus valores

# 2. Construir y ejecutar
docker-compose up -d

# 3. Ver logs
docker-compose logs -f backend

# 4. Crear usuario admin
curl -X POST http://localhost:5000/auth/setup \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@sinco.com","password":"Admin123!","nombre":"Administrador"}'
```

### URLs

| Servicio | URL | Descripción |
|----------|-----|-------------|
| API Backend | http://localhost:5000 | Backend REST API |
| Frontend (Vite) | http://localhost:5173 | Aplicación React (solo en modo dev local) |
| Swagger | http://localhost:5000/swagger | Documentación API |
| Frontend (Docker) | http://localhost:5000 | Frontend servido por backend en Docker |

> **Nota:** Consulta [README.Security.md](README.Security.md) para configuración de producción y mejores prácticas de seguridad.

---

## 📁 Estructura del Proyecto

```
SincoMaquinaria/
├── Domain/                          # Agregados y eventos (Event Sourcing)
│   ├── OrdenDeTrabajo.cs           # Órdenes de trabajo
│   ├── Equipo.cs                   # Equipos/Maquinaria
│   ├── Empleado.cs                 # Empleados
│   ├── RutinaMantenimiento.cs      # Rutinas de mantenimiento
│   ├── ConfiguracionGlobal.cs      # Configuración singleton
│   ├── Usuario.cs                  # ⭐ Usuarios del sistema
│   ├── Events.cs                   # Eventos de dominio
│   ├── DomainException.cs          # ⭐ Excepciones personalizadas
│   └── Enums.cs                    # ⭐ Enumeraciones compartidas
├── Endpoints/                       # Minimal API endpoints
│   ├── AuthEndpoints.cs            # ⭐ Autenticación JWT
│   ├── OrdenesEndpoints.cs         # Órdenes de trabajo
│   ├── EquiposEndpoints.cs         # Gestión de equipos
│   ├── EmpleadosEndpoints.cs       # Gestión de empleados
│   ├── RutinasEndpoints.cs         # Rutinas de mantenimiento
│   ├── ConfiguracionEndpoints.cs   # Configuración global
│   └── AdminEndpoints.cs           # Endpoints administrativos
├── Services/                        # Servicios de aplicación
│   ├── ExcelEmpleadoImportService.cs  # Importación de empleados
│   ├── ExcelEquipoImportService.cs    # Importación de equipos
│   └── JwtService.cs               # ⭐ Generación y validación de JWT
├── DTOs/Requests/                   # ⭐ DTOs de solicitud
│   └── AuthRequests.cs             # DTOs de autenticación
├── Validators/                      # ⭐ Validadores FluentValidation
│   ├── OrdenValidators.cs
│   ├── EquipoValidators.cs
│   ├── EmpleadoValidators.cs
│   └── ConfiguracionValidators.cs
├── Infrastructure/                  # ⭐ Infraestructura
│   └── ValidationFilter.cs         # Filtro de validación automática
├── Middleware/                      # Middleware personalizado
│   ├── ExceptionMiddleware.cs      # Manejo global de excepciones
│   └── SecurityHeadersMiddleware.cs # ⭐ Headers de seguridad
├── Extensions/                      # Extensiones de configuración
│   ├── ServiceCollectionExtensions.cs  # DI & servicios
│   └── WebApplicationExtensions.cs     # Pipeline HTTP
├── client-app/                      # ⭐ Frontend React + TypeScript
│   ├── src/
│   │   ├── components/             # Componentes reutilizables
│   │   ├── contexts/               # Context API (AuthContext)
│   │   ├── hooks/                  # Hooks personalizados (useAuthFetch)
│   │   ├── pages/                  # Páginas de la aplicación
│   │   │   ├── Login.tsx           # ⭐ Página de login
│   │   │   ├── UserManagement.tsx  # ⭐ Gestión de usuarios
│   │   │   ├── Dashboard.tsx
│   │   │   ├── EmployeeConfig.tsx
│   │   │   ├── EquipmentConfig.tsx
│   │   │   └── ImportarRutinas.tsx
│   │   └── layouts/                # Layouts de la aplicación
│   └── vite.config.ts              # Configuración Vite
├── SincoMaquinaria.Tests/          # Suite de tests
│   ├── Domain/                     # Tests unitarios de dominio
│   ├── Integration/                # Tests de integración
│   └── Services/                   # Tests de servicios
├── Dockerfile                       # ⭐ Multi-stage build
├── docker-compose.yml               # ⭐ Configuración Docker desarrollo
├── docker-compose.prod.yml          # ⭐ Configuración Docker producción
├── .env.example                     # ⭐ Template de variables de entorno
├── appsettings.json                 # Configuración base
├── appsettings.Docker.json          # ⭐ Configuración para Docker
├── README.Security.md               # ⭐ Guía de seguridad
└── SECURITY_IMPROVEMENTS.md         # ⭐ Resumen de mejoras de seguridad

⭐ = Archivos/características añadidos recientemente
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

| Agregado | Descripción | Event Sourcing |
|----------|-------------|----------------|
| `OrdenDeTrabajo` | Órdenes de trabajo con actividades | ✅ |
| `Equipo` | Equipos/maquinaria con mediciones | ✅ |
| `Empleado` | Personal de mantenimiento | ✅ |
| `RutinaMantenimiento` | Plantillas de mantenimiento | ✅ |
| `ConfiguracionGlobal` | Tipos de medidor, grupos, etc. (Singleton) | ✅ |
| `Usuario` | ⭐ Usuarios del sistema con roles | ✅ |

### Características de Seguridad

| Característica | Estado | Descripción |
|----------------|--------|-------------|
| **Autenticación JWT** | ✅ | Tokens JWT con expiración configurable |
| **HTTPS** | ✅ | Redirección automática en producción + HSTS |
| **Rate Limiting** | ✅ | Protección contra brute force y DoS |
| **Security Headers** | ✅ | CSP, X-Frame-Options, HSTS, etc. |
| **Input Validation** | ✅ | FluentValidation en todos los endpoints |
| **Secrets Management** | ✅ | Variables de entorno (nunca en código) |
| **Password Hashing** | ✅ | BCrypt con salt automático |
| **Autorización basada en roles** | ✅ | Admin/User policies |

> **Consulta [SECURITY_IMPROVEMENTS.md](SECURITY_IMPROVEMENTS.md)** para detalles completos de seguridad.

---

## 🔌 API Endpoints

### ⭐ Autenticación (Nuevo)

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/auth/setup` | - | Crear primer administrador (solo funciona si no hay usuarios) |
| POST | `/auth/login` | - | Iniciar sesión y obtener JWT token |
| POST | `/auth/register` | Admin | Registrar nuevo usuario (solo admins) |
| GET | `/auth/me` | User | Obtener información del usuario actual |
| GET | `/auth/users` | Admin | Listar todos los usuarios |

**Ejemplo de Login:**
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@sinco.com","password":"Admin123!"}'

# Respuesta:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-11T00:00:00Z",
  "email": "admin@sinco.com",
  "nombre": "Administrador",
  "rol": "Admin"
}
```

### Órdenes de Trabajo

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/ordenes` | User | Listar órdenes |
| GET | `/ordenes/{id}` | User | Obtener orden |
| POST | `/ordenes` | User | Crear orden |
| POST | `/ordenes/{id}/actividades` | User | Agregar actividad |
| PUT | `/ordenes/{id}/actividades/{actId}/avance` | User | Registrar avance |
| GET | `/ordenes/{id}/historial` | User | Historial de eventos |

### Equipos

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/equipos` | User | Listar equipos |
| PUT | `/equipos/{id}` | User | Actualizar equipo |
| POST | `/equipos/importar` | User | Importar desde Excel |

### Empleados

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/empleados` | User | Listar empleados |
| POST | `/empleados/importar` | User | Importar desde Excel |

### Rutinas de Mantenimiento

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/rutinas` | User | Listar rutinas |
| POST | `/rutinas/importar` | User | Importar desde Excel |

### Configuración

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/configuracion/medidores` | User | Tipos de medidor |
| POST | `/configuracion/medidores` | Admin | Crear tipo de medidor |
| GET | `/configuracion/grupos` | User | Grupos de mantenimiento |
| POST | `/configuracion/grupos` | Admin | Crear grupo |
| GET | `/configuracion/tipos-falla` | User | Tipos de falla |
| POST | `/configuracion/tipos-falla` | Admin | Crear tipo de falla |
| GET | `/configuracion/causas-falla` | User | Causas de falla |
| POST | `/configuracion/causas-falla` | Admin | Crear causa de falla |

> **Nota:** Todos los endpoints (excepto `/auth/login` y `/auth/setup`) requieren autenticación JWT.
> Incluir el header: `Authorization: Bearer <token>`

### ⭐ Paginación (Nuevo)

Todos los endpoints de listado soportan paginación mediante query parameters:

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `page` | int | 1 | Número de página (1-indexed) |
| `pageSize` | int | 20 | Elementos por página (max: 100) |
| `orderBy` | string | - | Campo para ordenar (opcional) |
| `orderDirection` | string | "asc" | Dirección: "asc" o "desc" |

**Ejemplo de uso:**
```bash
GET /ordenes?page=1&pageSize=10&orderBy=Numero&orderDirection=desc
```

**Respuesta paginada:**
```json
{
  "data": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 150,
  "totalPages": 15,
  "hasPrevious": false,
  "hasNext": true
}
```

**Endpoints con paginación:**
- `GET /ordenes`
- `GET /equipos`
- `GET /empleados`
- `GET /rutinas`

---

## 🧪 Tests & Code Coverage

### Ejecutar Tests

```bash
# Ejecutar todos los tests
dotnet test

# Con cobertura de código
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Usando el script (genera reporte HTML)
# Windows
.\run-tests-with-coverage.ps1

# Linux/Mac
chmod +x run-tests-with-coverage.sh
./run-tests-with-coverage.sh
```

### Métricas de Tests

| Métrica | Valor |
|---------|-------|
| **Tests totales** | 76 tests |
| **Cobertura** | 80.63% |
| **Framework** | xUnit + FluentAssertions |
| **Tests de integración** | ✅ Con PostgreSQL real |
| **Code Coverage Tool** | ⭐ Coverlet |

### Visualizar Reporte de Coverage

Después de ejecutar el script, abre el reporte HTML:
```bash
# Windows
start coverage-report/index.html

# Mac
open coverage-report/index.html

# Linux
xdg-open coverage-report/index.html
```

### CI/CD Pipeline

El proyecto incluye un workflow completo de GitHub Actions:

**Triggers:**
- Push a `main` y `develop`
- Pull Requests

**Jobs:**
1. **Backend Build & Test** - Build, tests con coverage, upload a Codecov
2. **Frontend Build & Test** - ESLint, Vitest, build de producción
3. **Docker Build** - Multi-stage build y push a Docker Hub (solo en main)
4. **Security Scan** - Trivy vulnerability scanner
5. **Code Quality** - Análisis de calidad (SonarCloud opcional)

**Ver configuración:** [.github/workflows/ci-cd.yml](.github/workflows/ci-cd.yml)

### Configurar CI/CD

1. Agregar secrets en GitHub:
   ```
   DOCKER_HUB_USERNAME
   DOCKER_HUB_TOKEN
   CODECOV_TOKEN (opcional)
   ```

2. El pipeline se ejecuta automáticamente en cada push/PR

3. Ver status del pipeline en la pestaña "Actions"

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

| Capa | Tecnología | Versión |
|------|------------|---------|
| **Backend** | .NET | 9.0 |
| **API Pattern** | Minimal APIs | - |
| **Event Store** | Marten | Latest |
| **Base de Datos** | PostgreSQL | 14+ |
| **Autenticación** | ⭐ JWT (System.IdentityModel.Tokens.Jwt) | Latest |
| **Password Hashing** | ⭐ BCrypt.Net-Next | Latest |
| **Validación** | ⭐ FluentValidation | Latest |
| **Rate Limiting** | ⭐ AspNetCoreRateLimit | 5.0.0 |
| **Frontend** | React | 18 |
| **Frontend Build** | Vite | Latest |
| **Lenguaje Frontend** | TypeScript | Latest |
| **State Management** | ⭐ React Context API | - |
| **Excel Import** | EPPlus | Latest |
| **Tests** | xUnit | Latest |
| **Assertions** | FluentAssertions | Latest |
| **Containerization** | ⭐ Docker + Docker Compose | - |

---

## 🐳 Docker

### Características del Dockerfile

- **Multi-stage build:** Optimiza el tamaño final de la imagen
  - Stage 1: Construcción del frontend (Node.js)
  - Stage 2: Compilación del backend (.NET SDK)
  - Stage 3: Runtime (ASP.NET Runtime)
- **Tamaño optimizado:** Solo contiene lo necesario para producción
- **Health checks:** Verificación automática del estado de la aplicación
- **Frontend estático:** Servido por el backend en `/wwwroot`

### Comandos Docker

```bash
# Desarrollo
docker-compose up -d                # Iniciar en modo detached
docker-compose logs -f backend      # Ver logs en tiempo real
docker-compose down                 # Detener y eliminar contenedores
docker-compose restart backend      # Reiniciar solo el backend

# Producción
docker-compose -f docker-compose.prod.yml up -d
docker-compose -f docker-compose.prod.yml logs -f
```

### Variables de Entorno

Consulta `.env.example` para la lista completa de variables de entorno disponibles.

**Variables críticas:**
- `Jwt__Key` - Clave secreta para JWT (REQUERIDO)
- `ConnectionStrings__DefaultConnection` - Cadena de conexión PostgreSQL
- `ASPNETCORE_ENVIRONMENT` - `Development` o `Production`

---

## 📚 Documentación Adicional

| Documento | Descripción |
|-----------|-------------|
| [README.Security.md](README.Security.md) | Guía completa de configuración de seguridad |
| [SECURITY_IMPROVEMENTS.md](SECURITY_IMPROVEMENTS.md) | Resumen de mejoras de seguridad implementadas |
| `.env.example` | Template de variables de entorno |

---

## 🚀 Despliegue

### Producción - Checklist

- [ ] Generar JWT key fuerte (64+ caracteres)
- [ ] Configurar PostgreSQL con SSL
- [ ] Establecer `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configurar HTTPS con certificados válidos
- [ ] Actualizar `AllowedOrigins` a dominios de producción
- [ ] Establecer `EnableAdminEndpoints=false`
- [ ] Configurar backups de la base de datos
- [ ] Implementar logging y monitoreo
- [ ] Revisar y ajustar límites de rate limiting

**Consulta [README.Security.md](README.Security.md) para instrucciones detalladas de producción.**

---

## 📄 Licencia

Proyecto propietario - Sinco S.A.
