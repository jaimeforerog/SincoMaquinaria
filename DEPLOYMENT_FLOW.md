# Flujo de Despliegue con E2E Tests - SincoMaquinaria

## 📊 Flujo Completo de CI/CD con Protección E2E

```
┌─────────────────────────────────────────────────────────────────┐
│                    PUSH to main/production                      │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│          WORKFLOW 1: CI/CD Pipeline (ci-cd.yml)                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────┐  ┌──────────────────┐                   │
│  │  Backend Test    │  │ Frontend Build   │                   │
│  │  (Unit Tests)    │  │  (Unit Tests)    │                   │
│  └────────┬─────────┘  └────────┬─────────┘                   │
│           │                     │                              │
│           └──────────┬──────────┘                              │
│                      ▼                                          │
│           ┌─────────────────────┐                              │
│           │   E2E Tests         │  🔐 GATE DE CALIDAD         │
│           │   (41+ tests)       │                              │
│           │   - Auth (12)       │                              │
│           │   - Orders (10)     │                              │
│           │   - Equipment (8)   │                              │
│           │   - Import (6)      │                              │
│           │   - Dashboard (5)   │                              │
│           └──────────┬──────────┘                              │
│                      │                                          │
│          ┌───────────┴───────────┐                             │
│          ▼                       ▼                              │
│     ✅ PASS                  ❌ FAIL                            │
│     (Continuar)              (BLOQUEAR)                         │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ ✅ CI/CD Success
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│       WORKFLOW 2: Deploy to Azure (azure-deploy.yml)            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Job 1: Verify CI/CD Success                             │  │
│  │  ✅ Verificar que E2E tests pasaron                      │  │
│  └────────────────────────┬─────────────────────────────────┘  │
│                            ▼                                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Job 2: Build and Test                                   │  │
│  │  - Compilar backend                                      │  │
│  │  - Ejecutar unit tests adicionales                       │  │
│  └────────────────────────┬─────────────────────────────────┘  │
│                            ▼                                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Job 3: Build and Push Docker Image                      │  │
│  │  - Construir imagen Docker                               │  │
│  │  - Push a Azure Container Registry                       │  │
│  └────────────────────────┬─────────────────────────────────┘  │
│                            ▼                                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Job 4: Deploy to Azure Web App                          │  │
│  │  - Configurar credenciales ACR                           │  │
│  │  - Actualizar container image                            │  │
│  │  - Reiniciar Web App                                     │  │
│  │  - Health check                                          │  │
│  └────────────────────────┬─────────────────────────────────┘  │
│                            ▼                                    │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Job 5: Notify                                           │  │
│  │  ✅ Deployment Success / ❌ Deployment Failed            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                         │
                         ▼
              🎉 DEPLOYED TO PRODUCTION
         https://sincomaquinaria-app-1601.azurewebsites.net
```

## 🔐 Protección del Despliegue

### Antes (Sin E2E Tests)
```
Push → Backend Tests → Deploy ❌
```
❌ **Problema:** Errores de integración no detectados (ejemplo: ErrorLog snapshot)

### Ahora (Con E2E Tests)
```
Push → Backend Tests → Frontend Tests → E2E Tests → Deploy ✅
```
✅ **Protección:** No se despliega si algún flujo crítico falla

## 📋 Condiciones para Despliegue

El despliegue a producción **SOLO** ocurre si:

1. ✅ **Backend tests** pasan (Unit tests)
2. ✅ **Frontend tests** pasan (Vitest)
3. ✅ **E2E tests** pasan (41+ tests en Playwright)
4. ✅ **Security scan** completa sin errores críticos
5. ✅ **Build** exitoso (Docker image)

**Si CUALQUIERA falla → ❌ DEPLOYMENT BLOQUEADO**

## 🔄 Triggers del Workflow

### CI/CD Pipeline (ci-cd.yml)
Se ejecuta en:
- ✅ Push a `main` o `develop`
- ✅ Pull Request a `main` o `develop`

### Azure Deploy (azure-deploy.yml)
Se ejecuta en:
- ✅ Después de que CI/CD Pipeline complete exitosamente
- ✅ Manual con `workflow_dispatch`

