# SincoMaquinaria - Despliegue en Azure

Este directorio contiene la infraestructura como código (Infrastructure as Code) y scripts de despliegue para Azure.

## 📋 Tabla de Contenidos

- [Arquitectura](#arquitectura)
- [Prerequisitos](#prerequisitos)
- [Recursos de Azure](#recursos-de-azure)
- [Despliegue Manual](#despliegue-manual)
- [Despliegue Automático con GitHub Actions](#despliegue-automático-con-github-actions)
- [Configuración de Secretos](#configuración-de-secretos)
- [Monitoreo y Logs](#monitoreo-y-logs)
- [Troubleshooting](#troubleshooting)
- [Costos Estimados](#costos-estimados)

## 🏗️ Arquitectura

La aplicación se despliega en Azure con la siguiente arquitectura:

```
┌─────────────────────────────────────────────────────────────┐
│                     Azure Container Apps                     │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  SincoMaquinaria App (Backend + Frontend)            │   │
│  │  - .NET 9 Backend (Minimal APIs)                     │   │
│  │  - React 19 Frontend (SPA)                           │   │
│  │  - Autoscaling: 1-5 replicas                         │   │
│  │  - Health checks & Probes                            │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                    ↓               ↓               ↓
        ┌───────────────┐  ┌──────────────┐  ┌──────────────┐
        │  PostgreSQL   │  │    Redis     │  │ App Insights │
        │  Flexible     │  │    Cache     │  │   Logging    │
        │  Server       │  │              │  │              │
        └───────────────┘  └──────────────┘  └──────────────┘
```

## ✅ Prerequisitos

### Software Requerido

1. **Azure CLI** (versión 2.50+)
   ```bash
   # Windows (PowerShell)
   winget install Microsoft.AzureCLI

   # macOS
   brew install azure-cli

   # Linux
   curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   ```

2. **Bicep CLI** (se instala automáticamente con Azure CLI)
   ```bash
   az bicep install
   az bicep version
   ```

3. **Docker** (opcional, para builds locales)
   ```bash
   docker --version
   ```

### Cuenta de Azure

- Suscripción activa de Azure
- Permisos de Contributor o superior en el Resource Group
- Suficientes cuotas para:
  - Azure Container Apps
  - Azure Database for PostgreSQL
  - Azure Cache for Redis
  - Azure Container Registry

## 🚀 Recursos de Azure

El template Bicep crea los siguientes recursos:

| Recurso | Tipo | SKU/Tier | Propósito |
|---------|------|----------|-----------|
| Container Registry | ACR | Basic | Almacenamiento de imágenes Docker |
| Container Apps Environment | Managed Environment | Consumption | Entorno de ejecución |
| Container App | Container App | 1.0 vCPU, 2 GiB RAM | Aplicación principal |
| PostgreSQL Server | Flexible Server | Standard_B2s | Base de datos principal |
| Redis Cache | Azure Cache for Redis | Basic C0 | Cache distribuido |
| Log Analytics | Workspace | Pay-as-you-go | Logs centralizados |
| Application Insights | APM | - | Monitoreo y métricas |

## 📦 Despliegue Manual

### Paso 1: Configurar Variables de Entorno

```bash
# Copiar archivo de ejemplo
cp .env.example .env

# Editar .env con tus valores
nano .env
```

Variables requeridas en `.env`:

```bash
RESOURCE_GROUP=rg-sincomaquinaria-prod
LOCATION=eastus
ENVIRONMENT=prod
BASE_NAME=sincomaquinaria
POSTGRES_ADMIN_PASSWORD=YourSecurePassword123!
JWT_SECRET_KEY=YourVerySecureJwtSecretKeyHere
```

### Paso 2: Login en Azure

```bash
# Login interactivo
az login

# Seleccionar suscripción (si tienes múltiples)
az account list --output table
az account set --subscription "YOUR_SUBSCRIPTION_ID"
```

### Paso 3: Ejecutar Script de Despliegue

**En Linux/macOS:**

```bash
cd infrastructure/azure
chmod +x deploy.sh
./deploy.sh
```

**En Windows (PowerShell):**

```powershell
cd infrastructure\azure
.\deploy.ps1
```

### Paso 4: Build y Push de la Imagen Docker

Una vez desplegada la infraestructura:

```bash
# Obtener nombre del ACR
ACR_NAME=$(az acr list --resource-group rg-sincomaquinaria-prod --query "[0].name" -o tsv)

# Login al ACR
az acr login --name $ACR_NAME

# Build y Push
cd ../..  # Volver al root del proyecto
docker build -t $ACR_NAME.azurecr.io/sincomaquinaria:latest .
docker push $ACR_NAME.azurecr.io/sincomaquinaria:latest
```

El Container App se actualizará automáticamente con la nueva imagen.

## 🤖 Despliegue Automático con GitHub Actions

### Configuración Inicial

#### 1. Crear Service Principal

```bash
# Crear Service Principal con permisos de Contributor
az ad sp create-for-rbac \
  --name "sincomaquinaria-github-actions" \
  --role contributor \
  --scopes /subscriptions/{SUBSCRIPTION_ID}/resourceGroups/rg-sincomaquinaria-prod \
  --sdk-auth
```

Esto retornará un JSON similar a:

```json
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "...",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

**⚠️ Guarda este JSON completo para el siguiente paso.**

#### 2. Configurar Secrets en GitHub

Ve a tu repositorio en GitHub: `Settings > Secrets and variables > Actions > New repository secret`

Crea los siguientes secrets:

| Secret Name | Value |
|-------------|-------|
| `AZURE_CREDENTIALS` | JSON completo del Service Principal |
| `POSTGRES_ADMIN_PASSWORD` | Password seguro para PostgreSQL |
| `JWT_SECRET_KEY` | Clave secreta para JWT (min 32 caracteres) |

#### 3. Ejecutar Despliegue Inicial Manual

La primera vez, necesitas desplegar la infraestructura manualmente (Paso 1-3 de Despliegue Manual).

#### 4. Triggers Automáticos

Una vez configurado, el pipeline se ejecuta automáticamente cuando:

- Haces push a la rama `main` o `production`
- Manualmente desde el tab "Actions" en GitHub

### Pipeline Workflow

El pipeline de GitHub Actions tiene 4 jobs:

1. **Build and Test**: Compila y ejecuta tests
2. **Build and Push**: Construye imagen Docker y la sube al ACR
3. **Deploy**: Actualiza el Container App con la nueva imagen
4. **Notify**: Notifica el resultado del despliegue

## 🔐 Configuración de Secretos

### Secrets Requeridos

| Secret | Descripción | Ejemplo |
|--------|-------------|---------|
| `POSTGRES_ADMIN_PASSWORD` | Password del admin de PostgreSQL | `MySecurePass123!` |
| `JWT_SECRET_KEY` | Clave secreta para JWT | `YourVerySecureJwtKeyHere32Chars` |
| `AZURE_CREDENTIALS` | Credenciales del Service Principal | Ver JSON arriba |

### Rotar Secretos

Para rotar secretos en producción:

```bash
# 1. Actualizar en Azure Container App
az containerapp secret set \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --secrets jwt-key="NEW_JWT_SECRET_KEY"

# 2. Reiniciar aplicación
az containerapp revision restart \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod
```

## 📊 Monitoreo y Logs

### Application Insights

Accede al dashboard de Application Insights:

```bash
# Obtener Instrumentation Key
az monitor app-insights component show \
  --app ai-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query instrumentationKey
```

URL del portal: [Azure Portal - Application Insights](https://portal.azure.com)

### Logs en Tiempo Real

```bash
# Ver logs del Container App
az containerapp logs show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --follow
```

### Log Analytics Queries

Ejemplos de queries útiles:

```kusto
// Errores en las últimas 24 horas
ContainerAppConsoleLogs_CL
| where TimeGenerated > ago(24h)
| where Log_s contains "error"
| project TimeGenerated, Log_s
| order by TimeGenerated desc

// Requests HTTP
AppRequests
| where TimeGenerated > ago(1h)
| summarize count() by resultCode, bin(timestamp, 5m)
| render timechart
```

## 🔧 Troubleshooting

### Problema: Container App no inicia

**Solución:**

```bash
# Ver logs detallados
az containerapp logs show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --tail 100

# Verificar configuración de secrets
az containerapp secret list \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod
```

### Problema: Error de conexión a PostgreSQL

**Solución:**

```bash
# Verificar firewall rules
az postgres flexible-server firewall-rule list \
  --resource-group rg-sincomaquinaria-prod \
  --name psql-sincomaquinaria-prod

# Agregar regla para Azure Services si no existe
az postgres flexible-server firewall-rule create \
  --resource-group rg-sincomaquinaria-prod \
  --name psql-sincomaquinaria-prod \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Problema: Redis timeout

**Solución:**

```bash
# Verificar estado de Redis
az redis show \
  --name redis-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query provisioningState

# Reiniciar Redis (causa downtime)
az redis force-reboot \
  --name redis-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --reboot-type AllNodes
```

### Problema: Health check failing

**Solución:**

```bash
# Verificar endpoint de health directamente
CONTAINER_APP_URL=$(az containerapp show \
  --name ca-sincomaquinaria-prod \
  --resource-group rg-sincomaquinaria-prod \
  --query properties.configuration.ingress.fqdn \
  -o tsv)

curl -v https://$CONTAINER_APP_URL/health
```

## 💰 Costos Estimados

Costos mensuales aproximados (región East US):

| Recurso | SKU | Costo Mensual (USD) |
|---------|-----|---------------------|
| Container App | 1 vCPU, 2 GiB RAM | ~$50-70 |
| PostgreSQL Flexible | Standard_B2s, 32 GB | ~$40-50 |
| Redis Cache | Basic C0 | ~$16 |
| Container Registry | Basic | ~$5 |
| Log Analytics | Pay-as-you-go | ~$10-20 |
| **Total Estimado** | | **~$120-160/mes** |

**Notas:**
- Los costos pueden variar según uso real y región
- Container Apps escala automáticamente (1-5 replicas)
- Usa Azure Cost Calculator para estimaciones precisas: https://azure.microsoft.com/pricing/calculator/

### Optimización de Costos

Para reducir costos en entornos dev/staging:

```bash
# Usar tier Burstable para PostgreSQL
# Reducir min replicas a 0 (scale-to-zero)
# Deshabilitar Redis en desarrollo
```

## 🔄 Actualizar Infraestructura

Para actualizar la infraestructura (cambiar SKUs, agregar recursos, etc.):

```bash
# 1. Modificar main.bicep o parameters.json

# 2. Ejecutar despliegue nuevamente
./deploy.sh

# O manualmente
az deployment group create \
  --name sincomaquinaria-update-$(date +%Y%m%d-%H%M%S) \
  --resource-group rg-sincomaquinaria-prod \
  --template-file main.bicep \
  --parameters parameters.json
```

## 📝 Notas Adicionales

### Regiones Recomendadas

- **East US**: Mejor precio/performance
- **West Europe**: Para clientes europeos
- **Brazil South**: Para clientes latinoamericanos

### Backup y Disaster Recovery

PostgreSQL tiene backup automático configurado:
- Retención: 7 días
- Geo-redundancia: Deshabilitada (habilitar en producción)

Para habilitar geo-redundancia:

```bash
az postgres flexible-server update \
  --resource-group rg-sincomaquinaria-prod \
  --name psql-sincomaquinaria-prod \
  --backup-retention 14 \
  --geo-redundant-backup Enabled
```

### SSL/TLS

Todos los servicios usan TLS 1.2+:
- Container Apps: HTTPS automático
- PostgreSQL: SSL requerido
- Redis: SSL habilitado

## 📚 Referencias

- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Azure Database for PostgreSQL](https://learn.microsoft.com/azure/postgresql/)
- [Azure Cache for Redis](https://learn.microsoft.com/azure/azure-cache-for-redis/)
- [Bicep Language Reference](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)

## 🆘 Soporte

Para problemas o preguntas:

1. Revisar esta documentación
2. Revisar logs en Application Insights
3. Contactar al equipo de DevOps

---

**Última actualización**: 2026-02-05