**NUNCA se ejecuta si CI/CD falla**

## 📊 Métricas de Calidad

### Coverage de Tests

```
┌─────────────────────┬──────────┬────────────┐
│ Tipo de Test        │ Cantidad │ Coverage   │
├─────────────────────┼──────────┼────────────┤
│ Unit Tests Backend  │ 20+      │ ~60%       │
│ Unit Tests Frontend │ 17+      │ ~50%       │
│ E2E Tests           │ 41+      │ 5 flujos   │
│ TOTAL               │ 78+      │ Completo   │
└─────────────────────┴──────────┴────────────┘
```

### Tiempo de Ejecución (Estimado)

```
Backend Tests:    ~2 min
Frontend Tests:   ~1 min
E2E Tests:        ~5 min (paralelo)
Docker Build:     ~3 min
Deploy:           ~2 min
─────────────────────────
TOTAL:           ~13 min
```

## 🚨 Qué Previene el E2E Gate

### Errores que se Detectan ANTES del Deploy:

1. **Errores de Autenticación**
   - Login/logout roto
   - Tokens no válidos
   - Sesiones no persistentes

2. **Errores de Integración Backend-Frontend**
   - API endpoints cambiados
   - Formatos de respuesta incorrectos
   - CORS issues

3. **Errores de UI/UX**
   - Botones que no funcionan
   - Forms con validación rota
   - Navegación incorrecta

4. **Errores de Configuración**
   - Variables de entorno faltantes
   - Conexión a base de datos
   - SignalR/WebSocket issues

5. **Errores de Lógica de Negocio**
   - Creación de órdenes fallida
   - Cálculos incorrectos
   - Permisos no respetados

## 🔧 Cómo Funciona workflow_run

```yaml
on:
  workflow_run:
    workflows: ["CI/CD Pipeline"]
    types:
      - completed
    branches:
      - main
      - production
```

**Esto significa:**
- Azure Deploy se ejecuta **SOLO** cuando CI/CD Pipeline completa
- Verifica que el resultado fue `success`
- Si CI/CD falló, Azure Deploy **NO SE EJECUTA**

## 📝 Verificación del Flujo

### 1. Ver Workflows en GitHub
```
GitHub → Actions → Workflows
```

Deberías ver:
- ✅ CI/CD Pipeline (verde) → E2E tests pasaron
- ✅ Deploy to Azure (verde) → Despliegue exitoso

### 2. Ver Dependencia
```
Deploy to Azure → "Triggered by workflow_run"
```

### 3. Ver Logs
Cada job muestra:
- ✅ Verify CI/CD Success
- ✅ Build and Test
- ✅ Build and Push Docker Image
- ✅ Deploy to Azure Web App
- ✅ Health Check

## 🎯 Resultado Final

```
┌────────────────────────────────────────────────────────────┐
│                   DEPLOYMENT PIPELINE                      │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  Code Push                                                 │
│      ↓                                                     │
│  Unit Tests (Backend + Frontend)                          │
│      ↓                                                     │
│  🔐 E2E Tests (41+ tests) ← GATE DE CALIDAD              │
│      ↓                                                     │
│  Docker Build                                             │
│      ↓                                                     │
│  Azure Deploy                                             │
│      ↓                                                     │
│  Health Check                                             │
│      ↓                                                     │
│  ✅ PRODUCTION                                            │
│                                                            │
└────────────────────────────────────────────────────────────┘

Si E2E falla → ❌ Pipeline se detiene
No se construye Docker, no se despliega nada
```

---

## ✅ Confirmación de Implementación

- ✅ E2E tests en CI/CD Pipeline
- ✅ Azure Deploy espera CI/CD success
- ✅ Verificación de estado antes de deploy
- ✅ Health check después de deploy
- ✅ Notificaciones de estado

**🎉 EL DEPLOYMENT ESTÁ PROTEGIDO POR E2E TESTS**

---

**Última Actualización:** 2026-02-07
**Workflows:** ci-cd.yml + azure-deploy.yml
**Protección:** E2E Tests como gate de calidad
